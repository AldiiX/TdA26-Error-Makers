﻿﻿﻿using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;

namespace tda26.Server.Controllers;

[ApiController]
[Route("api/courses"), Route("api/v1/courses")]
public sealed class CourseStreamController(
    IStreamBroker fsb,
    IGlobalStreamBroker globalBroker,
    AppDbContext db
) : ControllerBase {
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("{courseId:guid}/stream")]
    [Produces("text/event-stream")]
    public async Task<IActionResult> Stream([FromRoute] Guid courseId, CancellationToken ct) {
        var courseExists = await db.Courses.AsNoTracking().AnyAsync(c => c.Uuid == courseId, ct);
        if(!courseExists) {
            return NotFound(new { message = "Course not found" });
        }

        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("X-Accel-Buffering", "no"); // pro nginx, aby nebufferoval
        await Response.Body.FlushAsync(ct);

        using var keepAliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, HttpContext.RequestAborted);
        var linkedCt = linkedCts.Token;

        IAsyncEnumerator<StreamMessage>? enumerator = null;
        Task<bool>? moveNextTask = null;
        Task<bool>? keepAliveTask = null;

        try {
            enumerator = fsb.SubscribeAsync(courseId, linkedCt).GetAsyncEnumerator(linkedCt);
            moveNextTask = enumerator.MoveNextAsync().AsTask();
            keepAliveTask = keepAliveTimer.WaitForNextTickAsync(linkedCt).AsTask();

            while(true) {
                if(linkedCt.IsCancellationRequested) {
                    break;
                }

                var completed = await Task.WhenAny(moveNextTask, keepAliveTask);

                if(completed == keepAliveTask) {
                    bool ticked;
                    try {
                        ticked = await keepAliveTask;
                    } catch(OperationCanceledException) {
                        break;
                    }

                    if(!ticked || linkedCt.IsCancellationRequested) {
                        break;
                    }

                    try {
                        // sse komentar jako heartbeat
                        await Response.WriteAsync($": ping {DateTimeOffset.UtcNow:O}\n\n", linkedCt);
                        await Response.Body.FlushAsync(linkedCt);
                    } catch(OperationCanceledException) {
                        break;
                    } catch(IOException) {
                        break;
                    } catch(ObjectDisposedException) {
                        break;
                    }

                    // Vytvorime novy keep-alive task pouze po dokonceni predchoziho
                    keepAliveTask = keepAliveTimer.WaitForNextTickAsync(linkedCt).AsTask();
                    continue;
                }

                bool hasNext;
                try {
                    hasNext = await moveNextTask;
                } catch(OperationCanceledException) {
                    break;
                } catch(IOException) {
                    break;
                }

                if(!hasNext) {
                    break;
                }

                var msg = enumerator.Current;
                var json = JsonSerializer.Serialize(msg.Data, JsonOptions);

                try {
                    await Response.WriteAsync($"event: {msg.EventName}\n", linkedCt);
                    await Response.WriteAsync($"data: {json}\n\n", linkedCt);
                    await Response.Body.FlushAsync(linkedCt);
                } catch(OperationCanceledException) {
                    break;
                } catch(IOException) {
                    break;
                } catch(ObjectDisposedException) {
                    break;
                }

                // teprve ted dalsi cteni
                moveNextTask = enumerator.MoveNextAsync().AsTask();
            }
        } finally {
            // zajisti, ze se pripadny movenext odblokuje a dispose enumeratoru nepadne na race
            try { await linkedCts.CancelAsync(); } catch { }

            if(moveNextTask != null) {
                try { await moveNextTask; } catch { }
            }

            if(enumerator != null) {
                try { await enumerator.DisposeAsync(); } catch { }
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Single global SSE stream that emits status_changed events for ALL courses.
    /// Clients can use this instead of opening one stream per course.
    /// </summary>
    [HttpGet("stream")]
    [Produces("text/event-stream")]
    public async Task<IActionResult> GlobalStream(CancellationToken ct) {
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("X-Accel-Buffering", "no");
        await Response.Body.FlushAsync(ct);

        using var keepAliveTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, HttpContext.RequestAborted);
        var linkedCt = linkedCts.Token;

        IAsyncEnumerator<GlobalStreamMessage>? enumerator = null;
        Task<bool>? moveNextTask = null;
        Task<bool>? keepAliveTask = null;

        try {
            enumerator = globalBroker.SubscribeAsync(linkedCt).GetAsyncEnumerator(linkedCt);
            moveNextTask = enumerator.MoveNextAsync().AsTask();
            keepAliveTask = keepAliveTimer.WaitForNextTickAsync(linkedCt).AsTask();

            while(true) {
                if(linkedCt.IsCancellationRequested) break;

                var completed = await Task.WhenAny(moveNextTask, keepAliveTask);

                if(completed == keepAliveTask) {
                    bool ticked;
                    try { ticked = await keepAliveTask; } catch(OperationCanceledException) { break; }
                    if(!ticked || linkedCt.IsCancellationRequested) break;

                    try {
                        await Response.WriteAsync($": ping {DateTimeOffset.UtcNow:O}\n\n", linkedCt);
                        await Response.Body.FlushAsync(linkedCt);
                    } catch { break; }

                    keepAliveTask = keepAliveTimer.WaitForNextTickAsync(linkedCt).AsTask();
                    continue;
                }

                bool hasNext;
                try { hasNext = await moveNextTask; } catch { break; }
                if(!hasNext) break;

                var msg = enumerator.Current;
                var payload = new { courseId = msg.CourseId, data = msg.Data };
                var json = JsonSerializer.Serialize(payload, JsonOptions);

                try {
                    await Response.WriteAsync($"event: {msg.EventName}\n", linkedCt);
                    await Response.WriteAsync($"data: {json}\n\n", linkedCt);
                    await Response.Body.FlushAsync(linkedCt);
                } catch { break; }

                moveNextTask = enumerator.MoveNextAsync().AsTask();
            }
        } finally {
            try { await linkedCts.CancelAsync(); } catch { }
            if(moveNextTask != null) { try { await moveNextTask; } catch { } }
            if(enumerator != null) { try { await enumerator.DisposeAsync(); } catch { } }
        }

        return new EmptyResult();
    }
}



// pomocne classy
public sealed record StreamMessage(string EventName, object Data);

// --- Global (all-courses) stream broker ---
public sealed record GlobalStreamMessage(string EventName, Guid CourseId, object Data);

public interface IGlobalStreamBroker {
    Task PublishAsync(GlobalStreamMessage message, CancellationToken ct = default);
    IAsyncEnumerable<GlobalStreamMessage> SubscribeAsync(CancellationToken ct = default);
}

public class InMemoryGlobalStreamBroker : IGlobalStreamBroker {
    private readonly ConcurrentDictionary<Guid, Channel<GlobalStreamMessage>> _subscribers = new();

    public async Task PublishAsync(GlobalStreamMessage message, CancellationToken ct = default) {
        foreach(var (subscriberId, ch) in _subscribers) {
            try {
                await ch.Writer.WriteAsync(message, ct);
            } catch {
                _subscribers.TryRemove(subscriberId, out _);
                ch.Writer.TryComplete();
            }
        }
    }

    public IAsyncEnumerable<GlobalStreamMessage> SubscribeAsync(CancellationToken ct = default) {
        var subscriberId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<GlobalStreamMessage>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false
        });
        _subscribers[subscriberId] = channel;
        return new GlobalSubscriptionEnumerable(this, subscriberId, channel, ct);
    }

    internal void Unsubscribe(Guid subscriberId) {
        if(_subscribers.TryRemove(subscriberId, out var ch)) {
            ch.Writer.TryComplete();
        }
    }

    private sealed class GlobalSubscriptionEnumerable(
        InMemoryGlobalStreamBroker broker,
        Guid subscriberId,
        Channel<GlobalStreamMessage> channel,
        CancellationToken outerCt
    ) : IAsyncEnumerable<GlobalStreamMessage> {
        public IAsyncEnumerator<GlobalStreamMessage> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(outerCt, cancellationToken);
            return new GlobalSubscriptionEnumerator(broker, subscriberId, channel, linkedCts);
        }
    }

    private sealed class GlobalSubscriptionEnumerator(
        InMemoryGlobalStreamBroker broker,
        Guid subscriberId,
        Channel<GlobalStreamMessage> channel,
        CancellationTokenSource linkedCts
    ) : IAsyncEnumerator<GlobalStreamMessage> {
        private bool _disposed;
        public GlobalStreamMessage Current { get; private set; } = new("", Guid.Empty, new { });

        public async ValueTask<bool> MoveNextAsync() {
            if(_disposed) return false;
            try {
                while(true) {
                    if(channel.Reader.TryRead(out var msg)) {
                        Current = msg;
                        return true;
                    }
                    var canRead = await channel.Reader.WaitToReadAsync(linkedCts.Token);
                    if(!canRead) return false;
                }
            } catch(OperationCanceledException) {
                return false;
            }
        }

        public ValueTask DisposeAsync() {
            if(_disposed) return ValueTask.CompletedTask;
            _disposed = true;
            try { linkedCts.Cancel(); } catch { }
            broker.Unsubscribe(subscriberId);
            linkedCts.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
// --- end global stream broker ---

public interface IStreamBroker {
    // posle zpravu vsem pripojenym klientum pro dany kurz
    Task PublishAsync(Guid courseId, StreamMessage message, CancellationToken ct = default);

    // vrati stream zprav pro konkretni kurz (pro jednu sse connection)
    IAsyncEnumerable<StreamMessage> SubscribeAsync(Guid courseId, CancellationToken ct = default);
}

public class InMemoryStreamBroker : IStreamBroker {
    private readonly IGlobalStreamBroker _globalBroker;

    public InMemoryStreamBroker(IGlobalStreamBroker globalBroker) {
        _globalBroker = globalBroker;
    }

    // kurz -> (subscriberid -> channel)
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<StreamMessage>>> _subscribers = new();

    public async Task PublishAsync(Guid courseId, StreamMessage message, CancellationToken ct = default) {
        // forward status_changed to the global broker so a single client connection gets all updates
        if(message.EventName == "status_changed") {
            _ = _globalBroker.PublishAsync(new GlobalStreamMessage(message.EventName, courseId, message.Data), ct);
        }

        if(!_subscribers.TryGetValue(courseId, out var courseSubs)) {
            return;
        }

        foreach(var (subscriberId, ch) in courseSubs) {
            try {
                await ch.Writer.WriteAsync(message, ct);
            } catch {
                // klient je pravdepodobne dead, uklidi se
                courseSubs.TryRemove(subscriberId, out _);
                ch.Writer.TryComplete();
            }
        }

        if(courseSubs.IsEmpty) {
            _subscribers.TryRemove(courseId, out _);
        }
    }

    public IAsyncEnumerable<StreamMessage> SubscribeAsync(Guid courseId, CancellationToken ct = default) {
        return new StreamSubscriptionEnumerable(this, courseId, ct);
    }

    private sealed class StreamSubscriptionEnumerable(InMemoryStreamBroker broker, Guid courseId, CancellationToken outerCt) : IAsyncEnumerable<StreamMessage> {
        public IAsyncEnumerator<StreamMessage> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(outerCt, cancellationToken);
            return new StreamSubscriptionEnumerator(broker, courseId, linkedCts);
        }
    }

    private sealed class StreamSubscriptionEnumerator : IAsyncEnumerator<StreamMessage> {
        private readonly InMemoryStreamBroker _broker;
        private readonly Guid _courseId;
        private readonly Guid _subscriberId;
        private readonly Channel<StreamMessage> _channel;
        private readonly CancellationTokenSource _linkedCts;

        private bool _disposed;

        public StreamMessage Current { get; private set; } = new("", new { });

        public StreamSubscriptionEnumerator(InMemoryStreamBroker broker, Guid courseId, CancellationTokenSource linkedCts) {
            _broker = broker;
            _courseId = courseId;
            _linkedCts = linkedCts;

            _subscriberId = Guid.NewGuid();
            _channel = Channel.CreateUnbounded<StreamMessage>(new UnboundedChannelOptions {
                SingleReader = true,
                SingleWriter = false
            });

            var courseSubs = _broker._subscribers.GetOrAdd(_courseId, _ => new ConcurrentDictionary<Guid, Channel<StreamMessage>>());
            courseSubs[_subscriberId] = _channel;
        }

        public async ValueTask<bool> MoveNextAsync() {
            if(_disposed) {
                return false;
            }

            try {
                while(true) {
                    if(_channel.Reader.TryRead(out var msg)) {
                        Current = msg;
                        return true;
                    }

                    var canRead = await _channel.Reader.WaitToReadAsync(_linkedCts.Token);
                    if(!canRead) {
                        return false;
                    }
                }
            } catch(OperationCanceledException) {
                return false;
            }
        }

        public ValueTask DisposeAsync() {
            if(_disposed) {
                return ValueTask.CompletedTask;
            }

            _disposed = true;

            // zrusi cekani ve movenext
            try { _linkedCts.Cancel(); } catch { }

            // odregistruje subscriber
            if(_broker._subscribers.TryGetValue(_courseId, out var courseSubs)) {
                courseSubs.TryRemove(_subscriberId, out _);

                if(courseSubs.IsEmpty) {
                    _broker._subscribers.TryRemove(_courseId, out _);
                }
            }

            _channel.Writer.TryComplete();
            _linkedCts.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
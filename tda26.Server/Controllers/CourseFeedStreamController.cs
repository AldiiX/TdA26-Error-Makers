using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;

namespace tda26.Server.Controllers;

[ApiController]
[Route("api/courses"), Route("api/v1/courses"), Route("api/v2/courses")]
public class CourseFeedStreamController(
    IFeedStreamBroker fsb,
    AppDbContext db
) : ControllerBase {
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("{courseId:guid}/feed/stream")]
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

        IAsyncEnumerator<FeedStreamMessage>? enumerator = null;
        Task<bool>? moveNextTask = null;

        try {
            enumerator = fsb.SubscribeAsync(courseId, linkedCt).GetAsyncEnumerator(linkedCt);
            moveNextTask = enumerator.MoveNextAsync().AsTask();

            while(true) {
                if(linkedCt.IsCancellationRequested) {
                    break;
                }

                var keepAliveTask = keepAliveTimer.WaitForNextTickAsync(linkedCt).AsTask();
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
            try { linkedCts.Cancel(); } catch { }

            if(moveNextTask != null) {
                try { await moveNextTask; } catch { }
            }

            if(enumerator != null) {
                try { await enumerator.DisposeAsync(); } catch { }
            }
        }

        return new EmptyResult();
    }
}



// pomocne classy
public sealed record FeedStreamMessage(string EventName, object Data);

public interface IFeedStreamBroker {
    // posle zpravu vsem pripojenym klientum pro dany kurz
    Task PublishAsync(Guid courseId, FeedStreamMessage message, CancellationToken ct = default);

    // vrati stream zprav pro konkretni kurz (pro jednu sse connection)
    IAsyncEnumerable<FeedStreamMessage> SubscribeAsync(Guid courseId, CancellationToken ct = default);
}

public class InMemoryFeedStreamBroker : IFeedStreamBroker {
    // kurz -> (subscriberid -> channel)
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<FeedStreamMessage>>> _subscribers = new();

    public async Task PublishAsync(Guid courseId, FeedStreamMessage message, CancellationToken ct = default) {
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

    public IAsyncEnumerable<FeedStreamMessage> SubscribeAsync(Guid courseId, CancellationToken ct = default) {
        return new FeedStreamSubscriptionEnumerable(this, courseId, ct);
    }

    private sealed class FeedStreamSubscriptionEnumerable(InMemoryFeedStreamBroker broker, Guid courseId, CancellationToken outerCt) : IAsyncEnumerable<FeedStreamMessage> {
        public IAsyncEnumerator<FeedStreamMessage> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(outerCt, cancellationToken);
            return new FeedStreamSubscriptionEnumerator(broker, courseId, linkedCts);
        }
    }

    private sealed class FeedStreamSubscriptionEnumerator : IAsyncEnumerator<FeedStreamMessage> {
        private readonly InMemoryFeedStreamBroker _broker;
        private readonly Guid _courseId;
        private readonly Guid _subscriberId;
        private readonly Channel<FeedStreamMessage> _channel;
        private readonly CancellationTokenSource _linkedCts;

        private bool _disposed;

        public FeedStreamMessage Current { get; private set; } = new("", new { });

        public FeedStreamSubscriptionEnumerator(InMemoryFeedStreamBroker broker, Guid courseId, CancellationTokenSource linkedCts) {
            _broker = broker;
            _courseId = courseId;
            _linkedCts = linkedCts;

            _subscriberId = Guid.NewGuid();
            _channel = Channel.CreateUnbounded<FeedStreamMessage>(new UnboundedChannelOptions {
                SingleReader = true,
                SingleWriter = false
            });

            var courseSubs = _broker._subscribers.GetOrAdd(_courseId, _ => new ConcurrentDictionary<Guid, Channel<FeedStreamMessage>>());
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
using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.Services;

public sealed class DailyRewardsService(AppDbContext db) : IDailyRewardsService {
    private static readonly DailyRewardTaskDefinition[] TaskDefinitions = [
        new("view_material", "Zobrazit material", "Zobraz jeden material", 1, 50, 10),
        new("view_course", "Zobrazit kurz", "Otevři libovolný kurz", 1, 35, 8),
        new("submit_quiz", "Dokončit kvíz", "Odešli jeden kvíz", 1, 80, 20)
    ];

    private static readonly Dictionary<string, DailyRewardTaskDefinition> TaskDefinitionsByCode = TaskDefinitions
        .ToDictionary(def => def.TaskCode, def => def);

    private static readonly Dictionary<DailyRewardEventType, string[]> EventTaskMap = new() {
        [DailyRewardEventType.CourseView] = ["view_course"],
        [DailyRewardEventType.MaterialView] = ["view_material"],
        [DailyRewardEventType.QuizSubmit] = ["submit_quiz"]
    };

    public async Task TrackEventAsync(Guid accountUuid, DailyRewardEventType eventType, CancellationToken ct = default) {
        if (!EventTaskMap.TryGetValue(eventType, out var taskCodes) || taskCodes.Length == 0) return;

        var today = GetTodayDate();
        var day = await EnsureDayAsync(accountUuid, today, ct);

        var utcNow = DateTimeOffset.UtcNow;
        foreach (var task in day.Tasks.Where(t => taskCodes.Contains(t.TaskCode))) {
            if (task.IsCompleted) continue;

            task.CurrentValue = Math.Min(task.TargetValue, task.CurrentValue + 1);
            if (task.CurrentValue >= task.TargetValue) {
                task.IsCompleted = true;
                task.CompletedAt = utcNow;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<ReadDailyRewardsResponse> GetMonthAsync(Guid accountUuid, int year, int month, CancellationToken ct = default) {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = new DateOnly(year, month, daysInMonth);

        var existingDays = await db.DailyRewardDays
            .AsNoTracking()
            .Include(d => d.Tasks)
            .Where(d => d.AccountUuid == accountUuid)
            .Where(d => d.RewardDate >= monthStart && d.RewardDate <= monthEnd)
            .ToListAsync(ct);

        var existingByDate = existingDays.ToDictionary(d => d.RewardDate, d => d);
        var wallet = await GetWalletAsync(accountUuid, ct);
        var result = new ReadDailyRewardsResponse {
            Year = year,
            Month = month,
            DaysInMonth = daysInMonth,
            TotalXp = wallet.TotalXp,
            TotalDucks = wallet.TotalDucks,
            Days = []
        };

        for (var dayNumber = 1; dayNumber <= daysInMonth; dayNumber++) {
            var date = new DateOnly(year, month, dayNumber);
            if (existingByDate.TryGetValue(date, out var existing)) {
                result.Days.Add(ToDayDto(existing));
                continue;
            }

            result.Days.Add(new ReadDailyRewardDayResponse {
                Date = date.ToString("yyyy-MM-dd"),
                IsClaimed = false,
                ClaimedAt = null,
                CanClaim = false,
                IsCompleted = false,
                Tasks = TaskDefinitions.Select(def => ToTaskDto(def, 0, false, null)).ToList()
            });
        }

        return result;
    }

    public async Task<DailyRewardClaimResult> ClaimDayAsync(Guid accountUuid, DateOnly date, CancellationToken ct = default) {
        var today = GetTodayDate();
        if (date > today) {
            return new DailyRewardClaimResult {
                Success = false,
                Error = "Budoucí den nelze odemknout."
            };
        }

        var day = await EnsureDayAsync(accountUuid, date, ct);

        if (day.ClaimedAt != null) {
            return new DailyRewardClaimResult {
                Success = true,
                Day = ToDayDto(day)
            };
        }

        if (day.Tasks.Any(t => !t.IsCompleted)) {
            return new DailyRewardClaimResult {
                Success = false,
                Error = "Nejdřív dokonči všechny denní úkoly."
            };
        }

        day.ClaimedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return new DailyRewardClaimResult {
            Success = true,
            Day = ToDayDto(day)
        };
    }

    public async Task<DailyRewardsWallet> GetWalletAsync(Guid accountUuid, CancellationToken ct = default) {
        var completedTaskCodes = await db.DailyRewardTaskStates
            .AsNoTracking()
            .Where(task => task.IsCompleted)
            .Where(task => task.DailyRewardDay.AccountUuid == accountUuid)
            .Select(task => task.TaskCode)
            .ToListAsync(ct);

        var totalXp = 0;
        var totalDucks = 0;
        foreach (var code in completedTaskCodes) {
            if (!TaskDefinitionsByCode.TryGetValue(code, out var definition)) continue;
            totalXp += definition.RewardXp;
            totalDucks += definition.RewardDuck;
        }

        return new DailyRewardsWallet {
            TotalXp = totalXp,
            TotalDucks = totalDucks
        };
    }

    private async Task<DailyRewardDay> EnsureDayAsync(Guid accountUuid, DateOnly date, CancellationToken ct) {
        var day = await db.DailyRewardDays
            .Include(d => d.Tasks)
            .FirstOrDefaultAsync(d => d.AccountUuid == accountUuid && d.RewardDate == date, ct);

        if (day == null) {
            day = new DailyRewardDay {
                AccountUuid = accountUuid,
                RewardDate = date,
                Tasks = TaskDefinitions.Select(def => new DailyRewardTaskState {
                    TaskCode = def.TaskCode,
                    Title = def.Title,
                    Description = def.Description,
                    CurrentValue = 0,
                    TargetValue = def.TargetValue,
                    IsCompleted = false,
                    CompletedAt = null
                }).ToList()
            };

            db.DailyRewardDays.Add(day);
            await db.SaveChangesAsync(ct);
            return day;
        }

        var existingCodes = day.Tasks.Select(t => t.TaskCode).ToHashSet();
        var missingDefinitions = TaskDefinitions.Where(def => !existingCodes.Contains(def.TaskCode)).ToList();
        if (missingDefinitions.Count == 0) return day;

        foreach (var def in missingDefinitions) {
            day.Tasks.Add(new DailyRewardTaskState {
                TaskCode = def.TaskCode,
                Title = def.Title,
                Description = def.Description,
                CurrentValue = 0,
                TargetValue = def.TargetValue,
                IsCompleted = false,
                CompletedAt = null
            });
        }

        await db.SaveChangesAsync(ct);
        return day;
    }

    private static ReadDailyRewardDayResponse ToDayDto(DailyRewardDay day) {
        var tasks = day.Tasks
            .OrderBy(t => GetTaskOrder(t.TaskCode))
            .ThenBy(t => t.TaskCode)
            .Select(ToTaskDtoFromState)
            .ToList();

        var isCompleted = tasks.Count > 0 && tasks.All(t => t.IsCompleted);

        return new ReadDailyRewardDayResponse {
            Date = day.RewardDate.ToString("yyyy-MM-dd"),
            IsClaimed = day.ClaimedAt != null,
            ClaimedAt = day.ClaimedAt,
            CanClaim = day.ClaimedAt == null && isCompleted,
            IsCompleted = isCompleted,
            Tasks = tasks
        };
    }

    private static ReadDailyRewardTaskResponse ToTaskDto(
        DailyRewardTaskDefinition definition,
        int currentValue,
        bool isCompleted,
        DateTimeOffset? completedAt
    ) {
        return new ReadDailyRewardTaskResponse {
            TaskCode = definition.TaskCode,
            Title = definition.Title,
            Description = definition.Description,
            CurrentValue = currentValue,
            TargetValue = definition.TargetValue,
            IsCompleted = isCompleted,
            CompletedAt = completedAt,
            RewardXp = definition.RewardXp,
            RewardDuck = definition.RewardDuck
        };
    }

    private static ReadDailyRewardTaskResponse ToTaskDtoFromState(DailyRewardTaskState state) {
        var rewardXp = 0;
        var rewardDuck = 0;
        if (TaskDefinitionsByCode.TryGetValue(state.TaskCode, out var definition)) {
            rewardXp = definition.RewardXp;
            rewardDuck = definition.RewardDuck;
        }

        return new ReadDailyRewardTaskResponse {
            TaskCode = state.TaskCode,
            Title = state.Title,
            Description = state.Description,
            CurrentValue = state.CurrentValue,
            TargetValue = state.TargetValue,
            IsCompleted = state.IsCompleted,
            CompletedAt = state.CompletedAt,
            RewardXp = rewardXp,
            RewardDuck = rewardDuck
        };
    }

    private static int GetTaskOrder(string taskCode) {
        for (var i = 0; i < TaskDefinitions.Length; i++) {
            if (TaskDefinitions[i].TaskCode == taskCode) return i;
        }

        return int.MaxValue;
    }

    private static DateOnly GetTodayDate() {
        var pragueTimeZone = ResolvePragueTimeZone();
        var localDate = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, pragueTimeZone).Date;
        return DateOnly.FromDateTime(localDate);
    }

    private static TimeZoneInfo ResolvePragueTimeZone() {
        try {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
        } catch (TimeZoneNotFoundException) {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        }
    }

    private sealed record DailyRewardTaskDefinition(
        string TaskCode,
        string Title,
        string Description,
        int TargetValue,
        int RewardXp,
        int RewardDuck
    );
}




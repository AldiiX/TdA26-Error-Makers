using tda26.Server.DTOs.v1;

namespace tda26.Server.Services;

public enum DailyRewardEventType {
    Login,
    CourseView,
    MaterialView,
    QuizSubmit
}

public sealed class DailyRewardClaimResult {
    public bool Success { get; set; }
    public string? Error { get; set; }
    public ReadDailyRewardDayResponse? Day { get; set; }
}

public sealed class DailyRewardsWallet {
    public int TotalXp { get; set; }
    public int TotalDucks { get; set; }
}

public interface IDailyRewardsService {
    Task TrackEventAsync(Guid accountUuid, DailyRewardEventType eventType, CancellationToken ct = default);
    Task<ReadDailyRewardsResponse> GetMonthAsync(Guid accountUuid, int year, int month, CancellationToken ct = default);
    Task<DailyRewardClaimResult> ClaimDayAsync(Guid accountUuid, DateOnly date, CancellationToken ct = default);
    Task<DailyRewardsWallet> GetWalletAsync(Guid accountUuid, CancellationToken ct = default);
}


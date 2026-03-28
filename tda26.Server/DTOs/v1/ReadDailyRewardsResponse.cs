namespace tda26.Server.DTOs.v1;

public sealed class ReadDailyRewardsResponse {
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public int TotalXp { get; set; }
    public int TotalDucks { get; set; }
    public List<ReadDailyRewardDayResponse> Days { get; set; } = [];
}

public sealed class ReadDailyRewardDayResponse {
    public string Date { get; set; } = string.Empty;
    public bool IsClaimed { get; set; }
    public DateTimeOffset? ClaimedAt { get; set; }
    public bool CanClaim { get; set; }
    public bool IsCompleted { get; set; }
    public List<ReadDailyRewardTaskResponse> Tasks { get; set; } = [];
}

public sealed class ReadDailyRewardTaskResponse {
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int RewardXp { get; set; }
    public int RewardDuck { get; set; }
}



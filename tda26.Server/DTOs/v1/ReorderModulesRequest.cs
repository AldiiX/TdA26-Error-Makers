namespace tda26.Server.DTOs.v1;

public class ReorderModulesRequest {
    public required List<ReorderModulesItem> Modules { get; set; }
}

public class ReorderModulesItem {
    public required Guid Uuid { get; set; }
    public required int Order { get; set; }
}

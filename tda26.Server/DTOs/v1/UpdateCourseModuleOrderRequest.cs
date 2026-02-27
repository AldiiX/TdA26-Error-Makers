namespace tda26.Server.DTOs.v1;

public class UpdateCourseModuleOrderRequestItem {
    public Guid Uuid { get; set; }
    public required string ModuleType { get; set; }
    public int Order { get; set; }
}

public class UpdateCourseModuleOrderRequest {
    public required List<UpdateCourseModuleOrderRequestItem> ModuleOrders { get; set; }
}
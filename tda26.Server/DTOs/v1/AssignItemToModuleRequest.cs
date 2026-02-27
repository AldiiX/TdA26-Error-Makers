namespace tda26.Server.DTOs.v1;

public class AssignItemToModuleRequest {
    public Guid ItemUuid { get; set; }
    /// <summary>
    /// "material" or "quiz"
    /// </summary>
    public string ItemType { get; set; } = null!;
    /// <summary>
    /// Null to unassign the item from any module.
    /// </summary>
    public Guid? ModuleUuid { get; set; }
}

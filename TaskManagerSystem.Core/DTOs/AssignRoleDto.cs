namespace TaskManagerSystem.Core.DTOs
{
    // Bu DTO, "Hangi kullanıcıya hangi rolü atayalım?" sorusunu taşır.
    // Controller'ın isteğin (Request) gövdesinden (Body) okuyacağı veri formatıdır.
    public class AssignRoleDto
    {
        // Rolü atanacak kişinin kimliği
        public Guid UserId { get; set; }

        // Atanacak rolün adı: "Admin", "ProjectManager", "Employee" vb.
        public string RoleName { get; set; } = string.Empty;
    }
}

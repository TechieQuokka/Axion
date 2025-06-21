using Microsoft.AspNetCore.Identity;

namespace ERP.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int CompanyId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public DateTime? HireDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // 비즈니스 User와의 연결을 위한 속성 (선택사항)
        public int? BusinessUserId { get; set; }
    }
}

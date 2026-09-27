using System;
using System.Collections.Generic;

namespace AccuBooks_ERP.GetterNSetter
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsLockedOut { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class UserRole : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }

    public class Permission : BaseEntity
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public string ModuleName { get; set; } = string.Empty;
        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanExport { get; set; } = false;
    }

    public class AuditLog : BaseEntity
    {
        public string? Username { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? MachineName { get; set; }
        public string? Details { get; set; }
    }
}

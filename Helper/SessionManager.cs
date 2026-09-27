using System;
using System.Collections.Generic;
using System.Linq;
using AccuBooks_ERP.GetterNSetter;

namespace AccuBooks_ERP.Helper
{
    public class SessionManager
    {
        private static SessionManager? _instance;
        private static readonly object _lock = new object();

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SessionManager();
                    }
                }
                return _instance;
            }
        }

        public User? CurrentUser { get; private set; }
        public List<string> CurrentRoles { get; private set; } = new();
        public List<Permission> CurrentPermissions { get; private set; } = new();
        public CompanyProfile? ActiveCompany { get; set; }
        public FinancialYear? ActiveFinancialYear { get; set; }
        public DateTime LoginTime { get; private set; }

        public bool IsAuthenticated => CurrentUser != null;

        public event Action? SessionChanged;

        private SessionManager() { }

        public void SetSession(User user, IEnumerable<string> roles, IEnumerable<Permission> permissions, CompanyProfile? company, FinancialYear? fy)
        {
            CurrentUser = user;
            CurrentRoles = roles.ToList();
            CurrentPermissions = permissions.ToList();
            ActiveCompany = company;
            ActiveFinancialYear = fy;
            LoginTime = DateTime.UtcNow;

            SessionChanged?.Invoke();
        }

        public void ClearSession()
        {
            CurrentUser = null;
            CurrentRoles.Clear();
            CurrentPermissions.Clear();
            ActiveCompany = null;
            ActiveFinancialYear = null;

            SessionChanged?.Invoke();
        }

        public bool IsInRole(string roleName)
        {
            return CurrentRoles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasPermission(string moduleName, string action = "View")
        {
            if (IsInRole("Admin"))
                return true;

            var perm = CurrentPermissions.FirstOrDefault(p => p.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
            if (perm == null)
                return false;

            return action.ToLowerInvariant() switch
            {
                "view" => perm.CanView,
                "create" => perm.CanCreate,
                "edit" => perm.CanEdit,
                "delete" => perm.CanDelete,
                "export" => perm.CanExport,
                _ => false
            };
        }
    }
}

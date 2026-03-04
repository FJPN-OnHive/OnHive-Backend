namespace OnHive.Configuration.Library.Models
{
    public static class PermissionsStore
    {
        public static List<PermissionItem> Permissions { get; private set; } = [];

        public static void AddPermission(string permission, string endpoint)
        {
            var currentPermission = Permissions.FirstOrDefault(x => x.Permission == permission);
            if (currentPermission != null && !currentPermission.Endpoints.Contains(endpoint))
            {
                currentPermission.Endpoints.Add(endpoint);
            }
            else
            {
                Permissions.Add(new PermissionItem
                {
                    Permission = permission,
                    Endpoints = new List<string> { endpoint }
                });
            }
        }
    }

    public class PermissionItem
    {
        public string Permission { get; set; } = string.Empty;

        public List<string> Endpoints { get; set; } = [];
    }
}
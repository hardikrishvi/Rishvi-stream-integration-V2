using System;
using System.Collections.Generic;
using Rishvi.Modules.Core.Data;
using Rishvi.Web.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Models
{
    public class AdminPermission : IModificationHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public int? Left { get; set; }
        public int? Right { get; set; }

        public int Depth { get; set; }

        public Guid? ParentId { get; set; }

        public AdminPermission Parent { get; set; }
        public List<AdminPermission> Children { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<AdminRolesAdminPermissions> AdminRolesAdminPermissions { get; set; }

        public static AdminPermission Create(string displayName, string name, Guid? parentId = null)
        {
            return new AdminPermission
            {
                DisplayName = displayName,
                Name = name,
                ParentId = parentId
            };
        }

		public static AdminPermission[] CreateInsertUpdateDelete(string displayName, string name, Guid parentId,
			bool createPermission = true, bool editPermission = true, bool deletePermission = true)
		{
			var adminPermissions = new List<AdminPermission>();

			if (createPermission)
			{
				adminPermissions.Add(Create("Create", name + ".create", parentId));
			}

			if (editPermission)
			{
				adminPermissions.Add(Create("Edit", name + ".edit", parentId));
			}

			if (deletePermission)
			{
				adminPermissions.Add(Create("Delete", name + ".delete", parentId));
			}

			return adminPermissions.ToArray();
		}
	}
}

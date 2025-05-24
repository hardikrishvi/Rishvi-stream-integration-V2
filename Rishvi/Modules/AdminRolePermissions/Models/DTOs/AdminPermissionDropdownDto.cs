using System;
using System.Collections.Generic;
using System.Linq;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminPermissionDropDownDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        private string _displayName { get; set; }
        public int? Left { get; set; }
        public int? Right { get; set; }
        public int Depth { get; set; }        
        public Guid? ParentId { get; set; }

        public string DisplayName
        {
            get => string.Concat(Enumerable.Repeat("| - ", Depth)) + _displayName + " [" + Name + "]";
            set => _displayName = value;
        }

        //public List<PermissionChildItemDto> Children { get; set; }
    }

    public class PermissionChildItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        private string _displayName { get; set; }
        public int? Left { get; set; }
        public int? Right { get; set; }
        public int Depth { get; set; }
        public Guid? ParentId { get; set; }

        public string DisplayName
        {
            get => string.Concat(Enumerable.Repeat("| - ", Depth)) + _displayName + " [" + Name + "]";
            set => _displayName = value;
        }
    }
}

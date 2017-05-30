using System;
using System.Collections.Generic;

namespace MySql.AspNet.Identity
{
    public class ApplicationGroup
    {
        public ApplicationGroup()
        {
            Id = Guid.NewGuid().ToString();
            ApplicationGroupRoles = new List<ApplicationGroupRole>();
            ApplicationUserGroups = new List<ApplicationUserGroup>();
        }

        public ApplicationGroup(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NumberOfUsers { get; set; }
        public List<ApplicationGroupRole> ApplicationGroupRoles { get; set; }
        public List<ApplicationUserGroup> ApplicationUserGroups { get; set; }
    }

    public class ApplicationUserGroup
    {
        public string ApplicationUserId { get; set; }
        public string ApplicationGroupId { get; set; }
    }

    public class ApplicationGroupRole
    {
        public string ApplicationGroupId { get; set; }
        public string RoleId { get; set; }
    }
}

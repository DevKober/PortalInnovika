//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PortalInnovika.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Role
    {
        public Role()
        {
            this.UsersInRoles = new HashSet<UsersInRole>();
        }
    
        public string RoleName { get; set; }
    
        public virtual ICollection<UsersInRole> UsersInRoles { get; set; }
    }
}

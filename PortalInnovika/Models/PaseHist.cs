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
    
    public partial class PaseHist
    {
        public System.Guid idPaseGuardado { get; set; }
        public int Usuario { get; set; }
        public System.DateTime TmAlta { get; set; }
        public string PaseHash { get; set; }
        public string PaseSalt { get; set; }
    
        public virtual User User { get; set; }
    }
}
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
    
    public partial class Favorito
    {
        public Favorito()
        {
            this.FavArticulos = new HashSet<FavArticulo>();
        }
    
        public int IdFavorito { get; set; }
        public int Usuario { get; set; }
        public string ClienteERP { get; set; }
        public Nullable<System.DateTime> FechaFavorito { get; set; }
        public Nullable<bool> Exhibicion { get; set; }
        public string Observaciones { get; set; }
        public int Proyecto { get; set; }
        public string Nombre { get; set; }
    
        public virtual ICollection<FavArticulo> FavArticulos { get; set; }
        public virtual User User { get; set; }
    }
}

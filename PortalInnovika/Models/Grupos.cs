//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PortalInnovika.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Grupos
    {
        public Grupos()
        {
            this.DetalleGrupos = new HashSet<DetalleGrupos>();
        }
    
        public int IdGrupo { get; set; }
        public string Descripcion { get; set; }
        public string Estatus { get; set; }
    
        public virtual ICollection<DetalleGrupos> DetalleGrupos { get; set; }
    }
}

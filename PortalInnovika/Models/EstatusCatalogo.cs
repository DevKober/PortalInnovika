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
    
    public partial class EstatusCatalogo
    {
        public EstatusCatalogo()
        {
            this.ArtBases = new HashSet<ArtBas>();
            this.ArtColores = new HashSet<ArtColore>();
            this.ArtCubrecantos = new HashSet<ArtCubrecanto>();
            this.ArtJaladeraPos = new HashSet<ArtJaladeraPos>();
            this.ArtJaladeras = new HashSet<ArtJaladera>();
            this.ArtLineas = new HashSet<ArtLinea>();
            this.ArtSubLineas = new HashSet<ArtSubLinea>();
            this.ArtTipos = new HashSet<ArtTipos>();
            this.ArtVariantes = new HashSet<ArtVariante>();
            this.ArtVetas = new HashSet<ArtVeta>();
        }
    
        public string IdEstatus { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    
        public virtual ICollection<ArtBas> ArtBases { get; set; }
        public virtual ICollection<ArtColore> ArtColores { get; set; }
        public virtual ICollection<ArtCubrecanto> ArtCubrecantos { get; set; }
        public virtual ICollection<ArtJaladeraPos> ArtJaladeraPos { get; set; }
        public virtual ICollection<ArtJaladera> ArtJaladeras { get; set; }
        public virtual ICollection<ArtLinea> ArtLineas { get; set; }
        public virtual ICollection<ArtSubLinea> ArtSubLineas { get; set; }
        public virtual ICollection<ArtTipos> ArtTipos { get; set; }
        public virtual ICollection<ArtVariante> ArtVariantes { get; set; }
        public virtual ICollection<ArtVeta> ArtVetas { get; set; }
    }
}

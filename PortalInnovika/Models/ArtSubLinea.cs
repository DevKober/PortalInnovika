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
    
    public partial class ArtSubLinea
    {
        public ArtSubLinea()
        {
            this.ArtColores = new HashSet<ArtColore>();
        }
    
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Estatus { get; set; }
        public string RutaImagen { get; set; }
        public int Orden { get; set; }
    
        public virtual ICollection<ArtColore> ArtColores { get; set; }
        public virtual EstatusCatalogo EstatusCatalogo { get; set; }
    }
}

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
    
    public partial class DetalleGrupos
    {
        public int IdDetalleGrupo { get; set; }
        public string Tipo { get; set; }
        public string Base { get; set; }
        public string Color { get; set; }
        public string Cubrecanto { get; set; }
        public int Grupo { get; set; }
        public string Descripcion { get; set; }
        public string Estatus { get; set; }
    
        public virtual Grupos Grupos { get; set; }
    }
}
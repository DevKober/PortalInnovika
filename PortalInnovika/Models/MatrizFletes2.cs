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
    
    public partial class MatrizFletes2
    {
        public int ElementoMatrizId { get; set; }
        public int FleteraId { get; set; }
        public int CatalogoCiudadId { get; set; }
        public decimal Caj_1_3 { get; set; }
        public decimal Caj_4_10 { get; set; }
        public decimal Caj_11_20 { get; set; }
        public decimal Caj_21up { get; set; }
        public decimal Paq_1_3 { get; set; }
        public decimal Paq_4_10 { get; set; }
        public decimal Paq_11_20 { get; set; }
        public decimal Paq_21up { get; set; }
        public decimal CostoM2 { get; set; }
    
        public virtual CatalogoCiudade CatalogoCiudade { get; set; }
        public virtual Fletera Fletera { get; set; }
    }
}

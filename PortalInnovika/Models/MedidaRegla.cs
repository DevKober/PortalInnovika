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
    
    public partial class MedidaRegla
    {
        public int idReglaMedida { get; set; }
        public string Tipo { get; set; }
        public string Base { get; set; }
        public Nullable<int> AnchoMax { get; set; }
        public Nullable<int> AnchoMin { get; set; }
        public Nullable<int> AltoMax { get; set; }
        public Nullable<int> AltoMin { get; set; }
        public Nullable<decimal> Multiplo70 { get; set; }
        public Nullable<decimal> Multiplo72 { get; set; }
        public Nullable<decimal> Multiplo76 { get; set; }
        public Nullable<decimal> Multiplo80 { get; set; }
        public string PosicionJaladeras { get; set; }
        public Nullable<decimal> EquivalenciaM2 { get; set; }
        public string SubLinea { get; set; }
    }
}

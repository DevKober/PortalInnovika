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
    
    public partial class vProyectosRegistrado
    {
        public int IdProyecto { get; set; }
        public int Usuario { get; set; }
        public Nullable<System.DateTime> TmRegistrado { get; set; }
        public string ReferenciaCliente { get; set; }
        public Nullable<int> numArt { get; set; }
        public Nullable<decimal> PrecioTotal { get; set; }
        public string Estatus { get; set; }
        public string NomEstatus { get; set; }
        public string Direccion { get; set; }
        public Nullable<bool> EnvioOcurre { get; set; }
        public string Telefono { get; set; }
        public string Pais { get; set; }
        public string Estado { get; set; }
        public string CodigoPostal { get; set; }
        public string Colonia { get; set; }
        public string Poblacion { get; set; }
        public Nullable<bool> EnvioAsegurado { get; set; }
        public string FormaDeEnvio { get; set; }
        public Nullable<System.DateTime> TmEntregaTentativa { get; set; }
        public string GuiaEmbarque { get; set; }
        public Nullable<bool> EntregaEnPlanta { get; set; }
        public Nullable<bool> EnvioCobrado { get; set; }
        public string LoteProdInterno { get; set; }
        public Nullable<bool> EsExpress { get; set; }
        public Nullable<bool> FleteAutomatico { get; set; }
    }
}
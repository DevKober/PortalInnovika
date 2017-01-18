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
    
    public partial class ProyArticulo
    {
        public ProyArticulo()
        {
            this.ArticulosFavs = new HashSet<ArticulosFav>();
        }
    
        public long IdArticulo { get; set; }
        public int Proyecto { get; set; }
        public int Cantidad { get; set; }
        public string Unidad { get; set; }
        public string ADNTipo { get; set; }
        public string ADNBase { get; set; }
        public string ADNColor { get; set; }
        public string ADNCubrecanto { get; set; }
        public string ADNVeta { get; set; }
        public string ADNVariante { get; set; }
        public string CodigoADNBase { get; set; }
        public string CodigoADNInterno { get; set; }
        public bool tieneJaladera { get; set; }
        public bool tieneVidrio { get; set; }
        public bool tieneProtecta { get; set; }
        public bool tieneOrificios { get; set; }
        public string UnidadJaladera { get; set; }
        public string ADNJaladeraBase { get; set; }
        public string ADNJaladera { get; set; }
        public string ADNJaladeraOpcion { get; set; }
        public string ADNPosicionJaladera { get; set; }
        public string ADNVidrioBase { get; set; }
        public string ADNVidrioColor { get; set; }
        public string CodigoADNVidrio { get; set; }
        public string ADNProtectaBase { get; set; }
        public string ADNProtectaColor { get; set; }
        public string CodigoADNProtecta { get; set; }
        public string ADNOrificios { get; set; }
        public Nullable<short> OrificiosDistCanto { get; set; }
        public Nullable<byte> OrificiosCuantos { get; set; }
        public string CodigoADNOrificios { get; set; }
        public Nullable<int> Alto { get; set; }
        public Nullable<int> Ancho { get; set; }
        public string AltoEstandar { get; set; }
        public Nullable<decimal> PrecioUnitario { get; set; }
        public Nullable<decimal> DescuentoUnitario { get; set; }
        public Nullable<decimal> IVAUnitario { get; set; }
        public Nullable<decimal> Importe { get; set; }
        public Nullable<decimal> PrecioPrincipal { get; set; }
        public Nullable<decimal> PrecioJaladera { get; set; }
        public Nullable<decimal> PrecioVidrio { get; set; }
        public Nullable<decimal> PrecioProtecta { get; set; }
        public Nullable<decimal> PrecioServicios { get; set; }
        public Nullable<decimal> DescuentoPrincipal { get; set; }
        public Nullable<decimal> DescuentoJaladera { get; set; }
        public Nullable<decimal> DescuentoProtecta { get; set; }
        public Nullable<decimal> DescuentoVidrio { get; set; }
        public Nullable<decimal> DescuentoServicios { get; set; }
        public Nullable<decimal> PrecioListaPrincipal { get; set; }
        public Nullable<decimal> PrecioListaJaladera { get; set; }
        public Nullable<decimal> PrecioListaVidrio { get; set; }
        public Nullable<decimal> PrecioListaPelicula { get; set; }
        public Nullable<decimal> PrecioListaServicios { get; set; }
        public Nullable<long> ArticuloPatron { get; set; }
        public string InstruccionesExtra { get; set; }
        public Nullable<decimal> DescuentoLineal { get; set; }
        public Nullable<long> MAIdEmpaque { get; set; }
        public Nullable<long> MAIdArmado { get; set; }
        public System.DateTime TmUltimoCambio { get; set; }
        public Nullable<decimal> PrecioAdjuntos { get; set; }
        public bool EsConceptoDeProyecto { get; set; }
        public string TipoEmpaque { get; set; }
        public bool tieneColorExclusivo { get; set; }
        public string ADNColorExclusivo { get; set; }
        public Nullable<decimal> PrecioColorExclusivo { get; set; }
        public Nullable<decimal> PrecioListaColorExclusivo { get; set; }
        public string CodigoADNJaladera { get; set; }
    
        public virtual ICollection<ArticulosFav> ArticulosFavs { get; set; }
        public virtual Proyecto Proyecto1 { get; set; }
    }
}
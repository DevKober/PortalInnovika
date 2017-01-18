using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PortalInnovika.Models;

namespace PortalInnovika.Models
{
    public class ArticulosViewModel
    {
        private InnovikaComEntities db = new InnovikaComEntities();

        public int Proyecto { get; set; }
        public long IdArticulo { get; set; }
        public string Unidad { get; set; }
        public string ADNCubrecanto { get; set; }
        public string ADNVeta { get; set; }
        
        public Boolean tieneVidrio { get; set; }        

        public Boolean tieneJaladera { get; set; }
        public string ADNJaladeraBase { get; set; }
        public string ADNJaladera { get; set; }
        public string ADNJaladeraOpcion { get; set; }
        public string ADNPosicionJaladera { get; set; }
        public string UnidadJaladera { get; set; }

        public Boolean tieneProtecta { get; set; }
        public string ADNProtectaBase { get; set; }
        public string ADNProtectaColor { get; set; }

        public Boolean tieneOrificios { get; set; }
        public string ADNOrificios { get; set; }
        public int OrificiosDistCanto { get; set; }
        public int OrificiosCuantos { get; set; }
        public string CodigoADNOrificios { get; set; }

        public DateTime TmUltimoCambio { get; set; }
        public Boolean EsConceptoDeProyecto { get; set; }
        public Boolean tieneColorExclusivo { get; set; }

        public string CodigoADNInterno { get; set; }

        public string Descripcion { get; set; }
        public string DescripcionJaladera { get; set; }
        public string DescripcionJaladeraPos { get; set; }
        //private string _descripcion { get; set; }
        //public string Descripcion
        //{
        //    get
        //    {
        //        return this._descripcion;
        //    }
        //    set
        //    {
        //        _descripcion = value;

        //        //string c = (from i in db.ArtCubrecantos
        //        //            where i.Codigo == this.ADNCubrecanto
        //        //            select i.Nombre).FirstOrDefault() ?? "";

        //        string v = (from i in db.ArtVariantes
        //                    where i.Codigo == this.ADNVariante
        //                    select i.Nombre).FirstOrDefault() ?? "";
        //        string j = (from i in db.ArtADNCodigos
        //                    where i.Codigo == "JA" + this.ADNJaladeraBase + this.ADNJaladera + this.ADNJaladeraOpcion
        //                    select i.Descripcion1).FirstOrDefault() ?? "";

        //        this.DescripcionCompleta = value + " " + v + " " + j;
        //    }
        //}

        public string DescripcionCompleta { get; set; }
        public int Alto { get; set; }
        public int Ancho { get; set; }
        public int Cantidad { get; set; }
        public Decimal Importe { get; set; }
        public string ADNTipo { get; set; }
        public string ADNBase { get; set; }
        public string ADNColor { get; set; }
        public string CodigoJaladera { get; set; }
        public string ADNVariante { get; set; }
        public string CodigoADN { get; set; }

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
    }
}
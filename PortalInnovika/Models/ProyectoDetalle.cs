using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PortalInnovika.Models
{
    public class ProyectoDetalle
    {
        //public IEnumerable<ProyectosViewModel> Proyecto { get; set; }
        //public IEnumerable<ArticulosViewModel> Articulos { get; set; }
        public Proyecto Proyecto { get; set; }
        public IEnumerable<ProyArticulo> Articulos { get; set; }
        public IEnumerable<ArticulosViewModel> ArticulosVM { get; set; }
        
        public int IdProyecto { get; set; }
        public string Cliente { get; set; }
    }
}
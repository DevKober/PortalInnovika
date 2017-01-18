using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class ExportProyecto
    {
        public string Articulo { get; set; }
        public int Cantidad { get; set; }
        public int Ancho { get; set; }
        public int Alto { get; set; }
        public decimal Importe { get; set; }
    }
}
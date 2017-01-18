using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class ElementoViewModel
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string RutaImagen { get; set; }
        public string tipo { get; set; }
        public string Sublinea { get; set; }
        public string Veta { get; set; }
        public int Orden { get; set; }
        public string CodigoArmado { get; set; }
    }
}
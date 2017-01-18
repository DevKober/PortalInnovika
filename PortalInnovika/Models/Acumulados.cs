using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class Acumulados
    {        
        public string Descripcion { get; set; }
        public decimal Importe { get; set; }
        public decimal Total { get; set; }
        public int Piezas { get; set; }
        public string ADNColor { get; set; }
    }
}
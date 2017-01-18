using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class TotalesViewModel
    {
        public decimal Importe { get; set; }
        public decimal IVA { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
    }
}
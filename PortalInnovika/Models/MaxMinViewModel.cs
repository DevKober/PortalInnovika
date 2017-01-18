using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class MaxMinViewModel
    {
        public decimal AltoMin { get; set; }
        public decimal AltoMax { get; set; }
        public decimal AnchoMin { get; set; }
        public decimal AnchoMax { get; set; }
        public decimal Multiplo70 { get; set; }
        public decimal Multiplo72 { get; set; }
        public decimal Multiplo76 { get; set; }
        public decimal Multiplo80 { get; set; }
        public string PosicionJaladeras { get; set; }
    }
}
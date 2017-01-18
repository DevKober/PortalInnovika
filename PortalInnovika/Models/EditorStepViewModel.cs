using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class EditorStepViewModel
    {
        public IEnumerable<ElementoViewModel> Elementos { get; set; }
        public string Adn { get; set; }
        public string NombrePaso { get; set; }
        public Boolean UltimoPaso { get; set; }
        public Boolean JaladerasDisponibles { get; set; }
        public IEnumerable<TipoJaladeraViewModel> TiposJaladera { get; set; }        
        public IEnumerable<PosicionJaladeraViewModel> PosicionesJaladera { get; set; }
        public string JaladeraSel { get; set; }
        public decimal AltoMin { get; set; }
        public decimal AltoMax { get; set; }
        public decimal AnchoMin { get; set; }
        public decimal AnchoMax { get; set; }
        public string Posicion { get; set; }
        public string Sublinea { get; set; }
    }
}
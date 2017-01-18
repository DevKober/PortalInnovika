using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class TipoJaladeraViewModel
    {
        private string _base { get; set; }
        public string Base {
            get { 
                return this._base; 
            }

            set {
                _base = value;

                if (value == "JI")
                    this.Nombre = "Insertada";
                else
                    this.Nombre = "Embutida";
            } 
        }
        
        public string Nombre { get; set; }
        public string RutaImagen { get; set; }
    }
}
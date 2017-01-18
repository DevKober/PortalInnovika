using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalInnovika.Models
{
    public class ProyectosViewModel
    {
        public int Proyecto { get; set; }
        public string Identificador { get; set; }
        public string Estatus { get; set; }
        public string ClienteERP { get; set; }
        public string TmRegistrado { get; set; }
        public Nullable<DateTime> TmAprobado { get; set; }
        public Nullable<DateTime> TmEntregaTentativa { get; set; }
        public Nullable<DateTime> TmValidado { get; set; }
        public string EntregaTentativa { get; set; }
        public string LoteProdInterno { get; set; }
        public Boolean TieneMuebles { get; set; }

    }
}
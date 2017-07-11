using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PortalInnovika.Models;
using System.Data;

//ESTAS LIBRERIAS SE USAN PARA LA EXPORTACION A EXCEL O PDF
using System.IO;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace PortalInnovika.Controllers
{
    public struct ResponseObj
    {
        public bool Ok;
        public string Error;
        public int Data;
    }

    public class EditorController : Controller
    {
        // GET: /Editor/
        private InnovikaComEntities db = new InnovikaComEntities();
        private IntelisisDataContext db_intelisis = new IntelisisDataContext();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CheckExpress(int proyecto, bool express)
        {
            var proy = db.Proyectos.FirstOrDefault(p => p.IdProyecto == proyecto); //(from p in db.Proyectos where p.IdProyecto == proyecto select p).Count();
            if (proy == null)
            {
                return Json(new ResponseObj {Ok = false}, JsonRequestBehavior.AllowGet);
            }

            var tipo = new[] {"CE", "CA", "HE", "CM", "BO", "CL", "MU", "PM", "CO", "PB", "MM", "GI"};
            var area = db.ProyArticulos.Where(a => a.Proyecto == proyecto).Where(a => !tipo.Contains(a.ADNTipo))
                .GroupBy(a => a.Proyecto)
                .Select(b => new {total = b.Sum(a => (a.Alto * a.Ancho * a.Cantidad))}).FirstOrDefault();

            var items = db.ProyArticulos.Where(item => item.Proyecto == proyecto && !tipo.Contains(item.ADNTipo))
                .Select(item => item.CodigoADNInterno.Substring(0, 6)).ToArray();

            var lignova = db_intelisis.Arts.Count(art => art.Rama == "MADERA" && items.Contains(art.Articulo.Substring(0, 6)));

            proy.EsExpress = (express && (lignova == 0)) /*&& area?.total <= 1300000*/;

            db.Entry(proy).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new ResponseObj {Ok = proy.EsExpress}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DespliegaAvisoAdventa(int proyecto)
        {
            string c = "";
            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == proyecto
                          select i).FirstOrDefault();
            c = p != null && p.Vendedor > 0 ? "no despliega" : "despliega";

            return Json(c, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTipos()
        {            
            var tipos = (from t in db.ArtTipos
                         where t.Estatus == "A"
                         orderby t.Orden
                         select new ElementoViewModel
                         {
                             Codigo = t.Codigo,
                             Nombre = t.Nombre,
                             RutaImagen = t.RutaImagen,
                             Orden = t.Orden ?? 0
                         }).OrderBy(i => i.Orden);
            EditorStepViewModel datos = new EditorStepViewModel();
            datos.Elementos = tipos;
            datos.Adn = "";
            datos.NombrePaso = "tipo";
            datos.UltimoPaso = false;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBases(string codigo)
        {
            string codigoResultante = "";
            string lineas = "";
            var usuario = UsuarioActual();
            
            //SE TOMAN EN CUENTA LAS LINEAS PERMITIDAS
            int usrOld = UsuarioActual().UserOld ?? 0; //(from u in db.Users where u.ClienteERP == usuario.ClienteERP select u.UserId).FirstOrDefault();
            foreach (LineasUsuario l in db.LineasUsuarios.Where(x => x.Usuario == usrOld))
            {
                lineas += l.Linea + "|";
            }
            lineas += "N/A";
            //
            
            //REVISA SI HAY LINEAS CON PERMISO TEMPORAL            
            foreach (Oportunidade o in db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.LineaAbierta != null)))
            {
                lineas += o.LineaAbierta;
            }
            //

            //REVISA SI HAY BASES CON PERMISO TEMPORAL
            string it = "";            
            var itemTemp = db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.BaseAbierta != null)).Distinct();            
            foreach (Oportunidade o in itemTemp)
            {
                it += o.BaseAbierta + "|";
            }

            var artics = (from i in db.ArtADNCodigos
                          where (i.Estatus == "A" && i.Codigo.StartsWith(codigo)) || (i.Estatus == "A" && i.Codigo.StartsWith(codigo) && it.Contains(i.Base))
                          select i).Distinct();
            var codes = (from i in artics
                         select i.Base).Distinct();

            //si solo hay un item en codigos
            if ((codes.Count() == 1) && (codes.First() == "XX"))
            {
                codigoResultante = codigo + "XX";
                return GetColores(codigoResultante);
            }
            else
            {               
                var bases = (from b in db.ArtBases
                             where b.Estatus == "A" && ((codes.Contains(b.Codigo) && lineas.Contains(b.Linea)) || ((codes.Contains(b.Codigo) && it.Contains(b.Codigo))))
                             select new ElementoViewModel
                             {
                                 Codigo = b.Codigo,
                                 Nombre = b.Nombre,
                                 RutaImagen = b.RutaImagen,
                                 CodigoArmado = codigo.Substring(0,2) + b.Codigo
                             }).OrderBy(i => i.Nombre);
                if (it == "")
                {
                    bases = (from b in db.ArtBases
                             where b.Estatus == "A" && (codes.Contains(b.Codigo) && lineas.Contains(b.Linea))
                             select new ElementoViewModel
                             {
                                 Codigo = b.Codigo,
                                 Nombre = b.Nombre,
                                 RutaImagen = b.RutaImagen,
                                 CodigoArmado = codigo.Substring(0, 2) + b.Codigo
                             }).OrderBy(i => i.Nombre);
                }
                EditorStepViewModel datos = new EditorStepViewModel();
                datos.Elementos = bases;
                datos.Adn = codigo;
                datos.NombrePaso = "base";
                datos.UltimoPaso = false;
                return Json(datos, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetColores(string codigo)
        {
            string codigoResultante = "";

            string lineas = "";
            var usuario = UsuarioActual();

            //SE TOMAN EN CUENTA LAS LINEAS PERMITIDAS
            int usrOld = UsuarioActual().UserOld ?? 0; //(from u in db.Users where u.ClienteERP == usuario.ClienteERP select u.UserId).FirstOrDefault();
            foreach (LineasUsuario l in db.LineasUsuarios.Where(x => x.Usuario == usrOld))
            {
                lineas += l.Linea + "|";
            }
            lineas += "N/A";
            //
            
            //REVISA SI HAY LINEAS CON PERMISO TEMPORAL
            foreach (Oportunidade o in db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.LineaAbierta != null)))
            {
                lineas += o.LineaAbierta;
            }
            //

            //REVISA SI HAY COLORES CON PERMISO TEMPORAL
            string it = "";
            var itemTemp = db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.ColorAbierto != null)).Distinct();
            foreach (Oportunidade o in itemTemp)
            {
                it += o.ColorAbierto + "|";
            }
            //

            var artics = (from i in db.ArtADNCodigos
                          where (i.Estatus == "A" && i.Codigo.StartsWith(codigo)) || (i.Estatus == "A" && it.Contains(i.Color) && (i.Codigo.StartsWith(codigo)))
                          select i).Distinct();
            var codes = (from i in artics
                         select i.Color).Distinct();

            //si solo hay un item en codigos
            if ((codes.Count() == 1) && (codes.First() == "XX"))
            {
                codigoResultante = codigo + "XX";
                return GetVetas(codigoResultante);
            }
            else
            {
                var colores = (from i in db.ArtColores
                               where i.Estatus == "A" && ((codes.Contains(i.Codigo) && lineas.Contains(i.Linea)) || (codes.Contains(i.Codigo) && it.Contains(i.Codigo)))
                               select new ElementoViewModel
                               {
                                   Codigo = i.Codigo,
                                   Nombre = i.Nombre,
                                   RutaImagen = i.RutaImagen
                               }).OrderBy(i => i.Nombre);
                
                EditorStepViewModel datos = new EditorStepViewModel();
                datos.Elementos = colores;                
                datos.Adn = codigo;
                datos.NombrePaso = "color";
                datos.UltimoPaso = false;
                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            
        }        

        public JsonResult GetVetas(string codigo)
        {
            string codigoResultante = "";

            var artics = (from i in db.ArtADNCodigos
                          where i.Estatus == "A" && i.Codigo.StartsWith(codigo)
                          select i).Distinct();
            var codes = (from i in artics
                         select i.Veta).Distinct();

            //SI ES VIDRIO SUELTO BRINCA AL SIGUIENTE PASO (CUBRECANTOS)
            if (codigo.Substring(0, 2) == "VD")
            {
                return GetFinal(codigo);
                //return GetCubrecantos(codigoResultante, "", "");                
            }

            //si solo hay un item en codigos
            if ((codes.Count() == 1) && (codes.First() == "X"))
            {
                codigoResultante = codigo + "X";
                return GetCubrecantos(codigoResultante, "", "");
            }
            else
            {
                string sublinea = (from i in db.ArtColores
                                where i.Codigo == (codigo.Substring(4,2))
                                select i.SubLinea).FirstOrDefault().ToString();
                var vetas = (from i in db.ArtVetas
                             where i.Estatus == "A" && codes.Contains(i.Codigo)
                             select new ElementoViewModel
                             {
                                 Codigo = i.Codigo,
                                 Nombre = i.Nombre,
                                 RutaImagen = i.RutaImagen
                             }).OrderBy(i => i.Nombre);
                EditorStepViewModel datos = new EditorStepViewModel();
                datos.Elementos = vetas;
                datos.Adn = codigo;
                datos.NombrePaso = "veta";
                datos.UltimoPaso = false;
                datos.Sublinea = sublinea;
                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            
        }

        public JsonResult GetCubrecantos(string codigo, string sublinea, string posicion)
        {
            string codigoResultante = "";

            string lineas = "";
            var usuario = UsuarioActual();

            //SE TOMAN EN CUENTA LAS LINEAS PERMITIDAS
            int usrOld = UsuarioActual().UserOld ?? 0; //(from u in db.Users where u.ClienteERP == usuario.ClienteERP select u.UserId).FirstOrDefault();
            foreach (LineasUsuario l in db.LineasUsuarios.Where(x => x.Usuario == usrOld))
            {
                lineas += l.Linea + "|";
            }
            lineas += "N/A";
            //
            
            //REVISA SI HAY LINEAS CON PERMISO TEMPORAL
            foreach (Oportunidade o in db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.LineaAbierta != null)))
            {
                lineas += o.LineaAbierta;
            }
            //

            //REVISA SI HAY CUBRECANTOS CON PERMISO TEMPORAL
            string it = "";
            var itemTemp = db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.CubrecantoAbierto != null)).Distinct();
            foreach (Oportunidade o in itemTemp)
            {
                it += o.CubrecantoAbierto + "|";
            }  
            //

            var artics = (from i in db.ArtADNCodigos
                          where i.Estatus == "A" && i.Codigo.StartsWith(codigo)
                          select i).Distinct();
            var codes = (from i in artics
                         select i.Cubrecanto).Distinct();

            //si solo hay un item en codigos
            if ((codes.Count() == 1) && (codes.First() =="XX"))
            {
                codigoResultante = codigo + "XXX";
                return GetVariantes(codigoResultante, "", "");
            }
            else
            {
                var cubrecantos = (from i in db.ArtCubrecantos
                                   where i.Estatus == "A" && ((codes.Contains(i.Codigo) && lineas.Contains(i.Linea)) || (it.Contains(i.Codigo)))
                                   select new ElementoViewModel
                                   {
                                       Codigo = i.Codigo,
                                       Nombre = i.Nombre,
                                       RutaImagen = i.RutaImagen
                                   }).OrderBy(i => i.Nombre);

                if ((sublinea == "") || (sublinea == "null") || (sublinea == "undefined"))
                {
                    sublinea = null;
                }
                if ((posicion == "") || (posicion == "undefined"))
                {
                    posicion = null;
                }

                MaxMinViewModel maxmin = null;
                if ((sublinea == null) && (posicion == null))
                {
                    maxmin = (from i in db.MedidaReglas
                              where ((i.Tipo + i.Base) == codigo.Substring(0, 4))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else if ((sublinea == null) && (posicion != null))
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else if ((sublinea != null) && (posicion == null))
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                EditorStepViewModel datos = new EditorStepViewModel();
                datos.Elementos = cubrecantos;

                datos.AltoMin = ((MaxMinViewModel)maxmin).AltoMin;
                datos.AltoMax = ((MaxMinViewModel)maxmin).AltoMax;
                datos.AnchoMax = ((MaxMinViewModel)maxmin).AnchoMax;
                datos.AnchoMin = ((MaxMinViewModel)maxmin).AnchoMin;
                if (posicion == null)
                {
                    datos.Posicion = "";
                }
                else
                {

                    datos.Posicion = posicion;
                }
                datos.Sublinea = sublinea;

                datos.Adn = codigo;
                datos.NombrePaso = "cubrecanto";
                datos.UltimoPaso = false;
                return Json(datos, JsonRequestBehavior.AllowGet);
            }            
        }

        public JsonResult GetVariantes(string codigo, string sublinea, string posicion)
        {
            string codigoResultante = "";

            var artics = (from i in db.ArtADNCodigos
                          where i.Estatus == "A" && i.Codigo.StartsWith(codigo)
                          select i).Distinct();
            var codes = (from i in artics
                         select i.Variante).Distinct();

            //si solo hay un item en codigos
            if ((codes.Count() == 1) && (codes.First() == "XX"))
            {
                codigoResultante = codigo + "XX";
                return GetFinal(codigoResultante);
            }
            else
            {

                var variantes = (from i in db.ArtVariantes
                                 where i.Estatus == "A" && codes.Contains(i.Codigo)
                                 select new ElementoViewModel
                                 {
                                     Codigo = i.Codigo,
                                     Nombre = i.Nombre,
                                     RutaImagen = i.RutaImagen
                                 }).OrderBy(i => i.Nombre);

                if ((sublinea == "") || (sublinea == "null"))
                {
                    sublinea = null;
                }
                if (posicion == "")
                {
                    posicion = null;
                }

                MaxMinViewModel maxmin = null;
                if (((sublinea == null) || (sublinea == "undefined")) && ((posicion == null) || (posicion == "undefined")))
                {
                    maxmin = (from i in db.MedidaReglas
                              where ((i.Tipo + i.Base) == codigo.Substring(0, 4))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else if ((sublinea == null) && (posicion != null))
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else if ((sublinea != null) && (posicion == null))
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }
                else
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0
                              }).FirstOrDefault();
                }

                EditorStepViewModel datos = new EditorStepViewModel();
                datos.Elementos = variantes;

                if ((codigo.Substring(0, 2) != "BI") && (codigo.Substring(0, 2) != "PM") && (codigo.Substring(0, 2) != "CM") && (codigo.Substring(0, 2) != "MU"))
                {
                    datos.AltoMin = ((MaxMinViewModel)maxmin).AltoMin;
                    datos.AltoMax = ((MaxMinViewModel)maxmin).AltoMax;
                    datos.AnchoMax = ((MaxMinViewModel)maxmin).AnchoMax;
                    datos.AnchoMin = ((MaxMinViewModel)maxmin).AnchoMin;
                }
                if (posicion == null)
                {
                    datos.Posicion = "";
                }
                else
                {
                    datos.Posicion = posicion;
                }
                datos.Sublinea = sublinea;
                if (codigo.Length < 10)
                {
                    datos.Adn = codigo + "X";
                }
                else
                {
                    datos.Adn = codigo;
                }
                datos.NombrePaso = "variante";
                datos.UltimoPaso = false;
                return Json(datos, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMultiplos(string codigo, string estandar)
        {
            var mults = (from i in db.Multiplos
                         where i.Tipo == codigo.Substring(0, 2) && i.Base == codigo.Substring(2, 2) && i.Estandar == estandar
                         select i.Medida).Distinct();
            return Json(mults, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxMin(string codigo, string posicion)
        {
            //var sublinea = (from i in db.ArtColores
            //                where i.Codigo == codigo.Substring(4, 2)
            //                select i.SubLinea).FirstOrDefault();

            string sublinea = null;

            if (posicion == "")
            {
                posicion = null;
            }

            MaxMinViewModel maxmin = null;
            if ((sublinea == null) && ((posicion == null)) || (posicion == "X"))
            {
                //string color = codigo.Substring(4, 2);
                //if (color != null)
                //{

                //}

                maxmin = (from i in db.MedidaReglas
                          where ((i.Tipo + i.Base) == codigo.Substring(0, 4))
                          select new MaxMinViewModel
                          {                              
                              AltoMin = i.AltoMin ?? 0,
                              AltoMax = i.AltoMax ?? 0,
                              AnchoMin = i.AnchoMin ?? 0,
                              AnchoMax = i.AnchoMax ?? 0,
                              Multiplo70 = i.Multiplo70 ?? 0,
                              Multiplo72 = i.Multiplo72 ?? 0,
                              Multiplo76 = i.Multiplo76 ?? 0,
                              Multiplo80 = i.Multiplo80 ?? 0,
                              PosicionJaladeras = i.PosicionJaladeras ?? ""
                          }).FirstOrDefault();
            }
            else if ((sublinea == null) && (posicion != null))
            {
                if (posicion == "H") //SE INVIERTEN LOS MAX/MIN EN CASO DE QUE LA VETA SEA HORIZONTAL
                {
                    //maxmin = (from i in db.MedidaReglas
                    //          where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.PosicionJaladeras.Contains(posicion)))
                    //          select new MaxMinViewModel
                    //          {
                    //              AltoMin = i.AnchoMin ?? 0,
                    //              AltoMax = i.AnchoMax ?? 0,
                    //              AnchoMin = i.AltoMin ?? 0,
                    //              AnchoMax = i.AltoMax ?? 0,
                    //              Multiplo70 = i.Multiplo70 ?? 0,
                    //              Multiplo72 = i.Multiplo72 ?? 0,
                    //              Multiplo76 = i.Multiplo76 ?? 0,
                    //              Multiplo80 = i.Multiplo80 ?? 0,
                    //              PosicionJaladeras = i.PosicionJaladeras ?? ""
                    //          }).FirstOrDefault();
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0,
                                  Multiplo70 = i.Multiplo70 ?? 0,
                                  Multiplo72 = i.Multiplo72 ?? 0,
                                  Multiplo76 = i.Multiplo76 ?? 0,
                                  Multiplo80 = i.Multiplo80 ?? 0,
                                  PosicionJaladeras = i.PosicionJaladeras ?? ""
                              }).FirstOrDefault();
                }
                else
                {
                    maxmin = (from i in db.MedidaReglas
                              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.PosicionJaladeras.Contains(posicion)))
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin ?? 0,
                                  AltoMax = i.AltoMax ?? 0,
                                  AnchoMin = i.AnchoMin ?? 0,
                                  AnchoMax = i.AnchoMax ?? 0,
                                  Multiplo70 = i.Multiplo70 ?? 0,
                                  Multiplo72 = i.Multiplo72 ?? 0,
                                  Multiplo76 = i.Multiplo76 ?? 0,
                                  Multiplo80 = i.Multiplo80 ?? 0,
                                  PosicionJaladeras = i.PosicionJaladeras ?? ""
                              }).FirstOrDefault();
                }
            }

            //LOGICA PARA MAXIMOS Y MINIMO POR ACABADO O POR BASE-ACABADO
            if (codigo.Length > 4)
            {
                string vAcabados = "";
                foreach (MaxMinAcabado i in db.MaxMinAcabados)
                {
                    vAcabados = vAcabados + i.Color + "|";
                }

                string BaseColor = "";
                foreach (MaxMinBaseColor i in db.MaxMinBaseColors)
                {
                    BaseColor = BaseColor + i.Base + i.Color + "|";
                }

                if (vAcabados.Contains(codigo.Substring(4, 2)))
                {
                    maxmin = (from i in db.MaxMinAcabados
                              where i.Color == codigo.Substring(4, 2)
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin,
                                  AltoMax = i.AltoMax,
                                  AnchoMin = i.AnchoMin,
                                  AnchoMax = i.AnchoMax,
                                  Multiplo70 = 0,
                                  Multiplo72 = 0,
                                  Multiplo76 = 0,
                                  Multiplo80 = 0,
                                  PosicionJaladeras = ""
                              }).FirstOrDefault();
                }

                if (BaseColor.Contains(codigo.Substring(2, 4)))
                {
                    maxmin = (from i in db.MaxMinBaseColors
                              where i.Base + i.Color == codigo.Substring(2, 4)
                              select new MaxMinViewModel
                              {
                                  AltoMin = i.AltoMin,
                                  AltoMax = i.AltoMax,
                                  AnchoMin = i.AnchoMin,
                                  AnchoMax = i.AnchoMax,
                                  Multiplo70 = 0,
                                  Multiplo72 = 0,
                                  Multiplo76 = 0,
                                  Multiplo80 = 0,
                                  PosicionJaladeras = ""
                              }).FirstOrDefault();
                }
            }
            
            //else if ((sublinea != null) && (posicion == null))
            //{
            //    maxmin = (from i in db.MedidaReglas
            //              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea))
            //              select new MaxMinViewModel
            //              {
            //                  AltoMin = i.AltoMin ?? 0,
            //                  AltoMax = i.AltoMax ?? 0,
            //                  AnchoMin = i.AnchoMin ?? 0,
            //                  AnchoMax = i.AnchoMax ?? 0
            //              }).FirstOrDefault();
            //}
            //else
            //{
            //    maxmin = (from i in db.MedidaReglas
            //              where (((i.Tipo + i.Base) == codigo.Substring(0, 4)) && (i.SubLinea == sublinea) && (i.PosicionJaladeras.Contains(posicion)))
            //              select new MaxMinViewModel
            //              {
            //                  AltoMin = i.AltoMin ?? 0,
            //                  AltoMax = i.AltoMax ?? 0,
            //                  AnchoMin = i.AnchoMin ?? 0,
            //                  AnchoMax = i.AnchoMax ?? 0
            //              }).FirstOrDefault();
            //}

            return Json(maxmin, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJaladeras(string tipo, string codigo, string color)
        {
            //string codigoResultante = "";
            string lineas = "";
            var usuario = UsuarioActual();

            //SE TOMAN EN CUENTA LAS LINEAS PERMITIDAS
            int usrOld = UsuarioActual().UserOld ?? 0; //(from u in db.Users where u.ClienteERP == usuario.ClienteERP select u.UserId).FirstOrDefault();
            foreach (LineasUsuario l in db.LineasUsuarios.Where(x => x.Usuario == usrOld))
            {
                lineas += l.Linea + "|";
            }
            lineas += "N/A";
            //
            
            //REVISA SI HAY LINEAS CON PERMISO TEMPORAL            
            foreach (Oportunidade o in db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.LineaAbierta != null)))
            {
                lineas += o.LineaAbierta;
            }
            //

            //REVISA SI HAY JALADERAS CON PERMISO TEMPORAL
            string it = "";
            var itemTemp = db.Oportunidades.Where(x => (x.User.UserId == usrOld) && (x.Caducidad > DateTime.Today) && (x.JaladeraAbierta != null)).Distinct();
            foreach (Oportunidade o in itemTemp)
            {
                it += o.JaladeraAbierta + "|";
            }
            //

            var exepCodes = (from i in db.JaladeraColorExcepciones
                             where i.Color == color
                             select i.Jaladera).Distinct();

            var codes = (from i in db.ArtJaladeras
                         join basesjaladeras in db.BasesJaladeras on i.Codigo equals basesjaladeras.Jaladera                                                   
                         where i.Estatus == "A" && basesjaladeras.Base == codigo                               
                         select i.Codigo).Distinct().ToList();                      

            if (color != "")
            {
                codes = (from i in db.ArtJaladeras
                         join basesjaladeras in db.BasesJaladeras on i.Codigo equals basesjaladeras.Jaladera
                         where i.Estatus == "A" && basesjaladeras.Base == codigo
                         select i.Codigo).Except(exepCodes).Distinct().ToList();

                var extras = (from i in db.JaladerasExtras
                              where i.Tipo == tipo && i.Base == codigo && i.Color == color
                              select i.Jaladera).Distinct().ToList();

                foreach (string i in extras) { codes.Add(i); };
            }

            var jaladeras = (from i in db.ArtJaladeras
                             where (((i.Estatus == "A" && codes.Contains(i.Codigo)) || (i.Codigo == "XX") || (it.Contains(i.Codigo))) && lineas.Contains(i.Linea))
                             select new ElementoViewModel
                             {
                                 Codigo = i.Codigo,
                                 Nombre = i.Nombre,
                                 RutaImagen = i.RutaImagen,
                                 Orden = i.Orden ?? 0
                             }).OrderBy(i => i.Orden);
            EditorStepViewModel datos = new EditorStepViewModel();
            datos.Elementos = jaladeras;
            datos.Adn = codigo;
            datos.NombrePaso = "jaladera";
            datos.UltimoPaso = false;
            datos.JaladerasDisponibles = jaladeras.Count() > 0;            
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnidadJaladera(string codigo)
        {
            var unidad = "";
            if (codigo != "")
            {
                unidad = (from i in db_intelisis.Arts
                          where i.Articulo == codigo
                          select i.Unidad).FirstOrDefault();              
            }
            
            return Json(unidad, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetJaladeraTipo(string codigo)
        {            
            var tiposJal = (from i in db.ArtADNCodigos
                            where ((i.Tipo == "JA") && (i.Codigo.Substring(4,2) == codigo))
                            select new TipoJaladeraViewModel 
                            {
                                Base = i.Base
                            }).Distinct();

            EditorStepViewModel datos = new EditorStepViewModel();
            datos.TiposJaladera = tiposJal;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetJaladeraPosicion(string codigo, string variante)
        {            

            var codes = (from i in db.ArtJaladeras
                         join basesjaladeras in db.BasesJaladeras on i.Codigo equals basesjaladeras.Jaladera
                         where i.Estatus == "A" && basesjaladeras.Base == codigo
                         select i.Codigo).Distinct();

            IEnumerable<PosicionJaladeraViewModel> posJal = null;

            if ((codigo == "E") || (codigo == "P"))
            {
                posJal = (from i in db.ArtJaladeraPos
                          select new PosicionJaladeraViewModel
                          {
                              Codigo = i.Codigo,
                              Nombre = i.Nombre,
                              RutaImagen = i.RutaImagen
                          }).Distinct();
            }
            else if (variante == "A2")
            {
                posJal = (from i in db.ArtJaladeraPos
                          where i.Codigo == "V"
                          select new PosicionJaladeraViewModel
                          {
                              Codigo = i.Codigo,
                              Nombre = i.Nombre,
                              RutaImagen = i.RutaImagen
                          }).Distinct();
            }
            else if (variante == "S2")
            {
                posJal = (from i in db.ArtJaladeraPos
                          where i.Codigo == "H"
                          select new PosicionJaladeraViewModel
                          {
                              Codigo = i.Codigo,
                              Nombre = i.Nombre,
                              RutaImagen = i.RutaImagen
                          }).Distinct();
            }
            else
            {
                posJal = (from i in db.ArtJaladeraPos
                          where i.Estatus != "I"
                          select new PosicionJaladeraViewModel
                          {
                              Codigo = i.Codigo,
                              Nombre = i.Nombre,
                              RutaImagen = i.RutaImagen
                          }).Distinct();
            }

            EditorStepViewModel datos = new EditorStepViewModel();
            datos.PosicionesJaladera = posJal;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReglasMedidaJaladeras(string jaladera)
        {
            ArtJaladera j = (from i in db.ArtJaladeras
                             where i.Codigo == jaladera
                             select i).FirstOrDefault();
            ReglaMedidasJaladera r = new ReglaMedidasJaladera();            
             
            r.PuertaAltoMin = j.PuertaAltoMin ?? 0;
            r.PuertaAnchoMin = j.PuertaAnchoMin ?? 0;
            r.FrenteAnchoMin = j.FrenteAnchoMin ?? 0;
            r.FrenteAltoMin = j.FrenteAltoMin ?? 0;
            if (jaladera == "X")
            {
                r.Estatus = 0;
            }
            else
            {
                r.Estatus = 1;
            }

            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFinal(string codigo)
        {
            EditorStepViewModel datos = new EditorStepViewModel();
            datos.Adn = codigo;
            datos.UltimoPaso = true;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetElementos(string codigo, string tipo, string sublinea, string posicion)
        {
            if (tipo == "tipos")
            {
               var tipos = GetTipos();
               return tipos;
            }
            else if (tipo == "base")
            {
                var bases = GetBases(codigo);
                return bases;
            }
            else if (tipo == "color")
            {
                var colores = GetColores(codigo);
                return colores;
            }
            else if (tipo == "veta")
            {
                var vetas = GetVetas(codigo);
                return vetas;
            }
            else if (tipo == "cubrecanto")
            {
                var cubrecantos = GetCubrecantos(codigo, sublinea, posicion);
                return cubrecantos;
            }
            else if (tipo == "jaladera")
            {
                var jaladeras = GetJaladeras("", codigo, "");
                return jaladeras;
            }
            else if (tipo == "variante")
            {
                var variantes = GetVariantes(codigo, sublinea, posicion);
                return variantes;
            }
            else if (tipo == "final")
            {
                var final = GetFinal(codigo);
                return final;
            }
            else return null;
        }

        public JsonResult GetUltimoArticulo(int proy)
        {
            long a = (from i in db.ProyArticulos
                              where i.Proyecto == proy
                              select i.IdArticulo).Max();
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DuplicarArticulo(int articulo)
        {
            ProyArticulo art = new ProyArticulo();
            art = (from i in db.ProyArticulos
                   where i.IdArticulo == articulo
                   select i).FirstOrDefault();

            int IdProyecto = art.Proyecto;
            art.TmUltimoCambio = DateTime.Today;            

            UserProfile usuario = UsuarioActual();

            //REVISA SI ES EXPRESS
            Proyecto pr = (from i in db.Proyectos
                           where i.IdProyecto == art.Proyecto
                           select i).FirstOrDefault();
            //pr.EsExpress = EsExpress(art.Proyecto);
            pr.Observaciones += "#" + DateTime.Today.ToString() + "Articulo agregado: " + art.CodigoADNInterno;
            db.Entry(pr).State = EntityState.Modified;

            //SEGURO
            var grupo = (from i in db_intelisis.Ctes
                         where i.Cliente == usuario.ClienteERP.Trim()
                         select i.Grupo).FirstOrDefault();

            if ((grupo == "FORANEO") && (usuario.FleteraId != 5))
            {
                ProyArticulo artSeg = (from i in db.ProyArticulos
                                       where i.Proyecto == IdProyecto && i.CodigoADNInterno == "CESEEX"
                                       select i).FirstOrDefault();
                if (artSeg is ProyArticulo)
                {
                    //ya existe un seguro. 
                    decimal subt = GetSubtotal(IdProyecto);
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = decimal.Multiply(subt, 0.01M);
                    db.Entry(artSeg).State = EntityState.Modified;
                }
                else
                {
                    artSeg = new ProyArticulo();
                    artSeg.Proyecto = IdProyecto;
                    artSeg.ADNTipo = "CE";
                    artSeg.ADNBase = "SE";
                    artSeg.ADNColor = "EX";
                    artSeg.CodigoADNInterno = "CESEEX";
                    artSeg.CodigoADNBase = "CESEEX";
                    artSeg.TmUltimoCambio = DateTime.Today;
                    artSeg.Cantidad = 1;
                    artSeg.Unidad = "PZA";
                    artSeg.tieneJaladera = false;
                    artSeg.tieneVidrio = false;
                    artSeg.tieneProtecta = false;
                    artSeg.tieneOrificios = false;
                    artSeg.EsConceptoDeProyecto = true;
                    artSeg.tieneColorExclusivo = false;

                    var tots = GetTotales(IdProyecto);
                    decimal subt = ((TotalesViewModel)tots.Data).Subtotal; //GetSubtotal(IdProyecto);
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = artSeg.PrecioPrincipal;
                    db.ProyArticulos.Add(artSeg);
                }

            }

            //FLETE
            ProyArticulo af = null;
            ProyArticulo artFlete = null;
            if (grupo == "FORANEO")
            {
                af = (from i in db.ProyArticulos
                                   where i.Proyecto == IdProyecto && i.ADNTipo == "CE" && i.ADNBase == "EE" && i.ADNColor == "XX"
                                   select i).FirstOrDefault();
                artFlete = new ProyArticulo();
                if (af != null)
                {
                    artFlete = af;
                }
                artFlete.Proyecto = IdProyecto;
                artFlete.ADNTipo = "CE";
                artFlete.ADNBase = "EE";
                artFlete.ADNColor = "XX";
                artFlete.CodigoADNInterno = "CEEEXX";                                              
                artFlete.TmUltimoCambio = DateTime.Today;
                artFlete.Cantidad = 1;
                artFlete.Unidad = "PZA";
                artFlete.tieneJaladera = false;
                artFlete.tieneVidrio = false;
                artFlete.tieneProtecta = false;
                artFlete.tieneOrificios = false;
                artFlete.EsConceptoDeProyecto = true;
                artFlete.tieneColorExclusivo = false;
                var c = new FleteController().CostoFleteM2(art.Proyecto);

                artFlete.PrecioPrincipal = Convert.ToDecimal(c.Data);
                artFlete.PrecioListaPrincipal = Convert.ToDecimal(c.Data);
            }
            if (af != null) //POR SI YA TRAE UN ARTICULO DE FLETE PREVIO
            {
                af = artFlete;
                if (ModelState.IsValid)
                {
                    db.Entry(af).State = EntityState.Modified;
                }
            }
            else if (grupo == "FORANEO")
            {
                db.ProyArticulos.Add(artFlete);

            }

            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == IdProyecto
                          select i).FirstOrDefault();
            p.Observaciones += "#" + DateTime.Today.ToString() + " Se duplico el articulo: " + art.CodigoADNInterno;
            db.Entry(p).State = EntityState.Modified;

            db.ProyArticulos.Add(art);
            db.SaveChanges();
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public decimal GetPrecioJaladera(string adnJaladera, UserProfile usuario = null)
        {
            if (usuario == null)
            {
                usuario = UsuarioActual();
            }

            var j = (from i in db.ArtADNCodigos
                     where i.Codigo == adnJaladera
                     select i).FirstOrDefault();
            decimal precio = GetPrecioLista(j, usuario);
            //return j.PrecioLista ?? 0;
            return precio;
        }

        //METODO PARA OBTENER EL HISTORIAL DE UN PROYECTO 
        public JsonResult GetHistorial(int proyecto)
        {
            Proyecto p = (from i in db.Proyectos
                        where i.IdProyecto == proyecto
                        select i).FirstOrDefault();
            string o = p.Observaciones;
            List<HistorialItems> hist = new List<HistorialItems>();
            if (o != null)
            {
                string[] items = o.Split('#');
                foreach (string item in items)
                {
                    HistorialItems h = new HistorialItems();
                    h.item = item;
                    hist.Add(h);
                }

                return Json(hist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

            
        }

        //METODO PARA EXPORTAR PROYECTO A EXCEL
        public void ExportaProyectoExcel(int proyecto)
        {
            var grid = new System.Web.UI.WebControls.GridView();

            //List<ExportProyecto> arts = (from i in db.ProyArticulos
            //                   where i.Proyecto == proyecto && i.ADNTipo != "CE"
            //                   select new ExportProyecto
            //                   {
            //                       Articulo = i.CodigoADNInterno,
            //                       Cantidad = i.Cantidad,
            //                       Alto = i.Alto ?? 0,
            //                       Ancho = i.Ancho ?? 0
            //                   }).ToList();
                         var articulos = (from item in db.ProyArticulos
                             join codigo in db.ArtADNCodigos on item.CodigoADNInterno.Trim() equals codigo.Codigo.Trim()
                             where item.Proyecto == proyecto
                             orderby item.CodigoADNInterno
                             select new 
                             {
                                 Cantidad = item.Cantidad,
                                 Ancho = item.Ancho ?? 0,
                                 Alto = item.Alto ?? 0,                                 
                                 Descripcion = codigo.Descripcion1 + " " + item.ADNVariante,
                                 DescripcionJaladera = " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 DescripcionJaladeraPos = (from i in db.ArtJaladeraPos where i.Codigo == item.ADNPosicionJaladera select i.Nombre).FirstOrDefault() ?? "",
                                 //Descripcion = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 tieneJaladera = item.tieneJaladera,
                                 ADNJaladera = item.ADNJaladera,
                                 ADNJaladeraBase = item.ADNJaladeraBase,
                                 ADNJaladeraOpcion = item.ADNJaladeraOpcion,                                
                                 
                                 Importe = item.Importe ?? 0
                             });
            //Totales
                         var totales = GetTotales(proyecto);

            List<ExportProyecto> al = new List<ExportProyecto>();
            foreach (var a in articulos)
            {
                ExportProyecto e = new ExportProyecto();
                e.Cantidad = a.Cantidad;                
                e.Ancho = a.Ancho;
                e.Alto = a.Alto;
                e.Importe = decimal.Round(a.Importe, 2);
                if (a.tieneJaladera)
                {
                    e.Articulo = a.Descripcion + " " + a.DescripcionJaladera + " POS: " + a.DescripcionJaladeraPos.ToUpper();
                }
                else
                {
                    e.Articulo = a.Descripcion;
                }
                al.Add(e);
            }



            grid.DataSource = al;
            if (grid.DataSource != null)
            {
                grid.DataBind();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Proyecto-" + proyecto.ToString() + ".xls");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //Response.ContentType = "application/excel";
                //Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                //XhtmlTextWriter htw = new XhtmlTextWriter(sw);
                grid.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

        public void ExportaProyectoExcel2(int proyecto)
        {
            var grid = new System.Web.UI.WebControls.GridView();
            var grid2 = new System.Web.UI.WebControls.GridView();

            var articulos = (from item in db.ProyArticulos
                             join codigo in db.ArtADNCodigos on item.CodigoADNInterno.Trim() equals codigo.Codigo.Trim()
                             where item.Proyecto == proyecto
                             orderby item.CodigoADNInterno
                             select new
                             {
                                 Cantidad = item.Cantidad,
                                 Ancho = item.Ancho ?? 0,
                                 Alto = item.Alto ?? 0,
                                 Descripcion = codigo.Descripcion1 + " " + item.ADNVariante,
                                 DescripcionJaladera = " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 DescripcionJaladeraPos = (from i in db.ArtJaladeraPos where i.Codigo == item.ADNPosicionJaladera select i.Nombre).FirstOrDefault() ?? "",
                                 //Descripcion = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 tieneJaladera = item.tieneJaladera,
                                 ADNJaladera = item.ADNJaladera,
                                 ADNJaladeraBase = item.ADNJaladeraBase,
                                 ADNJaladeraOpcion = item.ADNJaladeraOpcion,

                                 Importe = item.Importe ?? 0
                             });
            //Totales
            //var t = GetTotales(proyecto);

            List<ExportProyecto> al = new List<ExportProyecto>();
            foreach (var a in articulos)
            {
                ExportProyecto e = new ExportProyecto();
                e.Cantidad = a.Cantidad;
                e.Ancho = a.Ancho;
                e.Alto = a.Alto;
                e.Importe = decimal.Round(a.Importe, 2);
                if (a.tieneJaladera)
                {
                    e.Articulo = a.Descripcion + " " + a.DescripcionJaladera + " POS: " + a.DescripcionJaladeraPos.ToUpper();
                }
                else
                {
                    e.Articulo = a.Descripcion;
                }
                al.Add(e);
            }

            var t = (from i in db.ProyArticulos
                     where i.Proyecto == proyecto
                     select i).Distinct();

            decimal Imp = t.Sum(i => i.PrecioUnitario * i.Cantidad) ?? 0;
            decimal Desc = t.Sum(i => i.DescuentoUnitario * i.Cantidad) ?? 0;
            decimal Sub = Imp - Desc;
            decimal Iva = Sub * (decimal)0.16;
            decimal Tot = Sub + Iva;
            TotalesViewModel total = new TotalesViewModel
            {
                Importe = decimal.Round(Imp, 2),
                Descuento = decimal.Round(Desc, 2),
                Subtotal = decimal.Round(Sub, 2),
                IVA = decimal.Round(Iva, 2),
                Total = decimal.Round(Tot, 2)
            };
            List<TotalesViewModel> totales = new List<TotalesViewModel>();
            totales.Add(total);
            grid2.DataSource = totales;

            grid.DataSource = al;

            System.Web.UI.WebControls.Table tb = new System.Web.UI.WebControls.Table();

            System.Web.UI.WebControls.TableRow tr1 = new System.Web.UI.WebControls.TableRow();
            System.Web.UI.WebControls.TableCell cell1 = new System.Web.UI.WebControls.TableCell();
            cell1.Controls.Add(grid);
            tr1.Cells.Add(cell1);
            System.Web.UI.WebControls.TableCell cell3 = new System.Web.UI.WebControls.TableCell();
            cell3.Controls.Add(grid2);
            System.Web.UI.WebControls.TableCell cell2 = new System.Web.UI.WebControls.TableCell();
            cell2.Text = "&nbsp;";

            tr1.Cells.Add(cell2);
            tr1.Cells.Add(cell3);
            tb.Rows.Add(tr1);

            //if (grid.DataSource != null)
            //{
                grid.DataBind();
                grid2.DataBind();   
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Proyecto-" + proyecto.ToString() + ".xls");
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //Response.ContentType = "application/excel";
                //Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                
                //grid.RenderControl(htw);
                tb.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.Flush();
                Response.End();
        }

        //EXPORTAR PROYECTO A PDF
        public void ExportaProyectoPdf(int proyecto)
        {
            Response.ContentType = "application/pdf";
            Response.AppendHeader(
              "Content-Disposition",
              "attachment; filename=" + proyecto.ToString() + ".pdf"
            );

            BaseFont bfHelvetica = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
            Font helvetica = new Font(bfHelvetica, 12, Font.NORMAL);
            string imagepath = Server.MapPath("..");
            var doc1 = new Document();
            
            //PdfWriter.GetInstance(doc1, new FileStream(path + "/Doc1.pdf", FileMode.Create));
            PdfWriter.GetInstance(doc1, Response.OutputStream);
            doc1.Open();
            var articulos = (from item in db.ProyArticulos
                             join codigo in db.ArtADNCodigos on item.CodigoADNInterno.Trim() equals codigo.Codigo.Trim()
                             where item.Proyecto == proyecto
                             orderby item.CodigoADNBase
                             select new ArticulosViewModel
                             {
                                 CodigoADNInterno = item.CodigoADNInterno,
                                 Descripcion = codigo.Descripcion1,
                                 DescripcionJaladera = " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 DescripcionJaladeraPos = (from i in db.ArtJaladeraPos where i.Codigo == item.ADNPosicionJaladera select i.Nombre).FirstOrDefault() ?? "",
                                 //Descripcion = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                 tieneJaladera = item.tieneJaladera,
                                 ADNJaladera = item.ADNJaladera,
                                 ADNJaladeraBase = item.ADNJaladeraBase,
                                 ADNJaladeraOpcion = item.ADNJaladeraOpcion,

                                 Ancho = item.Ancho ?? 0,
                                 Alto = item.Alto ?? 0,                                 
                                 Cantidad = item.Cantidad,
                                 Importe = item.Importe ?? 0
                             });
            List<ArticulosViewModel> al = new List<ArticulosViewModel>();
            foreach (ArticulosViewModel a in articulos)
            {
                if (a.tieneJaladera)
                {
                    a.Descripcion += a.DescripcionJaladera + " POS: " + a.DescripcionJaladeraPos.ToUpper();
                }
                al.Add(a);
            }
            Image logo = Image.GetInstance(imagepath + "/Images/InnovikaLogo.png");
            doc1.Add(logo);
            doc1.Add(new Paragraph(" "));
            doc1.Add(new Paragraph("Resumen del proyecto: " + proyecto.ToString() + "   Cliente: " + UsuarioActual().ClienteERP, helvetica));
            doc1.Add(new Paragraph(" "));
            doc1.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(5);
            float[] widths = new float[] { 1f, 1f, 1f, 6f, 1f };
            table.SetWidths(widths);
            //iTextSharp.text.List list = new iTextSharp.text.List(iTextSharp.text.List.UNORDERED);
            PdfPCell hCantidad = new PdfPCell(new Phrase("Cantidad", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
            hCantidad.Border = 0;
            table.AddCell(hCantidad);
            PdfPCell hAncho = new PdfPCell(new Phrase("Ancho", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
            hAncho.Border = 0;
            table.AddCell(hAncho);
            PdfPCell hAlto = new PdfPCell(new Phrase("Alto", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
            hAlto.Border = 0;
            table.AddCell(hAlto);
            PdfPCell hDescripcion = new PdfPCell(new Phrase("Descripción", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
            hDescripcion.Border = 0;
            table.AddCell(hDescripcion);
            PdfPCell hImporte = new PdfPCell(new Phrase("Importe", new Font(Font.FontFamily.HELVETICA, 8f, Font.BOLD)));
            hImporte.Border = 0;
            table.AddCell(hImporte);

            foreach (ArticulosViewModel a in al)
            {
                //list.Add(a.Cantidad.ToString() + "   " + a.Alto.ToString() + "   " + a.Ancho.ToString() + "  " + a.Descripcion + " " + a.DescripcionJaladera);
                //doc1.Add(list);
                PdfPCell cellCantidad = new PdfPCell(new Phrase(a.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
                cellCantidad.Border = 0;
                table.AddCell(cellCantidad);
                PdfPCell cellAncho = new PdfPCell(new Phrase(a.Ancho.ToString(), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
                cellAncho.Border = 0;
                table.AddCell(cellAncho);
                PdfPCell cellAlto = new PdfPCell(new Phrase(a.Alto.ToString(), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
                cellAlto.Border = 0;
                table.AddCell(cellAlto);
                PdfPCell cellDesc = new PdfPCell(new Phrase(a.Descripcion, new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
                cellDesc.Border = 0;
                table.AddCell(cellDesc);
                PdfPCell cellImporte = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(a.Importe, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
                cellImporte.Border = 0;
                table.AddCell(cellImporte);
            }
            doc1.Add(table);

            doc1.Add(new Paragraph(" "));
            doc1.Add(new Paragraph(" "));
            TotalesViewModel t = new TotalesViewModel();

            t = GetTotalesC(proyecto);
            PdfPTable tabTotales = new PdfPTable(2);
            float[] widthsTotales = new float[] { 1f, 1f };
            tabTotales.SetWidths(widthsTotales);
            PdfPCell cellTitImporte = new PdfPCell(new Phrase("Importe: ", new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTitImporte);
            PdfPCell cellTImporte = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(t.Importe, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTImporte);

            PdfPCell cellTitDescuento = new PdfPCell(new Phrase("Descuento: ", new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTitDescuento);
            PdfPCell cellTDescuento = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(t.Descuento, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTDescuento);

            PdfPCell cellTitSubtotal = new PdfPCell(new Phrase("Subtotal: ", new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTitSubtotal);
            PdfPCell cellTSubtotal = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(t.Subtotal, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTSubtotal);

            PdfPCell cellTitIVA = new PdfPCell(new Phrase("IVA: ", new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTitIVA);
            PdfPCell cellTIva = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(t.IVA, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTIva);

            PdfPCell cellTitTotal = new PdfPCell(new Phrase("Total: ", new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTitTotal);
            PdfPCell cellTTotal = new PdfPCell(new Phrase("$" + Convert.ToString(decimal.Round(t.Total, 2)), new Font(Font.FontFamily.HELVETICA, 8f, Font.NORMAL)));
            tabTotales.AddCell(cellTTotal);
            doc1.Add(tabTotales);

            //doc1.Add(new Paragraph("This is test"));
            doc1.Close();           
 
        }

        //METODO PARA APLICAR DESCUENTO DE EXHIBICION A LOS ARTICULOS DE UN PROYECTO
        public JsonResult AplicarDescExhibicion(int proyecto)
        {
            var pr = (from p in db.Proyectos
                           where p.IdProyecto == proyecto
                           select p).FirstOrDefault();
            pr.Exhibicion = true;
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();

            if (pr.Exhibicion)
            {
                //EN CASO DE SER UN PROYECTO DE EXHIBICION SE AÑADE UN 50% ADICIONAL AL PRECIO
                foreach(ProyArticulo i in (from a in db.ProyArticulos where a.Proyecto == pr.IdProyecto && ! a.EsConceptoDeProyecto select a).Distinct())
                {
                    if (!i.EsConceptoDeProyecto)
                    {
                        i.PrecioPrincipal -= (i.PrecioPrincipal * 0.5M);
                        if (i.tieneJaladera)
                        {
                            i.PrecioJaladera -= (i.PrecioJaladera * 0.5M);
                        }
                        if (i.tieneVidrio)
                        {
                            i.PrecioVidrio -= (i.PrecioVidrio * 0.5M);
                        }
                        if (i.tieneProtecta)
                        {
                            i.PrecioProtecta -= (i.PrecioProtecta * 0.5M);
                        }
                    }

                    db.Entry(i).State = EntityState.Modified;
                }                
            }
            db.SaveChanges();
            return Json("Descuento de exhibicion aplicado", JsonRequestBehavior.AllowGet);
        }

        //METODO PARA QUITAR EL DESCUENTO DE EXHIBICION A LOS ARTICULOS DE UN PROYECTO
        public JsonResult QuitarDescExhibicion(int proyecto)
        {
            Proyecto pr = (from p in db.Proyectos
                           where p.IdProyecto == proyecto
                           select p).FirstOrDefault();
            pr.Exhibicion = false;
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();

            if (!pr.Exhibicion)
            {
                //EN CASO DE SER UN PROYECTO DE EXHIBICION SE AÑADE UN 50% ADICIONAL AL PRECIO
                foreach (ProyArticulo i in (from a in db.ProyArticulos where a.Proyecto == pr.IdProyecto && !a.EsConceptoDeProyecto select a).Distinct())
                {
                    if (!i.EsConceptoDeProyecto)
                    {
                        i.PrecioPrincipal += (i.PrecioPrincipal);
                        if (i.tieneJaladera)
                        {
                            i.PrecioJaladera += (i.PrecioJaladera);
                        }
                        if (i.tieneVidrio)
                        {
                            i.PrecioVidrio += (i.PrecioVidrio);
                        }
                        if (i.tieneProtecta)
                        {
                            i.PrecioProtecta += (i.PrecioProtecta);
                        }
                    }

                    db.Entry(i).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            return Json("Descuento de exhibicion eliminado", JsonRequestBehavior.AllowGet);
        }

        //METODO PARA OBTENER EL PRECIO DE LISTA DE UN ARTICULO
        public decimal GetPrecioLista(ArtADNCodigo adn, UserProfile usuario = null)
        {
            if (usuario == null)
            {
                usuario = UsuarioActual();
            }

            decimal precio = 0;

            switch (usuario.ListaPreciosEsp)
            {
                case "(Precio Lista)":
                    precio = adn.PrecioLista ?? 0;
                    break;
                case "(Precio 2)":
                    precio = adn.Precio2 ?? 0;
                    break;
                case "(Precio 3)":
                    precio = adn.Precio3 ?? 0;
                    break;
                case "(Precio 4)":
                    precio = adn.Precio4 ?? 0;
                    break;
                case "(Precio 5)":
                    precio = adn.Precio5 ?? 0;
                    break;
                case "(Precio 6)":
                    precio = adn.Precio6 ?? 0;
                    break;
                case "(Precio 7)":
                    precio = adn.Precio7 ?? 0;
                    break;
                case "(Precio 8)":
                    precio = adn.Precio8 ?? 0;
                    break;
                case "(Precio 9)":
                    precio = adn.Precio9 ?? 0;
                    break;
                case "(Precio 10)":
                    precio = adn.Precio10 ?? 0;
                    break;
                default: precio = adn.PrecioLista ?? 0;
                    break;
            }
            return precio;
        }

        //METODO PARA OBTENER EL USUARIO ACTUAL LOGEADO
        public UserProfile UsuarioActual()
        {
            UserProfile user;
            using (UsersContext usuarios = new UsersContext())
            {
                user = usuarios.UserProfiles.FirstOrDefault(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper());
            }

            return user;
        }

        [HttpPost]
        public JsonResult AgregarArticulo(ProyArticulo jArticulo, int EsEdicion = 0, UserProfile usuario = null)
        {
            ProyArticulo art = new ProyArticulo();

            art = jArticulo;
            int IdProyecto = art.Proyecto;

            art.TmUltimoCambio = DateTime.Today;
            usuario = UsuarioActual();

            ArtADNCodigo artcod = (from c in db.ArtADNCodigos where c.Codigo == art.CodigoADNInterno select c).FirstOrDefault();
            decimal precioCorrecto = GetPrecioLista(artcod, usuario);

            //AJUSTA EL PRECIO
            if ((art.ADNTipo == "CA") || (art.ADNTipo == "HE") || (art.ADNTipo == "AJ") || (art.ADNTipo == "MU") || (art.ADNTipo == "MM") || (art.ADNTipo == "BO") ||                
                (art.ADNTipo == "BO")  || (art.ADNTipo == "CO") || (art.ADNTipo == "GI") || (art.ADNTipo == "CL"))
            {
                //art.PrecioPrincipal = art.PrecioListaPrincipal;
                art.PrecioPrincipal = precioCorrecto;
            }
            else
            {
                if ((art.ADNTipo == "BI") || (art.ADNTipo == "PM") || (art.ADNTipo == "CM") || (art.ADNTipo == "PB"))
                {
                    //art.PrecioPrincipal = art.PrecioListaPrincipal ?? 0;
                    art.PrecioPrincipal = precioCorrecto;
                }
                else if ((art.ADNTipo == "MA"))
                {
                    art.PrecioPrincipal = (((decimal)(((float)art.Alto + (float)art.Ancho) * 2) / 1000) * (decimal?)precioCorrecto) ?? 0;
                }
                else
                {
                    //art.PrecioPrincipal = (((decimal)(((float)art.Alto * (float)art.Ancho) / 1000000)) * art.PrecioListaPrincipal) ?? 0;
                    art.PrecioPrincipal = (((decimal)(((float)art.Alto * (float)art.Ancho) / 1000000)) * (decimal ?)precioCorrecto) ?? 0;
                }
            }

            //PRECIO DE LA JALADERA SI APLICA            
            if (art.tieneJaladera)
            {
                //var jaladera = "JA" + art.ADNJaladeraBase + art.ADNJaladera + art.ADNJaladeraOpcion;
                var jaladera = "JA" + art.ADNJaladeraBase + art.ADNJaladera;
                art.PrecioListaJaladera = GetPrecioJaladera(jaladera, usuario);
            }

            //BASE VETRA Y MARCOS DE ALUMINIO AVOLA SIEMPRE LLEVAN VIDRIO
            if ((art.ADNBase == "VT"))
            {
                //LLEVA VIDRIO
                string CodigoADNVidrio = "VBXX" + art.ADNColor;
                string ADNVidrioBase = "VB";
                string ADNVidrioColor = art.ADNColor;

                art.CodigoADNVidrio = CodigoADNVidrio;
                art.ADNVidrioBase = ADNVidrioBase;
                art.ADNVidrioColor = ADNVidrioColor;
                art.tieneVidrio = true;

                Art a = GetPrecioPieza(CodigoADNVidrio);
                decimal precioPieza = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == a.Articulo select c).FirstOrDefault(), usuario); //ESTO ES PARA HACERLO COMPATIBLE CON PRECIOS EN DOLAR Y OTRAS LISTAS
                //art.PrecioListaVidrio = a.PrecioLista;
                art.PrecioListaVidrio = precioPieza;
                art.PrecioVidrio = ((decimal)(((float)art.Alto * (float)art.Ancho) / 1000000)) * art.PrecioListaVidrio;

            }
            if ((art.ADNTipo == "AR") || ((art.ADNTipo == "MA") && ((art.ADNBase == "CH") || (art.ADNBase == "GR") || (art.ADNBase == "AV"))))
            {
                if (art.ADNColor != "XX")
                {
                    string CodigoADNVidrio = "VDXX" + art.ADNColor;
                    string ADNVidrioBase = "VD";
                    if (art.ADNTipo == "AR")
                    {
                        ADNVidrioBase = "VB";
                    }
                    string ADNVidrioColor = art.ADNColor;
                    art.CodigoADNVidrio = CodigoADNVidrio;
                    art.ADNVidrioBase = ADNVidrioBase;
                    art.ADNVidrioColor = ADNVidrioColor;
                    art.tieneVidrio = true;

                    Art a = GetPrecioPieza(CodigoADNVidrio);
                    decimal precioPieza = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == a.Articulo select c).FirstOrDefault()); //ESTO ES PARA HACERLO COMPATIBLE CON PRECIOS EN DOLAR Y OTRAS LISTAS
                    //art.PrecioListaVidrio = a.PrecioLista;
                    art.PrecioListaVidrio = precioPieza;
                    art.PrecioVidrio = ((decimal)(((float)art.Alto * (float)art.Ancho) / 1000000)) * art.PrecioListaVidrio;
                }

            }

            //DATOS DE VIDRIO PARA VENTANAS
            if ((art.ADNTipo == "VU") && (art.ADNVidrioBase != null))
            {
                Art a = GetPrecioPieza(art.ADNVidrioBase + "XX" + art.ADNVidrioColor);
                //ESTO ES PARA HACERLO COMPATIBLE CON PRECIOS EN DOLAR Y OTRAS LISTAS
                var precioPieza = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == a.Articulo select c).FirstOrDefault()); 
                //art.PrecioListaVidrio = a.PrecioLista;
                art.PrecioListaVidrio = precioPieza;
                art.PrecioVidrio = ((decimal)(((float)art.Alto * (float)art.Ancho) / 1000000)) * art.PrecioListaVidrio;
            }
            //
            
            //PONE PRECIO DE PERFORACIONES SI APLICA      
            if ((art.ADNOrificios != null) && (art.ADNOrificios != "XX"))
            {
                if (art.ADNOrificios.Length > 0)
                {
                    string orif = art.ADNOrificios + art.OrificiosDistCanto + "X" + art.OrificiosCuantos;
                    ArtADNCodigo precioOrificios = (from i in db.ArtADNCodigos
                                                    where i.Codigo == orif
                                                    select i).FirstOrDefault();
                    art.PrecioListaServicios = precioOrificios.PrecioLista;
                    art.PrecioServicios = precioOrificios.PrecioLista;
                }
            }            

            if (EsEdicion == 1)
            {
                db.Entry(art).State = EntityState.Modified;
            }
            else if (EsEdicion == 0)
            {
                //SI ES VIDRIO QUITAR LOS CODIGOS QUE NO VAN
                if (art.ADNTipo == "VD")
                {
                    art.ADNVariante = null;
                    art.ADNCubrecanto = null;
                    art.ADNVeta = null;
                }
                //

                db.ProyArticulos.Add(art);
            }

            try
            {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }

            //REVISA SI HAY MARCO DE ALUMINIO PARA ACTUALIZAR EL COSTO DE ARMADO
            var aluminios = (from i in db.ProyArticulos
                             where i.Proyecto == IdProyecto && i.ADNTipo == "MA"
                             select i);
            int cantMA = 0;
            if (aluminios.Count() > 0)
            {
                cantMA = aluminios.Sum(a => a.Cantidad);
            }

            if (cantMA > 0)
            {
                ProyArticulo artArmado;

                ProyArticulo armado = (from i in db.ProyArticulos
                                       where i.Proyecto == IdProyecto && i.ADNTipo == "CE" && i.ADNBase == "AM" && i.ADNColor == "XX"
                                       select i).FirstOrDefault();
                if (armado != null)
                {
                    armado.Cantidad = cantMA;
                    db.Entry(armado).State = EntityState.Modified;
                }
                else
                {
                    //decimal precio = (from i in db.ArtADNCodigos
                    //                  where i.Codigo.Trim() == "CEAMXX"
                    //                  select i.PrecioLista ?? 0).FirstOrDefault();

                    //SE CAMBIA A LA SIGUIENTE MANERA PARA QUE SEA COMPATIBLE CON PRECIOS EN DOLARES Y OTRAS LISTAS DE PRECIOS
                    decimal precio = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == "CEAMXX" select c).FirstOrDefault(), usuario);

                    artArmado = new ProyArticulo();
                    artArmado.Cantidad = cantMA;
                    artArmado.Proyecto = IdProyecto;
                    artArmado.ADNTipo = "CE";
                    artArmado.ADNBase = "AM";
                    artArmado.ADNColor = "XX";
                    artArmado.CodigoADNInterno = "CEAMXX";
                    artArmado.PrecioPrincipal = precio;
                    artArmado.PrecioListaPrincipal = precio;
                    artArmado.Unidad = "PZA";
                    artArmado.TmUltimoCambio = DateTime.Today;
                    artArmado.EsConceptoDeProyecto = false;
                    artArmado.tieneColorExclusivo = false;
                    db.ProyArticulos.Add(artArmado);
                }

                db.SaveChanges();
            }

            //SI HAY MARCOS DE ALUMINIO O VIDRIO SUELTO AGREGA EL COSTO DE EMPAQUE DE MARCOS Y VIDRIOS
            var vidrios = (from i in db.ProyArticulos
                           where i.Proyecto == IdProyecto && ((i.ADNTipo == "VD") || (i.ADNTipo == "VB") || ((i.ADNTipo == "VU") && (i.ADNVidrioBase != null)))
                           select i);
            int cantV = vidrios.Count();
            if ((cantV > 0) || (cantMA > 0))
            {
                ProyArticulo costoEmpVidMarco = (from i in db.ProyArticulos
                                                 where i.Proyecto == IdProyecto && i.CodigoADNInterno == "CEEIXX"
                                                 select i).FirstOrDefault();

                decimal precioEmp = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == "CEEIXX" select c).FirstOrDefault(), usuario);
                /*decimal metrajeVidrios = (from i in db.ProyArticulos
                                          where i.Proyecto == IdProyecto && ((i.CodigoADNInterno.Substring(0, 2) == "VD") || (i.CodigoADNInterno.Substring(0, 2) == "VB") || (i.tieneVidrio))
                                          select i).Distinct().Sum((a => ((a.Alto * a.Ancho) / 1000000) * a.Cantidad)) ?? 0;
                */
                
                var mv = (from i in db.ProyArticulos
                          where i.Proyecto == IdProyecto && ((i.CodigoADNInterno.Substring(0, 2) == "VD") || (i.CodigoADNInterno.Substring(0, 2) == "VB") || (i.tieneVidrio))
                          select i).Distinct();
                decimal metrajeVidrios = mv.Sum(m => (((decimal)m.Alto * (decimal)m.Ancho) / 1000000) * m.Cantidad);


                if (costoEmpVidMarco == null)
                {
                    ProyArticulo costEmpVidMa = new ProyArticulo();
                    costEmpVidMa.Proyecto = IdProyecto;
                    costEmpVidMa.PrecioPrincipal = metrajeVidrios * precioEmp;
                    costEmpVidMa.PrecioListaPrincipal = metrajeVidrios * precioEmp; //GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == "CEEIXX" select c).FirstOrDefault(), usuario);
                    costEmpVidMa.Cantidad = 1;
                    costEmpVidMa.ADNTipo = "CE";
                    costEmpVidMa.ADNBase = "EI";
                    costEmpVidMa.ADNColor = "XX";
                    costEmpVidMa.CodigoADNInterno = "CEEIXX";
                    costEmpVidMa.Unidad = "PZA";
                    costEmpVidMa.TmUltimoCambio = DateTime.Today;
                    costEmpVidMa.EsConceptoDeProyecto = true;
                    costEmpVidMa.tieneColorExclusivo = false;
                    db.ProyArticulos.Add(costEmpVidMa);
                    db.SaveChanges();
                }
                else
                {
                    ProyArticulo cost = (from i in db.ProyArticulos
                                         where i.Proyecto == IdProyecto && i.CodigoADNInterno == "CEEIXX"
                                         select i).FirstOrDefault();
                    cost.PrecioPrincipal = metrajeVidrios * precioEmp;
                    cost.PrecioListaPrincipal = metrajeVidrios * precioEmp;                   
                    db.Entry(cost).State = EntityState.Modified;
                }
            }


            //REVISA SI ES EXPRESS
            Proyecto pr = (from i in db.Proyectos
                           where i.IdProyecto == art.Proyecto
                           select i).FirstOrDefault();
            //pr.EsExpress = EsExpress(art.Proyecto);
            pr.Observaciones += "#" + DateTime.Today.ToString() + "Articulo agregado: " + art.CodigoADNInterno;
            db.Entry(pr).State = EntityState.Modified;

            //ASIGNAR DESCUENTO
            string cte = (from i in db.Proyectos
                          where i.IdProyecto == IdProyecto
                          select i.ClienteERP).FirstOrDefault();
            string desc = (from i in db_intelisis.Ctes
                           where i.Cliente == cte
                           select i.Descuento).FirstOrDefault();
            double porcent = (from i in db_intelisis.Descuentos
                              where i.Descuento1 == desc
                              select i.Porcentaje).FirstOrDefault() ?? 0;
            if (pr.EsExpress)
            {
                foreach (ProyArticulo i in db.ProyArticulos.Where(a => a.Proyecto == IdProyecto))
                {
                    //DISCRIMINAR PRODUCTOS DE LACA Y LIGNOVA
                    var artic = (from x in db_intelisis.Arts
                               where x.Articulo.Substring(0, 6) == art.CodigoADNInterno.Substring(0, 6)
                               select x).FirstOrDefault();

                    if ((i.ADNTipo == "CA") || (i.ADNTipo == "HE") || (i.ADNTipo == "PM") || ("LACA|MADERA".Contains(artic.Rama)))
                    {
                        i.DescuentoLineal = Convert.ToDecimal(porcent);
                        i.DescuentoPrincipal = i.PrecioPrincipal * (Convert.ToDecimal(porcent)) / 100;

                        if (("LACA|MADERA".Contains(artic.Rama)))
                        {
                            pr.EsExpress = false;
                        }
                    }
                    else
                    {
                        i.DescuentoLineal = 0;
                        i.DescuentoPrincipal = 0;
                        i.DescuentoJaladera = 0;
                        i.DescuentoVidrio = 0;
                        i.DescuentoServicios = 0;

                        //DETERMINA SI LA JALADERA ES ML O PZA Y CALCULA EL PRECIO CORRECTO
                        string unidadJal = (from j in db.ArtADNCodigos
                                            where j.Codigo == i.CodigoADNJaladera
                                            select j.Unidad).FirstOrDefault();
                        if (unidadJal == "ML")
                        {                            
                            if (i.ADNPosicionJaladera == "H")
                            {
                                i.PrecioJaladera = ((Convert.ToDecimal(i.Ancho) / 1000) * i.PrecioListaJaladera);
                            }
                            else
                            {
                                i.PrecioJaladera = ((Convert.ToDecimal(i.Alto) / 1000) * i.PrecioListaJaladera);
                            }
                        }
                        else
                        {
                            i.PrecioJaladera = i.PrecioListaJaladera;
                        }
                    }

                    //EN CASO DE SER UN PROYECTO DE EXHIBICION SE AÑADE UN 50% ADICIONAL AL PRECIO
                    //if (pr.Exhibicion)
                    //{
                    //    i.PrecioPrincipal -= (i.PrecioPrincipal * 0.5M);
                    //    if (i.tieneJaladera)
                    //    {
                    //        i.PrecioJaladera -= (i.PrecioJaladera * 0.5M);
                    //    }
                    //    if (i.tieneVidrio)
                    //    {
                    //        i.PrecioVidrio -= (i.PrecioVidrio * 0.5M);
                    //    }
                    //    if (i.tieneProtecta)
                    //    {
                    //        i.PrecioProtecta -= (i.PrecioProtecta * 0.5M);
                    //    }
                    //}
                    db.Entry(i).State = EntityState.Modified;
                }
                
                //if ((art.ADNTipo == "CA") || (art.ADNTipo == "HE"))
                //{
                //    art.DescuentoLineal = Convert.ToDecimal(porcent);
                //    art.DescuentoPrincipal = art.PrecioPrincipal * (Convert.ToDecimal(porcent)) / 100;
                //    art.DescuentoJaladera = art.PrecioListaJaladera * (Convert.ToDecimal(porcent)) / 100;
                //    art.PrecioJaladera = art.PrecioListaJaladera - art.DescuentoJaladera;
                //    //db.Entry(art).State = EntityState.Modified;
                //}
                //else
                //{
                //    art.DescuentoLineal = 0;
                //    art.DescuentoPrincipal = 0;
                //    art.DescuentoJaladera = 0;
                //    art.PrecioJaladera = art.PrecioListaJaladera - art.DescuentoJaladera;
                //}
            }
            else
            {
                foreach (ProyArticulo i in db.ProyArticulos.Where(a => a.Proyecto == IdProyecto))
                {
                    if ((i.CodigoADNInterno.Substring(0, 6) != "CEEEXX") && (i.CodigoADNInterno.Substring(0, 6) != "CESEEX")) //NO LLEVA DESCUENTO EN NINGUN SERVICIO (solo en flete y seguro)
                    {
                        i.DescuentoLineal = Convert.ToDecimal(porcent);
                        i.DescuentoPrincipal = i.PrecioPrincipal * (Convert.ToDecimal(porcent)) / 100;
                        i.DescuentoJaladera = i.PrecioListaJaladera * (Convert.ToDecimal(porcent)) / 100;
                        i.PrecioJaladera = i.PrecioListaJaladera - i.DescuentoJaladera;
                        i.DescuentoVidrio = i.PrecioListaVidrio * (Convert.ToDecimal(porcent)) / 100;
                        //i.DescuentoServicios = i.PrecioListaServicios * (Convert.ToDecimal(porcent)) / 100;

                        //DETERMINA SI LA JALADERA ES ML O PZA Y CALCULA EL PRECIO CORRECTO
                        string unidadJal = (from j in db.ArtADNCodigos
                                            where j.Codigo == i.CodigoADNJaladera
                                            select j.Unidad).FirstOrDefault();
                        if (unidadJal == "ML")
                        {
                            i.DescuentoJaladera = ((Convert.ToDecimal(i.Ancho) / 1000) * i.PrecioListaJaladera ?? 0) * Convert.ToDecimal(porcent) / 100;
                            if (i.ADNPosicionJaladera == "H")
                            {
                                i.PrecioJaladera = ((Convert.ToDecimal(i.Ancho) / 1000) * i.PrecioListaJaladera);
                            }
                            else
                            {
                                i.DescuentoJaladera = ((Convert.ToDecimal(i.Alto) / 1000) * i.PrecioListaJaladera ?? 0) * Convert.ToDecimal(porcent) / 100;
                                i.PrecioJaladera = ((Convert.ToDecimal(i.Alto) / 1000) * i.PrecioListaJaladera);
                            }
                        }
                        else
                        {
                            i.PrecioJaladera = i.PrecioListaJaladera;
                        }
                        
                        if (i.ADNTipo == "MA")
                        {
                            //i.DescuentoServicios = i.PrecioServicios * (Convert.ToDecimal(porcent)) / 100;
                        }                        
                    }
                    
                    db.Entry(i).State = EntityState.Modified;
                }
            }

            //db.Entry(art).State = EntityState.Modified;
            db.SaveChanges();

            //FLETE            
            var grupo = (from i in db_intelisis.Ctes
                         where i.Cliente == usuario.ClienteERP.Trim()
                         select i.Grupo).FirstOrDefault();
            ProyArticulo af = null;
            ProyArticulo artFlete = null;
            if (grupo == "FORANEO")
            {
                af = (from i in db.ProyArticulos
                      where i.Proyecto == IdProyecto && i.ADNTipo == "CE" && i.ADNBase == "EE" && i.ADNColor == "XX"
                      select i).FirstOrDefault();
                artFlete = new ProyArticulo();
                if (af != null)
                {
                    artFlete = af;
                }
                artFlete.Proyecto = IdProyecto;
                artFlete.ADNTipo = "CE";
                artFlete.ADNBase = "EE";
                artFlete.ADNColor = "XX";
                artFlete.CodigoADNInterno = "CEEEXX";                
                
                artFlete.TmUltimoCambio = DateTime.Today;
                artFlete.Cantidad = 1;
                artFlete.Unidad = "PZA";
                artFlete.tieneJaladera = false;
                artFlete.tieneVidrio = false;
                artFlete.tieneProtecta = false;
                artFlete.tieneOrificios = false;
                artFlete.EsConceptoDeProyecto = true;
                artFlete.tieneColorExclusivo = false;
                var c = new FleteController().CostoFleteM2(art.Proyecto);
                //decimal cd = c.Data
                artFlete.PrecioPrincipal = Convert.ToDecimal(c.Data);
                artFlete.PrecioListaPrincipal = Convert.ToDecimal(c.Data);
            }
            //db.SaveChanges();
            if (af != null) //POR SI YA TRAE UN ARTICULO DE FLETE PREVIO
            {
                af = artFlete;
                if (ModelState.IsValid)
                {
                    db.Entry(af).State = EntityState.Modified;
                    //db.SaveChanges();
                }
            }
            else
            {
                if (artFlete != null)
                {
                    db.ProyArticulos.Add(artFlete);
                }
            }

            //SEGURO
            //if ((grupo == "FORANEO") && (usuario.FleteraId != 5))
            if (usuario.FleteraId != 5)
            {
                ProyArticulo artSeg = (from i in db.ProyArticulos
                                       where i.Proyecto == IdProyecto && i.CodigoADNInterno == "CESEEX"
                                       select i).FirstOrDefault();
                if (artSeg is ProyArticulo)
                {
                    //ya existe un seguro. 
                    decimal subt = GetSubtotal(IdProyecto);
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = decimal.Multiply(subt, 0.01M);
                    db.Entry(artSeg).State = EntityState.Modified;
                }
                else
                {
                    artSeg = new ProyArticulo();
                    artSeg.Proyecto = IdProyecto;
                    artSeg.ADNTipo = "CE";
                    artSeg.ADNBase = "SE";
                    artSeg.ADNColor = "EX";
                    artSeg.CodigoADNInterno = "CESEEX";
                    artSeg.CodigoADNBase = "CESEEX";
                    artSeg.TmUltimoCambio = DateTime.Today;
                    artSeg.Cantidad = 1;
                    artSeg.Unidad = "PZA";
                    artSeg.tieneJaladera = false;
                    artSeg.tieneVidrio = false;
                    artSeg.tieneProtecta = false;
                    artSeg.tieneOrificios = false;
                    artSeg.EsConceptoDeProyecto = true;
                    artSeg.tieneColorExclusivo = false;

                    var tots = GetTotales(IdProyecto);
                    decimal subt = ((TotalesViewModel)tots.Data).Subtotal; //GetSubtotal(IdProyecto);
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = artSeg.PrecioPrincipal;
                    db.ProyArticulos.Add(artSeg);
                }

            }

            db.SaveChanges();

            return Json("", JsonRequestBehavior.AllowGet);            
        }

        public Boolean EsExpress(int proy)
        {
            float total = 0;

            int c = (from i in db.ProyArticulos
                     where i.Proyecto == proy && i.ADNTipo != "CE" && i.ADNTipo != "CA" && i.ADNTipo != "HE" && i.ADNTipo != "MU" && i.ADNTipo != "PB" && i.ADNTipo != "CM" && i.ADNTipo != "PM" && i.ADNTipo != "MM" &&
                                                 i.ADNTipo != "BO" && i.ADNTipo != "CO" && i.ADNTipo != "GI" && i.ADNTipo != "CL"  
                     select i).Count();

            if (c > 0)
            {
                total = (from i in db.ProyArticulos
                         where i.Proyecto == proy && i.ADNTipo != "CE" && i.ADNTipo != "CA" && i.ADNTipo != "HE" && i.ADNTipo != "MU" && i.ADNTipo != "PB" && i.ADNTipo != "CM" && i.ADNTipo != "PM" && i.ADNTipo != "MM" &&
                                                     i.ADNTipo != "BO" && i.ADNTipo != "CO" && i.ADNTipo != "GI" && i.ADNTipo != "CL"  
                             select i).Sum(i => ((float)((i.Alto * i.Ancho) * i.Cantidad) / 1000000));
            }                      
            
            return total <= 1.3;
        }

        public decimal GetSubtotal(int proy) //ESTA FUNCION ESTA REPETIDA EN EL CONTROLADOR HomeController.cs. Ver la forma de unificar esta funcionalidad
        {
            //return (from i in db.ProyArticulos
            //        where i.Proyecto == proy && i.ADNTipo != "CEEEXX" && i.ADNTipo != "CESEEX"
            //        select i).Sum(i => ((i.PrecioUnitario ?? 0 * i.Cantidad) - (i.DescuentoUnitario ?? 0 * i.Cantidad)));

            return (from i in db.ProyArticulos
                    where i.Proyecto == proy && i.ADNTipo != "CEEEXX" && i.ADNTipo != "CESEEX"
                    select i).Sum(i => i.Importe ?? 0);

        }

        public JsonResult GetDatosArticulo(string codigo, string jaladera = "")
        {
            //COLOCAR AQUI LA LOGICA PARA VER SI REGRESA EL PRECIO EN DOLARES O PESOS

            var u = (from i in db.Users
                     where i.ClienteERP == User.Identity.Name
                     select i).FirstOrDefault();

            var datos = (from i in db.ArtADNCodigos
                         where i.Codigo == codigo
                         select i).FirstOrDefault();

            //EN CASO DE DOLARES SE CAMBIA EL PRECIO DE LISTA A PRECIO3 
            switch (u.ListaPreciosEsp)
            {
                case "(Precio Lista)":
                    datos.PrecioLista = datos.PrecioLista ?? 0;
                    break;
                case "(Precio 2)":
                    datos.PrecioLista = datos.Precio2 ?? 0;
                    break;
                case "(Precio 3)":
                    datos.PrecioLista = datos.Precio3 ?? 0;
                    break;
                case "(Precio 4)":
                    datos.PrecioLista = datos.Precio4 ?? 0;
                    break;
                case "(Precio 5)":
                    datos.PrecioLista = datos.Precio5 ?? 0;
                    break;
                case "(Precio 6)":
                    datos.PrecioLista = datos.Precio6 ?? 0;
                    break;
                case "(Precio 7)":
                    datos.PrecioLista = datos.Precio7 ?? 0;
                    break;
                case "(Precio 8)":
                    datos.PrecioLista = datos.Precio8 ?? 0;
                    break;
                case "(Precio 9)":
                    datos.PrecioLista = datos.Precio9 ?? 0;
                    break;
                case "(Precio 10)":
                    datos.PrecioLista = datos.Precio10 ?? 0;
                    break;
                default:
                    datos.PrecioLista = datos.PrecioLista ?? 0;
                    break;
            }

            return Json(datos, JsonRequestBehavior.AllowGet);

        }

        public Art GetPrecioPieza(string codigo)
        {
            Art art = (from i in db_intelisis.Arts
                       where i.Articulo == codigo
                       select i).FirstOrDefault();

            return art;
        }

        public JsonResult GetUnidadMedida(string codigo)
        {
            ArtADNCodigo art = (from i in db.ArtADNCodigos
                                where i.Tipo == codigo.Substring(0, 2)
                                select i).FirstOrDefault();
            string unidad = art.Unidad ?? "";

            return Json(unidad, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotales(int proyecto)
        {

            var p = db.Proyectos.FirstOrDefault(i => i.IdProyecto == proyecto);
            var t = (from i in db.ProyArticulos
                     where i.Proyecto == proyecto
                     select i).Distinct();

            decimal Imp = t.Sum(i => i.PrecioUnitario * i.Cantidad) ?? 0;
            decimal Desc = !p.EsExpress ? (t.Sum(i => i.DescuentoUnitario * i.Cantidad) ?? 0):0;
            decimal Sub = Imp - Desc;
            decimal Iva = Sub * (decimal)0.16;
            decimal Tot = Sub + Iva;
            TotalesViewModel total = new TotalesViewModel
            {
                Importe = decimal.Round(Imp, 2),
                Descuento = decimal.Round(Desc, 2),
                Subtotal = decimal.Round(Sub, 2),
                IVA = decimal.Round(Iva, 2),
                Total = decimal.Round(Tot, 2)
            };
            
            return Json(total, JsonRequestBehavior.AllowGet);
        }

        public TotalesViewModel GetTotalesC(int proyecto) //SOBRECARGA DE GetTotales PARA USAR DENTRO DE ESTE MISMO CONTROLLER
        {
            var p = db.Proyectos.FirstOrDefault(i => i.IdProyecto == proyecto);
            var t = (from i in db.ProyArticulos
                     where i.Proyecto == proyecto
                     select i).Distinct();

            decimal Imp = t.Sum(i => i.PrecioUnitario * i.Cantidad) ?? 0;
            decimal Desc = p != null && p.EsExpress ? (t.Sum(i => i.DescuentoUnitario * i.Cantidad) ?? 0) : 0;
            decimal Sub = Imp - Desc;
            decimal Iva = Sub * (decimal)0.16;
            decimal Tot = Sub + Iva;
            TotalesViewModel total = new TotalesViewModel
            {
                Importe = decimal.Round(Imp, 2),
                Descuento = decimal.Round(Desc, 2),
                Subtotal = decimal.Round(Sub, 2),
                IVA = decimal.Round(Iva, 2),
                Total = decimal.Round(Tot, 2)
            };
            return total;
        }

        public JsonResult EsEditable(int proyecto)
        {
            Boolean editable = false;
            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == proyecto
                          select i).FirstOrDefault();
            if ((p.Estatus == "C") || (p.Estatus == "W"))
            {
                editable = true;
            }
            return Json(editable, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomTipo(string tipo)
        {
            var nom = (from i in db.ArtTipos
                       where i.Codigo == tipo
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomBase(string aBase)
        {
            var nom = (from i in db.ArtBases
                       where i.Codigo == aBase
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomColor(string color)
        {
            var nom = (from i in db.ArtColores
                       where i.Codigo == color
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomVeta(string veta)
        {
            var nom = (from i in db.ArtVetas
                       where i.Codigo == veta
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomCubrecanto(string cubrecanto)
        {
            var nom = (from i in db.ArtCubrecantos
                       where i.Codigo == cubrecanto
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNomVariante(string variante)
        {
            var nom = (from i in db.ArtVariantes
                       where i.Codigo == variante
                       select i.Nombre).FirstOrDefault();
            return Json(nom, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidaMezcla(string codigo, int proyecto)
        {
            //VALIDA SI EL PROYECTO OMITE LA VALIDACION DE TIEMPOS DE ENTREGA
            var v = (from i in db.Proyectos
                     where i.IdProyecto == proyecto
                     select i.ValidaTiempsEntrega).FirstOrDefault();
            if (v)
            {
                var art = (from i in db_intelisis.Arts
                           where i.Articulo.Substring(0, 6) == codigo.Substring(0, 6)
                           select i).FirstOrDefault();

                int entregaArt = 0;

                string e5 = "ABS|SIC|FOIL 3D|MEL|HPL|MKT|MUEBLES|ACR|DEC";
                string e8 = "VIDRIO|CERAMICA||ALU";
                string e20 = "MADERA|LACA|PI";

                if (e5.Contains(art.Rama))
                {
                    entregaArt = 5;
                }
                else if (e8.Contains(art.Rama))
                {
                    entregaArt = 8;
                }
                else if (e20.Contains(art.Rama))
                {
                    entregaArt = 20;
                }
                else
                {
                    entregaArt = 5;
                }

                foreach (ProyArticulo i in db.ProyArticulos.Where(p => p.Proyecto == proyecto && p.ADNTipo != "CE"))
                {
                    var ia = (from a in db_intelisis.Arts
                              where a.Articulo == i.CodigoADNInterno
                              select a).FirstOrDefault();
                    int entregaCompara = 0;
                    if (e5.Contains(ia.Rama))
                    {
                        entregaCompara = 5;
                    }
                    else if (e8.Contains(ia.Rama))
                    {
                        entregaCompara = 8;
                    }
                    else if (e20.Contains(ia.Rama))
                    {
                        entregaCompara = 20;
                    }

                    if (entregaCompara != entregaArt)
                    {
                        return Json("fail", JsonRequestBehavior.AllowGet);
                    }

                }
            }

            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVidriosVentana()
        {
            var vidrios = (from i in db.ArtADNCodigos
                           where i.Tipo == "VD"
                           select i.Color).Distinct();
            var colores = (from i in db.ArtColores
                           where vidrios.Contains(i.Codigo) || i.Codigo == "XX"
                           orderby i.Orden ascending
                           select new ElementoViewModel
                           {
                               Codigo = i.Codigo,
                               Nombre = i.Nombre,
                               RutaImagen = i.RutaImagen,
                               Orden = i.Orden ?? 0
                           }).Distinct().OrderBy(x => x.Orden);
            
            EditorStepViewModel datos = new EditorStepViewModel();
            datos.Elementos = colores;
            datos.Adn = "";
            datos.NombrePaso = "";
            datos.UltimoPaso = false;
            return Json(datos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMedidasFijas(string tipo, string bas)
        {
            var med = (from i in db.MedidasFijas
                       where i.Tipo == tipo && i.Base == bas
                       select i.Medida).Distinct();

            return Json(med, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAnchoAltoFijo(string medida)
        {
            //var med = (from i in db.MedidasFijas
            //           where i.Medida == medida
            //           select new MedidaFijaViewModel
            //           {
            //               Ancho = i.Ancho,
            //               Alto = i.Alto
            //           }).Distinct();
            var med = (from i in db.MedidasFijas
                       where i.Medida == medida
                       select i).FirstOrDefault();

            return Json(med, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTiempoRamas(string color)
        {
            var tiempoRama = db.TiempoRamas.FirstOrDefault(t => t.color == color);
            return Json(new {tiempo = tiempoRama.tiempo_normal}, JsonRequestBehavior.AllowGet);
        }
    }
}
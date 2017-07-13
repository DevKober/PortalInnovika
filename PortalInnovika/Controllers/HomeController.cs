using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using PortalInnovika.Models;

namespace PortalInnovika.Controllers
{
    public class HomeController : Controller
    {
        private InnovikaComEntities db = new InnovikaComEntities();
        private IntelisisDataContext db_intelisis = new IntelisisDataContext();

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult PopUpAdvt()
        {
            return View();
        }

        public ActionResult DatosEmbarque(int p)
        {
            Proyecto proy = (from i in db.Proyectos
                             where i.IdProyecto == p
                             select i).FirstOrDefault();
            return View(proy);
        }

        public ActionResult Historial(int proyecto)
        {
            return View(proyecto);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }    

        public ActionResult Dummy()
        {
            ViewBag.Message = "Pagina dummy";

            return View();
        }

        public ActionResult Detalle(int proyecto = 41300)
        {
            var articulos = (from item in db.ProyArticulos
                             where item.Proyecto == proyecto
                             select item).Distinct();

            return View(articulos);            
        }

        public ActionResult ConsultaProyecto(string cliente = "3", int proyecto = 0)
        {
            var proy = (from i in db.Proyectos
                        join estatus in db.EstatusProyectos on i.Estatus equals estatus.IdEstatusProy
                        where i.ClienteERP.Trim() == cliente
                        orderby i.IdProyecto
                        select new ProyectosViewModel { 
                            Proyecto = i.IdProyecto,
                            Identificador = i.ReferenciaCliente,
                            Estatus  = estatus.Nombre,
                            ClienteERP = i.ClienteERP
                        }).OrderBy(i => i.Proyecto);

            var articulos = (from item in db.ProyArticulos
                             where item.Proyecto == proyecto
                             orderby item.CodigoADNBase
                             select new ArticulosViewModel
                             {
                                 CodigoADNInterno = item.CodigoADNInterno,
                                 Alto = item.Alto ?? 0,
                                 Ancho = item.Ancho ?? 0,
                                 Importe = item.Importe ?? 0,
                                 ADNJaladera = item.ADNJaladera,
                                 ADNJaladeraBase = item.ADNJaladeraBase,
                                 ADNJaladeraOpcion = item.ADNJaladeraOpcion
                             });

            ProyectoDetalle detail = new ProyectoDetalle();
            detail.Proyecto = null; //proy;
            detail.Articulos = null; //articulos;
            detail.IdProyecto = proyecto; //((ProyectosViewModel)proy.FirstOrDefault()).Proyecto;
            detail.Cliente = cliente; //((Proyecto)proy.FirstOrDefault()).ClienteERP.Trim();

            return View(detail);
        }

        public JsonResult GetProjectField(int idProyecto, string column)
        {
            var proyecto = db.Proyectos.FirstOrDefault(p => p.IdProyecto == idProyecto);
            object c = proyecto?.GetType().GetProperty(column)?.GetValue(proyecto, null);
            return Json(new {result = c}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProyectosCliente(string cliente)
        {
            var proyectos = (from p in db.Proyectos
                             where p.ClienteERP == cliente
                             select new { 
                                 IdProyecto = p.IdProyecto,
                                 ReferenciaCliente = p.ReferenciaCliente,
                                 Estatus = p.Estatus
                             }).ToList();
            return Json(proyectos, JsonRequestBehavior.AllowGet);
        }

        //[System.Web.Mvc.Authorize]
        public JsonResult Get(string cliente, string status)
        {
            if (status == "-")
            {
                status = "A,C,D,E,F,H,K,L,M,N,P,R,S,T,W,X,Z";
            }

            int usuario = UsuarioActual().UserId;
            int usrOld = UsuarioActual().UserOld ?? 0;

            //SOLO SE TOMA EN CUENTA LOS PROYECTOS DESDE 6 MESES ATRAS HASTA LA FECHA ACTUAL
            DateTime hasta = DateTime.Now;
            DateTime desde = hasta.AddMonths(-6);
            //

            var proy = (from i in db.Proyectos.AsEnumerable()
                        join estatus in db.EstatusProyectos on i.Estatus equals estatus.IdEstatusProy
                        where i.ClienteERP.Trim() == cliente && ((i.Usuario == usuario) || (i.Usuario == usrOld)) && i.TmRegistrado.HasValue && status.Contains(i.Estatus) && i.TmRegistrado > desde
                        orderby i.IdProyecto
                        select new
                        {
                            Proyecto = i.IdProyecto,
                            Identificador = i.ReferenciaCliente,
                            Estatus = estatus.NombreCliente,
                            ClienteERP = i.ClienteERP,
                            TmRegistrado = i.TmRegistrado,
                            TmAprobado = i.TmAprobado,
                            TmEntregaTentativa = i.TmEntregaTentativa,
                            TmValidado = i.TmValidado,
                            LoteProdInterno = i.LoteProdInterno,
                            TieneMuebles = i.TieneMuebles
                        }).ToList()
                        .Select(x => new ProyectosViewModel()
                        {
                            Proyecto = x.Proyecto,
                            Identificador = x.Identificador,
                            Estatus = x.Estatus,
                            ClienteERP = x.ClienteERP,
                            TmRegistrado = x.TmRegistrado.Value.ToShortDateString(),
                            TmAprobado = x.TmAprobado, //x.TmAprobado.Value.ToShortDateString(),
                            TmEntregaTentativa = x.TmEntregaTentativa,
                            TmValidado = x.TmValidado,
                            LoteProdInterno = x.LoteProdInterno,
                            TieneMuebles = x.TieneMuebles
                        }).Distinct().OrderByDescending(i => i.Proyecto);

            List<ProyectosViewModel> proyUpd = new List<ProyectosViewModel>();
            foreach (ProyectosViewModel p in proy)
            {                             

                if (p.TieneMuebles && (p.TmEntregaTentativa != null))
                {
                    if (p.TmEntregaTentativa.HasValue)
                    {
                        p.TmEntregaTentativa = p.TmEntregaTentativa.Value.AddDays(7);
                            //AddDays(7);
                    }
                }
                proyUpd.Add(p);
            }
                        
            return Json(proyUpd.AsEnumerable(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetArt(int proyecto)
        {
            var art = (from a in db.ProyArticulos
                       where a.Proyecto == proyecto
                       orderby a.IdArticulo
                       select a).ToList();
            return Json(art, JsonRequestBehavior.AllowGet);
        }

        //[System.Web.Mvc.Authorize]
        public JsonResult GetArticulosProyecto(int proyecto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("NoAutorizado", "Home");
                //Response.Redirect("~/Home/NoAutorizado");
                return null;
            }
            else            
            {
                var articulos = (from item in db.ProyArticulos
                                 join codigo in db.ArtADNCodigos on item.CodigoADNInterno.Trim() equals codigo.Codigo.Trim()
                                 where item.Proyecto == proyecto
                                 orderby item.CodigoADNBase
                                 select new ArticulosViewModel
                                 {
                                     IdArticulo = item.IdArticulo,
                                     ADNTipo = item.ADNTipo,
                                     ADNBase = item.ADNBase,
                                     ADNColor = item.ADNColor,
                                     ADNVariante = item.ADNVariante,
                                     ADNCubrecanto = item.ADNCubrecanto,

                                     CodigoADNInterno = item.CodigoADNInterno,
                                     Descripcion = codigo.Descripcion1,
                                     DescripcionJaladera = " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                     DescripcionJaladeraPos = (from i in db.ArtJaladeraPos where i.Codigo == item.ADNPosicionJaladera select i.Nombre).FirstOrDefault() ?? "",
                                     //Descripcion = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                     tieneJaladera = item.tieneJaladera,
                                     ADNJaladera = item.ADNJaladera,
                                     ADNJaladeraBase = item.ADNJaladeraBase,
                                     ADNJaladeraOpcion = item.ADNJaladeraOpcion,
                                     
                                     Alto = item.Alto ?? 0,
                                     Ancho = item.Ancho ?? 0,
                                     Cantidad = item.Cantidad
                                 });
                List<ArticulosViewModel> al = new List<ArticulosViewModel>();
                foreach (ArticulosViewModel a in articulos)
                {
                    if (a.ADNVariante != "XX")
                    {
                        string v = (from i in db.ArtVariantes
                                    where i.Codigo == a.ADNVariante
                                    select i.Nombre).FirstOrDefault();

                        a.Descripcion += " " + v;
                    }

                    if (a.tieneJaladera)
                    {
                        a.Descripcion += a.DescripcionJaladera + " POS: " + a.DescripcionJaladeraPos.ToUpper();
                    }
                    al.Add(a);
                }
                return Json(al, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult NoAutorizado()
        {
            return View();
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

        //METODO PARA CHECAR SI HAY MENSAJE PARA USUARIOS
        public JsonResult GetMensajePersonalizado()
        {
            string msg = (from i in db.Parametros
                          where i.NombreParametro == "CUSTOM_MSG"
                          select i.ValorCadena).FirstOrDefault() ?? "";
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //METODO PARA CHECAR SI HAY MENSAJE PARA DIA INHABIL
        public JsonResult GetMensajeInhabil()
        {
            // Agradecemos su atención y preferencia
            string msg = "";
            Parametro p = (from i in db.Parametros
                           where i.NombreParametro == "SSIC_INHABIL"
                           select i).FirstOrDefault();
            if (p != null)
            {
                if (p.ValorBooleano ?? false)
                {
                    msg += "Por razones de horario o periodo no laboral, en este momento no hay agentes para dar curso a los proyectos solicitados.\n\n";
                    msg += p.ValorCadena;
                    msg += "\n\nAgradecemos su atención y preferencia.";
                }
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        //METODO PARA OBTENER LOS DOCUMENTOS PERMITIDOS AL USUARI ACTUAL
        public ActionResult Documentos()
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("NoAutorizado", "Home");
                //Response.Redirect("~/Home/NoAutorizado");
                return null;
            }
            else
            {
                //SE OBTIENE EL NUMERO DE USUARIO CORRESPONDIENTE AL VIEJO SISTEMA YA QUE A ESTE NUMERO ESTA RELACIONADA LA TABLA DE LineasUsuario
                string cteERP = UsuarioActual().ClienteERP;
                int userOld = UsuarioActual().UserOld ?? 0;
                /*(from i in db.Users
                 where i.ClienteERP == cteERP
                 select i.UserId).FirstOrDefault();*/

                List<LineasUsuario> lineas = (from i in db.LineasUsuarios
                                              where i.Usuario == userOld
                                              select i).Distinct().ToList();
                string sLineas = "";
                foreach (LineasUsuario l in lineas)
                {
                    sLineas += l.Linea;
                }

                List<Documento> docs = (from i in db.Documentos
                                        where sLineas.Contains(i.Linea) || "N/A".Contains(i.Linea)
                                        select i).Distinct().OrderBy(i => i.Descripcion).ToList();
                Docs d = new Docs();
                d.Documentos = docs;
                //return Json(d, JsonRequestBehavior.AllowGet);
                return View(d);
            }
        }

        public JsonResult NuevoProyecto()
        {
            UserProfile user = UsuarioActual();

            Proyecto proyecto = new Proyecto();
            proyecto.Usuario = user.UserId;

            proyecto.ClienteERP = user.ClienteERP.Trim();
            proyecto.Observaciones = "#" + DateTime.Today.ToString(CultureInfo.CurrentCulture) + " Proyecto creado ";
            proyecto.TmRegistrado = DateTime.Today;
            proyecto.UltimoSeguimiento = DateTime.Today;
            proyecto.Estatus = "C";

            //OBTENCION DE DATOS DE CLIENTE
            var cliente = db_intelisis.Ctes.FirstOrDefault(i => i.Cliente.Trim() == user.ClienteERP);

            proyecto.Direccion = cliente.Descripcion1;
            proyecto.Poblacion = cliente.Descripcion3;
            proyecto.Colonia = cliente.Descripcion5;
            proyecto.CodigoPostal = cliente.Descripcion7;
            proyecto.Estado = cliente.Descripcion9;
            proyecto.Pais = cliente.Descripcion11;
            proyecto.Telefono = cliente.Descripcion13;
            proyecto.FormaDeEnvio = cliente.FormaEnvio;
            proyecto.ValidaTiempsEntrega = true;
            
            var cteIntelisis = db_intelisis.Ctes.FirstOrDefault(i => i.Cliente.Trim() == user.ClienteERP.Trim());
            var formaEnvio = "";
            if (cteIntelisis.FormaEnvio.Trim() == "NINGUNO")
            {
                //formaEnvio = "SIN FLETERA ASIGNADA";
                formaEnvio = "NINGUNO";
            }
            else
            {
                formaEnvio = cteIntelisis.FormaEnvio.Trim();
            }
            var fletera = (from i in db.Fleteras
                            where i.Nombre.Trim() == formaEnvio
                            select i).FirstOrDefault();
            var ciudad = (from i in db.CatalogoCiudades
                            where i.Descripcion.Trim() == cteIntelisis.Descripcion3.Trim()
                            select i).FirstOrDefault();

            var matriz = (from i in db.MatrizFletes2
                            where (i.FleteraId == fletera.FleteraId) && (i.CatalogoCiudadId == ciudad.CatalogoCiudadId)
                            select i).FirstOrDefault();

            proyecto.Costo_1_3_c = matriz.Caj_1_3;
            proyecto.Costo_4_10_c = matriz.Caj_4_10;
            proyecto.Costo_11_20_c = matriz.Caj_11_20;
            proyecto.Costo_21up_c = matriz.Caj_21up;
            proyecto.Costo_1_3_p = matriz.Paq_1_3;
            proyecto.Costo_4_10_p = matriz.Paq_4_10;
            proyecto.Costo_11_20_p = matriz.Paq_11_20;
            proyecto.Costo_21up_p = matriz.Paq_21up;

            db.Proyectos.Add(proyecto);
            db.SaveChanges();
            int id = proyecto.IdProyecto;
            return Json(id, JsonRequestBehavior.AllowGet);              
            
            //return Json(cteIntelisis.FormaEnvio.Trim() + " " +
            //        +ciudad.CatalogoCiudadId + " " + fletera.FleteraId, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SolicitarProyecto(int proy)
        {
            try
            {
                var p = (from i in db.Proyectos
                         where i.IdProyecto == proy
                         select i).FirstOrDefault();
                p.TmSolicitado = DateTime.Today;
                p.Estatus = "S";
                Cte c = (from i in db_intelisis.Ctes
                         where i.Cliente == p.ClienteERP.Trim()
                         select i).FirstOrDefault();
                p.EnvioOcurre = c.INNOcurre;
                p.EnvioAsegurado = c.INNAsegurar;
                p.EnvioCobrado = c.INNCobrar;
                p.EntregaEnPlanta = c.INNEntregaPlanta;

                //    if (ModelState.IsValid)
                //    {
                //        //TryUpdateModel(proyecto);
                //        db.Entry(proyecto).State = EntityState.Modified;
                //        db.SaveChanges();
                //        return this.RedirectToAction("Index");
                //    }
                
                db.Entry(p).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Proyecto solicitado", JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult ClonaProyecto(int proy)
        {
            //ENCABEZADO DEL PROYECTO
            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == proy
                          select i).FirstOrDefault();
            Proyecto proyecto = new Proyecto();
            proyecto = p;
            proyecto.ReferenciaCliente = "Copia del proyecto: " + p.IdProyecto.ToString();
            proyecto.Estatus = "C";
            proyecto.TmRegistrado = DateTime.Today;
            proyecto.TmSolicitado = null;
            proyecto.TmAprobado = null;
            proyecto.TmCerrado = null;
            proyecto.TmEntregaTentativa = null;
            proyecto.TieneMuebles = p.TieneMuebles;
            proyecto.Observaciones = "#" + DateTime.Today.ToString() + " Proyecto copiado del: " + p.IdProyecto.ToString(); 
            
            db.Proyectos.Add(proyecto);
            db.SaveChanges();

            var usuario = UsuarioActual();

            //DETALLE DEL PROYECTO
            IEnumerable<ProyArticulo> arts = (from i in db.ProyArticulos
                                                   where i.Proyecto == proy && i.ADNTipo != "CE"
                                                   select i);

            //VALIDAR QUE LOS ARTICULOS EN EL OBJETO DE AQUI ARRIBA EXISTAN EN INTELISIS

            ProyArticulo a;
            int cantArmados = 0;
            foreach (ProyArticulo item in arts)
            {
                string codigoPortal = item.CodigoADNInterno;
                string codigoIntelisis = (from i in db_intelisis.Arts
                                          where i.Articulo == codigoPortal
                                          select i.Articulo).FirstOrDefault();
                if ((codigoIntelisis.Length > 0) && (codigoIntelisis != null))
                {
                    a = item;
                    a.Proyecto = proyecto.IdProyecto;                             

                    db.ProyArticulos.Add(a);
                    if (item.ADNTipo == "MA")
                    {
                        cantArmados += item.Cantidad;
                    }
                }
            }

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
                      where i.Proyecto == proyecto.IdProyecto && i.ADNTipo == "CE" && i.ADNBase == "EE" && i.ADNColor == "XX"
                      select i).FirstOrDefault();
                artFlete = new ProyArticulo();
                if (af != null)
                {
                    artFlete = af;
                }
                artFlete.Proyecto = proyecto.IdProyecto;
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
                var c = new FleteController().CostoFleteM2(proyecto.IdProyecto);
                //decimal cd = c.Data
                artFlete.PrecioPrincipal = Convert.ToDecimal(c.Data);
                artFlete.PrecioListaPrincipal = Convert.ToDecimal(c.Data);
            }
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

            if (cantArmados > 0)
            {
                decimal precio = (from i in db.ArtADNCodigos
                                  where i.Codigo.Trim() == "CEAMXX"
                                  select i.PrecioLista ?? 0).FirstOrDefault();
                ProyArticulo artArmado = new ProyArticulo();
                artArmado.Cantidad = cantArmados;
                artArmado.Proyecto = proyecto.IdProyecto;
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

                //SI HAY MARCOS DE ALUMINIO O VIDRIO SUELTO AGREGA EL COSTO DE EMPAQUE DE MARCOS Y VIDRIOS
                var vidrios = (from i in db.ProyArticulos
                               where i.Proyecto == proy && ((i.ADNTipo == "VD") || (i.ADNTipo == "VB") || ((i.ADNTipo == "VU") && (i.ADNVidrioBase != null)))
                               select i);
                int cantV = vidrios.Count();
                if ((cantV > 0) || (cantArmados > 0))
                {
                    ProyArticulo costoEmpVidMarco = (from i in db.ProyArticulos
                                                     where i.Proyecto == proyecto.IdProyecto && i.CodigoADNInterno == "CEEIXX"
                                                     select i).FirstOrDefault();

                    decimal precioEmp = GetPrecioLista((from c in db.ArtADNCodigos where c.Codigo == "CEEIXX" select c).FirstOrDefault(), usuario);
                    /*decimal metrajeVidrios = (from i in db.ProyArticulos
                                              where i.Proyecto == IdProyecto && ((i.CodigoADNInterno.Substring(0, 2) == "VD") || (i.CodigoADNInterno.Substring(0, 2) == "VB") || (i.tieneVidrio))
                                              select i).Distinct().Sum((a => ((a.Alto * a.Ancho) / 1000000) * a.Cantidad)) ?? 0;
                    */

                    var mv = (from i in db.ProyArticulos
                              where i.Proyecto == proyecto.IdProyecto && ((i.CodigoADNInterno.Substring(0, 2) == "VD") || (i.CodigoADNInterno.Substring(0, 2) == "VB") || (i.tieneVidrio))
                              select i).Distinct();
                    decimal metrajeVidrios = mv.Sum(m => (((decimal)m.Alto * (decimal)m.Ancho) / 1000000) * m.Cantidad);


                    if (costoEmpVidMarco == null)
                    {
                        ProyArticulo costEmpVidMa = new ProyArticulo();
                        costEmpVidMa.Proyecto = proyecto.IdProyecto;
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
                                             where i.Proyecto == proy && i.CodigoADNInterno == "CEEIXX"
                                             select i).FirstOrDefault();
                        cost.PrecioPrincipal = metrajeVidrios * precioEmp;
                        cost.PrecioListaPrincipal = metrajeVidrios * precioEmp;
                        db.Entry(cost).State = EntityState.Modified;
                    }
                }
            }

            //SEGURO
            //if ((grupo == "FORANEO") && (usuario.FleteraId != 5))
            if (usuario.FleteraId != 5)
            {
                ProyArticulo artSeg = (from i in db.ProyArticulos
                                       where i.Proyecto == proy && i.CodigoADNInterno == "CESEEX"
                                       select i).FirstOrDefault();
                if (artSeg is ProyArticulo)
                {
                    //ya existe un seguro. 
                    decimal subt = GetSubtotal(proyecto.IdProyecto);
                    artSeg.Proyecto = proyecto.IdProyecto;
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = decimal.Multiply(subt, 0.01M);
                    db.Entry(artSeg).State = EntityState.Modified;
                }
                else
                {
                    artSeg = new ProyArticulo();
                    artSeg.Proyecto = proyecto.IdProyecto;
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

                    var tots = GetTotales(proyecto.IdProyecto);
                    decimal subt = ((TotalesViewModel)tots.Data).Subtotal; //GetSubtotal(IdProyecto);
                    artSeg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                    artSeg.PrecioListaPrincipal = artSeg.PrecioPrincipal;
                    db.Entry(artSeg).State = EntityState.Modified;                    
                }
                db.ProyArticulos.Add(artSeg);
                db.SaveChanges();
            }
           
            //ASIGNAR DESCUENTO
            string cte = (from i in db.Proyectos
                          where i.IdProyecto == proyecto.IdProyecto
                          select i.ClienteERP).FirstOrDefault();
            string desc = (from i in db_intelisis.Ctes
                           where i.Cliente == cte
                           select i.Descuento).FirstOrDefault();
            double porcent = (from i in db_intelisis.Descuentos
                              where i.Descuento1 == desc
                              select i.Porcentaje).FirstOrDefault() ?? 0;

            foreach (ProyArticulo i in db.ProyArticulos.Where(n => n.Proyecto == proyecto.IdProyecto))
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

                //art.DescuentoLineal = Convert.ToDecimal(porcent);
                //art.DescuentoPrincipal = art.PrecioPrincipal * (Convert.ToDecimal(porcent)) / 100;
                //art.DescuentoJaladera = art.PrecioListaJaladera * (Convert.ToDecimal(porcent)) / 100;
                //art.PrecioJaladera = art.PrecioListaJaladera - art.DescuentoJaladera;

            db.SaveChanges();

            return Json(proyecto.IdProyecto.ToString(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult EliminarProyecto(int proy)
        {
            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == proy
                          select i).FirstOrDefault();
            foreach (ProyArticulo item in db.ProyArticulos.Where(i => i.Proyecto == proy))
            {
                db.ProyArticulos.Remove(item);
            }
            db.Proyectos.Remove(p);
            db.SaveChanges();
            return Json("Proyecto eliminado", JsonRequestBehavior.AllowGet);
        }

        public ActionResult EdicionProyecto(int proy)
        {
            UserProfile user = UsuarioActual();

            if (user != null)
            {
                var proyecto = (from p in db.Proyectos
                                where p.IdProyecto == proy
                                select p).FirstOrDefault();
                var artics = (from a in db.ProyArticulos
                              where a.Proyecto == proy
                              select a).Distinct();
                var articsVm = (from a in db.ProyArticulos
                                join cod in db.ArtADNCodigos on a.CodigoADNInterno equals cod.Codigo
                                where a.Proyecto == proy
                                select new ArticulosViewModel
                                {
                                    IdArticulo = a.IdArticulo,
                                    Cantidad = a.Cantidad,
                                    Alto = a.Alto ?? 0,
                                    Ancho = a.Ancho ?? 0,
                                    Descripcion = cod.Descripcion1,
                                    //Descripcion = cod.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == a.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                    tieneJaladera = a.tieneJaladera,
                                    ADNJaladera = a.ADNJaladera,
                                    ADNJaladeraBase = a.ADNJaladeraBase,
                                    ADNJaladeraOpcion = a.ADNJaladeraOpcion,
                                    CodigoJaladera = a.CodigoADNJaladera,

                                    Proyecto = a.Proyecto,
                                    Unidad = a.Unidad,
                                    ADNTipo = a.ADNTipo,
                                    ADNBase = a.ADNBase,
                                    ADNColor = a.ADNColor,
                                    ADNVeta = a.ADNVeta,
                                    ADNCubrecanto = a.ADNCubrecanto,
                                    ADNVariante = a.ADNVariante,
                                    CodigoADNInterno = a.CodigoADNInterno,
                                    TmUltimoCambio = a.TmUltimoCambio,
                                    Importe = a.Importe ?? 0
                                });
                //foreach (ArticulosViewModel a in articsVm)
                //{
                //    if (a.tieneJaladera)
                //    {
                //        string j = (from i in db.ArtADNCodigos
                //                    where i.Codigo == "JA" + a.ADNJaladeraBase + a.ADNJaladera + a.ADNJaladeraOpcion
                //                    select i.Descripcion1).FirstOrDefault();
                //        //a.DescripcionCompleta = a.Descripcion + " " + j;
                //        a.Descripcion += " " + j;
                //    }
                //}

                ProyectoDetalle pd = new ProyectoDetalle();
                pd.Proyecto = proyecto;
                pd.Articulos = (IEnumerable<ProyArticulo>)artics;
                pd.ArticulosVM = (IEnumerable<ArticulosViewModel>)articsVm;
                pd.IdProyecto = proy;
                pd.Cliente = user.ClienteERP.Trim();

                return View(pd);

                //Proyecto proyecto = (from p in db.Proyectos
                //                     where p.IdProyecto == proy
                //                     select p).FirstOrDefault();
                //return View(proyecto);
            }
            else
            {
                return null;
            }
        }

        public ActionResult EdicionArticulo(int proy, int articulo)
        {
            if (articulo == 0)
            {
                ProyArticulo art = new ProyArticulo();
                art.Proyecto = proy;
                ViewBag.Modo = "nuevo";
                return View(art);
            }
            else
            {
                ProyArticulo art = (from i in db.ProyArticulos
                                    where i.IdArticulo == articulo
                                    select i).FirstOrDefault();
                ViewBag.Modo = "edicion";
                return View(art);
            }

        }

        public ActionResult ModificarArticulo(int proy, int articulo)
        {
            ProyArticulo art = (from i in db.ProyArticulos
                                where i.IdArticulo == articulo
                                select i).FirstOrDefault();
            ViewBag.Modo = "edicion";
            return View(art);
        }

        public ActionResult EdicionJaladera()
        {
            return View();
        }

        //*********************************************************
        //METODO PAQRA OBTENER ELEMENTOS DEL SELECTOR SEGUN SU TIPO
        //*********************************************************
        public JsonResult GetElemento(string tipo)
        {
            switch (tipo)
            {
                case "tipo":
                    var tipos = (from i in db.ArtTipos
                                 where i.Estatus == "A"
                                 select new TiposViewModel
                                 {
                                     Codigo = i.Codigo,
                                     Nombre = i.Nombre,
                                     RutaImagen = i.RutaImagen
                                 });
                    return Json(tipos, JsonRequestBehavior.AllowGet);
                case "base":
                    var bases = (from i in db.ArtBases
                                 where i.Estatus == "A"
                                 select new ElementoViewModel
                                 {
                                     Codigo = i.Codigo,
                                     Nombre = i.Nombre,
                                     RutaImagen = i.RutaImagen
                                 });
                    return Json(bases, JsonRequestBehavior.AllowGet);
                case "color":
                    var colores = (from i in db.ArtColores
                                   where i.Estatus == "A"
                                   select new ElementoViewModel
                                   {
                                       Codigo = i.Codigo,
                                       Nombre = i.Nombre,
                                       RutaImagen = i.RutaImagen
                                   });
                    return Json(colores, JsonRequestBehavior.AllowGet);
                case "veta":
                    var vetas = (from i in db.ArtVetas
                                 where i.Estatus == "A"
                                 select new ElementoViewModel
                                 {
                                     Codigo = i.Codigo,
                                     Nombre = i.Nombre,
                                     RutaImagen = i.RutaImagen
                                 });
                    return Json(vetas, JsonRequestBehavior.AllowGet);
                case "cubrecanto":
                    var cubrecantos = (from i in db.ArtCubrecantos
                                       where i.Estatus == "A"
                                       select new ElementoViewModel
                                       {
                                           Codigo = i.Codigo,
                                           Nombre = i.Nombre,
                                           RutaImagen = i.RutaImagen
                                       });
                    return Json(cubrecantos, JsonRequestBehavior.AllowGet);
                case "variante":
                    var variantes = (from i in db.ArtVariantes
                                     where i.Estatus == "A"
                                     select new ElementoViewModel
                                     {
                                         Codigo = i.Codigo,
                                         Nombre = i.Nombre,
                                         RutaImagen = i.RutaImagen
                                     });
                    return Json(variantes, JsonRequestBehavior.AllowGet);
                case "jaladera":
                    var jaladeras = (from i in db.ArtVariantes
                                     where i.Estatus == "A"
                                     select new ElementoViewModel
                                     {
                                         Codigo = i.Codigo,
                                         Nombre = i.Nombre,
                                         RutaImagen = i.RutaImagen
                                     });
                    return Json(jaladeras, JsonRequestBehavior.AllowGet);
                default:
                    return null;
            }
        }

        public JsonResult GetArticulosPorProyecto(int proyecto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("NoAutorizado", "Home");
                //Response.Redirect("~/Home/NoAutorizado");
                return null;
            }
            else
            {
                var articulos = (from item in db.ProyArticulos
                                 join codigo in db.ArtADNCodigos on item.CodigoADNInterno.Trim() equals codigo.Codigo.Trim()
                                 where item.Proyecto == proyecto
                                 orderby item.IdArticulo
                                 select new ArticulosViewModel
                                 {
                                     IdArticulo = item.IdArticulo,
                                     ADNTipo = item.ADNTipo,
                                     ADNBase = item.ADNBase,
                                     ADNColor = item.ADNColor,
                                     ADNVariante = item.ADNVariante,
                                     ADNCubrecanto = item.ADNCubrecanto,
                                     CodigoADNInterno = item.CodigoADNInterno,
                                     DescripcionJaladera = (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                     DescripcionJaladeraPos = (from i in db.ArtJaladeraPos where i.Codigo == item.ADNPosicionJaladera select i.Nombre).FirstOrDefault() ?? "",
                                     Descripcion = codigo.Descripcion1,
                                     DescripcionCompleta = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                     //Descripcion = codigo.Descripcion1 + " " + (from i in db.ArtADNCodigos where i.Codigo == item.CodigoADNJaladera select i.Descripcion1).FirstOrDefault() ?? "",
                                     tieneJaladera = item.tieneJaladera,
                                     ADNJaladera = item.ADNJaladera,
                                     ADNJaladeraBase = item.ADNJaladeraBase,
                                     ADNJaladeraOpcion = item.ADNJaladeraOpcion,
                                     
                                     PrecioPrincipal = item.PrecioPrincipal,
                                     PrecioVidrio = item.PrecioVidrio,
                                     PrecioProtecta = item.PrecioProtecta,
                                     PrecioServicios = item.PrecioServicios,
                                     PrecioJaladera = item.PrecioJaladera,
                                     PrecioListaJaladera = item.PrecioListaJaladera,
                                     DescuentoJaladera = item.DescuentoJaladera,

                                     Alto = item.Alto ?? 0,
                                     Ancho = item.Ancho ?? 0,
                                     Cantidad = item.Cantidad,
                                     //Importe = item.Importe ?? 0//((decimal)(item.Alto * item.Ancho)) * item.Cantidad
                                     //Importe = (decimal)(item.Cantidad * (item.PrecioPrincipal ?? 0 + item.PrecioJaladera ?? 0 + item.PrecioVidrio ?? 0 + item.PrecioProtecta ?? 0 + (item.PrecioServicios ?? 0)))
                                     Importe = item.Importe ?? 0
                                 });
                List<ArticulosViewModel> al = new List<ArticulosViewModel>();
                foreach (ArticulosViewModel a in articulos)
                {
                    if (a.ADNVariante != "XX")
                    {
                        string v = (from i in db.ArtVariantes
                                    where i.Codigo == a.ADNVariante
                                    select i.Nombre).FirstOrDefault();

                        a.Descripcion += " " + v;
                    }

                    if (a.tieneJaladera)
                    {
                        a.Descripcion += a.DescripcionJaladera + " POS: " + a.DescripcionJaladeraPos.ToUpper();                        
                    }
                    al.Add(a);
                }

                return Json(al, JsonRequestBehavior.AllowGet);
            }

        }

        public string GetDescCubrecanto(string codigo)
        {
            string dCanto = (from i in db.ArtCubrecantos
                             where i.Codigo.Trim() == codigo.Substring(7, 2)
                             select i.Nombre).FirstOrDefault() ?? "";
            dCanto = " " + dCanto;
            return dCanto;
        }
        
        public string GetDescVariante(string codigo)
        {
            string dVariante = (from i in db.ArtVariantes
                                where i.Codigo.Trim() == codigo.Substring(10, 2)
                                select i.Nombre).FirstOrDefault() ?? "";
            return dVariante;
        }

        [HttpPost]
        public ActionResult ModificaProyecto(ProyectoDetalle pd, string RefCte)        
        {
            //var proyecto = db.Proyectos.Single(p => p.IdProyecto == IdProyecto);
            if (ModelState.IsValid)
            {
                pd.Proyecto.EsExpress = pd.Proyecto.EsExpress;
                //UpdateModel(pd.Proyecto, "Proyecto");}
                if (RefCte != null)
                {
                    pd.Proyecto.ReferenciaCliente = RefCte;
                }
                //if (Vend == null)
                //{
                //    Vend = 0;
                //}
                //pd.Proyecto.Vendedor = Vend;

                if (pd.ArticulosVM != null)
                {
                    pd.Articulos = pd.Proyecto.ProyArticulos;
                    ICollection<ProyArticulo> articulos = new List<ProyArticulo>();
                    foreach (ArticulosViewModel a in pd.ArticulosVM)
                    {
                        var articulo = new ProyArticulo();
                        articulo.IdArticulo = a.IdArticulo;
                        articulo.Proyecto = a.Proyecto;
                        articulo.Cantidad = a.Cantidad;
                        articulo.Alto = a.Alto;
                        articulo.Ancho = a.Ancho;
                        articulo.Unidad = a.Unidad;
                        articulo.ADNTipo = a.ADNTipo;
                        articulo.ADNBase = a.ADNBase;
                        articulo.ADNColor = a.ADNColor;
                        articulo.ADNVeta = a.ADNVeta;
                        articulo.ADNCubrecanto = a.ADNCubrecanto;
                        articulo.ADNVariante = a.ADNVariante;
                        articulo.CodigoADNInterno = a.CodigoADNInterno;
                        articulo.tieneJaladera = a.tieneJaladera;
                        articulo.tieneVidrio = a.tieneVidrio;
                        articulo.tieneProtecta = a.tieneProtecta;
                        articulo.tieneOrificios = a.tieneOrificios;
                        articulo.TmUltimoCambio = a.TmUltimoCambio;
                        articulo.EsConceptoDeProyecto = a.EsConceptoDeProyecto;

                        // ----------
                        //REVISA SI ES EXPRESS
                        var pr = (from i in db.Proyectos
                            where i.IdProyecto == articulo.Proyecto
                            select i).FirstOrDefault();

                        //ASIGNAR DESCUENTO
                        var cte = (from i in db.Proyectos
                            where i.IdProyecto == articulo.Proyecto
                            select i.ClienteERP).FirstOrDefault();
                        var desc = (from i in db_intelisis.Ctes
                            where i.Cliente == cte
                            select i.Descuento).FirstOrDefault();
                        var porcent = (from i in db_intelisis.Descuentos
                                             where i.Descuento1 == desc
                                             select i.Porcentaje).FirstOrDefault() ?? 0;
                        if (pr.EsExpress)
                        {
                            //DISCRIMINAR PRODUCTOS DE LACA Y LIGNOVA
                            var artic = (from x in db_intelisis.Arts
                                where x.Articulo.Substring(0, 6) == articulo.CodigoADNInterno.Substring(0, 6)
                                select x).FirstOrDefault();

                            if ((articulo.ADNTipo == "PM") || ("MADERA".Contains(artic.Rama)))
                            {
                                articulo.DescuentoLineal = Convert.ToDecimal(porcent);
                                articulo.DescuentoPrincipal = articulo.PrecioPrincipal * (Convert.ToDecimal(porcent)) / 100;

                                pr.EsExpress = false;
                            }
                            else
                            {
                                articulo.DescuentoLineal = 0;
                                articulo.DescuentoPrincipal = 0;
                                articulo.DescuentoJaladera = 0;
                                articulo.DescuentoVidrio = 0;
                                articulo.DescuentoServicios = 0;

                                //DETERMINA SI LA JALADERA ES ML O PZA Y CALCULA EL PRECIO CORRECTO
                                var unidadJal = (from j in db.ArtADNCodigos
                                    where j.Codigo == articulo.CodigoADNJaladera
                                    select j.Unidad).FirstOrDefault();
                                if (unidadJal == "ML")
                                {
                                    articulo.PrecioJaladera = (Convert.ToDecimal((int) (articulo.ADNPosicionJaladera == "H"? articulo.Ancho : articulo.Alto)) / 1000) * articulo.PrecioListaJaladera;
                                }
                                else
                                {
                                    articulo.PrecioJaladera = articulo.PrecioListaJaladera;
                                }
                            }
                            db.Entry(articulo).State = EntityState.Modified;

                        }
                        else
                        {

                            if ((articulo.CodigoADNInterno.Substring(0, 6) != "CEEEXX") && (articulo.CodigoADNInterno.Substring(0, 6) != "CESEEX")
                            ) //NO LLEVA DESCUENTO EN NINGUN SERVICIO (solo en flete y seguro)
                            {
                                var cteInt = int.Parse(cte);
                                var descArticulo = db.ArtDescuentoes.Where(d => d.color == articulo.ADNColor && d.cliente == cteInt).Select(d => d.descuento).FirstOrDefault();

                                articulo.DescuentoLineal = Convert.ToDecimal(porcent) + (Convert.ToDecimal(100 - porcent) * (descArticulo / 100)); //articulo.DescuentoLineal = Convert.ToDecimal(porcent);
                                articulo.DescuentoPrincipal = articulo.PrecioPrincipal * (articulo.DescuentoLineal / 100); //articulo.DescuentoPrincipal = articulo.PrecioPrincipal * (Convert.ToDecimal(porcent) / 100);
                                articulo.DescuentoJaladera = articulo.PrecioListaJaladera * (Convert.ToDecimal(porcent) / 100);
                                articulo.PrecioJaladera = articulo.PrecioListaJaladera - articulo.DescuentoJaladera;
                                articulo.DescuentoVidrio = articulo.PrecioListaVidrio * (Convert.ToDecimal(porcent) / 100);
                                //i.DescuentoServicios = i.PrecioListaServicios * (Convert.ToDecimal(porcent)) / 100;

                                //DETERMINA SI LA JALADERA ES ML O PZA Y CALCULA EL PRECIO CORRECTO
                                var unidadJal = (from j in db.ArtADNCodigos
                                    where j.Codigo == articulo.CodigoADNJaladera
                                    select j.Unidad).FirstOrDefault();

                                var descJaladera = Convert.ToDecimal(porcent) / 100;


                                if (unidadJal == "ML")
                                {
                                    articulo.PrecioJaladera = ((Convert.ToDecimal((int) (articulo.ADNPosicionJaladera == "H"? articulo.Ancho : articulo.Alto)) / 1000) * (articulo.PrecioListaJaladera ?? 0));
                                    articulo.DescuentoJaladera = articulo.PrecioJaladera * descJaladera;
                                }
                                else
                                {
                                    articulo.PrecioJaladera = articulo.PrecioListaJaladera;
                                }

                                if (articulo.ADNTipo == "MA")
                                {
                                    //i.DescuentoServicios = i.PrecioServicios * (Convert.ToDecimal(porcent)) / 100;
                                }
                            }

                            db.Entry(articulo).State = EntityState.Modified;

                        }
                        // ----------

                        articulos.Add(articulo);
                    }
                    pd.Articulos = articulos;

                    foreach (ProyArticulo a in pd.Articulos)
                    {
                        db.Entry((ProyArticulo) a).State = EntityState.Modified;
                    }

                }
                db.Entry((Proyecto)pd.Proyecto).State = EntityState.Modified;
                db.SaveChanges();
                return this.RedirectToAction("Dashboard");                

            }
            return this.RedirectToAction("Dashboard");            
        }

        //[HttpPost]
        public JsonResult GuardaEncabezado(Proyecto p)
        {
            db.Entry((Proyecto)p).State = EntityState.Modified;
            db.SaveChanges();
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarcaExhibicion(int proyecto)
        {
            Proyecto p = (from i in db.Proyectos
                          where i.IdProyecto == proyecto
                          select i).FirstOrDefault();
            p.Exhibicion = true;
            db.Entry(p).State = EntityState.Modified;

            var arts = (from i in db.ProyArticulos
                        where i.Proyecto == proyecto && i.ADNTipo != "CE"
                        select i).Distinct();
            foreach (ProyArticulo a in arts)
            {
                a.DescuentoLineal = 50;
            }

            return Json("", JsonRequestBehavior.AllowGet);

        }

        public JsonResult BorraArticulo(int IdArticulo, int Proyecto)
        {
            ProyArticulo art = (from i in db.ProyArticulos
                                where i.IdArticulo == IdArticulo
                                select i).FirstOrDefault();

            //SI HAY MARCO DE ALUMINIO TAMBIEN ELIMINA EL COSTO ADICIONAL DE ARMADO
            int cuantosMarcos = (from i in db.ProyArticulos
                                 where i.Proyecto == Proyecto && i.ADNTipo == "MA"
                                 select i).Count();

            if (cuantosMarcos == 1) //SOLO LO BORRA SI QUEDA UN MARCO
            {
                ProyArticulo costoMarco = (from i in db.ProyArticulos
                                           where i.Proyecto == Proyecto && i.CodigoADNInterno == "CEAMXX"
                                           select i).FirstOrDefault();
                if (costoMarco is ProyArticulo)
                {
                    db.ProyArticulos.Remove(costoMarco);
                }

            }

            //SI APLICA ELIMINA EL COSTO DE EMPAQUE DE MARCO Y VIDRIO
            int cuantosVidrios = (from i in db.ProyArticulos
                                  where i.Proyecto == Proyecto && "VD|VB".Contains(i.ADNTipo)
                                  select i).Count();
            if ((cuantosMarcos == 1) || (cuantosVidrios == 1))
            {
                ProyArticulo a = (from i in db.ProyArticulos
                                  where i.Proyecto == Proyecto && i.CodigoADNInterno == "CEEIXX"
                                  select i).FirstOrDefault();
                if (a != null)
                {
                    db.ProyArticulos.Remove(a);
                }
            }

            //SI ES ESTA BORRANDO EL UNICO ARTICULO Y HAY COSTO DE SEGURO Y/O FLETE, BORRA LOS ARTICULOS DE SEGURRO Y FLETE TAMBIEN
            int cuantosArts = (from i in db.ProyArticulos
                               where i.Proyecto == Proyecto && i.ADNTipo != "CE"
                               select i).Count();
            if (cuantosArts == 1)
            {
                ProyArticulo costoSeguro = (from i in db.ProyArticulos
                                            where i.Proyecto == Proyecto && i.CodigoADNInterno == "CESEEX"
                                            select i).FirstOrDefault();
                ProyArticulo costoFlete = (from i in db.ProyArticulos
                                           where i.Proyecto == Proyecto && (i.CodigoADNInterno == "CEEEXX" || i.CodigoADNInterno == "CEET1X")
                                           select i).FirstOrDefault();
                if (costoSeguro is ProyArticulo)
                {
                    db.ProyArticulos.Remove(costoSeguro);
                }
                if (costoFlete is ProyArticulo)
                {
                    db.ProyArticulos.Remove(costoFlete);
                }
            }

            db.ProyArticulos.Remove(art);
            db.SaveChanges();

            //ACTUALIZA FLETE
            ProyArticulo af = null;
            af = (from i in db.ProyArticulos
                  where i.Proyecto == Proyecto && i.ADNTipo == "CE" && i.ADNBase == "EE" && i.ADNColor == "XX"
                  select i).FirstOrDefault();

            if (af != null)
            {
                var c = new FleteController().CostoFleteM2(art.Proyecto);
                af.PrecioPrincipal = Convert.ToDecimal(c.Data);
                af.PrecioListaPrincipal = Convert.ToDecimal(c.Data);
            }

            //ACTUALIZA COSTO DE SEGURO, SI APLICA
            ProyArticulo seg = (from i in db.ProyArticulos
                                where i.Proyecto == Proyecto && i.CodigoADNInterno == "CESEEX"
                                select i).FirstOrDefault();
            if (seg is ProyArticulo)
            {
                decimal subt = GetSubtotal(Proyecto);
                seg.PrecioPrincipal = decimal.Multiply(subt, 0.01M);
                db.Entry(seg).State = EntityState.Modified;
            }

            //REVISA SI ES EXPRESS
            Proyecto pr = (from i in db.Proyectos
                           where i.IdProyecto == art.Proyecto
                           select i).FirstOrDefault();
            //pr.EsExpress = EsExpress(art.Proyecto);
            pr.Observaciones += "#" + DateTime.Now.ToString() + " Se eliminó el artículo: " + art.CodigoADNInterno.Trim();
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new {Ok = true}, JsonRequestBehavior.AllowGet);
            //EdicionProyecto(Proyecto);

            //return new EmptyResult();
            //return View("EdicionProyecto", Proyecto);
        }

        public Boolean EsExpress(int proy)
        {
            float total = 0;

            int c = (from i in db.ProyArticulos
                     where i.Proyecto == proy && i.ADNTipo != "CA" && i.ADNTipo != "HE" && i.ADNTipo != "CE" && i.ADNTipo != "MU" && i.ADNTipo != "PB" && i.ADNTipo != "CM" && i.ADNTipo != "PM" && i.ADNTipo != "MM" &&
                                                 i.ADNTipo != "BO" && i.ADNTipo != "CO" && i.ADNTipo != "GI" && i.ADNTipo != "CL"  
                     select i).Count();

            if (c > 0)
            {
                total = (from i in db.ProyArticulos
                         where i.Proyecto == proy && i.ADNTipo != "CA" && i.ADNTipo != "HE" && i.ADNTipo != "CE" && i.ADNTipo != "MU" && i.ADNTipo != "PB" && i.ADNTipo != "CM" && i.ADNTipo != "PM" && i.ADNTipo != "MM" &&
                                                     i.ADNTipo != "BO" && i.ADNTipo != "CO" && i.ADNTipo != "GI" && i.ADNTipo != "CL"  
                         select i).Sum(i => ((float)((i.Alto * i.Ancho) * i.Cantidad) / 1000000));
            }

            return total <= 1.3;
        }

        public decimal GetSubtotal(int proy) //ESTA FUNCION ESTA REPETIDA EN EL CONTROLADOR EditorController.cs. Ver la forma de unificar esta funcionalidad
        {
            //return (from i in db.ProyArticulos
            //        where i.Proyecto == proy && i.ADNTipo != "CEEEXX" && i.ADNTipo != "CESEEX"
            //        select i).Sum(i => ((i.PrecioUnitario ?? 0 * i.Cantidad) - (i.DescuentoUnitario ?? 0 * i.Cantidad)));

            return (from i in db.ProyArticulos
                    where i.Proyecto == proy && i.ADNTipo != "CEEEXX" && i.ADNTipo != "CESEEX"
                    select i).Sum(i => i.Importe ?? 0);
        }

        public JsonResult GetAcumulados(int proyecto)
        {
            //var a = (from i in db.ProyArticulos
            //         join cod in db.ArtADNCodigos on i.CodigoADNInterno equals cod.Codigo
            //         where i.Proyecto == proyecto
            //         select new Acumulados
            //         {
            //             Descripcion = cod.Descripcion1,
            //             Piezas = i.Cantidad,
            //             Importe = i.PrecioUnitario ?? 0,
            //             ADNColor = i.ADNColor,
            //             Total = (((float)((i.Alto ?? 0) * (i.Ancho ?? 0)) / 1000000)) * i.Cantidad
            //         });
            //var acu = (from i in a
            //           group i by new { i.Descripcion, i.Importe, i.Total, i.Piezas } into g
            //           select new { g.Key.Descripcion, g.Key.Importe, g.Key.Total, Piezas = g.Sum(i => i.Piezas) });          
            
            var acuOld = (from i in db.vAcumuladosColor
                       where i.Proyecto == proyecto
                       select i).Distinct();

            var acu = (from i in db.vAcumuladosColor
                       where i.Proyecto == proyecto
                       select new Acumulados
                       {                         
                           Descripcion = i.Nombre,
                           Total = i.Metraje ?? 0,
                           Importe = i.Importe ?? 0,
                           Piezas = i.Piezas ?? 0
                       });

            return Json(acu, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Art()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            UserProfile user = UsuarioActual();            
            return View(user);
        }

        public ActionResult Widget()
        {
            UserProfile user = UsuarioActual();

            return View(user);
        }

        public ActionResult DescargaCFDI()
        {
            UserProfile user = UsuarioActual();
            return View(user);
        }

        public JsonResult CFDIs(string cte, string periodo, string mes)
        {
            string dirPath = "F://FacInnovikaXML//" + periodo + "//Innovika//" + mes + "//";
            List<string> files = new List<string>();
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            List<MapaCFDI> archivos = new List<MapaCFDI>();            

            foreach (FileInfo i in dirInfo.GetFiles())
            {
                int pos = i.Name.IndexOf(" "); //String.Compare(i.Name, " ");

                if (i.Name.Substring(0, pos) == cte)
                {
                    MapaCFDI a = new MapaCFDI();
                    a.Nombre = i.Name;
                    a.Ruta = i.FullName;
                    a.Carpeta = "F://FacInnovikaXML";
                    a.Periodo = periodo;
                    a.Mes = mes;
                    a.Archivo = i.Name;
                    archivos.Add(a);
                    files.Add(i.Name);
                }
            }
            //return Json(files.ToArray(), JsonRequestBehavior.AllowGet);
            return Json(archivos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCfdi(string carpetaCfdi, string periodo, string empresa, string mes, string archivo)
        {
            //
            string path = carpetaCfdi + "\\" + periodo + "\\" + empresa + "\\" + mes;
            path += "\\" + archivo;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            string fileName = archivo;
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            //
        }

        public JsonResult GetTotales(int proyecto)
        {
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

            return Json(total, JsonRequestBehavior.AllowGet);
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
                default:
                    precio = adn.PrecioLista ?? 0;
                    break;
            }
            return precio;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PortalInnovika.Models;
using System.Web.Mvc;


namespace PortalInnovika.Controllers
{
    public class FletesController : ApiController
    {
        private InnovikaComEntities db = new InnovikaComEntities();
        private IntelisisDataContext db_intelisis = new IntelisisDataContext();

        public decimal totalFlete = 0;        

        public MatrizFletes2 infoFlete = null;

        public CostoFlete Get(int proyecto)
        {
            CostoFlete costo = new CostoFlete { Costo = 0M };            

            //OBTIENE EL DETALLE DEL PROYECTO 
            var articulos = (from a in db.ProyArticulos
                             where (a.Proyecto == proyecto) && (a.ADNTipo != "CE")
                             select new ArticulosViewModel { 
                                 CodigoADNInterno = a.CodigoADNInterno,
                                 Descripcion = "",
                                 Cantidad = a.Cantidad,
                                 Alto = a.Alto ?? 0,
                                 Ancho = a.Ancho ?? 0,
                                 Importe = a.Importe ?? 0,
                                 CodigoADN = a.CodigoADNInterno,
                                 ADNTipo = a.ADNTipo,
                                 ADNColor = a.ADNColor,
                                 ADNBase = a.ADNBase,
                                 ADNVariante = a.ADNVariante
                             });

            //OBTIENE DATOS DE CLIENTE Y FLETERA
            var clienteERP = (from p in db.Proyectos
                              where p.IdProyecto == proyecto
                              select p.ClienteERP.Trim());

            var usuario = (from u in db.Users
                           where u.ClienteERP.Trim() == clienteERP.ToString()
                           select u).FirstOrDefault();

            var cliente = (from c in db_intelisis.Ctes
                           where c.Cliente == clienteERP.ToString()
                           select c).FirstOrDefault();

            string cveEstado = (from e in db.CatalogoEstados
                                where e.Descripcion.Trim().ToUpper() == cliente.Descripcion9.Trim().ToUpper()
                                select e.Estado).FirstOrDefault();

            int cveCiudad = (from c in db.CatalogoCiudades
                             where ((c.Descripcion.Trim().ToUpper() == cliente.Descripcion3.Trim().ToUpper()) && (c.Estado == cveEstado))
                             select c.CatalogoCiudadId).FirstOrDefault();

            //OBTIENE DATOS DE COSTOS DE MATRIZ DE FLETES
            infoFlete = (from m in db.MatrizFletes2
                         where (m.CatalogoCiudadId == cveCiudad) && (m.FleteraId == usuario.FleteraId)
                         select m).FirstOrDefault();

            //CALCULA EL COSTO DE FLETE EN FUNCION DE LOS ARTICULOS DEL PROYECTO
            foreach (ArticulosViewModel a in articulos)
            {
                totalFlete = CalculaCostoFlete((a.Alto * a.Ancho * a.Cantidad)/1000000M, a);
            }

            //
            costo.Costo = totalFlete;
            return costo;
        }

        public int cajasChicas = 0;
        public int cajasMedianas = 0;
        public int cajasChCount = 0;
        public int cajasMdCount = 0;
        public int paquetes = 0;
        public int cilindros = 0;
        public int cantos = 0;
        public int bisagras = 0;
        public decimal m2ch = 0;
        public decimal m2md = 0;
        public decimal m2pq = 0;
        public int pzaXpaq = 0;
        public int pzasDe4 = 0;
        public int pzasDe2 = 0;
        public int pzasDe6 = 0;
        public string tipo = "";
        public string jaladera = "";
        public string Artbase = "";
        public string color = "";
        public string variante = "";

        public int countArticulos = 0;
        public int countDe6 = 0;
        public int countDe4 = 0;
        public int countDe2 = 0;
        int itemsCount = 0;

        private bool EsCajaChica(decimal ancho, decimal alto)
        {
            if ((ancho <= 700) && (alto <= 700))
            {
                return true;
            }
            else if ((alto <= 700) && (ancho <= 700))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool EsCajaMediana(decimal ancho, decimal alto)
        {
            if ((ancho <= 600) && (alto <= 1150))
            {
                return true;
            }
            else if ((alto <= 600) && (ancho <= 1150))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private decimal CostoCajas(int cajas)
        {
            if (cajas == 0)
            {
                return 0;
            }
            else if ((cajas <= 4) && (cajas > 0))
            {
                //return Convert.ToDecimal(proyecto.Costo_1_3_c * cajas);
                return infoFlete.Caj_1_3 * cajas;
            }
            else if ((cajas <= 10) && (cajas > 3))
            {
                //return Convert.ToDecimal(proyecto.Costo_4_10_c * cajas);
                return infoFlete.Caj_4_10 * cajas;
            }
            else if ((cajas <= 20) && (cajas > 10))
            {
                //return Convert.ToDecimal(proyecto.Costo_11_20_c * cajas);
                return infoFlete.Caj_11_20 * cajas;
            }
            else if ((cajas >= 21))
            {
                //return Convert.ToDecimal(proyecto.Costo_21up_c * cajas);
                return infoFlete.Caj_21up * cajas;
            }
            else { return 0; }
        }

        private decimal CostoPaquetes(int paquetes)
        {
            if (paquetes == 0)
            {
                return 0;
            }
            else if ((paquetes <= 4) && (paquetes > 0))
            {
                //return Convert.ToDecimal(proyecto.Costo_1_3_p * paquetes);
                return infoFlete.Paq_1_3 * paquetes;
            }
            else if ((paquetes <= 10) && (paquetes > 3))
            {
                //return Convert.ToDecimal(proyecto.Costo_4_10_p * paquetes);
                return infoFlete.Paq_4_10 * paquetes;
            }
            else if ((paquetes <= 20) && (paquetes > 10))
            {
                //return Convert.ToDecimal(proyecto.Costo_11_20_p * paquetes);
                return infoFlete.Paq_11_20 * paquetes;
            }
            else if ((paquetes >= 21))
            {
                //return Convert.ToDecimal(proyecto.Costo_21up_p * paquetes);
                return infoFlete.Paq_21up * paquetes;
            }
            else { return 0; }
        }

        private decimal CalculaCostoFlete(decimal metros, ArticulosViewModel art)
        {

            //CALCULO DE CAJAS Y PAQUETES
            int pzasCount = 0;
            int cilCount = 0;
            decimal dimenciones = metros / art.Cantidad;

            //DISTINCION DE TIPOS EN LAS PARTICIONES
            if (tipo == "") { tipo = art.ADNTipo; }

            if (tipo != art.ADNTipo)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                tipo = art.ADNTipo;
            }
            //                       

            //DISTINCION DE BASES EN LAS PARTICIONES
            if (Artbase == "") { Artbase = art.ADNBase; }

            if (Artbase != art.ADNBase)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                Artbase = art.ADNBase;
            }

            //DISTINCION DE COLORES EN LAS PARTICIONES
            if (color == "") { color = art.ADNColor; }

            if (color != art.ADNColor)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                color = art.ADNColor;
            }

            //DISTINCION DE JALADERAS EN LAS PARTICIONES

            //if (jaladera == "") { jaladera = art.CodigoJaladera; }

            if (jaladera != art.CodigoJaladera)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                jaladera = art.CodigoJaladera;
            }

            //DISTINCION DE VARIANTES EN LAS PARTICIONES
            if (variante == "") { variante = art.ADNVariante; }

            if (variante != art.ADNVariante)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                variante = art.ADNVariante;
            }



            countArticulos += 1;
            for (int i = 1; i <= art.Cantidad; i++)
            {
                //countArticulos += 1;

                if (EsCajaChica(Convert.ToDecimal(art.Ancho), Convert.ToDecimal(art.Alto)) && (dimenciones > 0) && (art.ADNTipo != "CE")) //ES CAJA CHICA
                {
                    m2ch += dimenciones;

                    if ((art.ADNBase == "IK") || (art.ADNBase == "VT")) //[MRB: 24/05/2012] SE AGREGÓ ESTE OTRO PROMEDIO (.80 M2) PARA CAJAS CHICAS EN CASO DE BASES IK O VT
                    {
                        if (m2ch > Convert.ToDecimal(0.80))
                        {
                            if (art.ADNTipo == "MA") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA MARCO DE ALUMINIO (2 PIEZAS POR CAJA)
                            {
                                pzaXpaq = 2;
                                if (cajasChCount < pzaXpaq)
                                {
                                    cajasChCount += 1;
                                }
                                if (cajasChCount == pzaXpaq)
                                {
                                    cajasChicas += 1;
                                    cajasChCount = 0;
                                }
                            }
                            else if ((art.ADNTipo == "ML") || (art.ADNTipo == "MR") || (art.ADNTipo == "MS")) //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA MAMPARAS (1 PIEZA POR CAJA)
                            {
                                pzaXpaq = 1;
                                if (cajasChCount < pzaXpaq)
                                {
                                    cajasChCount += 1;
                                }
                                if (cajasChCount == pzaXpaq)
                                {
                                    cajasChicas += 1;
                                    cajasChCount = 0;
                                }
                            }
                            else if (art.ADNTipo == "VD") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA VIDRIO (2 PIEZAS POR CAJA)
                            {
                                pzaXpaq = 2;
                                if (cajasChCount < pzaXpaq)
                                {
                                    cajasChCount += 1;
                                }
                                if (cajasChCount == pzaXpaq)
                                {
                                    cajasChicas += 1;
                                    cajasChCount = 0;
                                }
                            }
                            else
                            {
                                cajasChicas += 1;
                                m2ch = dimenciones;
                            }
                        }
                    }
                    else if (m2ch > Convert.ToDecimal(1.22))
                    {
                        if (art.ADNTipo == "MA") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA MARCO DE ALUMINIO (2 PIEZAS POR CAJA)
                        {
                            pzaXpaq = 2;
                            if (cajasChCount < pzaXpaq)
                            {
                                cajasChCount += 1;
                            }
                            if (cajasChCount == pzaXpaq)
                            {
                                cajasChicas += 1;
                                cajasChCount = 0;
                            }
                        }
                        else if ((art.ADNTipo == "ML") || (art.ADNTipo == "MR") || (art.ADNTipo == "MS")) //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA MAMPARAS (1 PIEZA POR CAJA)
                        {
                            pzaXpaq = 1;
                            if (cajasChCount < pzaXpaq)
                            {
                                cajasChCount += 1;
                            }
                            if (cajasChCount == pzaXpaq)
                            {
                                cajasChicas += 1;
                                cajasChCount = 0;
                            }
                        }
                        else if (art.ADNTipo == "VD") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS CHICAS PARA VIDRIO (2 PIEZAS POR CAJA)
                        {
                            pzaXpaq = 2;
                            if (cajasChCount < pzaXpaq)
                            {
                                cajasChCount += 1;
                            }
                            if (cajasChCount == pzaXpaq)
                            {
                                cajasChicas += 1;
                                cajasChCount = 0;
                            }
                        }
                        else
                        {
                            cajasChicas += 1;
                            m2ch = dimenciones;
                        }
                    }
                }
                else if (EsCajaMediana(Convert.ToDecimal(art.Ancho), Convert.ToDecimal(art.Alto)) && (dimenciones > 0) && (art.ADNTipo != "CE")) //ES CAJA MEDIANA
                {
                    m2md += dimenciones;

                    if ((art.ADNBase == "IK") || (art.ADNBase == "VT")) //[MRB: 24/05/2012] SE AGREGÓ ESTE OTRO PROMEDIO (.80 M2) PARA CAJAS MEDIANAS EN CASO DE BASES IK O VT
                    {
                        if (m2ch > Convert.ToDecimal(1.10))
                        {
                            if (art.ADNTipo == "MA") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA MARCO DE ALUMINIO (2 PIEZAS POR CAJA)
                            {
                                pzaXpaq = 2;
                                if (cajasMdCount < pzaXpaq)
                                {
                                    cajasMdCount += 1;
                                }
                                if (cajasMdCount == pzaXpaq)
                                {
                                    cajasMedianas += 1;
                                    cajasMdCount = 0;
                                }
                            }
                            if ((art.ADNTipo == "ML") || (art.ADNTipo == "MR") || (art.ADNTipo == "MS")) //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA MAMPARAS (1 PIEZA POR CAJA)
                            {
                                pzaXpaq = 1;
                                if (cajasMdCount < pzaXpaq)
                                {
                                    cajasMdCount += 1;
                                }
                                if (cajasMdCount == pzaXpaq)
                                {
                                    cajasMedianas += 1;
                                    cajasMdCount = 0;
                                }
                            }
                            if (art.ADNTipo == "VD") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA VIDRIO (2 PIEZAS POR CAJA)
                            {
                                pzaXpaq = 2;
                                if (cajasMdCount < pzaXpaq)
                                {
                                    cajasMdCount += 1;
                                }
                                if (cajasMdCount == pzaXpaq)
                                {
                                    cajasMedianas += 1;
                                    cajasMdCount = 0;
                                }
                            }
                            else
                            {
                                cajasMedianas += 1;
                                m2md = dimenciones;
                            }
                            //eaea

                        }
                    }
                    else if (m2md > Convert.ToDecimal(1.38))
                    {
                        if (art.ADNTipo == "MA") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA MARCO DE ALUMINIO (2 PIEZAS POR CAJA)
                        {
                            pzaXpaq = 2;
                            if (cajasMdCount < pzaXpaq)
                            {
                                cajasMdCount += 1;
                            }
                            if (cajasMdCount == pzaXpaq)
                            {
                                cajasMedianas += 1;
                                cajasMdCount = 0;
                            }
                        }
                        if ((art.ADNTipo == "ML") || (art.ADNTipo == "MR") || (art.ADNTipo == "MS")) //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA MAMPARAS (1 PIEZA POR CAJA)
                        {
                            pzaXpaq = 1;
                            if (cajasMdCount < pzaXpaq)
                            {
                                cajasMdCount += 1;
                            }
                            if (cajasMdCount == pzaXpaq)
                            {
                                cajasMedianas += 1;
                                cajasMdCount = 0;
                            }
                        }
                        if (art.ADNTipo == "VD") //EXCEPCIÓN DENTRO DE LA REGLA DE CAJAS MEDIANAS PARA VIDRIO (2 PIEZAS POR CAJA)
                        {
                            pzaXpaq = 2;
                            if (cajasMdCount < pzaXpaq)
                            {
                                cajasMdCount += 1;
                            }
                            if (cajasMdCount == pzaXpaq)
                            {
                                cajasMedianas += 1;
                                cajasMdCount = 0;
                            }
                        }
                        else
                        {
                            cajasMedianas += 1;
                            m2md = dimenciones;
                        }
                    }
                }
                else if ((art.ADNTipo == "LP") || (art.ADNTipo == "SP") || (art.ADNTipo == "EP"))
                {
                    countDe6 += 1;

                    pzaXpaq = 6;
                    if (pzasDe6 < pzaXpaq)
                    {
                        pzasDe6 += 1;
                    }
                    if (pzasDe6 == pzaXpaq)
                    {
                        cajasMedianas += 1;
                        pzasDe6 = 0;
                    }
                }
                else if (((art.ADNTipo != "ML") || (art.ADNTipo != "MR") || (art.ADNTipo != "MS")) && ((art.Ancho < Convert.ToDecimal(300)) && (art.Alto > Convert.ToDecimal(1150))) || ((art.Alto < Convert.ToDecimal(300)) && (art.Ancho > Convert.ToDecimal(1150))) && (art.ADNTipo != "CE"))
                {  //NO ES MAMPARA
                    countDe4 += 1;

                    pzaXpaq = 4;
                    if (pzasDe4 < pzaXpaq)
                    {
                        pzasDe4 += 1;
                    }
                    if (pzasDe4 == pzaXpaq)
                    {
                        paquetes += 1;
                        pzasDe4 = 0;
                    }
                }
                else if (((art.ADNTipo == "ML") || (art.ADNTipo == "MR") || (art.ADNTipo == "MS")) && ((art.Ancho < Convert.ToDecimal(300)) && (art.Alto > Convert.ToDecimal(1150))) || ((art.Alto < Convert.ToDecimal(300)) && (art.Ancho > Convert.ToDecimal(1150))) && (art.ADNTipo != "CE"))
                {  //SI ES MAMPARA
                    countDe2 += 1;

                    pzaXpaq = 2;
                    if (pzasDe2 < pzaXpaq)
                    {
                        pzasDe2 += 1;
                    }
                    if (pzasDe2 == pzaXpaq)
                    {
                        paquetes += 1;
                        pzasDe2 = 0;
                    }
                }
                else if ((art.ADNTipo == "PU") || (art.ADNTipo == "VC") || (art.ADNTipo == "VO") || (art.ADNTipo == "VS") || (art.ADNTipo == "VU") || (art.ADNTipo == "VI") || (art.ADNTipo == "MH") || (art.ADNTipo == "FR"))
                {
                    pzaXpaq = 2;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        paquetes += 1;
                        pzasCount = 0;
                    }
                }
                else if ((art.ADNTipo == "AJ") || (art.ADNTipo == "CO") || (art.ADNTipo == "CR") || (art.ADNTipo == "TL"))
                {
                    pzaXpaq = 4;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        paquetes += 1;
                        pzasCount = 0;
                    }
                }
                else if (art.ADNTipo == "TS")
                {
                    pzaXpaq = 1;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        cajasMedianas += 1;
                        pzasCount = 0;
                    }
                }
                else if (art.ADNTipo == "BO")
                {
                    pzaXpaq = 1;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        cajasMedianas += 1;
                        pzasCount = 0;
                    }
                }
                else if (art.ADNTipo == "TI")
                {
                    pzaXpaq = 1;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        cajasMedianas += 1;
                        pzasCount = 0;
                    }
                }
                else if (art.ADNTipo == "CA")
                {
                    cantos = cantos + 1;
                }
                else if (art.ADNTipo == "BI")
                {
                    bisagras = bisagras + 1;
                }
                else if ((art.ADNTipo == "HE") || (art.ADNTipo == "BF"))
                {
                    pzaXpaq = 3;
                    if (cilCount < pzaXpaq)
                    {
                        cilCount += 1;
                    }
                    if (cilCount == pzaXpaq)
                    {
                        cilindros += 1;
                        cilCount = 0;
                    }
                }
                else if (art.ADNTipo == "VD")
                {
                    pzaXpaq = 2;
                    if (pzasCount < pzaXpaq)
                    {
                        pzasCount += 1;
                    }
                    if (pzasCount == pzaXpaq)
                    {
                        paquetes += 1;
                        pzasCount = 0;
                    }
                }
                else if ((art.ADNTipo != "CE") && (art.CodigoADN.Trim() != "CHXXVRXXXXXX")) //ES PAQUETE
                {
                    paquetes += 1;
                }
            }

            //SE MANEJAN LOS SOBRANTES DE METRO PARA EL CASO DE TENER QUE METERLOS EN UN EMPAQUE ADICIONAL
            if (EsCajaChica(Convert.ToDecimal(art.Ancho), Convert.ToDecimal(art.Alto)) && (dimenciones > 0)) //ES CAJA CHICA
            {
                //AGREGAR A ESTOS IF UNA VALIDACION EXTRA PARA EL TOPE DE LA MEDIDA DE LA CAJA ACTUAL (MULTIPLICAR POR CAJASACTUALES +1)
                if ((m2ch > 0) && (m2ch > (Convert.ToDecimal(1.22) * (cajasChicas + 1)))) { cajasChicas += 1; m2ch = 0; }
            }
            else if (EsCajaMediana(Convert.ToDecimal(art.Ancho), Convert.ToDecimal(art.Alto)) && (dimenciones > 0)) //ES CAJA MEDIANA
            {
                //if (m2md > 0) { cajasMedianas += 1; }
                if (m2md > (Convert.ToDecimal(1.38) * (cajasMedianas + 1))) { cajasMedianas += 1; m2md = 0; }
            }
            else if (cilCount > 0) { cilindros += 1; }
            else if (pzasCount > 0) { paquetes += 1; }

            if (countArticulos >= itemsCount - 2)
            {
                if (m2ch > 0) { cajasChicas += 1; m2ch = 0; }
                if (cajasChCount > 0) { cajasChicas += 1; cajasChCount = 0; }
                if (m2md > 0) { cajasMedianas += 1; m2md = 0; }
                if (cajasMdCount > 0) { cajasMedianas += 1; cajasMdCount = 0; }

                if (pzasDe2 > 0) { paquetes += 1; }
                if (pzasDe4 > 0) { paquetes += 1; }
            }

            //CALCULO DE COSTOS
            decimal totalCajas = CostoCajas(cajasChicas + cajasMedianas);
            decimal totalPaquetes = CostoPaquetes(paquetes + cilindros);
            //METE TODOS LOS CANTOS DE LA MISMA BASE EN UN SOLO PAQUETE
            if (cantos > 0) { paquetes += 1; }
            //METE TODAS LAS BISAGRAS EN UN SOLO PAQUETE
            if (bisagras > 0) { paquetes += 1; }



            //return totalChicas + totalMedianas + totalPaquetes;
            return totalCajas + totalPaquetes;

        }

    }
}

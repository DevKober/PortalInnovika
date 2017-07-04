﻿//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PortalInnovika.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class InnovikaComEntities : DbContext
    {
        public InnovikaComEntities()
            : base("name=InnovikaComEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<ArtADNCodigo> ArtADNCodigos { get; set; }
        public DbSet<ArtBas> ArtBases { get; set; }
        public DbSet<ArtColore> ArtColores { get; set; }
        public DbSet<ArtColoresExclusivo> ArtColoresExclusivos { get; set; }
        public DbSet<ArtCubrecanto> ArtCubrecantos { get; set; }
        public DbSet<ArticulosAdjunto> ArticulosAdjuntos { get; set; }
        public DbSet<ArticulosFav> ArticulosFavs { get; set; }
        public DbSet<ArtJaladeraPos> ArtJaladeraPos { get; set; }
        public DbSet<ArtJaladera> ArtJaladeras { get; set; }
        public DbSet<ArtLinea> ArtLineas { get; set; }
        public DbSet<ArtSubLinea> ArtSubLineas { get; set; }
        public DbSet<ArtTipos> ArtTipos { get; set; }
        public DbSet<ArtVariante> ArtVariantes { get; set; }
        public DbSet<ArtVeta> ArtVetas { get; set; }
        public DbSet<BasesJaladera> BasesJaladeras { get; set; }
        public DbSet<CapturasPago> CapturasPagos { get; set; }
        public DbSet<CatalogoCiudade> CatalogoCiudades { get; set; }
        public DbSet<CatalogoEstado> CatalogoEstados { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<EstatusCatalogo> EstatusCatalogos { get; set; }
        public DbSet<EstatusProceso> EstatusProcesos { get; set; }
        public DbSet<EstatusProyecto> EstatusProyectos { get; set; }
        public DbSet<Exclusividade> Exclusividades { get; set; }
        public DbSet<FavArticulo> FavArticulos { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Fletera> Fleteras { get; set; }
        public DbSet<LineasUsuario> LineasUsuarios { get; set; }
        public DbSet<MatrizFlete> MatrizFletes { get; set; }
        public DbSet<MatrizFletes2> MatrizFletes2 { get; set; }
        public DbSet<MaxMinColor> MaxMinColors { get; set; }
        public DbSet<MedidaRegla> MedidaReglas { get; set; }
        public DbSet<Noticia> Noticias { get; set; }
        public DbSet<Oportunidade> Oportunidades { get; set; }
        public DbSet<Parametro> Parametros { get; set; }
        public DbSet<PaseHist> PaseHists { get; set; }
        public DbSet<ProyArticulo> ProyArticulos { get; set; }
        public DbSet<ProyNota> ProyNotas { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UsersInRole> UsersInRoles { get; set; }
        public DbSet<vArticulosFavorito> vArticulosFavoritos { get; set; }
        public DbSet<vArticulosResuman> vArticulosResumen { get; set; }
        public DbSet<vExclusividade> vExclusividades { get; set; }
        public DbSet<vMatrizFletera> vMatrizFleteras { get; set; }
        public DbSet<vNotasProyecto> vNotasProyectoes { get; set; }
        public DbSet<vOportunidade> vOportunidades { get; set; }
        public DbSet<vProyectosAprobado> vProyectosAprobados { get; set; }
        public DbSet<vProyectosPorRevisar> vProyectosPorRevisars { get; set; }
        public DbSet<vProyectosRegistrado> vProyectosRegistrados { get; set; }
        public DbSet<vProyectosSolicitado> vProyectosSolicitados { get; set; }
        public DbSet<vUsuario> vUsuarios { get; set; }
        public DbSet<webpages_Membership> webpages_Membership { get; set; }
        public DbSet<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        public DbSet<vAcumuladosColor> vAcumuladosColor { get; set; }
        public DbSet<vMetraje> vMetraje { get; set; }
        public DbSet<vMetrajeEquivalencia> vMetrajeEquivalencia { get; set; }
        public DbSet<Multiplos> Multiplos { get; set; }
        public DbSet<MedidasFija> MedidasFijas { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<MaxMinAcabado> MaxMinAcabados { get; set; }
        public DbSet<MaxMinBaseColor> MaxMinBaseColors { get; set; }
        public DbSet<JaladeraColorExcepcione> JaladeraColorExcepciones { get; set; }
        public DbSet<JaladerasExtra> JaladerasExtras { get; set; }
        public DbSet<DetalleGrupos> DetalleGrupos { get; set; }
        public DbSet<Grupos> Grupos { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Vendedore> Vendedores { get; set; }
        public DbSet<webpages_Roles> webpages_Roles { get; set; }
        public DbSet<TiempoRama> TiempoRamas { get; set; }
    
        public virtual int spAltaCotizacion(Nullable<int> iProyecto, Nullable<int> iUsuarioAprueba, ObjectParameter bCorrecto)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            var iUsuarioApruebaParameter = iUsuarioAprueba.HasValue ?
                new ObjectParameter("iUsuarioAprueba", iUsuarioAprueba) :
                new ObjectParameter("iUsuarioAprueba", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spAltaCotizacion", iProyectoParameter, iUsuarioApruebaParameter, bCorrecto);
        }
    
        public virtual int spCalculaPreciosProyecto(Nullable<int> idProyecto)
        {
            var idProyectoParameter = idProyecto.HasValue ?
                new ObjectParameter("IdProyecto", idProyecto) :
                new ObjectParameter("IdProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spCalculaPreciosProyecto", idProyectoParameter);
        }
    
        public virtual int spClienteSolicitaProyecto(Nullable<int> iDUSUARIO, Nullable<int> iDPROYECTO)
        {
            var iDUSUARIOParameter = iDUSUARIO.HasValue ?
                new ObjectParameter("IDUSUARIO", iDUSUARIO) :
                new ObjectParameter("IDUSUARIO", typeof(int));
    
            var iDPROYECTOParameter = iDPROYECTO.HasValue ?
                new ObjectParameter("IDPROYECTO", iDPROYECTO) :
                new ObjectParameter("IDPROYECTO", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spClienteSolicitaProyecto", iDUSUARIOParameter, iDPROYECTOParameter);
        }
    
        public virtual ObjectResult<spClonaProyecto_Result> spClonaProyecto(Nullable<int> iProyecto, ObjectParameter iProyectoClonado)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spClonaProyecto_Result>("spClonaProyecto", iProyectoParameter, iProyectoClonado);
        }
    
        public virtual ObjectResult<Nullable<int>> spCreaFavoritodesdeProyecto(Nullable<int> iProyecto, string nvcNombre)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            var nvcNombreParameter = nvcNombre != null ?
                new ObjectParameter("nvcNombre", nvcNombre) :
                new ObjectParameter("nvcNombre", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("spCreaFavoritodesdeProyecto", iProyectoParameter, nvcNombreParameter);
        }
    
        public virtual ObjectResult<spCreaProyectodesdeFavorito_Result> spCreaProyectodesdeFavorito(Nullable<int> iFavorito, ObjectParameter iProyecto)
        {
            var iFavoritoParameter = iFavorito.HasValue ?
                new ObjectParameter("iFavorito", iFavorito) :
                new ObjectParameter("iFavorito", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spCreaProyectodesdeFavorito_Result>("spCreaProyectodesdeFavorito", iFavoritoParameter, iProyecto);
        }
    
        public virtual int spDatosEmbarquexProyecto(Nullable<int> iProyecto, ObjectParameter iPiezas, ObjectParameter iBultos, ObjectParameter mMonto, ObjectParameter dtFechaEnvio, ObjectParameter dtFechaEntrega, ObjectParameter dtFechaForma, ObjectParameter vcFormaEmbarque, ObjectParameter vcObservaciones, ObjectParameter vcNumGuia)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spDatosEmbarquexProyecto", iProyectoParameter, iPiezas, iBultos, mMonto, dtFechaEnvio, dtFechaEntrega, dtFechaForma, vcFormaEmbarque, vcObservaciones, vcNumGuia);
        }
    
        public virtual ObjectResult<Nullable<bool>> spEliminaCancelaProyecto(Nullable<int> iProyecto)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("spEliminaCancelaProyecto", iProyectoParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> spEliminaFavorito(Nullable<int> iFavorito)
        {
            var iFavoritoParameter = iFavorito.HasValue ?
                new ObjectParameter("iFavorito", iFavorito) :
                new ObjectParameter("iFavorito", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("spEliminaFavorito", iFavoritoParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> spEstatusProyectoxCliente(string cCliente)
        {
            var cClienteParameter = cCliente != null ?
                new ObjectParameter("cCliente", cCliente) :
                new ObjectParameter("cCliente", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("spEstatusProyectoxCliente", cClienteParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> spEstatusProyectoxProyecto(Nullable<int> iProyecto)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("spEstatusProyectoxProyecto", iProyectoParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> spEstatusTodosProyectos()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("spEstatusTodosProyectos");
        }
    
        public virtual int spExcepcionDescuentoHeCa(Nullable<int> iProyecto, ObjectParameter bAplicaDescPuertas)
        {
            var iProyectoParameter = iProyecto.HasValue ?
                new ObjectParameter("iProyecto", iProyecto) :
                new ObjectParameter("iProyecto", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spExcepcionDescuentoHeCa", iProyectoParameter, bAplicaDescPuertas);
        }
    
        public virtual int spGeneraSubCodigosArt(ObjectParameter bCorrecto)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("spGeneraSubCodigosArt", bCorrecto);
        }
    }
}

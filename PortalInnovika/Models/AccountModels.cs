using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace PortalInnovika.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    /*[Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ClienteERP { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string AgenteServicio { get; set; }
        public Nullable<bool> PuedeExpress { get; set; }
        public Nullable<int> FleteraId { get; set; }
        public Nullable<bool> FleteAutomatico { get; set; }
        public Nullable<System.DateTime> UltimaVistaDocumentos { get; set; }
        public Nullable<bool> ClienteMuebles { get; set; }
        public string ZonaImpuesto { get; set; }
        public string ListaPreciosEsp { get; set; }
        public Nullable<int> UserOld { get; set; }
        public bool Estatus { get; set; }
    }*/

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "La contraseña y contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Seguir conectado")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        //[Display(ClienteERP = "ClienteERP")]
        public string ClienteERP { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string AgenteServicio { get; set; }
        public Nullable<bool> PuedeExpress { get; set; }
        public int FleteraId { get; set; }
        public Nullable<bool> FleteAutomatico { get; set; }
        public Nullable<bool> ClienteMuebles { get; set; }
        public string ZonaImpuesto { get; set; }
        public string ListaPreciosEsp { get; set; }
        public int UserOld { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}

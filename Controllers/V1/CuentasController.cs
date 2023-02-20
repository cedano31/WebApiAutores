using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtection;

        public CuentasController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager,
            IDataProtectionProvider dataProtectionProvider,
            HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            dataProtection = dataProtectionProvider.CreateProtector("valor_secreto");
        }


        [HttpPost("registrar", Name = "registrarUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);

            }

            else
            {
                return BadRequest(resultado.Errors);
            }
        }


        [HttpPost("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }

            else
            {
                return BadRequest("Datos incorrectos");
            }

        }

        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(c => c.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }


        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email",credencialesUsuario.Email),
                new Claim("warever","xdxd")
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var ClaimsDB = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(ClaimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddYears(1);
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion,
            };


        }

        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "si"));
            return NoContent();

        }

        [HttpPost("RemoverAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "no"));
            return NoContent();

        }


        //[HttpGet("hash/{textoPlano}")]
        //public IActionResult RealizarHash(string textoPlano) {
        //    var resultado1 = hashService.Hash(textoPlano);
        //    var resultado2 = hashService.Hash(textoPlano);
        //    return Ok(new {
        //        textoPlano = textoPlano,
        //        resultado1 = resultado1,
        //        resultado2 = resultado2          
        //    });
        //}





        //[HttpGet("Encriptar")]
        //public ActionResult Encriptar() {

        //    var textoPlano = "Gabriel";
        //    var textoCifrado = dataProtection.Protect(textoPlano);
        //    var textoDesencriptado = dataProtection.Unprotect(textoCifrado);

        //    return Ok(new {
        //        textoPlano = textoPlano,
        //        textoCifrado = textoCifrado,
        //        textoDesencriptado = textoDesencriptado
        //    });

        //}

        //[HttpGet("EncriptarPorTiempo")]
        //public ActionResult EncriptarPorTiempo() {

        //    var EncriptacionPorLimiteDeTiempo = dataProtection.ToTimeLimitedDataProtector();

        //    var textoPlano = "Gabriel";
        //    var textoCifrado = EncriptacionPorLimiteDeTiempo.Protect(textoPlano,lifetime: TimeSpan.FromSeconds(3));
        //    var textoDesencriptado = EncriptacionPorLimiteDeTiempo.Unprotect(textoCifrado);

        //    return Ok(new {
        //        textoPlano = textoPlano,
        //        textoCifrado = textoCifrado,
        //        textoDesencriptado = textoDesencriptado
        //    });

        //}




    }
}

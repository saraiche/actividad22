using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using smvcfei.Data;
using smvcfei.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smvcfei.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly SignInManager<CustomIdentityUser> _signInManager;

        public CuentasController(IdentityContext context, UserManager<CustomIdentityUser> userManager, SignInManager<CustomIdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        //cuentas/login no necesita ystorizacion
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool signInResult = false;
                    //verifica correo y contraseña
                    var result = await _signInManager.PasswordSignInAsync(model.Correo, model.Password, isPersistent: false, lockoutOnFailure: false);
                    signInResult = result.Succeeded;
                    if (signInResult)
                    {
                        //usuario válido -> Home
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("Correo", "Credenciales no válidas. Inténtelo nuevamente.");
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        //aunque no este loggeado puede acceder al registro
        [AllowAnonymous]
        public async Task<IActionResult> Registro(bool creado = false)
        {
            ViewData["creado"] = creado;
            ViewData["RolesSelect"] = new SelectList(await _context.Roles.OrderBy(r => r.Name).AsNoTracking().ToListAsync(), "Name", "Name", null);
            return View();
        }
        
        //recibir los datos ingresados, validarlos y agregar el nuevo usuario
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroAsync(UsuarioViewModel model)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    //busca si el correo ya existe
                    var usuario = await _userManager.FindByEmailAsync(model.Correo);
                    if (usuario == null) 
                    {
                        //crear objeto para guardar
                        var usuarioToCreate = new CustomIdentityUser
                        {
                            UserName = model.Correo,
                            Email = model.Correo,
                            NormalizedEmail = model.Correo.ToUpper(),
                            Nombrecompleto = model.Nombre,
                            NormalizedUserName = model.Correo.ToUpper(),
                        };
                        //se crea el usuario con el pwd ingresado
                        IdentityResult result = await _userManager.CreateAsync(usuarioToCreate, model.Password);
                        //en caso de éxito, se regresa al formulario para crear otro usuario
                        if (result.Succeeded) 
                        {
                            await _userManager.AddToRoleAsync(usuarioToCreate, model.Rol);
                            return RedirectToAction("Registro", new {creado = true});
                        }

                        List<IdentityError> errorList = result.Errors.ToList();
                        var errors = string.Join(" ", errorList.Select(error => error.Description));
                        ModelState.AddModelError("Password", errors);
                    }
                    else
                    {
                        ModelState.AddModelError("Correo", $"El usuario {usuario.UserName} ya existe en el sistema");
                    }

                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["creado"] = false;
            return View();
        }
        public async Task<IActionResult> PerfilAsync()
        {
            //busca el correo
            var identityUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            var roles = _userManager.GetRolesAsync(identityUser).Result;
            var rol = string.Join(",", roles);

            UsuarioViewModel usuario = new()
            {
                Nombre = identityUser.Nombrecompleto,
                Correo = identityUser.Email,
                Rol = rol
            };
            return View(usuario);
        }
        public async Task<IActionResult> LogoutAsync(string returnUrl = null)
        {
            //Cierra la sesión
            await _signInManager.SignOutAsync();

            if(returnUrl != null) 
            {
                //si hay una acción a donde regresar, se realiza un redirect
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public IActionResult AccessDenied() 
        {
            //Vista con un mnesjae de acceso denegado
            return View();
        }
    }
}

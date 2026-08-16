using DevExtremeMvcApp1.Data;
using DevExtremeMvcApp1.Models;
using DevExtremeMvcApp1.Services;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace DevExtremeMvcApp1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly PasswordService passwordService = new PasswordService();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userName = (model.UserName ?? string.Empty).Trim();
            AppUser user = db.AppUsers.FirstOrDefault(x => x.UserName == userName);
            if (user == null || !passwordService.VerifyPassword(model.Password, user.PasswordSalt, user.PasswordHash))
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.UserName, model.RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "CalculationResults");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userName = (model.UserName ?? string.Empty).Trim();
            if (db.AppUsers.Any(x => x.UserName == userName))
            {
                ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten kullanılıyor.");
                return View(model);
            }

            string salt = passwordService.CreateSalt();
            var user = new AppUser
            {
                UserName = userName,
                PasswordSalt = salt,
                PasswordHash = passwordService.HashPassword(model.Password, salt),
                CreatedDate = DateTime.Now
            };

            db.AppUsers.Add(user);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.UserName, false);
            TempData["SuccessMessage"] = "Hesabınız oluşturuldu. Panele hoş geldiniz.";
            return RedirectToAction("Index", "CalculationResults");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "Çıkış yapıldı. Tekrar giriş yapabilirsiniz.";
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BattleCards.Services;
using SUS.HTTP;
using SUS.MvcFramework;

namespace BattleCards.Controllers
{
    public class UsersController : Controller
    {
        private UsersService userService;

        public UsersController()
        {
            this.userService = new UsersService();
        }

        public HttpResponse Login()
        {
            if (this.IsUserSignedIn())
            {
                return this.Redirect("/");
            }

            return this.View();
        }

        [HttpPost("/Users/Login")]
        public HttpResponse DoLogin()
        {
            if (this.IsUserSignedIn())
            {
                return this.Redirect("/");
            }

            var username = this.Request.FormData["username"];
            var password = this.Request.FormData["password"];
            var userId = this.userService.GetUserId(username, password);
            
            if (userId == null)
            {
                return this.Error("Invalid username or password!");
            }

            this.SignIn(userId);
            return this.Redirect("/Cards/All");
        }

        public HttpResponse Register()
        {
            if (this.IsUserSignedIn())
            {
                return this.Redirect("/");
            }

            return this.View();
        }

        [HttpPost("/Users/Register")]
        public HttpResponse DoRegister()
        {
            if (this.IsUserSignedIn())
            {
                return this.Redirect("/");
            }

            var username = this.Request.FormData["username"];
            var password = this.Request.FormData["password"];
            var confirmPassword = this.Request.FormData["confirmPassword"];
            var email = this.Request.FormData["email"];

            if (username == null ||username.Length < 5 || username.Length > 20)
            {
                return this.Error("Invalid username. The username should be between 5 and 20 characters.");
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9.]+$"))
            {
                return this.Error(
                    "Invalid username. The username should be contained of only alphanumeric characters.");
            }

            if (!this.userService.IsUsernameAvailable(username))
            {
                return this.Error("Username is not available!");
            }

            if (password == null || password.Length < 6 || password.Length > 20)
            {
                return this.Error("Invalid password! The password should be between 6 and 20 characters.");
            }

            if (password != confirmPassword)
            {
                return this.Error("Passwords should match!");
            }

            if (!this.userService.IsEmailAvailable(email))
            {
                return this.Error("Email is not available!");
            }

            if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
            {
                return this.Error("Invalid email");
            }

            this.userService.CreateUser(username, password, email);
            return this.Redirect("/Users/Login");
        }

        public HttpResponse Logout()
        {
            if (!this.IsUserSignedIn())
            {
                return this.Error("Only logged-in users can logout.");
            }

            this.SignOut();
            return this.Redirect("/");
        }
    }
}

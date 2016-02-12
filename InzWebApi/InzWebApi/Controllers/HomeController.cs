using InzWebApi.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InzApp;
using Newtonsoft.Json.Linq;

namespace InzWebApi.Controllers
{
    public class HomeController : Controller
    {
        private const string WebApiAddress = "http://inzwebapi.azurewebsites.net";
        private const string SecureWebApiAddress = "https://inzwebapi.azurewebsites.net";
        private const string Email = "inzwebapi@azurewebsites.net";
        private const string Password = "LenOVO20!%";

        public ActionResult Index()
        {
            ViewBag.Title = "Jedźmy Razem";

            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(string userId, string code)
        {
            code = HttpUtility.UrlEncode(code);
            ViewBag.Title = "Resetowanie hasła";
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPassword resetingPw)
        {
            // get bearer token for reset password post access
            var url = string.Format("{0}/token", SecureWebApiAddress);
            HttpClient client = new HttpClient();

            var xFormData = string.Format("grant_type=password&username={0}&password={1}", Email, Password);
            var content = new StringContent(xFormData, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = null;
            response = await client.PostAsync(new Uri(url), content);

            if (!response.IsSuccessStatusCode) return View(resetingPw);

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonToken = JObject.Parse(responseContent);
            var token = new AuthenticationToken
            {
                AccessToken = jsonToken["access_token"].ToString(),
                TokenType = jsonToken["token_type"].ToString(),
                ExpiresIn = Convert.ToInt64(jsonToken["expires_in"]),
                Expires = DateTime.UtcNow.AddSeconds(Convert.ToDouble(jsonToken["expires_in"])),
                UserId = jsonToken["userId"].ToString()
            };
            // post reset password
            resetingPw.Code = HttpUtility.UrlEncode(resetingPw.Code);
            url = $"{WebApiAddress}/api/account/resetpassword";
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

            var json = JsonConvert.SerializeObject(resetingPw);
            content = new StringContent(json, Encoding.UTF8, "application/json");
            
            response = await client.PostAsync(new Uri(url), content);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    ViewBag.response = response;
                    return RedirectToAction("ResetPasswordConfirmation", "Home");
                case HttpStatusCode.NotFound:
                    ViewBag.response = response;
                    return RedirectToAction("ResetPasswordConfirmation", "Home");
                case HttpStatusCode.BadRequest:
                    ViewBag.response = response;
                    return View(resetingPw);
                default:
                    ViewBag.response = response;
                    return View();
            }
            //return View(resetingPw);
        }

        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}

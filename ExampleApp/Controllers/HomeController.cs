using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExampleApp.Controllers {

    /// <summary>
    /// Default MVC controller.
    /// </summary>
    [RoutePrefix("")]
    public class HomeController : Controller {

        /// <summary>
        /// Displays the welcome page, or redirects to the data viewer if the user is already logged in.
        /// </summary>
        /// <returns>
        /// The welcome page (or a redirection to the data viewer if the user is already logged in).
        /// </returns>
        [HttpGet]
        [Route("")]
        public ActionResult Index() {
            var owinContext = Request.GetOwinContext();
            if (owinContext.Authentication.User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "DataViewer");
            }
            else {
                return RedirectToAction("Login", new { returnUrl = "/" });
            }
        }


        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after a successful login.</param>
        /// <returns>
        /// The login page.
        /// </returns>
        [HttpGet]
        [Route("account/login")]
        public ActionResult Login(string returnUrl = "/") {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        /// <summary>
        /// Initiates the login workflow.
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to after a successful login.</param>
        /// <returns>
        /// A redirection to the App Store login URL.
        /// </returns>
        [HttpPost]
        [Route("account/login")]
        public ActionResult LoginPost(string returnUrl = "/") {
            var loginUrl = Url.Content("~/auth/login");
            if (!String.IsNullOrWhiteSpace(returnUrl)) {
                loginUrl = loginUrl + "?ReturnUrl=" + Uri.EscapeDataString(returnUrl);
            }

            return Redirect(loginUrl);
        }


        /// <summary>
        /// Displays the session expired page.
        /// </summary>
        /// <returns>
        /// The session expired page.
        /// </returns>
        [HttpGet]
        [Route("account/sessionexpired")]
        public ActionResult SessionExpired() {
            return View();
        }

    }

}
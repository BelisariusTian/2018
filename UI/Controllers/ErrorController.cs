using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// 错误视图控制器
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        /// <summary>
        /// 错误页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            string ErrInfo = HttpContext.Application["Error"].ToString().Replace("\r\n", "<br />");
            ViewBag.ErrorInfo = ErrInfo;
            return View();
        }

    }
}

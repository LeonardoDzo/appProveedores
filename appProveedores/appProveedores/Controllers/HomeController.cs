using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace appProveedores.Controllers
{
    public class HomeController : Controller
    {
     
        public ActionResult Index()
        {
            if (User.IsInRole("Cliente"))
            {
                return RedirectToAction("Lista","Productos");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

    
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
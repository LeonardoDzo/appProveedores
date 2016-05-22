using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using appProveedores.Models;
using System.Data.Entity;

namespace appProveedores.Controllers
{
    [Authorize(Roles ="Cliente")]
    public class CompraController : Controller
    {
        private dbProveedoresEntities db = new dbProveedoresEntities();
        // GET: Compra
        public ActionResult Revisa()
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var productosPedido = db.ProductoPedido.Where(x => x.Pedido.idCliente == _idCliente && x.Pedido.estadoPedido == 1);                
            return View(productosPedido);
        }
        [Authorize(Roles = "Cliente")]
        [HttpPost, ActionName("Revisa")]
        public ActionResult Quitar(int? id)
        {
            
            var producto = db.ProductoPedido.Find(id);
            db.ProductoPedido.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Revisa");
        }
        public ActionResult _Carrito()
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var productosPedido = db.ProductoPedido.Where(x => x.Pedido.idCliente == _idCliente && x.Pedido.estadoPedido == 1);
            return PartialView(productosPedido);
        }
        public ActionResult _Pedido()
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var pedido = db.Pedido.Where(x => x.idCliente == _idCliente && x.estadoPedido == 1).First();
            return View(pedido);
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ActualizaPedido(string calle, string numE, string numI, string colonia, string estado, string ciudad, string codigo)
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var pedido = db.Pedido.Where(x => x.idCliente == _idCliente && x.estadoPedido == 1).First();
        
            pedido.calle = calle;
            pedido.numeroExterior = numE.ToString();
            pedido.numeroInterior = numI.ToString();
            pedido.colonia = colonia;
            pedido.ciudad = ciudad;
            pedido.estado = estado;
            pedido.codigoPostal = codigo;
            pedido.fechaPedido = DateTime.Now;
            db.Entry(pedido).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Pago");
        }
        public ActionResult Pago()
        {
            ViewBag.Nombre = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).First().Nombre;
            ViewBag.Apellidos = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).First().Apellidos;
            return View();
        }
    }
}
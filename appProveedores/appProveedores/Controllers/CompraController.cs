using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using appProveedores.Models;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;

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

            ViewBag.Nombre = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).First().Nombre;
            ViewBag.Apellidos = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).First().Apellidos;
            ViewBag.direccion = pedido.calle + " número exterior " + pedido.numeroExterior + " número interior " + pedido.numeroInterior +
                                 " " + pedido.colonia;
          
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
            return RedirectToAction("Pay");
        }
        public ActionResult Pay()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
            ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(Pagar pago)
        {
            if (VerificaPago(pago))
            {
                return View("Factura");
            }else
            {
                return View();
            }
        }

        public ActionResult Factura()
        {
            return View();
        }

        private bool VerificaPago(Pagar pago)
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _Pedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u).FirstOrDefault();
            var request = (HttpWebRequest)WebRequest.Create("http://189.170.117.153:8080/api/Transaction");
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _Pedido.idPedido).ToList();
            Pagar userPaymment = new Pagar()
            {
                Amount = pago.Amount,
                CardNumber = pago.CardNumber,
                ExpirationDate = pago.ExpirationDate,
                SecurityCode = pago.SecurityCode
            };

            var userData = new JavaScriptSerializer().Serialize(userPaymment);
            var data = Encoding.ASCII.GetBytes(userData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                var response = request.GetResponse();
                Stream datastream = response.GetResponseStream();
                StreamReader reader = new StreamReader(datastream);
                String responseFromServer = reader.ReadToEnd();
                Pago _pay = new Pago
                {
                    idPago = responseFromServer,
                    idPedido = _Pedido.idPedido,
                    montoPago = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad),
                    fechaPago = DateTime.Now

                };
                _Pedido.estadoPedido = 2;
                db.Entry(_Pedido).State = EntityState.Modified;
                db.Pago.Add(_pay);
                db.SaveChanges();
                actualizaProductos(_Pedido.idPedido);
                return true;
            }
            catch (System.Net.WebException ex)
            {
                return false;
            }
        }

        private void actualizaProductos(int id)
        {
            foreach(var item in db.ProductoPedido.Where(x=>x.id == id))
            {
                var product = db.Productos.Where(x => x.idProducto == item.idProducto).First();
                product.cantxUnidad -= item.cantidad;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
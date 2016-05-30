using System;
using System.Linq;
using System.Web.Mvc;
using appProveedores.Models;
using System.Data.Entity;
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
      
        public ActionResult Index()
        {

            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
            var productosPedido = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
            ViewBag.Total = productosPedido.Sum(x => x.Productos.precioUnidad * x.cantidad);
            if (productosPedido.Count == 0)
            {
                return RedirectToAction("Lista", "Productos", new { error = "Compra" });
            }         
            return View(productosPedido);
        }
        [HttpPost]
        public ActionResult Quitar(int? id)
        {
            if (id != null)
            {
                var producto = db.ProductoPedido.Find(id);
                if(producto!= null)
                {
                    db.ProductoPedido.Remove(producto);
                    db.SaveChanges();
                }
               

            }
            return RedirectToAction("Index");
        }

        public ActionResult direccion()
        {
            Dirección _direccion;
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _Pedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u).FirstOrDefault();
            if (_Pedido == null)
            {
                return HttpNotFound();
            }
            else
            {
                _direccion = new Dirección()
                {
                    calle = _Pedido.calle,
                    ciudad = _Pedido.ciudad,
                    codigo = _Pedido.codigoPostal,
                    colonia = _Pedido.colonia,
                    estado = _Pedido.estado,
                    numE = _Pedido.numeroExterior,
                    numI = _Pedido.numeroInterior
                };
            }
            return View(_direccion);
        }
        [HttpPost]
        public ActionResult direccion(Dirección model)
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var pedido = db.Pedido.Where(x => x.idCliente == _idCliente && x.estadoPedido == 1).First();

            if (actualizaPedido(pedido, model))
                return RedirectToAction("Pay");
            else
                return View("direccion", model);
        }

        public ActionResult Pay(bool error = false)
        {
            if (error) ViewBag.error = "si";
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
            ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
            if(ViewBag.Total == 0)
            {
              return RedirectToAction("Lista", "Productos");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(Pagar pago)
        {
         
            string res = VerificaPago(pago);
            if (res !=string.Empty)
            {

                return RedirectToAction("generaFactura", "Compra", new {idPago = res});
            }
            else
            {
                var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
                var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
                var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();

                ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
                if (ViewBag.Total == 0)
                {
                    return RedirectToAction("Lista", "Productos");
                }
                return RedirectToAction("Pay", "Compra", new { error = true});
            }
        }

        public ActionResult generaFactura(string idPago)
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.direccion = _idCliente.direccion;
            var pago = (from u in db.Pago where u.idPago == idPago select u).FirstOrDefault();
            if(pago != null)
            {
                ViewBag.pago = idPago;
                return View();
            }

            return RedirectToAction("Lista", "Productos");
           
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult generaFactura(Facturas factura, string idPago, string direccion ="")
        {
            
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            var existeFactura = db.Facturas.Where(x => x.idPago == idPago).FirstOrDefault();
            if (existeFactura == null)
            {
                if (direccion!="")
                {
                    _idCliente.direccion = direccion;
                    db.Entry(_idCliente).State = EntityState.Modified;
                }
                
                Facturas _factura = new Facturas()
                {
                    idPago = idPago,
                    razonSocial = factura.razonSocial,
                    RFC = factura.RFC,
                    fechaFacturacion = DateTime.Now.ToString()
                };
                db.Facturas.Add(_factura);
                db.SaveChanges();
                return RedirectToAction("Factura", "Compra", new { id= _factura.idFactura});
            }

            return RedirectToAction("Lista", "Productos");
        }

        public ActionResult Factura(int id=0)
        {
            if (id > 0)
            {
                ViewBag.fecha = db.Facturas.Find(id).fechaFacturacion;
                ViewBag.ID = db.Facturas.Find(id).idFactura;
                return View();
            }else
            {
                return RedirectToAction("Lista", "Productos");
            }
           
        }

        #region Vistas Parciales
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

        public ActionResult _datosFiscales(int idFactura)
        {
            ViewBag.direccion = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().direccion;
            var factura = db.Facturas.Find(idFactura);
            return PartialView(factura);
        }
        public ActionResult _ProductosComprados(int idFactura)
        {
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            var idPago = db.Facturas.Find(idFactura).idPago;
            var idPedido = db.Pago.Find(idPago).idPedido;

            var productos = (from u in db.ProductoPedido where u.idPedido == idPedido select u).ToList();
            ViewBag.TOTAL = productos.Sum(x => x.cantidad * x.Productos.precioUnidad);
            return PartialView(productos);
        }
        #endregion

        #region Metodos Internos
        private string VerificaPago(Pagar pago)
        {
            try
            {

                var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
                var _Pedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u).FirstOrDefault();
             
                var request = (HttpWebRequest)WebRequest.Create("http://189.170.144.90:8080/api/Transaction");
                var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _Pedido.idPedido).ToList();
                var total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
                Pagar userPaymment = new Pagar()
                {
                    Amount = total,
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
                _Pedido.fechaEntrega = DateTime.Now.AddDays(10);
                _Pedido.fechaEnvio = DateTime.Now;
                db.Entry(_Pedido).State = EntityState.Modified;
                db.Pago.Add(_pay);
                db.SaveChanges();
                actualizaProductos(_Pedido.idPedido);
                return _pay.idPago;
            }
            catch (System.Net.WebException ex)
            {
                return string.Empty;
            }
        }


        private void actualizaProductos(int id)
        {
            var productos = db.ProductoPedido.Where(x => x.idPedido == id).ToList();
            foreach (var item in productos)
            {
                var product = db.Productos.Where(x => x.idProducto == item.idProducto).First();
                product.cantxUnidad -= item.cantidad;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                product = null;
            }
        }

        private bool actualizaPedido(Pedido pedido, Dirección model)
        {
            try
            {
                pedido.calle = model.calle;
                pedido.numeroExterior = model.numE.ToString();
                pedido.numeroInterior = model.numI.ToString();
                pedido.colonia = model.colonia;
                pedido.ciudad = model.ciudad;
                pedido.estado = model.estado;
                pedido.codigoPostal = model.codigo;
                pedido.fechaPedido = DateTime.Now;
                
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch(Exception ex){
             
                return false;
            }
            
        }
        #endregion
    }
}
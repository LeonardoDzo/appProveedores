﻿using System;
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
            var _idCliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var productosPedido = db.ProductoPedido.Where(x => x.Pedido.idCliente == _idCliente && x.Pedido.estadoPedido == 1);                
            return View(productosPedido);
        }
        [HttpPost]
        public ActionResult Quitar(int? id)
        {
            var producto = db.ProductoPedido.Find(id);
            db.ProductoPedido.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Revisa");
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
        public ActionResult Pay(Pagar pago, string razon, string rfc)
        {
            string res = VerificaPago(pago);
            if (res !=string.Empty)
            {
                Facturas factura = new Facturas()
                {
                    RFC = rfc,
                    razonSocial = razon,
                    fechaFacturacion =  DateTime.Now.ToString(),
                    idPago = res.ToString(),
                    
                };
                db.Facturas.Add(factura);
                db.SaveChanges();
                return View("Factura");
            }else
            {
                var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
                var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
                var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
                ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
                return View("pay");
            }
        }

        public ActionResult Factura()
        {
            var facturas = db.Facturas.ToList();
            ViewBag.fecha = facturas.Last().fechaFacturacion;
            ViewBag.ID = facturas.Last().idFactura;
            return View();
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

            return PartialView(productos);
        }
        #endregion
        

        private string VerificaPago(Pagar pago)
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _Pedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u).FirstOrDefault();
            var request = (HttpWebRequest)WebRequest.Create("http://192.168.1.156:8081/api/Transaction");
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
               // actualizaProductos(_Pedido.idPedido);
                return _pay.idPago;
            }
            catch (System.Net.WebException ex)
            {
                return string.Empty;
            }
        }

        private void actualizaProductos(int id)
        {
            foreach(var item in db.ProductoPedido.Where(x=>x.idPedido == id))
            {
                var product = db.Productos.Where(x => x.idProducto == item.idProducto).First();
                product.cantxUnidad -= item.cantidad;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
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
    }
}
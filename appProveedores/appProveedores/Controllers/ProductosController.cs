using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using appProveedores.Models;

namespace appProveedores.Controllers
{ 
    public class ProductosController : Controller
    {
        private dbProveedoresEntities db = new dbProveedoresEntities();
        [Authorize(Roles ="Administrador")]
        // GET: Productos
        public ActionResult Index()
        {
            var productos = db.Productos.Include(p => p.Categorias);
            return View(productos.ToList());
        }
        [Authorize(Roles = "Cliente")]
        public ActionResult Lista()
        {
            var productos = db.Productos.Where(x => x.unidadExistencia == true).
                Include(p => p.Categorias).OrderBy(c => c.idCategoria);
            return View(productos);
        }
        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public ActionResult Seleccion(int id)
        {
            return View(db.Productos.Find(id));
        }
        [Authorize(Roles = "Cliente")]
        [HttpPost, ActionName("Seleccion")]
        public ActionResult SeleccionConfirmada(int id, int cantidad)
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var pedidos = db.Pedido;

            if (pedidos.Count() == 0 || pedidos.Count(x=> x.idCliente == idCte)==0)
            {
                Random random = new Random();
                var codigo = Convert.ToInt32(DateTime.Now.ToString("yyyyMMddHH")) + random.Next(10000000);
                Pedido pedido = new Pedido()
                {
                    idCliente = idCte,
                    fechaEntrega = DateTime.Now,
                    fechaEnvio = DateTime.Now,
                    fechaPedido = DateTime.Now,
                    codigoSeguridad = codigo.ToString(),
                    direccion = "",
                    estado = 1
                };
                db.Pedido.Add(pedido);
                db.SaveChanges();
                pedido = null;

            }

            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estado == 1 select u.idPedido).FirstOrDefault();

            ProductoPedido producto = new ProductoPedido()
            {
                idPedido = _idPedido,
                idProducto = id,
                cantidad = cantidad
            };
            db.ProductoPedido.Add(producto);
            db.SaveChanges();


            return RedirectToAction("Lista", "Productos");
        }
        [Authorize(Roles = "Cliente")]
        public ActionResult _Carrito()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estado == 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido);
            ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
            return PartialView(productosPedidos);
        }
        [Authorize(Roles = "Cliente")]
        [HttpPost, ActionName("_Carrito")]
        public ActionResult Quitar(int? id)
        {
            var producto = db.ProductoPedido.Find(id);
            db.ProductoPedido.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Lista", "Productos");
        }
        [Authorize(Roles = "Cliente")]
        public ActionResult Cotizacion()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estado == 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido);
            ViewBag.SubTotal = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad) ;
            ViewBag.IVA = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad) * .16;
            ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad)*1.16;

            var cotizacion = new Cotización()
            {
                //idPedido = _idPedido,
                fechaCotización = DateTime.Now
            };
            db.Cotización.Add(cotizacion);
            db.SaveChanges();
            return View(productosPedidos);
        }
        //public ActionResult buscarCotizacion(int id)
        //{

        //    var idCte = db.Cotización.Find(id).Pedido.idCliente;
        //    var _idPedido = db.Cotización.Find(id).idPedido;
        //    var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido);
        //    ViewBag.SubTotal = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
        //    ViewBag.IVA = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad) * .16;
        //    ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad) * 1.16;

        //    return View(productosPedidos);
        //}
        [Authorize(Roles = "Cliente")]
        public ActionResult _InfoCliente()
        {
            var cliente = db.AspNetUsers.Where(x => x.UserName == User.Identity.Name).First();
            return PartialView(cliente);
        }
        [Authorize(Roles = "Administrador")]
        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }
        [Authorize(Roles = "Administrador")]
        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.idCategoria = new SelectList(db.Categorias, "idCategoria", "nombreCategoria");
            return View();
        }
        [Authorize(Roles = "Administrador")]
        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "idProducto,idCategoria,nombre,cantxUnidad,precioUnidad,unidadExistencia")] Productos productos)
        {
            if (ModelState.IsValid)
            {
                db.Productos.Add(productos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.idCategoria = new SelectList(db.Categorias, "idCategoria", "nombreCategoria", productos.idCategoria);
            return View(productos);
        }
        [Authorize(Roles = "Administrador")]
        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            ViewBag.idCategoria = new SelectList(db.Categorias, "idCategoria", "nombreCategoria", productos.idCategoria);
            return View(productos);
        }
        [Authorize(Roles = "Administrador")]
        // POST: Productos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "idProducto,idCategoria,nombre,cantxUnidad,precioUnidad,unidadExistencia")] Productos productos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.idCategoria = new SelectList(db.Categorias, "idCategoria", "nombreCategoria", productos.idCategoria);
            return View(productos);
        }
        [Authorize(Roles = "Administrador")]
        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = db.Productos.Find(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }
        [Authorize(Roles = "Administrador")]
        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Productos productos = db.Productos.Find(id);
            db.Productos.Remove(productos);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Administrador")]
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

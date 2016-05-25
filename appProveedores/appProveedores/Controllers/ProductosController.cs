using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using appProveedores.Models;
using SelectPdf;

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
        public ActionResult Seleccion(int id, bool error = false)
        {
            if (error)
            {
                ViewBag.error = "si";
            }
            return View(db.Productos.Find(id));
        }
        [Authorize(Roles = "Cliente")]
        [HttpPost, ActionName("Seleccion")]
        public ActionResult SeleccionConfirmada(int id, int cantidad)
        {
            if (guardaProducto(id, cantidad))
                return RedirectToAction("Lista", "Productos");
            else
                return RedirectToAction("Seleccion", "Productos", new { id = id, error = true });

        }

        private bool guardaProducto(int id, int cantidad)
        {

            if(!verificaCantidad(id, cantidad))
            {
                return false;
            }

            var _idPedido= verificaPedido();
            var producto = (from u in db.ProductoPedido where u.idPedido == _idPedido && u.idProducto == id select u).FirstOrDefault();
            var _Producto = db.Productos.Find(id);
            
            if (producto != null)
            {
                producto.cantidad += cantidad;
                db.Entry(producto).State = EntityState.Modified;
            }
            else
            {
                ProductoPedido product = new ProductoPedido()
                {
                    idPedido = _idPedido,
                    idProducto = id,
                    cantidad = cantidad
                };
                db.ProductoPedido.Add(product);
            }
            db.SaveChanges();
            return true;
        }
        private bool verificaCantidad(int id,  int cantidad)
        {
            var _Producto = db.Productos.Find(id);
            if (cantidad <= _Producto.cantxUnidad)
                return true;
            else
                return false;
            
        }
        private int verificaPedido()
        {
            Pedido pedidos;
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            if (db.Pedido.Count() > 0)
            {
                pedidos = db.Pedido.Where(x => x.idCliente == idCte && x.estadoPedido == 1).FirstOrDefault();
            }
            else
            {
                pedidos = null;
            }

            if (pedidos == null)
            {
                Pedido pedido = new Pedido()
                {
                    idCliente = idCte,
                    fechaPedido = DateTime.Now,
                    estadoPedido = 1,
                    fechaEntrega = DateTime.Now,
                    fechaEnvio = DateTime.Now
                };
                db.Pedido.Add(pedido);
                db.SaveChanges();
                pedido = null;
                return pedido.idPedido;
            }

            return pedidos.idPedido;
        }
        public ActionResult _Carrito()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido== 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
            ViewBag.Total = productosPedidos.Sum(x => x.Productos.precioUnidad * x.cantidad);
            return PartialView(productosPedidos);
        }
        [Authorize(Roles = "Cliente")]
        [HttpPost, ActionName("_Carrito")]
        public ActionResult Quitar(int? id)
        {
            if (id != null)
            {
                var producto = db.ProductoPedido.Find(id);
                db.ProductoPedido.Remove(producto);
                db.SaveChanges();

            }
     
            return RedirectToAction("Lista", "Productos");
        }
        [Authorize(Roles = "Cliente")]
        public async System.Threading.Tasks.Task<ActionResult> Cotizacion()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var cotizacion = new Cotización()
            {
                idCliente = idCte,
                fechaCotización = DateTime.Now
            };
            db.Cotización.Add(cotizacion);
            await db.SaveChangesAsync();

            var _idCotizacion = (from u in db.Cotización where u.idCliente == idCte select u.idCotización).ToList().LastOrDefault();
            var _idPedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 1 select u.idPedido).FirstOrDefault();
            var productosPedidos = db.ProductoPedido.Where(x => x.idPedido == _idPedido).ToList();
            foreach (var item in productosPedidos)
            {
                ProductoCotizacion producto = new ProductoCotizacion()
                {
                    idCotizacion = _idCotizacion,
                    idProducto = item.idProducto,
                    cantidad = item.cantidad
                };
                db.ProductoCotizacion.Add(producto);
                await db.SaveChangesAsync();
                producto = null;
            }
           
            var productosCotizacion = db.ProductoCotizacion.Where(x => x.idCotizacion == _idCotizacion);
            ViewBag.SubTotal = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad);
            ViewBag.IVA = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad) * .16;
            ViewBag.Total = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad) * 1.16;


            return View(db.ProductoPedido.Where(x=>x.idPedido == _idPedido));
        }

        public void GenerarPDF()
        {
            HtmlToPdf converter = new HtmlToPdf();

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertUrl("http://localhost:15392/Productos/Cotizacion");

            // save pdf document
            doc.Save( "Sample.pdf");

            // close pdf document
            doc.Close();
        }
      
        public ActionResult buscarCotizacion(int id)
        {

            var idCte = db.Cotización.Find(id).idCliente;
            var _idPedido = db.Cotización.Find(id).idCotización;
            var productosCotizacion = db.ProductoCotizacion.Where(x => x.idCotizacion == _idPedido);
            ViewBag.SubTotal = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad);
            ViewBag.IVA = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad) * .16;
            ViewBag.Total = productosCotizacion.Sum(x => x.Productos.precioUnidad * x.cantidad) * 1.16;

            return View(productosCotizacion);
        }
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

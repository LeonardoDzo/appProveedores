using System;
using System.Linq;
using System.Web.Mvc;
using appProveedores.Models;
using System.Data.Entity;

namespace appProveedores.Controllers
{
    [Authorize(Roles ="Cliente")]
    public class PedidoController : Controller
    {
        dbProveedoresEntities db = new dbProveedoresEntities();
        // GET: Pedido
        public ActionResult Index()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            actualizaPedidos();
            var pedidos = db.Pedido.Where(x => x.idCliente == idCte && x.estadoPedido >1).OrderByDescending(x=> x.fechaEntrega).Include(p=>p.Pago).ToList();
            return View(pedidos);
        }
        [HttpPost]
        public ActionResult ConfirmarPedido(int idPed, int calificacion=1)
        {
            var _Pedido = db.Pedido.Find(idPed);
            _Pedido.estadoPedido = 3;
            _Pedido.fechaEntrega = DateTime.Now;
            _Pedido.calificacion = calificacion;
            db.Entry(_Pedido).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        private void actualizaPedidos()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            var _Pedido = (from u in db.Pedido where u.idCliente == idCte && u.estadoPedido == 2 select u).ToList();
            foreach(var pedido in _Pedido)
            {
                if(DateTime.Now >= pedido.fechaEntrega)
                {
                    pedido.estadoPedido = 3;
                    db.Entry(pedido).State = EntityState.Modified;
                }
            }
        }

        public ActionResult Detalles(int? id)
        {
            if(id != null)
            {
                var pedido = db.ProductoPedido.Where(x=>x.idPedido== id).ToList();
                ViewBag.Total = pedido.Sum(x => x.Productos.cantxUnidad * x.cantidad);
                return View(pedido);
            }else
            {
                return View("Index");
            }
           
        }
    }
}
using System;
using System.Linq;
using System.Web.Mvc;
using appProveedores.Models;
using System.Data.Entity;

namespace appProveedores.Controllers
{
    public class PedidoController : Controller
    {
        dbProveedoresEntities db = new dbProveedoresEntities();
        // GET: Pedido
        public ActionResult Index()
        {
            var idCte = (from c in db.AspNetUsers where c.UserName == User.Identity.Name select c.Id).First();
            actualizaPedidos();
            var pedidos = db.Pedido.Where(x => x.idCliente == idCte && x.estadoPedido >1);
            return View();
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

        public ActionResult Detalles(int? idPedido)
        {
            if(idPedido != null)
            {
                var pedido = db.ProductoPedido.Where(x=>x.idPedido== idPedido).ToList();
                return View(pedido);
            }else
            {
                return View("Index");
            }
           
        }
    }
}
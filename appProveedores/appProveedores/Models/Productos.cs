//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace appProveedores.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public partial class Productos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Productos()
        {
            this.ProductoCotizacion = new HashSet<ProductoCotizacion>();
            this.ProductoPedido = new HashSet<ProductoPedido>();
        }
    
        public int idProducto { get; set; }
        public int idCategoria { get; set; }
        [Required]
        [Display(Name = "Producto")]
        [DataType(DataType.Text)]
        public string nombre { get; set; }
        [Display(Name = "Catnidad")]
        public int cantxUnidad { get; set; }
        [Display(Name = "Precio")]
        [DataType(DataType.Currency)]
        public float precioUnidad { get; set; }
        [Display(Name = "Existencia")]
        public bool unidadExistencia { get; set; }
    
        public virtual Categorias Categorias { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductoCotizacion> ProductoCotizacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductoPedido> ProductoPedido { get; set; }
    }
}

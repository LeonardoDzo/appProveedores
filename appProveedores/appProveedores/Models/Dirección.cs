using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace appProveedores.Models
{
    public class Dirección
    {
        [Required]
        [Display(Name = "Calle")]
        public string calle { get; set; }
        [Required]
        [Display(Name = "Número Exterior")]
        public string numE { get; set; }
        [Required]
        [Display(Name = "Número Interior")]
        public string numI { get; set; }
        [Required]
        [Display(Name = "Colonia")]
        public string colonia { get; set; }
        [Required]
        [Display(Name = "Estado")]
        public string estado { get; set; }
        [Required]
        [Display(Name = "Ciudad")]
        public string ciudad { get; set; }
        [Required]
        [Display(Name = "Código Postal")]
        [DataType(DataType.PostalCode)]
        public string codigo { get; set; }
    }
}
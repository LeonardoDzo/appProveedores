using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace appProveedores.Models
{
    public class Pagar {

        [Required]
        [Display(Name = "Monto")]
        [DataType(DataType.Currency)]
        public double Amount { get; set; }
        [Required]
        [Display(Name = "Numero de tarjeta")]
        [DataType(DataType.CreditCard)]
        public string CardNumber { get; set; }
        [Required]
        [Display(Name = "Fecha de Expiración")]
        public string ExpirationDate { get; set; }
        [Required]
        [Display(Name = "Código de seguridad")]
        public int SecurityCode {get; set;}

        public string Token = "6465fae5-e445-476b-b3d0-39cd1ddbf510";

    }
}
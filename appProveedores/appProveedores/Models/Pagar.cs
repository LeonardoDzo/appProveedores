using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace appProveedores.Models
{
    public class Pagar { 


        public double Amount { get; set; }
        [Display(Name = "Numero de tarjeta")]
        [DataType(DataType.CreditCard)]
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        [Display(Name = "Código de seguridad")]
        public int SecurityCode {get; set;}

        public string Token = "F0E9D75D-6AA8-404C-B25D-9BD6B242D359";

    }
}
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;

namespace APIBAnco.Models
{
    public class CuentasBancaria
    {
        [Key]
        public int IdCuenta { get; set; }
        [Required]
        public string NumeroCuenta { get; set; } = string.Empty;
        [Required]
        public string IdCliente { get; set; } = string.Empty;
        [Required]
        public string TipoCuenta { get; set; } = string.Empty;
        [Required]
        public string Moneda { get; set; } = string.Empty;
 
        [Range( 0, double.MaxValue)]
        public decimal Saldo  { get; set; }

        public string Estado { get; set; } = "Activa";
        
    }
}

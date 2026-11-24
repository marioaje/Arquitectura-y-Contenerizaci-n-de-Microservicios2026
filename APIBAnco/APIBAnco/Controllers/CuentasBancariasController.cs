using APIBAnco.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace APIBAnco.Controllers
{

    //Las opciones para crear mi api
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasBancariasController : ControllerBase
    {
        //Almacenamiento temporal
        private static readonly object bloqueo = new();

        private static readonly List<CuentasBancaria> CuentaBancarias = new List<CuentasBancaria>()
        {
            new CuentasBancaria
            {
              IdCuenta = 2,
              NumeroCuenta = "CR0012",
              IdCliente = "123404040",
              TipoCuenta="Ahorro",
              Moneda="USD",
              Saldo=100000,
              Estado = "Activa"
             },
                        new CuentasBancaria
            {
              IdCuenta = 1,
              NumeroCuenta = "CR0018",
              IdCliente = "18884040",
              TipoCuenta="Corriente",
              Moneda="USD",
              Saldo=1000000,
              Estado = "Activa"
            }

        };


        //Get
        [HttpGet]
        public IActionResult Consultar()
        {
            //Proteger datos
            lock (bloqueo)
            {
                return Ok(CuentaBancarias);
            }
        }


        //Get
        [HttpGet("{id:int}")]
        public IActionResult ConsultarPorId(int _IdCuenta)
        {
            //Proteger datos
            lock (bloqueo)
            {
                var CuentasBancaria = CuentaBancarias.FirstOrDefault(c => c.IdCuenta == _IdCuenta);

                if (CuentasBancaria == null)
                    return NotFound("CuentasBancaria no se encontro");


                return Ok(CuentasBancaria);
            }
        }

        //Post
        [HttpPost]
        public IActionResult Crear([FromBody] CuentasBancaria datos)
        {
            if (string.IsNullOrWhiteSpace(datos.NumeroCuenta) || string.IsNullOrWhiteSpace(datos.TipoCuenta) ||
                 string.IsNullOrWhiteSpace(datos.Moneda))
            {
                return BadRequest("NumeroCuenta, Moneda, TipoCuenta son obligatorios");
            }


            lock (bloqueo)
            {
                //Verificar
                if (CuentaBancarias.Any(c => c.IdCuenta == datos.IdCuenta))
                {
                    return Conflict("Ya existe esa identificacion");
                }

                datos.IdCuenta = CuentaBancarias.Count == 0 ? 1 : CuentaBancarias.Max(c => c.IdCuenta) + 1;
                datos.Estado = "Activa";


                CuentaBancarias.Add(datos);


                return CreatedAtAction(
                    nameof(ConsultarPorId),
                    new { id = datos.IdCuenta }, datos);
            }


        }



        //Put
        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int _IdCuenta, [FromBody] CuentasBancaria datos)
        {

            if (string.IsNullOrWhiteSpace(datos.NumeroCuenta) || string.IsNullOrWhiteSpace(datos.Estado) ||
                string.IsNullOrWhiteSpace(datos.Moneda))
            {
                return BadRequest("Identificacion, nombre, apellido son obligatorios");
            }

            lock (bloqueo)
            {
                //Verificar
                var CuentasBancaria = CuentaBancarias.FirstOrDefault(c => c.IdCuenta == _IdCuenta);
                if (CuentasBancaria == null)
                {
                    return NotFound("CuentasBancaria no encontrado");
                }

                ///Actualizar con el DAO
                CuentasBancaria.Saldo = datos.Saldo;

                CuentasBancaria.NumeroCuenta = datos.NumeroCuenta;
                CuentasBancaria.Moneda = datos.Moneda;
                CuentasBancaria.Estado = datos.Estado;
                CuentasBancaria.Saldo = datos.Saldo;
                CuentasBancaria.TipoCuenta = datos.TipoCuenta;
                

                return NoContent();


            }

        }


        //Delete
        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int IdCuenta)
        {
            lock (bloqueo)
            {
                //Verificar
                var CuentasBancaria = CuentaBancarias.FirstOrDefault(c => c.IdCuenta == IdCuenta);
                if (CuentasBancaria == null)
                {
                    return NotFound("CuentasBancaria no encontrado");
                }

                CuentasBancaria.Estado = "Inactiva";
                return NoContent();

            }
          }



    }
}

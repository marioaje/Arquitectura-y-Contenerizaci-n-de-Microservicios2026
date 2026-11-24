using APIBAnco.Models;
using Microsoft.AspNetCore.Mvc;

namespace APIBAnco.Controllers
{

    //Las opciones para crear mi api
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        //Almacenamiento temporal
        private static readonly object bloqueo = new();

        private static readonly List<Cliente> clientes = new List<Cliente>()
        {

            new Cliente
            {
                Id=1,
                Identificacion="123404040",
                Nombre = "Mario",
                ApellidoPaterno = "Jimenez",
                ApellidoMaterno = "Espinoza",
                Telefono = "6161611",
                Correo= "mjimeenz@gmail.com",
                Direccion="La oficina de mi casa",
                Activo = true
            },
                        new Cliente
            {
                Id=1,
                Identificacion="18884040",
                Nombre = "Isaac",
                ApellidoPaterno = "Jimenez",
                ApellidoMaterno = "Espinoza",
                Telefono = "6161611",
                Correo= "mjimeenz@gmail.com",
                Direccion="La oficina de mi casa",
                Activo = true
            }

        };


        //Get
        [HttpGet]
        public IActionResult Consultar()
        {
            //Proteger datos
            lock (bloqueo)
            {
                return Ok(clientes);
            }
        }
    }
}

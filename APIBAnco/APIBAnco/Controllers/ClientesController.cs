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


        //Get
        [HttpGet("{id:int}")]
        public IActionResult ConsultarPorId(int id)
        {
            //Proteger datos
            lock (bloqueo)
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == id);

                if (cliente == null)
                    return NotFound("Cliente no se encontro");


                return Ok(cliente);
            }
        }

        //Post
        [HttpPost]
        public IActionResult Crear([FromBody] Cliente datos)
        {
            if ( string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Identificacion) ||
                 string.IsNullOrWhiteSpace(datos.ApellidoPaterno) )
            {
                return BadRequest("Identificacion, nombre, apellido son obligatorios");
            }


            lock (bloqueo)
            {
                //Verificar
                if(clientes.Any(c=> c.Identificacion == datos.Identificacion))
                {
                    return Conflict("Ya existe esa identificacion");
                }
            }

            datos.Id = clientes.Count == 0 ? 1 : clientes.Max(c => c.Id) + 1;
            datos.Activo = true;


            clientes.Add(datos);


            return CreatedAtAction(
                nameof(ConsultarPorId),
                new { id = datos.Id }, datos);
        }

    }
}

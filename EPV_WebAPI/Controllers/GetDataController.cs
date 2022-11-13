using Microsoft.AspNetCore.Mvc;

namespace EPV_WebAPI.Controllers
{
    public class GetDataController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("getClientes")]
        public async Task<ActionResult> GetClientes([FromForm] string token)
        {
            try
            {
                Clases.Log.LogWrite($"GetClientes: token={token}");
                // Validar token
                Clases.Login log = new Clases.Login();
                if (!log.ValidarTokenUsuario(token)) return BadRequest("Token caducado o incorrecto");

                // Ejecutar acción
                string jsonResultado = await Clases.AccesoDatos.JsonDataReader("SELECT * FROM dbo.Clientes");
                return Content(jsonResultado, "application/json");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}

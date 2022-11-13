using Microsoft.AspNetCore.Mvc;

namespace EPV_WebAPI.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }    

        [HttpPost("getTokenLogin")]
        public ActionResult GetTokenLogin([FromForm] string email, [FromForm] string password)
        {
            Clases.Log.LogWrite($"GetTokenLogin: email={email}, password={password}");
            Clases.Login log = new Clases.Login();
            return Ok(log.getTokenLogin(email, password));
        }

        [HttpPost("loginByToken")]
        public ActionResult LoginByToken([FromForm] string loginToken)
        {
            Clases.Log.LogWrite($"LoginByToken: loginToken={loginToken}");
            Clases.Login log = new Clases.Login();
            string token = log.LoginByToken(loginToken);

            switch (token)
            {
                case "-1": return BadRequest("Límite de tiempo excedido");
                case "-2": return BadRequest("Usuario o clave incorrectos");
                case "-3": return BadRequest("No se pudo hacer el login, revise los datos enviados");
                default: return Ok(token);
            }
        }


        [HttpPost("setPassword")]
        public ActionResult SetPassword([FromForm] string token, [FromForm] string encriptedOldPassword, [FromForm] string encriptedNewPassword)
        {
            Clases.Log.LogWrite($"SetPassword: token={token}, encriptedOldPassword={encriptedOldPassword}, encriptedNewPassword={encriptedNewPassword}");
            Clases.Login log = new Clases.Login();
            bool resultado = log.SetPassword(token, encriptedOldPassword, encriptedNewPassword);
            if (resultado)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }

        [HttpPost("logout")]
        public ActionResult logout([FromForm] string token)
        {
            Clases.Log.LogWrite($"Logout");
            return Ok("");
        }


    }
}

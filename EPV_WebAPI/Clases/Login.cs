using System.Data;
using System.Data.SqlClient;

namespace EPV_WebAPI.Clases
{
    public class Login
    {
        public bool userInsert(string email, string password)
        {
            Encriptacion encrip = new Encriptacion();

            List<SqlParameter> SqlParams = new List<SqlParameter>();
            SqlParameter p = new SqlParameter();

            p = new SqlParameter(); p.ParameterName = "Email"; p.SqlDbType = System.Data.SqlDbType.VarChar; p.SqlValue = email; SqlParams.Add(p);
            p = new SqlParameter(); p.ParameterName = "Password"; p.SqlDbType = System.Data.SqlDbType.VarChar; p.SqlValue = password; SqlParams.Add(p);
            AccesoDatos.ExecuteStoredProcedure("dbo.spUsuariosInsert", SqlParams.ToArray());

            return true;
        }

        public string getTokenLogin(string email, string password)
        {
            Encriptacion encrip = new Encriptacion();
            string fecha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string tokenLogin = encrip.AES256_Encriptar(encrip.AES256_LOGIN_Key, fecha + '#' + email + '#' + encrip.GetSHA256(password));
            return tokenLogin;
        }

        public string LoginByToken(string loginToken)
        {
            try
            {
                Encriptacion encrip = new Encriptacion();
                string tokenUsuario = "";

                string tokenDescoficado = encrip.AES256_Desencriptar(encrip.AES256_LOGIN_Key, loginToken);
                string fecha = tokenDescoficado.Split('#')[0];
                string email = tokenDescoficado.Split('#')[1];
                string password = tokenDescoficado.Split('#')[2];

                // Validar fecha
                DateTime fechaLogin = DateTime.ParseExact(fecha, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                if (DateTime.UtcNow.Subtract(fechaLogin).TotalSeconds >= 30)
                {
                    return "-1";    // -1 = Límite de tiempo excedido
                }

                // Validar login
                string SQL = $"SELECT * FROM dbo.USuarios WHERE email='{email}' AND password=0x{password}";

                DataTable dt = AccesoDatos.GetTmpDataTable(SQL);
                if (dt.Rows.Count > 0)
                {
                    tokenUsuario = dt.Rows[0]["email"].ToString() + "#" + DateTime.UtcNow.AddHours(18).ToString("yyyyMMddHHmmss");        // Email # FechaCaducidad -> Encriptar con AES
                    tokenUsuario = encrip.AES256_Encriptar(encrip.AES256_USER_Key, tokenUsuario);
                    return tokenUsuario;
                }
                else
                {
                    return "-2";    // -2 = Usuario o clave incorrectas
                }
            }
            catch (Exception)
            {
                return "-3";        // -3 = Error
            }
        }

        public bool SetPassword(string token, string encriptedOldPassword, string encriptedNewPassword)
        {
            try
            {
                if (!ValidarTokenUsuario(token))
                {
                    return false;
                }
                string emailUsuario = this.GetEmailUsuarioFromToken(token);

                Encriptacion encrip = new Encriptacion();
                string oldPassword = encrip.AES256_Desencriptar(encrip.AES256_LOGIN_Key, encriptedOldPassword);
                string newPassword = encrip.AES256_Desencriptar(encrip.AES256_LOGIN_Key, encriptedNewPassword);


                List<SqlParameter> SqlParams = new List<SqlParameter>();
                SqlParameter p = new SqlParameter();

                p = new SqlParameter(); p.ParameterName = "Email"; p.SqlDbType = System.Data.SqlDbType.VarChar; p.SqlValue = emailUsuario; SqlParams.Add(p);
                p = new SqlParameter(); p.ParameterName = "OldPassword"; p.SqlDbType = System.Data.SqlDbType.VarChar; p.SqlValue = oldPassword; SqlParams.Add(p);
                p = new SqlParameter(); p.ParameterName = "NewPassword"; p.SqlDbType = System.Data.SqlDbType.VarChar; p.SqlValue = newPassword; SqlParams.Add(p);

                DataTable dt = AccesoDatos.ExecuteStoredProcedure("dbo.spUsuariosSetPassword", SqlParams.ToArray());
                // Obtener el resultado del SP
                if (dt.Rows[0][0].ToString() == "1")
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetEmailUsuarioFromToken(string token)
        {
            Encriptacion encrip = new Encriptacion();
            token = encrip.CorregirToken(token);
            string tokenDescodificado = encrip.AES256_Desencriptar(encrip.AES256_LOGIN_Key, token);
            string emailUsuario = tokenDescodificado.Split('#')[0];
            return emailUsuario;
        }

        public bool ValidarTokenUsuario(string tokenUsuario)
        {
            try
            {
                Encriptacion encrip = new Encriptacion();
                tokenUsuario = encrip.CorregirToken(tokenUsuario);
                string tokenDescodificado = encrip.AES256_Desencriptar(encrip.AES256_USER_Key, tokenUsuario);
                string emailUsuario = tokenDescodificado.Split('#')[0];
                string fecha = tokenDescodificado.Split('#')[1];

                // Validar fecha
                DateTime fechaCaducidad = DateTime.ParseExact(fecha, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                if (DateTime.UtcNow > fechaCaducidad)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



    }
}

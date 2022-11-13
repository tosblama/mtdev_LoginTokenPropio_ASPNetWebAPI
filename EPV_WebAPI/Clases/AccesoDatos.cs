using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace EPV_WebAPI.Clases
{
    public class AccesoDatos
    {

        // Cadena de conexión a la BBDD SQL
        public static string cadenaConexion = "";

        #region " Acceso a datos "

        /// <summary>
        /// Ejecuta una consulta SQL en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parametros">Array de string con formato: nombre:valor</param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecución de la consulta.</returns>
        public static DataTable GetDataTable(string SQL, string[] parametros)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand(SQL, conexion);
                for (int i = 0; i < parametros.Length; i++)
                    comando.Parameters.Add(new SqlParameter(parametros[i].Split(':')[0], parametros[i].Split(':')[1]));
                SqlDataAdapter da = new SqlDataAdapter(comando);
                DataSet ds = new DataSet();
                da.Fill(ds);
                conexion.Close();
                da.Dispose();
                conexion.Dispose();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una consulta SQL en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// Este método es vulnerable a inyección de dependencias, por lo que debe usarse sólamente de forma interna.
        /// Para consultas que vengan desde fuera, usar GetDataTable.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parametros">Array de string con formato: nombre:valor</param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecución de la consulta.</returns>
        public static DataTable GetTmpDataTable(string SQL)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand(SQL, conexion);
                SqlDataAdapter da = new SqlDataAdapter(comando);
                DataSet ds = new DataSet();
                da.Fill(ds);
                conexion.Close();
                da.Dispose();
                conexion.Dispose();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado en la base de datos, y devuelve los resultados obtenidos en un objeto DataTable.
        /// </summary>
        /// <param name="procedimientoAlmacenado"></param>
        /// <param name="parametros"></param>
        /// <returns>Devuelve un objeto DataTable con los resultados obtenidos tras ejecutar el procedimiento almacenado.</returns>
        public static DataTable ExecuteStoredProcedure(string procedimientoAlmacenado, SqlParameter[] parametros)
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                SqlCommand comando = new SqlCommand();
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandText = procedimientoAlmacenado;
                comando.Connection = conexion;
                if (parametros != null)
                {
                    for (int i = 0; i < parametros.Length; i++)
                    {
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].Value != null) { parametros[i].Value = parametros[i].Value.ToString().Replace(" ", "T"); }
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].SqlValue != null) { parametros[i].SqlValue = parametros[i].SqlValue.ToString().Replace(" ", "T"); }
                        comando.Parameters.Add(parametros[i]);
                    }
                }
                DataTable dt = new DataTable();
                conexion.Open();
                dt.Load(comando.ExecuteReader());
                conexion.Close();
                comando.Dispose(); conexion.Dispose();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecutar un comando SQL en la base de datos, sin devolución de resultados.
        /// </summary>
        /// <param name="SQL"></param>
        public static void ExecuteQuery(string SQL)
        {
            SqlConnection con = new SqlConnection(cadenaConexion);
            SqlCommand cmd = new SqlCommand(SQL, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        #endregion


        #region " Funciones para convertir DataTables/DataReaders a JSON "

        /// <summary>
        /// Datatable a JSON
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string DataTableToJSON(DataTable tabla)
        {
            var JSONString = new StringBuilder();
            if (tabla.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < tabla.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < tabla.Columns.Count; j++)
                    {
                        if (tabla.Columns[j].DataType == System.Type.GetType("System.DateTime"))
                        {
                            if (tabla.Rows[i][j] is DBNull)
                                JSONString.Append("\"" + tabla.Columns[j].ColumnName.ToString() + "\":" + "\"" + DBNull.Value + "\"");
                            else
                                JSONString.Append("\"" + tabla.Columns[j].ColumnName.ToString() + "\":" + "\"" + Convert.ToDateTime(tabla.Rows[i][j]).ToString("yyyy-MM-dd HH:mm:ss") + "\"");
                        }
                        else if (tabla.Columns[j].DataType == System.Type.GetType("System.String"))
                        {
                            JSONString.Append("\"" + tabla.Columns[j].ColumnName.ToString() + "\":" + "\"" + DataTableToJson_CorreccionesJSONString(tabla.Rows[i][j].ToString()) + "\"");
                        }
                        else
                        {
                            JSONString.Append("\"" + tabla.Columns[j].ColumnName.ToString() + "\":" + "\"" + tabla.Rows[i][j].ToString() + "\"");
                        }

                        if (j < tabla.Columns.Count - 1) { JSONString.Append(","); }
                    }
                    JSONString.Append("}");
                    if (i < tabla.Rows.Count - 1) { JSONString.Append(","); }
                }
                JSONString.Append("]");
            }
            else
            {
                JSONString.Append("[]");
            }

            // Correcciones de caracters que podrian romper el JSON
            JSONString = JSONString.Replace("\\", "\\\\");          // para que las rutas UNC no fallen en cliente
            return JSONString.ToString();
        }

        private static string DataTableToJson_CorreccionesJSONString(string json)
        {
            json = json.Replace("\"", "'");
            json = json.Replace("\t", " ");
            json = json.Replace("\r", "");
            json = json.Replace("\n", "");
            return json;
        }


        /// <summary>
        /// DataReader a JSON
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static string DataReaderToJson(SqlDataReader dr)
        {
            var dt = new DataTable();
            dt.Load(dr);
            return DataTableToJSON(dt);
        }

        /// <summary>
        /// Stored Procedure to JSON
        /// </summary>
        /// <param name="procedimientoAlmacenado"></param>
        /// <param name="parametros"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static string StoredProcedureToJson(string procedimientoAlmacenado, SqlParameter[] parametros, SqlDataReader dr)
        {
            var json = new StringBuilder();

            int numParametrosEntrada = 0;
            int numParametrosSalida = 0;

            // Inicio
            json.Append("[{");
            json.Append("\"SP\": \"" + procedimientoAlmacenado + "\"");
            json.Append(",");
            // Parametros de entrada
            json.Append("\"ParametrosEntrada\":{");
            for (int i = 0; i < parametros.Length; i++)
            {
                if (parametros[i].Direction == ParameterDirection.Input)
                {
                    if (numParametrosEntrada > 0) json.Append(",");
                    json.Append("\"" + parametros[i].ParameterName + "\":\"" + parametros[i].Value + "\"");
                    numParametrosEntrada += 1;
                }
            }
            if (numParametrosEntrada == 0) { json.Append("\"Sin parámetros de entrada\": \"Fin\""); }
            json.Append("},");
            // Parametros de salida
            json.Append("\"ParametrosSalida\":{");
            for (int i = 0; i < parametros.Length; i++)
            {
                if (parametros[i].Direction == ParameterDirection.Output)
                {
                    if (numParametrosSalida > 0) json.Append(",");
                    json.Append("\"" + parametros[i].ParameterName + "\":\"" + parametros[i].Value + "\"");
                    numParametrosSalida += 1;
                }
            }
            if (numParametrosSalida == 0) { json.Append("\"Sin parámetros de salida\": \"Fin\""); }
            json.Append("},");
            // Data
            json.Append("\"Data\":" + DataReaderToJson(dr));
            // Fin
            json.Append("}]");

            json = json.Replace("\\", "\\\\");  // 15/11/2019, para que las rutas UNC no fallen en cliente
            return json.ToString();
        }

        #endregion


        #region " Acceso a datos con resultados en formato JSON "

        /// <summary>
        /// Ejecutar una Query y devolver los resultados en formato JSON
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public static async Task<string> JsonDataReader(string SQL)
        {
            string json = "";
            using (SqlConnection con = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(SQL, con);
                await con.OpenAsync();
                SqlDataReader dr = await cmd.ExecuteReaderAsync();
                json = DataReaderToJson(dr);
                dr.Close();
            }
            return json;
        }


        /// <summary>
        /// Ejecutar un Stored Procedure y devolver los resultados en formato JSON
        /// </summary>
        /// <param name="procedimientoAlmacenado"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public static async Task<string> JsonStoredProcedure(string procedimientoAlmacenado, SqlParameter[] parametros)
        {
            string json = "";
            using (SqlConnection con = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedimientoAlmacenado;
                cmd.Connection = con;
                if (parametros != null)
                {
                    for (int i = 0; i < parametros.Length; i++)
                    {
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].Value != null) { parametros[i].Value = parametros[i].Value.ToString().Replace(" ", "T"); }
                        if (parametros[i].DbType == DbType.DateTime && parametros[i].SqlValue != null) { parametros[i].SqlValue = parametros[i].SqlValue.ToString().Replace(" ", "T"); }
                        cmd.Parameters.Add(parametros[i]);
                    }
                }
                await con.OpenAsync();
                SqlDataReader dr = await cmd.ExecuteReaderAsync();
                json = AccesoDatos.StoredProcedureToJson(procedimientoAlmacenado, parametros, dr);
                dr.Close();
            }
            return json;
        }
        #endregion


    }
}

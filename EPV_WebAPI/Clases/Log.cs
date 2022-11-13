namespace EPV_WebAPI.Clases
{
    public class Log
    {
        public static void LogWrite(string texto)
        {
            try
            {
                StreamWriter sw;
                string logPath = $"Logs/{DateTime.Now.ToString("yyyy-MM-dd")}-log.txt";
                sw = (File.Exists(logPath)) ? File.AppendText(logPath) : File.CreateText(logPath);
                sw.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {texto}");
                sw.Close();
            }
            catch (Exception)
            {
            }
        }

    }
}

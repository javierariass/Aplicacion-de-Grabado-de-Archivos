using System;
using System.IO;

namespace AppForm
{
    internal static class BaseDatos
    {
        private const string NombreArchivo = "facturas.db";

        // La carpeta del ejecutable puede ser de solo lectura (por ejemplo en Program Files),
        // por eso la base de datos se guarda en %LocalAppData%, donde el usuario siempre puede escribir.
        public static string ObtenerRuta()
        {
            string carpetaBD = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AplicacionDeGrabacion",
                "Base de Datos");
            Directory.CreateDirectory(carpetaBD);

            string rutaDB = Path.Combine(carpetaBD, NombreArchivo);
            if (!File.Exists(rutaDB))
            {
                MigrarBaseDatosAntigua(rutaDB);
            }

            QuitarSoloLectura(rutaDB);
            return rutaDB;
        }

        public static string CadenaConexion() => $"Data Source={ObtenerRuta()}";

        // Copia la base de datos de la ubicacion anterior (junto al ejecutable) para no perder facturas.
        private static void MigrarBaseDatosAntigua(string rutaNueva)
        {
            try
            {
                string rutaAntigua = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Base de Datos", NombreArchivo);
                if (File.Exists(rutaAntigua))
                {
                    File.Copy(rutaAntigua, rutaNueva);
                }
            }
            catch
            {
                // Si no se puede copiar, se crea una base de datos nueva.
            }
        }

        private static void QuitarSoloLectura(string ruta)
        {
            try
            {
                if (File.Exists(ruta))
                {
                    var atributos = File.GetAttributes(ruta);
                    if ((atributos & FileAttributes.ReadOnly) != 0)
                    {
                        File.SetAttributes(ruta, atributos & ~FileAttributes.ReadOnly);
                    }
                }
            }
            catch
            {
            }
        }
    }
}

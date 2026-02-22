using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AppForm;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        try
        {
            ConfigurarAutoInicio();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al iniciar la aplicación:\n" + ex.Message,
                            "Error crítico",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
    }

    private static void ConfigurarAutoInicio()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key == null)
                {
                    return;
                }

                string appPath = Application.ExecutablePath;
                string appName = "AplicacionDeGrabacion";
                key.SetValue(appName, $"\"{appPath}\"");
            }
        }
        catch
        {
            // No interrumpir el inicio si falla el registro.
        }
    }
}
using Abdal_Security_Group_App.Core;
using System.Reflection;

namespace Abdal_Security_Group_App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
              string abdal_app_name_for_url = Assembly.GetExecutingAssembly().GetName().ToString().Split(',')[0]
     .ToLower().Replace(' ', '-');

            if (!InstanceChecker.IsSingleInstance(abdal_app_name_for_url))
                return;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main());

            InstanceChecker.Release();
        }
    }
}
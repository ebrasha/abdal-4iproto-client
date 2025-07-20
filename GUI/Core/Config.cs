// -------------------------------------------------------------------
// Programmer       : Ebrahim Shafiei (EbraSha)
// Email            : Prof.Shafiei@Gmail.com
// -------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Abdal_Security_Group_App.Core
{
    internal class Config
    {
        public string ssh_host { get; set; } = "";
        public int ssh_port { get; set; } = 0;
        public string ssh_user { get; set; } = "";
        public string ssh_password { get; set; } = "";
        public int socks5_port { get; set; } = 0;
        public string auto_reconnect { get; set; } = "yes";
        public int auto_reconnect_timeout { get; set; } = 2000;

        private static readonly string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static void CreateIfNotExists(string sshHost = "", string sshPort = "", string sshUser = "", string sshPassword = "", string socks5Port = "")
        {
            if (File.Exists(configFilePath))
                return;

            var config = new Config
            {
                ssh_host = "0.0.0.0",
                ssh_port = int.TryParse(sshPort, out var p1) ? p1 : 22,
                ssh_user = sshUser ?? "empty",
                ssh_password = sshPassword ?? "empty",
                socks5_port = int.TryParse(socks5Port, out var p2) ? p2 : 52905,
                auto_reconnect = "yes",
                auto_reconnect_timeout = 2000
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(configFilePath, json);
        }

        public static void Save(string sshHost, string sshPort, string sshUser, string sshPassword, string socks5Port, string autoReconnect = "yes", int timeout = 2000)
        {
            var config = new Config
            {
                ssh_host = sshHost ?? "",
                ssh_port = int.TryParse(sshPort, out var p1) ? p1 : 22,
                ssh_user = sshUser ?? "",
                ssh_password = sshPassword ?? "",
                socks5_port = int.TryParse(socks5Port, out var p2) ? p2 : 52905,
                auto_reconnect = autoReconnect,
                auto_reconnect_timeout = timeout
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, jsonOptions);
            File.WriteAllText(configFilePath, json);
        }

        public static Dictionary<string, string> LoadAsDictionary()
        {
            if (!File.Exists(configFilePath))
                return null;

            try
            {
                string json = File.ReadAllText(configFilePath);
                using JsonDocument doc = JsonDocument.Parse(json);
                var dict = new Dictionary<string, string>();

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                        dict[prop.Name] = prop.Value.GetString();
                    else
                        dict[prop.Name] = prop.Value.ToString(); // works for numbers like int
                }

                return dict;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading config: " + ex.Message);
                return null;
            }
        }

        // Function to ensure 'domains.txt' exists beside the executable
        public static void EnsureDomainFileExists()
        {
            try
            {
                // Get the directory of the running executable
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Full path to domains.txt
                string filePath = Path.Combine(basePath, "domains.txt");

                // If the file does not exist, create it empty
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "");
                }
            }
            catch
            {
                // Silent fail, no output or exception thrown
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abdal_Security_Group_App.Core
{
    internal class VersionHelper
    {
        /*
         * 
           GetCurrentVersionParts("major+minor");     // 1.2
           GetCurrentVersionParts("major+minor+build"); // 1.2.3
           GetCurrentVersionParts("build+revision");  //3.4
           GetCurrentVersionParts("full"); // 1.2.3.4
         */
        public static string GetCurrentVersionParts(string parts)
        {
            // Get the full version object from assembly
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Split input string by '+' and prepare output
            string[] requestedParts = parts.ToLower().Split('+');
            string result = "";

            foreach (string part in requestedParts)
            {
                switch (part.Trim())
                {
                    case "major":
                        result += version.Major.ToString();
                        break;
                    case "minor":
                        result += "." + version.Minor.ToString();
                        break;
                    case "build":
                        result += "." + version.Build.ToString();
                        break;
                    case "revision":
                        result += "." + version.Revision.ToString();
                        break;
                    case "full":
                        return version.ToString();
                    default:
                        throw new ArgumentException("Invalid version part specified: " + part);
                }
            }

            // Clean up possible leading dot
            return result.TrimStart('.');
        }
    }
}

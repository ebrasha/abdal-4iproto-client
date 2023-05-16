﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASBL;
using Abdal_Proxy_Bridge;

namespace Abdal_Proxy_Bridge
{
    internal class PTerminator
    {
         

        public static void ProcessKiller(string ProcessName)
        {
            foreach (var process in Process.GetProcessesByName(ProcessName))
            {
                process.Kill(true);
            }
        }

        public static void ProcessStarter(string ProcessFA,string ProcessWd,bool HiddenWindow)
        {
           
            if (HiddenWindow)
            {
                // start process
                Process process = new Process();
                process.StartInfo.FileName = ProcessFA;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WorkingDirectory = ProcessWd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
            }
            else
            {
                // start process
                Process process = new Process();
                process.StartInfo.FileName = ProcessFA;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WorkingDirectory = ProcessWd;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                process.Start();
            }
           
            
        }
        public static bool ProcessExists(string ProcessName)
        {
            bool processExists = false;
            Process[] _proceses = null;
            _proceses = Process.GetProcessesByName(ProcessName);
            foreach (Process proces in _proceses)
            {
                if (proces.ProcessName == "Abdal Socks Bridge")
                {
                     processExists = true;
                }
            }
            return processExists;
        }
    }
}
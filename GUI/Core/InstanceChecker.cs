using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abdal_Security_Group_App.Core
{
    public static class InstanceChecker
    {
        private static Mutex mutex;

        public static bool IsSingleInstance(string mutexName)
        {
            bool createdNew;

            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("The application is already running!",
                                "Instance Already Running",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        public static void Release()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }
    }
}

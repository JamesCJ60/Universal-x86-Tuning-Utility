using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Scripts.Misc
{
    internal class ToastNotification
    {
        public static void ShowToastNotification(string title = "", string body = "")
        {
            string iconUri = "";
            iconUri = "pack://application:,,,/Assets/applicationIcon-1024.png";

            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            path = path.Replace("Universal x86 Tuning Utility.dll", null);
            iconUri = path + "Assets\\icon.png";

            new ToastContentBuilder()
               .AddText(title)
               .AddText(body)
               .AddAppLogoOverride(new Uri(iconUri))
               .Show();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using PoGoMITM.Launcher.ViewModels;

namespace PoGoMITM.Launcher
{

    public class NotificationHub : Hub
    {
        public void SendMessage(string message)
        {
            Clients.All.sendMessage(message);
        }

        public static IHubContext HubContext => GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

        public static void Send(string message)
        {
            HubContext.Clients.All.sendMessage(message);
        }

        public static void SendRawContext(RawContextListItemViewModel vm)
        {
            HubContext.Clients.All.rc(vm);
        }
    }

}

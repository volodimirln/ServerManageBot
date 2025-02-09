using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace HotlineManageBot.Modules.Networks
{
    public class WOLService
    {
       /* public void WakeUpAllDevices()
        {
            Networking networking = new Networking();
            networking.Ping_all();
            for (int i = 0; i < networking.ListIPAddress.Count; i++)
            {
                if (i != 0)
                {
                    if (!networking.ListIPAddress[i].macaddres.Contains(networking.ListIPAddress[i - 1].macaddres))
                    {
                        SendWakeOnLan(PhysicalAddress.Parse(networking.ListIPAddress[i].macaddres), 9);
                    }
                }
                else
                {
                    SendWakeOnLan(PhysicalAddress.Parse(networking.ListIPAddress[i].macaddres), 9);
                }
            }
        }*/
        public void SendWakeOnLan(PhysicalAddress target, int potr)
        {
  
                var header = Enumerable.Repeat(byte.MaxValue, 6);
                var data = Enumerable.Repeat(target.GetAddressBytes(), 16).SelectMany(mac => mac);

                var magicPacket = header.Concat(data).ToArray();

                using var client = new UdpClient();
            for (int i = 0; i < 10;i++)
            {
                client.Send(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Parse("192.168.88.255"), potr));
            } 
        }
    }
}

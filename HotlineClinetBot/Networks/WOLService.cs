using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace HotlineManageBot.Modules.Networks
{
    public class WOLService
    {
        public void SendWakeOnLan(PhysicalAddress target, int port, string address)
        {
  
            var header = Enumerable.Repeat(byte.MaxValue, 6);
            var data = Enumerable.Repeat(target.GetAddressBytes(), 16).SelectMany(mac => mac);

            var magicPacket = header.Concat(data).ToArray();

            using var client = new UdpClient();
            for (int i = 0; i < 10;i++)
            {
                client.Send(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Parse(address), port));
            } 
        }
    }
}

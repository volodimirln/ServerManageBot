using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;


namespace HotlineManageBot.Modules.Networks
{

    public class Networking
    {
        public void PingAll()
        {
            string gate_ip = NetworkGateway();
            string[] array = gate_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
                Ping(ping_var, 4, 4000);
            }
        }

        public List<HostMashine> ListIPAddress = new List<HostMashine>();
        public string IPAddress = "";
        public Networking() 
        { 
            PingAll();

            for (int i = 0; i < ListIPAddress.Count; i++)
            {
                if (i != 0)
                {
                    if (!ListIPAddress[i].macaddres.Contains(ListIPAddress[i - 1].macaddres))
                    {
                        IPAddress += ListIPAddress[i].ip + " " + ListIPAddress[i].macaddres + " " + ListIPAddress[i].hostname;
                    }
                }
                else
                {
                    IPAddress += ListIPAddress[i].ip + " " + ListIPAddress[i].macaddres + " " + ListIPAddress[i].hostname;
                }
            }
        }
        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                ListIPAddress.Add(new HostMashine() { hostname = hostname, ip = ip, macaddres = macaddres});

            }
        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        
                    }
                }).Start();
            }
        }
        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }
        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
            }

            return null;
        }

        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "Сама машина";
            }
        }
        
    }
}

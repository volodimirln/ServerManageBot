using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Xml;

namespace HotlineManageBot.Modules.Scripts
{
    public class PowerShellHM
    {
        public void RunScript(string scriptText)
        {
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.AddScript(scriptText);
                foreach (var result in powerShell.Invoke())
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}

using HotlineManageBot.Modules.Auntification;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotlineManageBot.Modules.Data
{
    public class FileChatsId
    {
        public static bool ChatidIsExists(long chatId, string path)
        {
            bool result = false;
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    string txt = System.IO.File.ReadAllText(path);

                    List<DataSet> datasets = new List<DataSet>();
                    datasets = JsonConvert.DeserializeObject<List<DataSet>>(txt);

                    if (datasets == null)
                    {
                        return false;
                    }
                    else
                    {
                        if (datasets.Where(p => p.Id == chatId).Count() > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex) { return result; }
            }
        public static bool Add(string key, long chatId, string path)
        {
            bool result = false;
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                {
                    System.IO.File.Create(path);
                    System.IO.File.Exists(path);

                }
                Console.WriteLine(System.IO.File.ReadAllText(path));
                string name = new AuthOptions().GetName(key);
                List<DataSet> datasets = new List<DataSet>();
                string txt = System.IO.File.ReadAllText(path);
                datasets = JsonConvert.DeserializeObject<List<DataSet>>(txt);
                if (datasets == null)
                {
                    datasets = new List<DataSet>();
                }
                if (datasets.Where(p => p.Id == chatId).Count() == 0)
                {
                    datasets.Add(new DataSet() { Id = chatId });
                    result = true;
                }
                else
                {
                    result = false;
                }
                string updat = JsonConvert.SerializeObject(datasets);
                Console.WriteLine(updat);
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(datasets));
                return result;
            }
            catch (Exception ex) 
            {
                return false;
            }
        }
    }
}

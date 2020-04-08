using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Data
{
    public class Utility
    {
        /// <summary>
        /// 反射获取所有实体类名称
        /// </summary>
        /// <returns></returns>

        public static List<string> GetModelNames()
        {
            string AssemblyName = "Edu.Entity";
            List<string> list = new List<string>();
            foreach (Type item in Assembly.Load(AssemblyName).GetTypes())
            {
                list.Add(item.Name);
            }
            return list;
        }
        /// <summary>
        /// 获取字段名称
        /// </summary>
        /// <param name="ModelName"></param>
        /// <returns></returns>
        public static List<string> GetFeildName(string ModelName)
        {
            string AssemblyName = "Edu.Entity";
            List<string> list = new List<string>();
            foreach (Type item in Assembly.Load(AssemblyName).GetTypes())
            {
                if (item.Name == ModelName)
                {

                    foreach (var info in item.GetProperties())
                    {
                        list.Add(info.Name);
                    }
                }
            }
            return list;
        }
    
    }
}

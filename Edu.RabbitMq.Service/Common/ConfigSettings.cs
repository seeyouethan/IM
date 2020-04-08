using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.RabbitMq.Service.Common
{
    public class ConfigSettings
    {
        public static string ISMSConnectionString { get; set; }

        static ConfigSettings()
        {
            ISMSConnectionString = ConfigurationManager.ConnectionStrings["EduContext"].ConnectionString;
        }
    }
}

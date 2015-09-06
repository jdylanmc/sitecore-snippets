using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWebsite.Configuration
{
	// this class can pull custom sitecore configuration from App_Config/Include
    public static class Configuration
    {
        public static string ApiEndpoint
        {
            get { return Sitecore.Configuration.Settings.GetSetting("MyWebsite.ApiEndpoint"); }
        }

        public static string EnvironmentSetting
        {
            get { return Sitecore.Configuration.Settings.GetSetting("MyWebsite.EnvironmentSetting"); }
        }
    }
}

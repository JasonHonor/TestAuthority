using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestAuthorityCore
{
    public class ConfigService
    {
        IConfiguration mConfiguration;
        public ConfigService()
        {
            mConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Directory where the json files are located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public string GetString(string key)
        {
            return mConfiguration.GetValue<string>(key);
        }

        public int GetInt(string key)
        {
            return mConfiguration.GetValue<int>(key);
        }

        public string GetConnectionString(string name)
        {
            return mConfiguration.GetConnectionString(name);
        }
    }
}

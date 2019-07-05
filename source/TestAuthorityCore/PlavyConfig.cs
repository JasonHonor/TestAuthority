using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;

namespace Plavy.Core.Common
{
    public class PlavyConfig
    {
        public static string GetConnectionString(string name)
        {
            //读取appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Directory where the json files are located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return configuration.GetConnectionString(name);
        }

        public static IConfigurationSection GetSection(string name)
        {
            //读取appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Directory where the json files are located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            return configuration.GetSection(name);
        }

        public static string GetString(string key)
        {
            //读取appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Directory where the json files are located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return configuration.GetValue<string>(key);
        }
    }
}

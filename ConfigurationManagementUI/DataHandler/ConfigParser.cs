using System;
using System.Collections.Generic;
using System.IO;

namespace ConfigurationManagementUI.DataHandler
{
    public class ConfigParser
    {
        // dictionary use for key-value pairs
        public Dictionary<string, Dictionary<string, string>> ParseConfigFile(string filePath)
        {
            var config = new Dictionary<string, Dictionary<string, string>>();
            string currentServer = "DEFAULT";

            foreach (string line in File.ReadLines(filePath))
            {
                if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("SERVER_NAME"))
                {
                    currentServer = line.Split('=')[1].Trim();
                    config[currentServer] = new Dictionary<string, string>();
                }
                else
                {
                    var parts = line.Split('=');
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    config[currentServer][key] = value;
                }
            }

            return config;
        }
    }
}

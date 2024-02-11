using ConfigurationManagementUI.DataHandler;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationManagementUI.Controllers
{
    public class ConfigController : Controller
    {
        private readonly ConfigParser _configParser;
        private readonly string _configFilePath = "Config.txt";

        public ConfigController(ConfigParser configParser)
        {
            _configParser = configParser;
        }

        public IActionResult Index()
        {
            var config = _configParser.ParseConfigFile(_configFilePath);          
            return View(config);
        }


        [HttpPost]
        public IActionResult AddServer([FromBody] ConfigServerViewModel server)
        {
            try
            {
                // Append the new server section to the config file
                System.IO.File.AppendAllText(_configFilePath, $"{Environment.NewLine}SERVER_NAME={server.ServerName}");
                System.IO.File.AppendAllText(_configFilePath, $"{Environment.NewLine};END {server.ServerName}{Environment.NewLine}");
                // Optionally, you can also add some default settings for the new server here if needed.

                return Ok();
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, $"Error adding server: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult AddSetting([FromBody] ConfigViewModel model)
        {
            try
            {
                // Read all lines from the file
                var lines = System.IO.File.ReadAllLines(_configFilePath).ToList();

                // Find the starting and ending indexes of the server section
                int startIndex = -1;
                int endIndex = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith($"SERVER_NAME=") && line.Substring($"SERVER_NAME=".Length) == model.ServerName)
                    {
                        startIndex = i;
                    }
                    else if (startIndex != -1 && (line.StartsWith(";START") || line.StartsWith(";END")))
                    {
                        endIndex = i;
                        break;
                    }
                }

                if (startIndex != -1 && endIndex != -1)
                {
                    // Insert the new setting before the ending of the server section
                    lines.Insert(endIndex, $"{model.Key}={model.Value}");

                    // Write the updated lines back to the file
                    System.IO.File.WriteAllLines(_configFilePath, lines);

                    return Ok();
                }
                else
                {
                    // Server section not found, handle error
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, $"Error adding setting: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult UpdateSetting([FromBody] ConfigViewModel model)
        {
            try
            {
                // Read all lines from the file
                var lines = System.IO.File.ReadAllLines(_configFilePath).ToList();

                // Find the line that needs to be updated
                //var startIndex = lines.FindIndex(line =>
                //    line.StartsWith($";Contains {model.ServerName}"));

                int startIndex = -1;
                foreach (var line in lines)
                {
                    if (line.StartsWith($"SERVER_NAME=") && line.Substring($"SERVER_NAME=".Length) == model.ServerName)
                    {
                        startIndex = lines.IndexOf(line);
                        break;
                    }
                }

                int endIndex = -1;
                if (startIndex != -1)
                {
                    for (int i = startIndex + 1; i < lines.Count; i++)
                    {
                        var line = lines[i];
                        if (line.StartsWith($"SERVER_NAME=") || line.StartsWith(";END"))
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }


                if (startIndex != -1 && endIndex != -1)
                {
                    var index = lines.FindIndex(startIndex, endIndex - startIndex, line =>
                        line.StartsWith($"{model.Key}="));

                    if (index != -1)
                    {
                        // Update the line
                        lines[index] = $"{model.Key}={model.Value}";

                        // Write the updated lines back to the file
                        System.IO.File.WriteAllLines(_configFilePath, lines);
                    }
                    else
                    {
                        // Key not found within the server section, handle error
                        return NotFound();
                    }
                }
                else
                {
                    // Server section not found, handle error
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, $"Error updating setting: {ex.Message}");
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteSetting([FromBody] ConfigViewModel model)
        {
            try
            {
                // Read all lines from the file
                var lines = System.IO.File.ReadAllLines(_configFilePath).ToList();

                // Find the starting index of the server section
                int startIndex = -1;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith($"SERVER_NAME=") && lines[i].Substring($"SERVER_NAME=".Length) == model.ServerName)
                    {
                        startIndex = i;
                        break;
                    }
                }

                // Find the ending index of the server section
                int endIndex = -1;
                for (int i = startIndex + 1; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith(";START") || lines[i].StartsWith(";END"))
                    {
                        endIndex = i;
                        break;
                    }
                }

                // Find the line that needs to be deleted within the server section
                int index = -1;
                if (startIndex != -1 && endIndex != -1)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (lines[i].StartsWith($"{model.Key}="))
                        {
                            index = i;
                            break;
                        }
                    }
                }

                if (index != -1)
                {
                    // Remove the line
                    lines.RemoveAt(index);

                    // Write the updated lines back to the file
                    System.IO.File.WriteAllLines(_configFilePath, lines);
                }
                else
                {
                    // Key not found within the server section, handle error
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, $"Error deleting setting: {ex.Message}");
            }

            return Ok();
        }
    }

    public class ConfigViewModel
    {
        public string ServerName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ConfigServerViewModel
    {
        public string ServerName { get; set; }
    }


}

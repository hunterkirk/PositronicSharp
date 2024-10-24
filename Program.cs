using System;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Tommy;


class Program
{
    static async Task Main(string[] args)
    {

        using (StreamReader reader = File.OpenText("../../../prompts.toml"))
        {
            TomlTable table = TOML.Parse(reader);

            foreach (TomlNode node in table["prompts"])
            {
                Console.WriteLine(node["prompt"].ToString());
                using var client = new HttpClient();
                var url = "http://localhost:11434/api/generate";
                var data = new StringContent("{\"model\":\"phi3.5\",\"prompt\":\"" + node["prompt"].ToString() + "\", \"stream\":false}", Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(url, data);

                    if (response.IsSuccessStatusCode)
                    {


                        var responseBody = await response.Content.ReadAsStringAsync();
                        string messageField = ExtractJsonField(responseBody, "response");
                        string code = ExtractCodeFromBackticks(messageField);

                        Console.WriteLine(code);
                        string uuidFileName = Guid.NewGuid().ToString();
                        string file_ext = node["file_extension"].ToString();
                        WriteToFile("output/", uuidFileName, code, file_ext);

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught: " + ex.Message);
                }

            }

        }



        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

    }

    static string ExtractJsonField(string jsonResponse, string fieldName)
    {
        try
        {
            // Parse the JSON into a dynamic object
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            // Extract the specified field
            if (jsonDoc.RootElement.TryGetProperty(fieldName, out var field))
            {
                return field.GetString();
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
            return string.Empty;
        }
    }

    static string ExtractCodeFromBackticks(string input)
    {
        // Regular expression to capture content between the first triple backticks
        var regex = new Regex(@"```(?:[a-zA-Z0-9]*?)\n(.*?)```", RegexOptions.Singleline);

        // Match the regex against the input string
        var match = regex.Match(input);

        // Return the code if found, otherwise return an empty string
        return match.Success ? match.Groups[1].Value : input;
    }

    public static void WriteToFile(string folderPath, string fileNameWithoutExtension, string content, string fileExtension)
    {
        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Create the full file path with the specified extension
        string fullPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}.{fileExtension}");

        // Write content to the file
        File.WriteAllText(fullPath, content);

        Console.WriteLine($"File saved to: {fullPath}");
    }

}

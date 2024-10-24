using System;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());

        var yamlFilePath = "../../../prompts.yaml";

        try
        {
            // Read YAML file contents
            var yamlContent = File.ReadAllText(yamlFilePath);

            // Deserializer with default naming convention
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // Deserialize YAML content into PromptsRoot object
            var promptsRoot = deserializer.Deserialize<PromptsRoot>(yamlContent);

            // Loop through each prompt and display the data
            foreach (var prompt in promptsRoot.Prompts)
            {
                //Console.WriteLine($"Prompt: {prompt.prompt}");
                //Console.WriteLine($"File Extension: {prompt.file_extension}");
                //Console.WriteLine();
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("The specified YAML file was not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        // Create HttpClient
        using var client = new HttpClient();

        // Define the request URL and data
        var url = "http://localhost:11434/api/generate";
        var data = new StringContent("{\"model\":\"phi3.5\",\"prompt\":\"return ruby code that is a hello world app\", \"stream\":false}", Encoding.UTF8, "application/json");

        try
        {
            // Send POST request
            var response = await client.PostAsync(url, data);

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                //var responseBody = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("Response: " + responseBody);

                var responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON and extract the relevant field
                string messageField = ExtractJsonField(responseBody, "response");

                // Now extract the code from the field
                string code = ExtractCodeFromBackticks(messageField);

                Console.WriteLine(code);

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

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

    }

    // Method to extract a specific field from the JSON response
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

    // Method to extract the code between the first set of triple backticks
    static string ExtractCodeFromBackticks(string input)
    {
        // Regular expression to capture content between the first triple backticks
        var regex = new Regex(@"```(?:[a-zA-Z0-9]*?)\n(.*?)```", RegexOptions.Singleline);

        // Match the regex against the input string
        var match = regex.Match(input);

        // Return the code if found, otherwise return an empty string
        return match.Success ? match.Groups[1].Value : input;
    }

}

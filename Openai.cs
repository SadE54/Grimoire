using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChatGPT.Net;
using ChatGPT.Net.DTO.ChatGPT;

namespace Grimoire
{
    public class PromptDatabase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
         
        [JsonPropertyName("language")]
        public string Language { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("prompts")]
        public List<SystemPrompt> Prompts { get; set; } 
    }

    public class SystemPrompt
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; }
    }


    public static  class Openai
    {
        public static string DatabasePath = "";
        private static string OpenaiToken = "";

        public static PromptDatabase Database { get; private set; } = new PromptDatabase();

        public static int LoadDatabase(string database_path)
        {
            if (string.IsNullOrEmpty(database_path))
            {
                AnsiConsole.MarkupLine("[red]Prompts database path is empty. Please provide a valid path.[/]");
                return -1;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var jsonText = File.ReadAllText(database_path);
                Database = JsonSerializer.Deserialize<PromptDatabase>(jsonText, options);
            }
            catch (FileNotFoundException)
            {
                AnsiConsole.MarkupLine("[red]Prompts database file not found.[/]");
                return -1;
            }
            catch 
            {
                AnsiConsole.MarkupLine($"[red]Error parsing prompts database file[/]");
                return -2;
            }
            return 0;
        }


        public static int SetApiToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                AnsiConsole.MarkupLine("[red]OpenAI token is empty. Please provide a valid token.[/]");
                return -1;
            }
            OpenaiToken = token;
            return 0;
        }


       public static async Task<string> ExecutePrompt(string command, string input)
        {
            var prompt = Database.Prompts.FirstOrDefault(p => p.Command.Equals(command, StringComparison.OrdinalIgnoreCase));
            if (prompt == null)
            {
                AnsiConsole.MarkupLine($"[red]Prompt '{command}' not found.[/]");
                return "";
            }
            var systemMessage = prompt.System.Replace("{input}", input);
            var temperature = prompt.Temperature;
            var bot = new ChatGpt(OpenaiToken,
                            new ChatGptOptions
                            {
                                Model = "gpt-3.5-turbo",
                                MaxTokens = 2000,
                            });

            bot.SetConversationSystemMessage(prompt.Command, systemMessage);

            string response = "";
            await AnsiConsole.Status()
            .StartAsync("Asking...", async ctx =>
            {
                // Update the status and spinner
                ctx.Status("Asking...");
                ctx.Spinner(Spinner.Known.Dots9);
                ctx.SpinnerStyle(Style.Parse("gold3_1"));
                response = await bot.Ask(input, prompt.Command);
            });

            return response;
        }
    }
}

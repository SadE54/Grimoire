using Spectre.Console;
using PrettyPrompt;
using System.Text;
using PrettyPrompt.Highlighting;
using System.Reflection;
using Tommy;



namespace Grimoire
{
    partial class Program
    {
        static string RulesDatabasePath = "";
        static string PromptsDatabasePath = "";


        static async Task Main(string[] args)
        {
            System.Console.OutputEncoding = Encoding.UTF8;
            System.Console.InputEncoding = Encoding.UTF8;

            AnsiConsole.Clear();
            var rule = new Rule().RuleStyle(Color.SkyBlue2).Centered();
            AnsiConsole.Write(rule);
            AnsiConsole.Write((new FigletText("Grimoire").Centered().Color(Color.SkyBlue2)));
            rule = new Rule($"{Assembly.GetExecutingAssembly().GetName().Version}").RuleStyle(Color.SkyBlue2).Centered();
            AnsiConsole.Write(rule);
            rule = new Rule().RuleStyle(Color.SkyBlue2).Centered();

            LoadConfig("config.toml");
            Rules.LoadDatabase(RulesDatabasePath);
            Openai.LoadDatabase(PromptsDatabasePath);
            CommandParser.DisplayInfo();

            var prompt = new Prompt(
                callbacks: new CliCallbacks(),
                configuration: new PromptConfiguration(
                            prompt: new FormattedString(">>> ", new FormatSpan(0, 3, AnsiColor.Cyan)),
                            completionItemDescriptionPaneBackground: AnsiColor.Rgb(30, 30, 30),
                            selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                            selectedTextBackground: AnsiColor.Rgb(20, 61, 102)));


            while (true)
            {
                var response = await prompt.ReadLineAsync();
                if (response.IsSuccess)
                {
                    CommandParser.Parse(response.Text);
                }
                AnsiConsole.Write(rule);
            }

        }


        public static int LoadConfig(string configFile)
        {
            if (File.Exists(configFile) == false)
            {
                AnsiConsole.MarkupLine("[red]Config file not found. Please ensure 'config.toml' exists in the current directory.[/]");
                return -1;
            }

            try
            {
                using (StreamReader reader = File.OpenText(configFile))
                {
                    TomlTable table = TOML.Parse(reader);
                    if (table.HasKey("rules") == true)
                    {
                        if (table["rules"].HasKey("database_path") == true)
                        {
                            var safe_path = table["rules"]["database_path"].ToString().TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), safe_path));
                            if (File.Exists(path) == true)
                            {
                                RulesDatabasePath = path;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine($"[red]Rules database file not found. Please ensure '{path}' exists in the current directory.[/]");
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Config file is missing the 'rules' section.[/]");
                    }

                    if (table.HasKey("openai") == true)
                    {
                        if (table["openai"].HasKey("api_token") == true)
                        {
                            Openai.SetApiToken(table["openai"]["api_token"].ToString() ?? "");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]OpenAI token not found in config file.[/]");
                            return -1;
                        }
                        if (table["openai"].HasKey("database_path") == true)
                        {
                            var safe_path = table["openai"]["database_path"].ToString().TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            var path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), safe_path));
                            if (File.Exists(path) == true)
                            {
                                PromptsDatabasePath = path;
                            }
                            else
                            {
                                AnsiConsole.MarkupLine($"[red]OpenAI database file not found. Please ensure '{path}' exists in the current directory.[/]");
                                return -1;
                            }
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Config file is missing the 'openai' section.[/]");
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error reading config file: {ex.Message}[/]");
                return -2;
            }

            return 0;
        }
    }
}


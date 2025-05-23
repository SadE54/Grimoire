using Spectre.Console;
using PrettyPrompt;
using System.Text;
using PrettyPrompt.Highlighting;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using System.Reflection;
using Tommy;
using System.Threading;



namespace Grimoire
{
    partial class Program
    {
        static string RulesDatabasePath = "";
        static string PromptsDatabasePath = "";
        static HueSettings HueSettings = new HueSettings();
        public static TorchManager Torch = new TorchManager();


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

            HueManager.Init(HueSettings).Wait();




            var prompt = new Prompt(
                callbacks: new CliCallbacks(),
                configuration: new PromptConfiguration(
                            prompt: new FormattedString(">>> ", new FormatSpan(0, 3, AnsiColor.Cyan)),
                            completionItemDescriptionPaneBackground: AnsiColor.Rgb(30, 30, 30),
                            selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                            selectedTextBackground: AnsiColor.Rgb(20, 61, 102)));

            while (true)
            {
                var cancellationToken = new CancellationTokenSource();

                AnsiConsole.Write(rule);
                Torch.SetToken(cancellationToken.Token);

                var response = await prompt.ReadLineAsync(Torch.Token).ConfigureAwait(false);
                if (response.IsSuccess)
                {
                    CommandParser.Parse(response.Text);
                }
                else
                {
                    if (response.CancellationToken.IsCancellationRequested)
                    {
                        AnsiConsole.MarkupLine("[red]Prompt canceled.[/]");
                        break;
                    }
                }

                if (Torch.IsExpired == true)
                {
                    AnsiConsole.MarkupLine("[gold3_1]🔥 The torch goes out.[/]");
                }
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
                        if ((table["rules"].HasKey("database_path") == true) && table["rules"]["database_path"] != null)
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
                        if ((table["openai"].HasKey("database_path") == true) && (table["openai"]["database_path"].ToString() != null))
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

                    // Hue Section
                    if (table.HasKey("hue") == true)
                    {
                        HueSettings.Enabled = table["hue"]["enabled"] ?? false;
                        HueSettings.BridgeIp = table["hue"]["bridge_ip"].ToString() ?? "";
                        HueSettings.ApiKey = table["hue"]["api_token"].ToString() ?? "";
                        HueSettings.RoomName = table["hue"]["room"].ToString() ?? "";
                        HueSettings.Color = table["hue"]["color"].ToString() ?? "";
                        HueSettings.Effect = table["hue"]["effect"].ToString() ?? "";
                        HueSettings.Brightness = byte.TryParse(table["hue"]["brightness"].ToString(), out var brightness) ? brightness : (byte)50;
                        var lights= table["hue"]["lights_ids"];
                        if (lights != null)
                        {
                            foreach (var light in lights)
                            {
                                HueSettings.LightsId.Add(light.ToString());
                            }
                        }
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


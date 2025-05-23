using Spectre.Console;
using Spectre.Console.Extensions.Markup;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Grimoire
{

    public static class CommandParser
    {
        private static readonly Regex RollRegex = new Regex(@"^!roll\s+(\d+)d(\d+)([+-]?\d*)\s*(adv|dis)?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RuleRegex = new Regex(@"!rule\s+(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex OpenaiRegex = new Regex(@"^!ai\s+([a-zA-Z0-9_]+)\s+(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex EventRegex = new Regex(@"!event\s+(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TorchRegex = new Regex(@"^!torch(?:\s+(start)\s+(\d+)|\s+(stop))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ClearRegex = new Regex(@"!clear", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex CreditsRegex = new Regex(@"!credits", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ExitRegex = new Regex(@"!exit", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HelpRegex = new Regex(@"!help", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex InfoRegex = new Regex(@"!info", RegexOptions.IgnoreCase | RegexOptions.Compiled);


        public static void Parse(string input)
        {
            if (ParseRollCommand(input) == true) return;
            if (ParseRuleCommand(input) == true) return;
            if (ParseEventCommand(input) == true) return;
            if (ParseOpenAiCommand(input) == true) return;
            if (ParseTorchCommand(input) == true) return;
            if (ParseClearCommand(input) == true) return;
            if (ParseCreditsCommand(input) == true) return;
            if (ParseExitCommand(input) == true) return;
            if (ParseHelpCommand(input) == true) return;
            if (ParseInfoCommand(input) == true) return;
            AnsiConsole.MarkupLine("[red]Invalid command ![/]");
        }


        public static bool ParseRollCommand(string input)
        {
            var match = RollRegex.Match(input);

            if (match.Success == false)
            {
                return false;
            }

            var diceCount = int.Parse(match.Groups[1].Value);
            var diceSides = int.Parse(match.Groups[2].Value);
            var modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);
            var mode = match.Groups[4].Value.ToLower() switch
            {
                "adv" => RollMode.Advantage,
                "dis" => RollMode.Disadvantage,
                _ => RollMode.Normal,
            };

            var RollInstruction = new RollInstruction
            {
                DiceCount = diceCount,
                DiceSides = diceSides,
                Modifier = modifier,
                Mode = mode
            };

            var result = DiceExecutor.ExecuteRollCommand(RollInstruction);
            AnsiConsole.Markup($"[green]{result}[/]");
            return true;
        }

        public static bool ParseRuleCommand(string input)
        {
            var match = RuleRegex.Match(input);
            if (match.Success == false)
            {
                return false;
            }

            var ruleTags = match.Groups[1].Value.Trim().Split(" ").ToList();
            Rules.Search(ruleTags);

            return true;

        }


        public static bool ParseOpenAiCommand(string input)
        {
            var match = OpenaiRegex.Match(input);
            if (match.Success)
            {
                var type = match.Groups[1].Value.ToLower();
                var prompt = match.Groups[2].Value.Trim();

                try
                {
                    var response = Openai.ExecutePrompt(type, prompt).Result;
                    AnsiConsole.MarkupLine($"[green]{response}[/]");
                    return true;
                }
                catch
                {
                    AnsiConsole.MarkupLine("❌ [red]OpenAI API request failed.[/]");
                    return true;
                }
            }
            return false;
        }


        public static bool ParseEventCommand(string input)
        {
            var match = EventRegex.Match(input);
            if (match.Success)
            {
                var tableTags = match.Groups[1].Value.Trim().Split(" ").ToList();
                var response = Table.GetEvent(tableTags);
                AnsiConsole.MarkupLine(response);
                return true;
            }
            return false;
        }


        public static bool ParseTorchCommand(string input)
        {
            var match = TorchRegex.Match(input);
            if (match.Success)
            {
                var action = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
                var duration = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                if (action == "start")
                {
                    Program.Torch.Start(duration); 
                    
                }
                else if (action == "stop")
                {
                    Program.Torch.Stop();
                }
                else
                {
                    var remainingTime = Program.Torch.GetRemainingTime();
                    if (remainingTime.TotalSeconds > 0)
                    {
                        AnsiConsole.MarkupLine($"[gold3_1]The torch is still lit for {remainingTime.Minutes}m{remainingTime.Seconds}s.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[gold3_1]🔥 The torch is not lit.[/]");
                    }
                }
                return true;
            }
            return false;
        }


        public static bool ParseClearCommand(string input)
        {
            var match = ClearRegex.Match(input);
            if (match.Success)
            {
                AnsiConsole.Clear();
                return true;
            }
            return false;
        }

        public static bool ParseCreditsCommand(string input)
        {
            var match = CreditsRegex.Match(input);
            if (match.Success)
            {
                Rules.DisplayCredits();
                return true;
            }
            return false;
        }


        public static bool ParseHelpCommand(string input)
        {
            var match = HelpRegex.Match(input);
            if (match.Success)
            {
                if (File.Exists("help.md"))
                {
                    var md = File.ReadAllText("help.md");
                    var markup = new MarkdownRenderable(md);
                    Rules.SetMarkdownStyles(ref markup);
                    AnsiConsole.Write(markup);
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Help file not found. Please ensure 'help.md' exists in the working directory.[/]");
                }
                return true;
            }
            return false;
        }

        public static bool ParseInfoCommand(string input)
        {
            var match = InfoRegex.Match(input);
            if (match.Success)
            {
                DisplayInfo();
                return true;
            }
            return false;
        }

        public static bool ParseExitCommand(string input)
        {
            var match = ExitRegex.Match(input);
            if (match.Success)
            {
                Environment.Exit(0);
            }
            return false;
        }


        public static void DisplayInfo()
        {
            AnsiConsole.MarkupLine("[gold3_1]Loaded databases:[/]");
            if (Rules.Database != null)
            {
                AnsiConsole.MarkupLine($"[gold3_1]🛢️ {Rules.Database.Game} ({Rules.Database.Language}, v{Rules.Database.Version})[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Rules database not loaded.[/]");
            }

            if (Openai.Database != null)
            {
                AnsiConsole.MarkupLine($"[gold3_1]🛢️ {Openai.Database.Name} ({Openai.Database.Language}, v{Openai.Database.Version})[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Prompts database not loaded.[/]");
            }

        }
    }
}

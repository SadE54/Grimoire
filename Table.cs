using Spectre.Console.Extensions.Markup;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Grimoire
{
    internal class Table
    {
        private static readonly Regex EventRegex = new Regex(@"^(\d+)(?:-(\d+))?[\.:]\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex EventRegexMd = new Regex(@"^\|\s*(\d{1,2}|00)(?:\s*-\s*(\d{1,2}|00))?\s*\|\s*(.+?)\s*\|?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
      
        public static string GetEvent(List<string> tags)
        { 
            var result = Rules.GetEntry(tags);
            if (result != null)
            {
                var full_path = Path.Combine(Rules.DatabasePath, result.Entry.Path);
                var lines = File.ReadAllLines(full_path);
                var options = new List<(int min, int max, string texte)>();

                foreach (var line in lines)
                {
                    var match = EventRegex.Match(line);
                    if (match.Success == false)
                        // test with markdown table format
                        match = EventRegexMd.Match(line);

                    if (match.Success)
                    {
                        int min = int.Parse(match.Groups[1].Value);
                        int max = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : min;
                        // specific case for 00 entry in tables
                        if (max == 0) max = 100;
                        string text = match.Groups[3].Value.Trim();
                        options.Add((min, max, text));
                    }
                }

                if (options.Count == 0)
                    return "[red]❗No table detected in this file.[/]";

                var maxDice = options.Max(o => o.max);
                var rng = new Random();
                int roll = rng.Next(1, maxDice + 1);

                var resultat = options.FirstOrDefault(opt => roll >= opt.min && roll <= opt.max);
                return $"[gold3_1]🎲 d{maxDice}={roll} → [/][green]{resultat.texte}[/]";
            }
            else
            {
                return "[red]❗Cannot find any entry[/]";
            }
        }
    }
}

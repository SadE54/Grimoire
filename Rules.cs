using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PrettyPrompt.Highlighting;
using Spectre.Console;
using Spectre.Console.Extensions.Markup;

namespace Grimoire
{
    public class MarkdownEntry
    {
        public required string Name { get; set; }
        public required List<string> Tags { get; set; }
        public required string Path { get; set; }
    }

    public class MarkdownDatabase
    {
        public string Game { get; set; }

        public string Language { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string License { get; set; }
        public string Credits { get; set; }
        public List<MarkdownEntry> Entries { get; set; }
    }


    public class SearchResult
    {
        public MarkdownEntry Entry { get; set; }
        public int Score { get; set; }
    }



    public static class Rules
    {
        public static string DatabasePath = "";

        public static MarkdownDatabase Database { get; private set; } = new MarkdownDatabase();

        public static int LoadDatabase(string database_path)
        {
            if (string.IsNullOrEmpty(database_path))
            {
                AnsiConsole.MarkupLine("[red]Database path is empty. Please provide a valid path.[/]");
                return -1;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            DatabasePath = Path.GetDirectoryName(database_path) ?? string.Empty;

            try
            {
                var jsonText = File.ReadAllText(database_path);
                Database = JsonSerializer.Deserialize<MarkdownDatabase>(jsonText, options);
            }
            catch (FileNotFoundException)
            {
                AnsiConsole.MarkupLine("[red]Database file not found. Please ensure 'database.json' exists in the current directory.[/]");
                return -1;
            }
            catch 
            {
                AnsiConsole.MarkupLine($"[red]Error parsing database file[/]");
                return -2;
            }
            return 0;
        }


        public static void Search(List<string> tags)
        {
            var result = GetEntry(tags);

            if (result != null)
            {
                    var full_path = Path.Combine(DatabasePath, result.Entry.Path);
                    if (File.Exists(full_path) == false)
                    {
                        AnsiConsole.MarkupLine($"[red]Database file not found.({full_path})[/] ");
                        return;
                    }
                    var md = File.ReadAllText(full_path);
                    var markup = new MarkdownRenderable(md);
                    SetMarkdownStyles(ref markup);
                    AnsiConsole.Write(markup);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No results found.[/]");
            }
        }


        public static SearchResult? GetEntry(List<string> tags)
        {
            var results = ScoredSearch(Database.Entries, tags);

            if (results.Count == 0)
            {
                return null;
            }

            //foreach (SearchResult result in results)
            //{
            //    var entry = result.Entry;
            //    var tagsString = string.Join(", ", entry.Tags);
            //    var path = entry.Path;
            //    AnsiConsole.MarkupLine($"[green]{entry.Name}[/] [grey]({tagsString})[/]");
            //}

            SearchResult? selectedResult = null;
            if (results.Count > 1)
            {
                var entry = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold red]There's several results:[/]")
                        .PageSize(5)
                        .MoreChoicesText("[grey](⬆️ et ⬇️ to selct)[/]")
                        .AddChoices(results.Select(r => r.Entry.Name).ToList()));

                selectedResult = results.FirstOrDefault(r => r.Entry.Name == entry);
            }
            else if (results.Count == 1)
            {
                selectedResult = results.FirstOrDefault();
            }
            return selectedResult;
        }



        public static List<SearchResult> ScoredSearch(List<MarkdownEntry> entries, List<string> tags_to_search)
        {
            var results = new List<SearchResult>();

            foreach (var entry in entries)
            {
                int score = 0;

                foreach (var item in tags_to_search)
                {
                    var itemLower = item.ToLower();
                    var nameLower = entry.Name.ToLower();

                    if (nameLower == itemLower)
                        score += 100;
                    else if (nameLower.Contains(itemLower))
                        score += 30;

                    foreach (var tag in entry.Tags)
                    {
                        var tagLower = tag.ToLower();
                        if (tagLower == itemLower)
                            score += 50;
                        else if (tagLower.Contains(itemLower))
                            score += 20;
                    }
                }

                if (score > 0)
                {
                    results.Add(new SearchResult
                    {
                        Entry = entry,
                        Score = score
                    });
                }
            }

            return results
                    .OrderByDescending(r => r.Score)
                    .ToList();
        }

        public static int DisplayCredits()
        {
            if (Database.Credits != null)
            {
                var full_path = Path.Combine(Rules.DatabasePath, Rules.Database.Credits);

                if (File.Exists(full_path) == false)
                {
                    AnsiConsole.MarkupLine("[red]Credits file not found. Please ensure 'credits.md' exists in the current directory.[/]");
                    return -1;
                }
                var md = File.ReadAllText(full_path);
                var markup = new MarkdownRenderable(md);
                SetMarkdownStyles(ref markup);
                AnsiConsole.Write(markup);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No credits available.[/]");
                return -1;
            }
            return 0;
        }

        private static void SetMarkdownStyles(ref MarkdownRenderable markup)
        {
            markup.HeadingLevel1Color = Color.DeepSkyBlue1;
            markup.HeadingLevel2To4Style = Color.DeepSkyBlue1;
            markup.HeadingLevel5AndAboveStyle = Color.SpringGreen1;
            markup.ListBlockMarkerStyle = Color.DeepSkyBlue1;
            markup.TableBorderStyle = Color.DeepSkyBlue1;
            markup.QuoteBlockBorderStyle = Color.Gold3_1;
        }
    }
}

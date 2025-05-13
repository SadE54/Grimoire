using PrettyPrompt.Highlighting;
using PrettyPrompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grimoire
{
    partial class Program
    {
        private static readonly (string Name, AnsiColor Color)[] Commands =
        {
           ("!rule",   AnsiColor.Rgb(135, 95,  215)), // Purple
           ("!roll",    AnsiColor.Rgb(135, 95,  215)),
           ("!event",   AnsiColor.Rgb(135, 95,  215)),
           ("!ai",      AnsiColor.Rgb(135, 95,  215)),
           ("!help",    AnsiColor.Rgb(135, 95,  215)),
           ("!credits", AnsiColor.Rgb(135, 95,  215)),
           ("!clear",   AnsiColor.Rgb(135, 95,  215)),
           ("!exit",    AnsiColor.Rgb(135, 95,  215)),
        };

        private static FormattedString GetFormattedString(string text)
        => new(text, EnumerateFormatSpans(text, Commands));

        private static IEnumerable<FormatSpan> EnumerateFormatSpans(string text, IEnumerable<(string TextToFormat, AnsiColor Color)> formattingInfo)
        {
            foreach (var (textToFormat, color) in formattingInfo)
            {
                int startIndex;
                int offset = 0;
                while ((startIndex = text.AsSpan(offset).IndexOf(textToFormat)) != -1)
                {
                    yield return new FormatSpan(offset + startIndex, textToFormat.Length, color);
                    offset += startIndex + textToFormat.Length;
                }
            }
        }

        internal class CliCallbacks : PromptCallbacks
        {
            //protected override IEnumerable<(KeyPressPattern Pattern, KeyPressCallbackAsync Callback)> GetKeyPressCallbacks()
            //{
            // registers functions to be called when the user presses a key. The text
            // currently typed into the prompt, along with the caret position within
            // that text are provided as callback parameters.
            //yield return (new(ConsoleModifiers.Control, ConsoleKey.F1), ShowFruitDocumentation);
            //
            //}

            //protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
            //{
            //    // demo completion algorithm callback
            //    // populate completions and documentation for autocompletion window
            //    var typedWord = text.AsSpan(spanToBeReplaced.Start, spanToBeReplaced.Length).ToString();
            //    return Task.FromResult<IReadOnlyList<CompletionItem>>(
            //        Commands
            //        .Select(command =>
            //        {
            //            var displayText = new FormattedString(command.Name, new FormatSpan(0, command.Name.Length, command.Highlight));
            //            //var description = GetFormattedString(fruit.Description);
            //            return new CompletionItem(
            //                replacementText: command.Name,
            //                displayText: displayText,
            //                commitCharacterRules: new[] { new CharacterSetModificationRule(CharacterSetModificationKind.Add, new[] { ' ' }.ToImmutableArray()) }.ToImmutableArray()
            //            //getExtendedDescription: _ => Task.FromResult(description)
            //            );
            //        })
            //        .ToArray()
            //    );
            //}


            protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
            {
                IReadOnlyCollection<FormatSpan> spans = EnumerateFormatSpans(text, Commands.Select(f => (f.Name, f.Color))).ToList();
                return Task.FromResult(spans);
            }

        }
    }
}

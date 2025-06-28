using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Kevsoft.WLED;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace Grimoire
{
    public class WledSettings
    {
        public bool Enabled { get; set; } = false;
        public string Uri { get; set; } = "";
        public int Preset { get; set; } = 0;
        public int Brightness { get; set; } = 127;
    }

    internal class WledManager
    {
        public static WledSettings Settings { get; set; } = new();
        private static WLedClient? client;
        private static bool initialized = false;

        public static async Task Init(WledSettings settings)
        {
            Settings = settings;
            if (!Settings.Enabled)
            {
                AnsiConsole.MarkupLine($"💡 [gold3_1]WLED system disabled.[/]");
                return;
            }

            var regex = new Regex(@"http:\/\/\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            if (regex.IsMatch(Settings.Uri) == false)
            {
                AnsiConsole.MarkupLine($"❌ [red]Wled URL device is not valid :{Settings.Uri}[/]");
                return;
            }

            try
            {
                client = new WLedClient(Settings.Uri);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"❌ [red]Error connecting to WLED device: {ex.Message}[/]");
                return;
            }

            var infoTask = client.Post(new StateRequest { On = false, PresetId = Settings.Preset , Brightness = Convert.ToByte(Settings.Brightness) });
            var timeoutTask = Task.Delay(3000);
            Task completed = await AnsiConsole.Status()
                   .StartAsync("Init Wled system...", async ctx =>
                   {
                       // Update the status and spinner  
                       ctx.Status("[gold3_1]Init Wled System...[/]");
                       ctx.Spinner(Spinner.Known.Dots9);
                       ctx.SpinnerStyle(Style.Parse("gold3_1"));
                       return await Task.WhenAny(infoTask, timeoutTask);
                   });

            if (completed == timeoutTask)
            {
                AnsiConsole.MarkupLine($"❌ [red]Error connecting to WLED device[/]");
                return;
            }
            else if (completed == infoTask)
            {
                await infoTask;
                initialized = true;
                AnsiConsole.MarkupLine($"💡 [gold3_1]WLED system initialized[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"❌ [red]Unknown error during WLED initialization[/]");
            }
        }

        public static async Task<int> SetState(bool state, int preset = 0, int brightness = 0)
        {
            if (initialized == false) return 0;
                
            if (client == null)
            {
                AnsiConsole.MarkupLine($"❌ [red]WLED client not initialized[/]");
                return -1;
            }

            if (preset == 0)
            {
                preset = Settings.Preset;
            }
            if (brightness == 0)
            {
                brightness = Settings.Brightness;
            }
            if (brightness > 255)
            {
                brightness = 255; // Ensure brightness is within valid range
            }


            var lightTask = client.Post(new StateRequest { On = state, Brightness = Convert.ToByte(brightness) }); 
            var timeoutTask = Task.Delay(2000);
            var completedTask = await Task.WhenAny(lightTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                // Timeout 
                Console.WriteLine("Timeout setting preset state.");
                return -1;
            }
            
            try
            {
                await lightTask;
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"❌ [red]Error setting WLED preset: {ex.Message}[/]");
                return -2;
            }
        }
    }
}

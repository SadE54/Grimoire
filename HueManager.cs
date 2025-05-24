using HueApi;
using HueApi.Models;
using HueApi.Models.Requests;
using HueApi.ColorConverters.Original.Extensions;
using Spectre.Console;
using PrettyPrompt;

namespace Grimoire
{
    public class HueSettings
    {
        public bool Enabled { get; set; } = false;
        public string BridgeIp { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string RoomName { get; set; } = "";
        public List<string> LightsId { get; set; } = new List<string>();
        public string Color { get; set; } = "";
        public string Effect { get; set; } = "";
        public byte Brightness { get; set; } = 50;
    }

    public static class HueManager
    {

        public static HueSettings Settings { get; set; } = new HueSettings();
        private static LocalHueApi? localHueClient;
        private static Dictionary<string, List<Guid>> lightsId = new();

        // Fixed method signature and applied static modifier as the method does not access instance data.    
        public static async Task Init(HueSettings settings)
        {
            Settings = settings;

            if (!Settings.Enabled)
            {
                AnsiConsole.MarkupLine($"💡 [gold3_1]Hue system disabled.[/]");
                return;
            }

            try
            {
                localHueClient = new LocalHueApi(Settings.BridgeIp, Settings.ApiKey);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"❌ [red]Error connecting to Hue bridge[/]");
                return;
            }
           

            // Initialize the lightsId dictionary with room names and their corresponding light IDs  
            if (Settings.RoomName != "")
            {
                var response = new List<Guid>();
                await AnsiConsole.Status()
                    .StartAsync("Init Hue system...", async ctx =>
                    {
                        // Update the status and spinner  
                        ctx.Status("[gold3_1]Init Hue System...[/]");
                        ctx.Spinner(Spinner.Known.Dots9);
                        ctx.SpinnerStyle(Style.Parse("gold3_1"));
                        response = await GetLightsFromRoom(Settings.RoomName);
                    });
                if (response.Count == 0)
                {
                    AnsiConsole.MarkupLine($"❌ [red]Error initializing Hue System[/]");
                    AnsiConsole.MarkupLine($"[red]Please check the configuration file.[/]");
                    return;
                }
            }
            AnsiConsole.MarkupLine($"💡 [gold3_1]Hue system initialized.[/]");
        }


        public static async Task<List<Guid>> GetLightsFromRoom(string name)
        {
            if (localHueClient == null)
            {
                throw new InvalidOperationException("LocalHueApi client is not initialized.");
            }

            var getRoomsTask = localHueClient.GetRoomsAsync();
            var getLightsTask = localHueClient.GetLightsAsync();
            var getDevicesTask = localHueClient.GetDevicesAsync();
            var timeoutTask = Task.Delay(5000);

            var completedTask = await Task.WhenAny(getRoomsTask, getDevicesTask, getLightsTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                // Timeout 
                //AnsiConsole.WriteLine("Timeout gathering data from Hue bridge.");
                return new List<Guid>();
            }

            var allRooms = await getRoomsTask;
            var allLights = await getLightsTask;
            var allDevices = await getDevicesTask;

            // Ensure allRooms.Data is not null before accessing it    
            var room = allRooms.Data?.FirstOrDefault(x => x.Metadata?.Name == name);

            if (room == null)
            {
                Console.WriteLine($"Room '{name}' not found.");
                return new List<Guid>(); // Return an empty list instead of 'return;'  
            }

            var devices_id = room.Children?.Select(x => x.Rid).ToList();

            // Fix for CS8602: Dereference of a possibly null reference.  
            // Fix for CS8603: Possible null reference return.  

            var lights = devices_id?
               .Select(device_id => allDevices.Data?.FirstOrDefault(x => x.Id == device_id))
               .Where(device => device?.Services != null)
               .SelectMany(device => device!.Services!) // Use null-forgiving operator (!) to suppress warnings after null checks.
               .Where(service => service.Rtype == "light")
               .Select(service => service.Rid)
               .Where(lightServiceId => lightServiceId != Guid.Empty)
               .ToList() ?? new List<Guid>();

            lightsId[name] = lights;


            return lights;
        }

        public static async Task<int> SetLightState(Guid lightId, bool state, string color = "", int brightness = 0, string effect = "")
        {
            if (Settings.Enabled == false)
            {
                //AnsiConsole.MarkupLine($"💡 [gold3_1]Hue system disabled.[/]");
                return -1;
            }

            if (localHueClient == null)
            {
                throw new InvalidOperationException("LocalHueApi client is not initialized.");
            }

            if (color == "")
            {
                color = Settings.Color;
            }
            if (brightness == 0)
            {
                brightness = Settings.Brightness;
            }
            if (effect == "")
            {
                effect = Settings.Effect;
            }

            UpdateLight req = new();
            req = state ? req.TurnOn() : req.TurnOff();
            req = color != "" ? req.SetColor(new HueApi.ColorConverters.RGBColor(color)) : req;
            req = brightness != 0 ? req.SetBrightness(brightness) : req;

            if (effect == "fire")
                req.EffectsV2 = new EffectsV2()
                {
                    Action = new EffectAction
                    {
                        Effect = Effect.fire,
                    }
                };

            var lightTask = localHueClient.UpdateLightAsync(lightId, req);
            var timeoutTask = Task.Delay(2000);
            var completedTask = await Task.WhenAny(lightTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                // Timeout 
                Console.WriteLine("Timeout settingState to light.");
                return -1;
            }
            var result = await lightTask;

            //foreach (var error in result.Errors)
            //{
            //    Console.WriteLine($"Error: {error.Description}");
            //}
            return result.HasErrors ? -1 : 0;
        }


        public static async Task<int> SetRoomLightsState(bool state, string roomName = "", string color = "", int brightness = 0, string effect = "")
        {
            if (Settings.Enabled == false)
            {
                //AnsiConsole.MarkupLine($"💡 [gold3_1]Hue system disabled.[/]");
                return -1;
            }

            if (roomName == "")
            {
                roomName = Settings.RoomName;
            }
            if (color == "")
            {
                color = Settings.Color;
            }
            if (brightness == 0)
            {
                brightness = Settings.Brightness;
            }
            if (effect == "")
            {
                effect = Settings.Effect;
            }

            if (lightsId.TryGetValue(roomName, out var lightIds)) // Fix: Added the required 'out' parameter
            {
                foreach (var lightId in lightIds)
                {
                    var result = await SetLightState(lightId, state, color, brightness, effect);
                    if (result == -1)
                    {
                        return -1; // Return -1 if any error occurs
                    }
                }
            }
            return 0; // Return 0 if all operations are successful
        }
    }
}


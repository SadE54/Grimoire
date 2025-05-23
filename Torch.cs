using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Spectre.Console;

namespace Grimoire
{
    internal class TorchManager : IDisposable
    {
        private readonly System.Timers.Timer _timer;
        private DateTime _expirationTime;

        public bool IsExpired { get; private set; }

        public CancellationToken Token => _cts.Token;
        private CancellationTokenSource _cts = new();

        public TorchManager()
        {
            _timer = new System.Timers.Timer();
            _timer.AutoReset = false;   
            _timer.Elapsed += OnTimerElapsed;
        }

        public void SetToken(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        }


        public void Start(int durationInMinutes)
        {
            Stop();

            TimeSpan duration = TimeSpan.FromMinutes(durationInMinutes);
            IsExpired = false;
            _expirationTime = DateTime.Now.Add(duration);
            _timer.Interval = duration.TotalMilliseconds;
            _timer.Start();
            SetLights(true);
            AnsiConsole.MarkupLine($"[gold3_1]🔥 The torch is lit for {duration.TotalMinutes} minutes.[/]");
        }



        public void Stop()
        {
            _timer.Stop();
            IsExpired = true;
            _cts.Cancel();
            SetLights(false);
        }


        public TimeSpan GetRemainingTime()
        {
            TimeSpan remainingTime;

            if (IsExpired)
            {
                remainingTime = TimeSpan.Zero;
            }
            else
            {
                var now = DateTime.Now;
                remainingTime = _expirationTime - now;
            }

            double pourcentage = (remainingTime.TotalMilliseconds / _timer.Interval) * 100;
            int fireActive = (int)Math.Round(pourcentage / 100 * 10);
            string status = "[[" + string.Concat(Enumerable.Repeat("🔥", fireActive)) + new string('➖', 10 - fireActive) + "]]";
            
            AnsiConsole.MarkupLine($"[gold3_1]{status}[/]");
            return remainingTime;
        }


        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            IsExpired = true;
            _cts.Cancel();
            SetLights(false);
        }

        // Fix for CS1503: Ensure the correct type (Guid) is passed to SetLightState.  
        // Assuming HueManager.Settings.LightsId contains strings that need to be converted to Guid.  

        public void SetLights(bool state)
        {
            if (!string.IsNullOrEmpty(HueManager.Settings.RoomName))
            {
                _ = HueManager.SetRoomLightsState(state, HueManager.Settings.RoomName)
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            AnsiConsole.MarkupLine($"[red]Error trying to set Hue devices {t.Exception.InnerException?.Message}[/]");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }

            if (HueManager.Settings.LightsId.Count > 0)
            {
                foreach (var lightId in HueManager.Settings.LightsId)
                {
                    if (Guid.TryParse(lightId.ToString(), out Guid parsedGuid))
                    {
                        _ = HueManager.SetLightState(parsedGuid, state)
                            .ContinueWith(t =>
                            {
                                if (t.Exception != null)
                                {
                                    AnsiConsole.MarkupLine($"[red]Error trying to set Hue devices {t.Exception.InnerException?.Message}[/]");
                                }
                            }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                    else
                    {
                        //AnsiConsole.MarkupLine($"[red]Invalid Light ID: {lightId}[/]");
                    }
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
            _cts.Dispose();
        }
    }
}

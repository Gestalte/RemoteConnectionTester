using Akka.Actor;
using Akka.Configuration;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Common;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Server.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        if (Design.IsDesignMode)
        {
            Logs =
            [
                "test",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat."
            ];

            DeviceHostName = "localhost";
        }

        LogWriter.WriteLogAction = (message) => Avalonia.Threading.Dispatcher.UIThread.Invoke(() => Logs.Add(message));

        Logs.CollectionChanged += (s, e) => ListBox?.ScrollIntoView(ListBox.ItemCount);

        try
        {
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);

            StringBuilder sb = new();

            DeviceHostName = hostName;
            System.Diagnostics.Debug.WriteLine($"Host Name: {hostName}", "[INFO]");
            sb.AppendLine($"Host Name:\t{hostName}");

            foreach (var address in hostEntry.AddressList)
            {
                var ipBytes = address.GetAddressBytes();
                var ip = address.ToString();
                System.Diagnostics.Debug.WriteLine($"Address:\tIP {ip}", "[INFO]");
                sb.AppendLine($"Address:\tIP {ip}");
            }

            var output = sb.ToString();

            Logs.Add(hostEntry.AddressList.Length switch
            {
                0 => output,
                _ => output[..(output.Length - 2)]
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            Logs.Add(ex.ToString());
        }

        try
        {
            HttpClient httpClient = new();
            var response = httpClient.GetAsync("https://myexternalip.com/raw").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var ip = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            DeviceExternalIP = ip;
            System.Diagnostics.Debug.WriteLine($"External IP: {ip}", "[INFO]");
            httpClient.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            Logs.Add(ex.ToString());
        }
    }

    public ListBox? ListBox { get; set; } // This gets set in the code behind.
    public IClipboard? Clipboard { get; set; } // This gets set in the code behind.

    private string? serverIp;
    public string? ServerIp
    {
        get { return this.serverIp; }
        set { this.RaiseAndSetIfChanged(ref this.serverIp, value); }
    }

    private string? serverPort;
    public string? ServerPort
    {
        get { return this.serverPort; }
        set { this.RaiseAndSetIfChanged(ref this.serverPort, value); }
    }

    private string? deviceHostName;
    public string? DeviceHostName
    {
        get { return this.deviceHostName; }
        set { this.RaiseAndSetIfChanged(ref this.deviceHostName, value); }
    }

    private string? deviceExternalIP;
    public string? DeviceExternalIP
    {
        get { return this.deviceExternalIP; }
        set { this.RaiseAndSetIfChanged(ref this.deviceExternalIP, value); }
    }

    public ObservableCollection<string> Logs { get; set; } = [];

    const string hocon = """
        akka {
            loglevel = INFO
            actor {
                provider = "Akka.Remote.RemoteActorRefProvider, Akka.Remote"
                debug {
                    receive = on
                    autoreceive = on
                    lifecycle = on
                    event-stream = on
                    unhandled = on
                }
            }
            loggers = ["Ⅲ, Ⅳ"]
            remote {
                helios.tcp {
                    port = Ⅰ
                    hostname = Ⅱ
                    public-hostname = Ⅴ
                }
            }
        }
        """;

    private string Hocon()
    => hocon
        .Replace("Ⅰ", ServerPort)
        .Replace("Ⅱ", ServerIp)
        .Replace("Ⅲ", $"{typeof(CustomLogger).Namespace}.{typeof(CustomLogger).Name}")
        .Replace("Ⅳ", typeof(CustomLogger).Assembly.GetName().Name)
        .Replace("Ⅴ", DeviceExternalIP);

    private ActorSystem? actorSystem;

    public void CreateActorSystem()
    {
        try
        {
            var hocon = ConfigurationFactory.ParseString(Hocon());

            this.actorSystem?.Terminate();
            this.actorSystem = ActorSystem.Create("server-actor-system", hocon);

            Thread.Sleep(1000);

            var props = Props.Create(() => new PongActor());
            var pong_actor = this.actorSystem.ActorOf(props, "pong-actor");
        }
        catch (Exception ex)
        {
            Logs.Add(ex.ToString());
        }
    }

    public void ClearLogs()
    {
        Logs.Clear();
    }

    public void CopyText(string text)
    {
        Clipboard?.SetTextAsync(text).GetAwaiter().GetResult();
    }
}

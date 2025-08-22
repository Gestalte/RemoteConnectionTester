using Mono.Nat;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace NatTraversal
{
    public sealed class Utils : IDisposable
    {
        private static INatDevice? natDevice;
        private static Mapping? portMapping;

        internal static event Action<bool>? Finished;

        internal static void CreatePortMapping(int port)
        {
            NatUtility.DeviceFound += (o, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(NatTraversal)} NAT Device found", "[INFO]");
                natDevice = e.Device;

                portMapping = natDevice.CreatePortMap(new Mapping(Protocol.Tcp, port, port));
                System.Diagnostics.Debug.WriteLine($"{nameof(NatTraversal)} Port mapping added for port: {portMapping.PublicPort}", "[INFO]");

                Finished?.Invoke(portMapping.PublicPort == port);
            };

            NatUtility.StartDiscovery();
        }

        /// <summary>
        /// Call if you want mapping to be removed after you are done.
        /// </summary>
        public void Dispose()
        {
            if (natDevice is not null && portMapping is not null)
            {
                natDevice.DeletePortMap(portMapping);
                System.Diagnostics.Debug.WriteLine($"{nameof(NatTraversal)} Mapping removed for port: {portMapping.PublicPort}", "[INFO]");
            }
        }

        public static string DiscoverExternalIP()
        {
            var externalIP = natDevice.GetExternalIP().ToString() ?? "localhost";
            System.Diagnostics.Debug.WriteLine($"{nameof(NatTraversal)} External IP: {externalIP}", "[INFO]");
            return externalIP;
        }

        public static string DiscoverInternalIP()
        {
            UdpClient udpClient = new();
            string result = "";

            try
            {
                // NOTE: 8.8.8.8 is one of Google's DNS servers. This can be
                // anything external, it just needs to establish a connection.
                udpClient.Connect("8.8.8.8", 80);
                var localEndPoint = udpClient.Client.LocalEndPoint;
                if (localEndPoint is not null)
                {
                    var internalIp = ((IPEndPoint)localEndPoint).Address.ToString();
                    System.Diagnostics.Debug.WriteLine($"{nameof(NatTraversal)} Internal IP: {internalIp}", "[INFO]");
                    result = internalIp;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex, "[ERROR]");
            }
            finally
            {
                try
                {
                    udpClient.Close();
                    udpClient.Dispose();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex, "[ERROR]");
                }
            }

            return result;
        }

        /// <summary>
        /// Awaiter that tries to create a Port mapping on the NAT.
        /// </summary>
        public class CreatePortMappingAwaiter : INotifyCompletion
        {
            private Action? continuation;
            private bool result;

            public CreatePortMappingAwaiter(int port)
            {
                Utils.Finished += OnFinished;
                Utils.CreatePortMapping(port);
            }

            public bool IsCompleted { get; private set; }

            public void OnFinished(bool result)
            {
                IsCompleted = true;
                this.result = result;
                Utils.Finished -= OnFinished;
                this.continuation?.Invoke();
            }

            public void OnCompleted(Action continuation)
            {
                if (IsCompleted)
                {
                    continuation();
                }
                else
                {
                    this.continuation = continuation;
                }
            }

            public bool GetResult()
            {
                return this.result;
            }
        }

        /// <summary>
        /// This is a custom awaitable. It allows you to use the await keyword without a Task.
        /// </summary>
        public class CreatePortMappingAwaitable(int port)
        {
            private readonly int port = port;

            public CreatePortMappingAwaiter GetAwaiter()
            {
                return new CreatePortMappingAwaiter(this.port);
            }
        }
    }
}

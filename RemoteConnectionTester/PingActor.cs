using Akka.Actor;
using Akka.Event;
using Common;
using System;

namespace RemoteConnectionTester;

internal sealed class PingActor : ReceiveActor
{
    internal sealed record PingRequest();

    private readonly ILoggingAdapter logger = Context.GetLogger();

    private readonly string hostname, port;
    private readonly TimeSpan timeoutInterval = TimeSpan.FromSeconds(5);

    public static Props CreateProps(string hostname, string port)
        => Props.Create(() => new PingActor(hostname, port));

    public PingActor(string hostname, string port)
    {
        this.hostname = hostname;
        this.port = port;

        Receive<Pong>(_ => logger.Info($"Received {nameof(Pong)}"));

        Receive<PingRequest>(_ =>
        {
            logger.Info($"Received {nameof(PingRequest)}");

            var remotePath = $"akka.tcp://server-actor-system@{this.hostname}:{this.port}/user/pong-actor";
            logger.Info($"Remote path: {remotePath}");

            var pongActor = Context.ActorSelection(remotePath);

            pongActor.Ask<Pong>(new Ping(), timeoutInterval).PipeTo(Self);
        });
    }
}
using Akka.Actor;
using Akka.Event;
using Common;

namespace Server;

public sealed class PongActor : ReceiveActor
{
    private readonly ILoggingAdapter logger = Context.GetLogger();

    public PongActor()
    {
        Receive<Ping>(_ =>
        {
            logger.Info($"Received {nameof(Ping)}");

            Sender.Tell(new Pong());
        });
    }
}

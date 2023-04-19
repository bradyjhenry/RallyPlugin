using AssettoServer.Network.Packets.Incoming;
using AssettoServer.Network.Packets.Shared;
using AssettoServer.Server;

namespace RallyPlugin;

public class EntryCarRally
{
    public EntryCar EntryCar { get; }

    public int CurrentFlags { get; set; }

    public bool insideStartingBox { get; set; }

    public bool hasStopped { get; set; }

    public bool isReadySent { get; set; }

    public EntryCarRally(EntryCar entryCar)
    {
        EntryCar = entryCar;
        EntryCar.PositionUpdateReceived += OnPositionUpdateReceived;
        EntryCar.ResetInvoked += OnResetInvoked;
        hasStopped = false;
        insideStartingBox = false;
        isReadySent = false;
    }

    private void OnResetInvoked(EntryCar sender, EventArgs args)
    {
        CurrentFlags = 0;
        hasStopped = false;
        insideStartingBox = false;
        isReadySent = false;
    }

    private void OnPositionUpdateReceived(EntryCar sender, in PositionUpdateIn positionUpdateIn)
    {
        var velocity = EntryCar.Status.Velocity;
        var speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);

        if (insideStartingBox && speed <= 0.5)
        {
            hasStopped = true;
        }
    }

    private void SendMessage(EntryCar car, string message)
    {
        car.Client?.SendPacket(new ChatMessage { SessionId = 255, Message = message });
    }
}

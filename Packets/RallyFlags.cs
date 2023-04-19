using AssettoServer.Network.Packets;
using AssettoServer.Network.Packets.Outgoing;

namespace RallyPlugin.Packets;

public class RallyFlags : IOutgoingNetworkPacket
{
    public Flags Flags { get; set; }

    public void ToWriter(ref PacketWriter writer)
    {
        writer.Write((byte)ACServerProtocol.Extended);
        writer.Write((byte)CSPMessageTypeTcp.ClientMessage);
        writer.Write<byte>(255);
        writer.Write<ushort>(60000);
        writer.Write(0x79E75238);
        writer.Write(Flags);
    }
}

[Flags]
public enum Flags : byte
{
    Open = 1,
    Occupied = 2,
    Wait = 3,
    Stop = 4,
    Ready = 5,
    Penalty = 6
}

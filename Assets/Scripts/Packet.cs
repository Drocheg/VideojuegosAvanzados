
public abstract class Packet
{
    private int packetType;

    protected Packet(int packetType)
    {
        this.packetType = packetType;
    }
}
public interface IChannelReader {
	void Read(Packet packet, BitReader bitReader);
}
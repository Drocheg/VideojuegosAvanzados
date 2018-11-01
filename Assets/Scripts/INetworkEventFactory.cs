public interface INetworkEventFactory {
	void Serialize(BitWriter writer);
	INetworkEvent Deserialize(BitReader reader);
}
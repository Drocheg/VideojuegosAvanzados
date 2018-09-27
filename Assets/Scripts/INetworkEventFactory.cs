public interface INetworkEventFactory {
	INetworkEvent Serialize(BitWriter writer);
	INetworkEvent Deserialize(BitReader reader);
}
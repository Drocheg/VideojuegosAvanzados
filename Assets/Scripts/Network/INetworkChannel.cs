using System.Collections.Generic;


public interface INetworkChannel {
	List<Packet> ReceivePackets();
	void EnqueRecvPacket(Packet packet);
	void SendPacket(Serialize serializable);
	List<Packet> GetPacketsToSend();
}

public enum ChanelType {
	UNRELIABLE,
	RELIABLE,
	TIMED,
}
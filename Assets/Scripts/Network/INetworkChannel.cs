using System.Collections.Generic;


public interface INetworkChannel {
	List<Packet> Receive();
	void EnqueRecvPacket(Packet packet);
	void Send(ISerial serializable);
}

public enum ChanelType {
	UNRELIABLE,
	RELIABLE,
	TIMED,
}
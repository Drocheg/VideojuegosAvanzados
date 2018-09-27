using System;
using System.Collections.Generic;

public class UnreliableReader : IChannelReader
{
	private ulong _lastSeq;
	private INetworkEventFactory _factory;
	private Queue<INetworkEvent> _queue;

	public void Read(Packet packet, BitReader bitReader) {
		if (packet.seq >= _lastSeq) {
			var serial = _factory.Deserialize(bitReader);
			_queue.Enqueue(serial);
		}
	}

	public INetworkEvent Dequeue() {
		if (_queue.Peek() == null) {
			return null;
		}
		return _queue.Dequeue();
	}
}
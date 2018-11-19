public class DisconnectCommand {
	
	public uint playerId;
	public uint maxPlayers;
	
	public DisconnectCommand(uint playerId, uint maxPlayers)
	{
		this.playerId = playerId;
		this.maxPlayers = maxPlayers;
	}

	public void Serialize(BitWriter writer) {
		writer.WriteInt((uint) NetworkCommand.DISCONNECT_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt(playerId, 0, maxPlayers);
	}

	public static DisconnectCommand Deserialize(BitReader reader, uint maxPlayers) {
		return new DisconnectCommand((uint)reader.ReadInt(0, (int)maxPlayers), maxPlayers);
	}
}
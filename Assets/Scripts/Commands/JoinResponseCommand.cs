public class JoinResponseCommand
{
	private uint playerId;
	private uint maxPlayers;
	
	public JoinResponseCommand(uint playerId, uint maxPlayers)
	{
		this.playerId = playerId;
		this.maxPlayers = maxPlayers;

	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt((uint) NetworkCommand.JOIN_RESPONSE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt(playerId, 0, maxPlayers);
	}

	public static JoinResponseCommand Deserialize(BitReader reader, uint playerId, uint maxPlayers) {
		return new JoinResponseCommand(playerId, maxPlayers);
	}
}
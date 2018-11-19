using UnityEngine;

public class JoinResponseCommand
{
	public uint playerId;
	public uint maxPlayers;
	
	public JoinResponseCommand(uint playerId, uint maxPlayers)
	{
		this.playerId = playerId;
		this.maxPlayers = maxPlayers;
		
		Debug.Log("Creating Join Response Command playerId: " + playerId);
	}

	public void Serialize(BitWriter writer) {
		writer.WriteInt((uint) NetworkCommand.JOIN_RESPONSE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt(playerId, 0, maxPlayers);
	}

	public static JoinResponseCommand Deserialize(BitReader reader, uint maxPlayers) {
		return new JoinResponseCommand((uint)reader.ReadInt(0, (int)maxPlayers), maxPlayers);
	}
}
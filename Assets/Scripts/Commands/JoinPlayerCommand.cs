using UnityEngine;

public class JoinPlayerCommand
{
	public uint playerId;
	public uint maxPlayers;
	
	public JoinPlayerCommand(uint playerId, uint maxPlayers)
	{
		this.playerId = playerId;
		this.maxPlayers = maxPlayers;
		
		Debug.Log("Creating Join Player Command playerId: " + playerId);
	}

	public void Serialize(BitWriter writer) {
		writer.WriteInt((uint) NetworkCommand.JOIN_PLAYER_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt(playerId, 0, maxPlayers);
	}

	public static JoinPlayerCommand Deserialize(BitReader reader, uint maxPlayers) {
		return new JoinPlayerCommand((uint)reader.ReadInt(0, (int)maxPlayers), maxPlayers);
	}
}
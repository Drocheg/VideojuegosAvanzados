using UnityEngine;

public class PlayerInfoCommand
{
	public uint playerId;
	public string Name;
	public uint maxPlayers;
	
	public PlayerInfoCommand(string name, uint playerId, uint maxPlayers) {
		Debug.Log("Creating Player Info Command name " + name + " id: " + playerId);
		Name = name;
		this.playerId = playerId;
		this.maxPlayers = maxPlayers;
	}

	public void Serialize(BitWriter writer) {
		writer.WriteInt((uint) NetworkCommand.PLAYER_INFO_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt((uint)Name.Length, 0, 100);
		writer.WriteString(Name);
		writer.WriteInt(playerId, 0, maxPlayers);
	}

	public static PlayerInfoCommand Deserialize(BitReader reader, uint maxPlayers)
	{
		int size = reader.ReadInt(0, 100);
		return new PlayerInfoCommand(reader.ReadString(size), (uint)reader.ReadInt(0, (int)maxPlayers), maxPlayers);
	}
}
using UnityEngine;

public class JoinCommand {
	
	
	public JoinCommand() {
		Debug.Log("Creating Join Command");
	}

	public void Serialize(BitWriter writer) {
		// 
		writer.WriteInt((uint) NetworkCommand.JOIN_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
	}

	public static JoinCommand Deserialize() {
		return new JoinCommand();
	}
}
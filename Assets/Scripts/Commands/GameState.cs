public class GameState {
	public uint TotalPlayers, MaxTotalPlayers, MaxDeaths, MaxKills, MaxEntities;
	public uint[] Ids;
	public uint[] Deaths;
	public uint[] Kills;

	public void Serialize(BitWriter writer) {
		writer.WriteInt((uint) NetworkCommand.GAME_STATE_COMMAND, 0, (uint) System.Enum.GetValues(typeof(NetworkCommand)).Length);
		writer.WriteInt(TotalPlayers, 0, MaxTotalPlayers);
		for (int i = 0; i < TotalPlayers; i++) {
			writer.WriteInt(Ids[i], 0, MaxEntities);
			writer.WriteInt(Deaths[i], 0, MaxDeaths );
			writer.WriteInt(Kills[i], 0, MaxKills);
		}	
	}

	public static GameState Deserialize(
			BitReader reader,
			uint TotalPlayers,
			uint MaxTotalPlayers,
			uint MaxDeaths,
			uint MaxKills,
			uint MaxEntities
			) {
		var totalPlayers = reader.ReadInt(0, (int) MaxTotalPlayers);
		var deaths = new uint[totalPlayers];
		var kills = new uint[totalPlayers];
		var ids = new uint[totalPlayers];
		for (int i = 0; i < totalPlayers; i++) {
			ids[i] = (uint) reader.ReadInt(0, (int) MaxEntities);
			deaths[i] = (uint) reader.ReadInt(0, (int) MaxDeaths);
			kills[i] = (uint) reader.ReadInt(0, (int) MaxKills);
		}
		return new GameState() {
			TotalPlayers = TotalPlayers,
			MaxTotalPlayers = MaxTotalPlayers,
			MaxDeaths = MaxDeaths,
			MaxEntities = MaxEntities,
			MaxKills = MaxKills,
			Ids = ids,
			Deaths = deaths,
			Kills = kills,
		};
	}
}
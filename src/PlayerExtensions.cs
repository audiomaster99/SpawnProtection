namespace SpawnProt
{
	using CounterStrikeSharp.API.Core;
	using static global::SpawnProt.SpawnProt;
	public static class PlayerExtensions
	{
		public static bool IsProtected(this CCSPlayerController player)
		{
			return playerHasSpawnProt[player.Index] == SpawnProtectionState.Protected;
		}

		public static bool IzGud(this CCSPlayerController? player)
		{
			return player?.IsValid == true && player.PlayerPawn?.IsValid == true && player?.Connected == PlayerConnectedState.PlayerConnected;
		}

		public static bool IsAlive(this CCSPlayerController player)
		{
			return player?.PlayerPawn.Value?.LifeState == (byte)LifeState_t.LIFE_ALIVE;
		}
	}
}
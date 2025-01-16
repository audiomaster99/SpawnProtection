namespace SpawnProtection
{
	using CounterStrikeSharp.API.Core;
	using static SpawnProtection.SpawnProt;

	public static class Extensions
	{
		public static bool IsProtected(this CCSPlayerController player)
			=> Instance.protectedPlayers.Contains(player)
			|| Instance._playerStates.TryGetValue(player.Index, out var playerState)
			&& playerState.ProtectionState == ProtectionState.Protected;

		public static bool IzGud(this CCSPlayerController? player)
			=> player?.IsValid == true && player.PlayerPawn?.IsValid == true;

		public static bool IsBot(this CCSPlayerController? player)
			=> player?.IsBot == true || player?.IsHLTV == true;

		public static bool IsAlive(this CCSPlayerController player)
			=> player.Pawn.Value?.LifeState == (byte)LifeState_t.LIFE_ALIVE && player.PlayerPawn.Value?.Health > 0;
	}
}
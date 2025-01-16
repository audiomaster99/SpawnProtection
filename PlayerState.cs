namespace SpawnProtection
{
	using CounterStrikeSharp.API.Modules.Timers;
	using static SpawnProtection.SpawnProt;

	public class PlayerState
	{
		public ProtectionState ProtectionState { get; set; } = ProtectionState.None;
		public bool ShowCenterMessage { get; set; }
		public float ProtectionTimer { get; set; } = Instance.Config.SpawnProtTime;
		public Timer? SpawnTimer { get; set; }
	}
}
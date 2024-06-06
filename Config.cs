namespace SpawnProt
{
	using CounterStrikeSharp.API.Core;
	using System.Text.Json.Serialization;

	public sealed class PluginConfig : BasePluginConfig
	{
		[JsonPropertyName("spawn-protection-time")]
		public float SpawnProtTime { get; set; } = 10.0f;

		[JsonPropertyName("spawn-prot-center-message")]
		public bool SpawnProtCenterMsg { get; set; } = true;

		[JsonPropertyName("spawn-prot-end-announce")]
		public bool SpawnProtEndAnnouce { get; set; } = true;

		[JsonPropertyName("attacker-center-message")]
		public bool AttackerCenterMsg { get; set; } = true;

		[JsonPropertyName("enable-center-html-message")]
		public bool CenterHtmlMessage { get; set; } = true;

		[JsonPropertyName("spawn-prot-transparent-model")]
		public bool TransparentModel { get; set; } = true;

		[JsonPropertyName("ct-protection-only")]
		public bool CTProtOnly { get; set; } = false;

		[JsonPropertyName("trigger-hurt-compatibility")]
		public bool TriggerHurtEnabled { get; set; } = false;

		[JsonPropertyName("stop-protection-on-player-move")]
		public bool StopProtectionOnMove { get; set; } = false;

		[JsonPropertyName("stop-protection-on-weapon-fire")]
		public bool StopProtectionOnWeaponFire { get; set; } = false;

		[JsonPropertyName("ConfigVersion")]
		public override int Version { get; set; } = 4;
	}
}
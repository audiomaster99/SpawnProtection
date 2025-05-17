namespace SpawnProtection
{
	using System.Drawing;
	using CounterStrikeSharp.API;
	using CounterStrikeSharp.API.Core;
	using CounterStrikeSharp.API.Modules.Timers;

	public sealed partial class SpawnProt
	{
		public enum ProtectionState
		{
			None,
			Protected
		}

		private string GenerateProgressBar(float progress)
		{
			int totalBars = Math.Max(1, int.Parse(Localizer["progressbar.total_bars"].Value));
			var filledBars = Math.Max(0, Math.Min(totalBars, (int)(totalBars * progress)));
			var emptyBars = Math.Max(0, totalBars - filledBars);

			string filledPart = new string(Localizer["progressbar.filled_part"].Value[0], filledBars);
			string emptyPart = new string(Localizer["progressbar.empty_part"].Value[0], emptyBars);

			return $"<font color='{GetColorBasedOnProgress(progress, 1.0f)}' class='fontSize-l'>{filledPart}</font>" +
				   $"<font color='{GetColorBasedOnProgress(progress, 0.6f)}' class='fontSize-l'>{emptyPart}</font>";
		}

		private string GetColorBasedOnProgress(float progress, float brightness = 1.0f)
		{
			progress = Math.Clamp(progress, 0, 1);
			brightness = Math.Clamp(brightness, 0, 1);

			int red, green, blue = 0;

			if (progress < 0.5f)
			{
				red = (int)(255 * brightness);
				green = (int)(255 * (progress * 2) * brightness);
			}
			else
			{
				red = (int)(255 * (2 * (1 - progress)) * brightness);
				green = (int)(255 * brightness);
			}

			return $"#{red:X2}{green:X2}{blue:X2}";
		}

		private void HandleTransparentModel(CCSPlayerController? player, bool isTransparent = false)
		{
			if (!Config.TransparentModel)
				return;

			if (!player.IzGud() || !player.IsAlive() || player.PlayerPawn.Value == null)
				return;

			if (isTransparent)
			{
				player.PlayerPawn!.Value!.Render = Color.FromArgb(255, 255, 255, 255);
			}
			else
			{
				player.PlayerPawn.Value.Render = Color.FromArgb(185, 0, 255, 0);
			}

			Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
		}

		private void CreateProtectionTimer(CCSPlayerController player, PlayerState state)
		{
			state.SpawnTimer?.Kill();
			state.SpawnTimer = AddTimer(0.1f, () =>
			{
				if (state.ProtectionTimer <= 0.1f)
				{
					StopSpawnProtection(player, state);
					return;
				}
				state.ProtectionTimer -= 0.1f;
			}, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
		}

		void GetGameRules() => gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

		public bool IsWarmup
		{
			get
			{
				if (gameRules is null)
					GetGameRules();

				return gameRules is not null && gameRules.WarmupPeriod;
			}
		}

		public bool IsFreezeTime
		{
			get
			{
				if (gameRules is null)
					GetGameRules();

				return gameRules is not null && gameRules.FreezePeriod;
			}
		}
	}
}
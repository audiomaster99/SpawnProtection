namespace SpawnProt
{
	using System.Drawing;
	using CounterStrikeSharp.API;
	using CounterStrikeSharp.API.Core;
	using CounterStrikeSharp.API.Modules.Utils;

	public sealed partial class SpawnProt
	{
		public enum SpawnProtectionState
		{
			None,
			Protected
		}

		private string GenerateProgressBar(float progress)
		{
			int totalBars = 19;
			int filledBars = (int)(totalBars * progress);

			string filledPart = new string('█', filledBars);
			string emptyPart = new string('░', totalBars - filledBars);
			return $"{filledPart}{emptyPart}";
		}

		private string GetColorBasedOnProgress(float progress)
		{
			int red, green, blue;

			if (progress < 0.40)
			{
				red = 255;
				green = (int)(255 * progress * 3);
				blue = 0;
			}
			else if (progress < 0.66)
			{
				red = 255;
				green = 255;
				blue = 0;
			}
			else
			{
				red = (int)(255 * (1 - ((progress - 0.86) * 3)));
				green = 255;
				blue = 0;
			}

			// Clamp the values to avoid any overflow or underflow
			red = Math.Clamp(red, 0, 255);
			green = Math.Clamp(green, 0, 255);
			blue = Math.Clamp(blue, 0, 255);

			return $"#{red:X2}{green:X2}{blue:X2}";
		}

		private void SetPlayerColor(CCSPlayerController? player)
		{

			if (player is null)
				return;
			Color transparentColor;

			if (player.TeamNum == (byte)CsTeam.Terrorist)
			{
				transparentColor = Color.FromArgb(180, 100, 0, 0);
			}
			else transparentColor = Color.FromArgb(180, 0, 0, 100);

			if (player.PlayerPawn is not null && player.PlayerPawn.Value is not null && player.IsAlive())
			{
				player.PlayerPawn.Value.Render = transparentColor;
				Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
			}
		}

		private void ResetPlayerColor(CCSPlayerController? player)
		{

			if (player is null)
				return;

			Color defaultColor = Color.FromArgb(255, 255, 255, 255);
			Server.NextFrame(() =>
			{
				if (player.PlayerPawn is not null && player.PlayerPawn.Value is not null && player.IsAlive())
				{
					player.PlayerPawn.Value.Render = defaultColor;
					Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
				}
			});
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
	}


}
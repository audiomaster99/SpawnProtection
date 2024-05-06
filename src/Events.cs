namespace SpawnProt
{
	using CounterStrikeSharp.API;
	using CounterStrikeSharp.API.Core;
	using CounterStrikeSharp.API.Modules.Cvars;
	using CounterStrikeSharp.API.Modules.Memory;
	using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
	using CounterStrikeSharp.API.Modules.Timers;
	using CounterStrikeSharp.API.Modules.Utils;
	using Microsoft.Extensions.Logging;

	public sealed partial class SpawnProt
	{
		public void RegisterEventsListeners()
		{
			RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
			RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
			RegisterEventHandler<EventRoundStart>(OnRoundStart);
			RegisterEventHandler<EventRoundPrestart>(OnRoundPrestart);
			RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
			RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
			VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

			RegisterListener<Listeners.OnTick>(() =>
			{
				if (Config.CenterHtmlMessage && (!IsWarmup || gameRules is null))
				{
					Utilities.GetPlayers().Where(player => player.IzGud() && player.IsAlive() && CenterMessage[player.Index] == true).ToList().ForEach(p => OnTick(p));
				}
			});

			RegisterListener<Listeners.OnMapStart>(name =>
			{
				gameRules = null;
				AddTimer(1.0f, GetGameRules);
				FreezeTime = ConVar.Find("mp_freezetime")!.GetPrimitiveValue<int>();
			});
			Logger.LogInformation("Registered Events and Listeners");
		}

		public CounterStrikeSharp.API.Modules.Timers.Timer? SpawnTimer;
		public CounterStrikeSharp.API.Modules.Timers.Timer? renderTimer;
		public float CountdownTimer;

		public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
		{
			SpawnTimer?.Kill();
			CountdownTimer = Config.SpawnProtTime;

			if (IsWarmup)
				return HookResult.Continue;

			AddTimer(FreezeTime, () =>
			{
				SpawnTimer = AddTimer(0.1f, () =>
				{
					if (CountdownTimer <= 0) { SpawnTimer?.Kill(); return; }
					CountdownTimer -= 0.1f;
				}, TimerFlags.REPEAT);
			});
			return HookResult.Continue;
		}
		public HookResult OnRoundPrestart(EventRoundPrestart @event, GameEventInfo info)
		{
			Utilities.GetPlayers().
			Where(player => player is not null && player.IsValid == true).ToList().ForEach(x => HandleCenterMessage(x));

			return HookResult.Continue;
		}

		public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
		{
			Utilities.GetPlayers().
			Where(player => player is not null && player.IsValid == true).ToList().ForEach(x => CenterMessage[x.Index] = false);

			SpawnTimer?.Kill();

			return HookResult.Continue;
		}

		public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
		{
			CCSPlayerController? player = @event.Userid;

			if (player is null)
				return HookResult.Continue;

			CenterMessage[player.Index] = false;
			playerHasSpawnProt[player.Index] = SpawnProtectionState.None;

			return HookResult.Continue;
		}

		public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
		{
			CCSPlayerController? player = @event.Userid;

			if (player is null || !player.IzGud())
				return HookResult.Continue;

			if (Config.CTProtOnly && player.Team == CsTeam.Terrorist)
				return HookResult.Continue;

			if (IsWarmup)
				return HookResult.Continue;

			AddTimer(FreezeTime, () =>
			{
				HandleSpawnProt(player);

				if (Config.TransparentModel)
					HandlePlayerModel(player);
			});

			return HookResult.Continue;
		}

		public static HookResult OnTakeDamage(DynamicHook hook)
		{
			CEntityInstance entity = hook.GetParam<CEntityInstance>(0);
			var playerPawn = hook.GetParam<CCSPlayerPawn>(0);
			var player = playerPawn?.Controller.Value?.As<CCSPlayerController>();

			if (player is null)
				return HookResult.Continue;

			if (entity.DesignerName != "trigger_hurt" || !player.IsProtected())
				return HookResult.Continue;

			hook.SetReturn(false);
			return HookResult.Handled;
		}

		public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
		{
			CCSPlayerController? player = @event.Userid;
			CCSPlayerController? Attacker = @event.Attacker;

			if (player is null || !player.IsProtected() || !player.IzGud() || !player.IsAlive())
				return HookResult.Continue;

			if (player.PlayerPawn.Value is null || player.PlayerPawn.Value is null)
				return HookResult.Continue;

			HandleProtectedPlayer(player, @event);

			return HookResult.Continue;
		}

		private void HandleProtectedPlayer(CCSPlayerController player, EventPlayerHurt @event)
		{
			player.PlayerPawn!.Value!.Health += @event.DmgHealth;
			Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");

			player.PlayerPawn.Value.ArmorValue += @event.DmgArmor;
			Utilities.SetStateChanged(player.PlayerPawn.Value, "CCSPlayerPawn", "m_ArmorValue");

			if (Config.SpawnProtCenterMsg && player.IsAlive())
				player.PrintToCenter($"{Localizer["player_isprotected", (int)CountdownTimer]}");

			CCSPlayerController? Attacker = @event.Attacker;

			if (Attacker != null && Config.AttackerCenterMsg && Attacker.IsAlive())
				Attacker.PrintToCenter($" {Localizer["attacker_playerisprotected", player.PlayerName]}");
		}
	}
}
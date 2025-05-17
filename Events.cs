namespace SpawnProtection
{
	using CounterStrikeSharp.API.Core;
	using CounterStrikeSharp.API.Modules.Cvars;
	using CounterStrikeSharp.API.Modules.Memory;
	using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
	using CounterStrikeSharp.API.Modules.Utils;

	public sealed partial class SpawnProt
	{
		public void RegisterEventsListeners()
		{
			RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Pre);
			RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
			RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Pre);
			RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
			RegisterEventHandler<EventPlayerActivate>(OnPlayerActivate);
			RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect, HookMode.Pre);

			VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

			if (Config.CenterHtmlMessage)
			{
				RegisterListener<Listeners.OnTick>(() =>
				{
					if (Config.DisableDuringWarmup && (IsWarmup || gameRules is null))
						return;

					protectedPlayers.ToList().ForEach(OnTick);
				});
			}

			RegisterListener<Listeners.OnMapStart>(name =>
			{
				protectedPlayers.Clear();
				gameRules = null;

				AddTimer(1.0f, () =>
				{
					_freezeTime = ConVar.Find("mp_freezetime")!.GetPrimitiveValue<int>();
					GetGameRules();
				});
			});
		}

		public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
		{
			protectedPlayers.Clear();
			foreach (var state in _playerStates.Values)
			{
				state.ShowCenterMessage = false;
				state.SpawnTimer?.Kill();
			}

			return HookResult.Continue;
		}

		public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
		{
			var player = @event.Userid;

			if (!player.IzGud() || !_playerStates.TryGetValue(player.Index, out var state))
				return HookResult.Continue;

			protectedPlayers.Remove(player);
			StopSpawnProtection(player, state, true);

			return HookResult.Continue;
		}

		public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
		{
			var player = @event.Userid;

			if (!player.IzGud() || (Config.CTProtOnly && player.Team == CsTeam.Terrorist) || IsWarmup)
				return HookResult.Continue;

			StartSpawnProtection(player);

			return HookResult.Continue;
		}

		public static HookResult OnTakeDamage(DynamicHook hook)
		{
			CEntityInstance entity = hook.GetParam<CEntityInstance>(0);
			var playerPawn = hook.GetParam<CCSPlayerPawn>(0);
			var player = playerPawn?.Controller.Value?.As<CCSPlayerController>();

			if (!player.IzGud() || !player.IsProtected())
				return HookResult.Continue;

			hook.SetReturn(false);
			return HookResult.Handled;
		}

		public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
		{
			if (!Config.StopProtectionOnWeaponFire)
				return HookResult.Continue;

			CCSPlayerController? player = @event.Userid;

			if (!player.IzGud() || !player.IsProtected())
				return HookResult.Continue;

			StopSpawnProtection(player, _playerStates[player.Index]);

			return HookResult.Continue;
		}

		public HookResult OnPlayerActivate(EventPlayerActivate @event, GameEventInfo _)
		{
			CCSPlayerController? player = @event.Userid;

			if (player.IzGud())
				playerCache.Add(player);

			return HookResult.Continue;
		}

		public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo _)
		{
			CCSPlayerController? player = @event.Userid;

			if (!player.IzGud())
				return HookResult.Continue;

			protectedPlayers.Remove(player);
			playerCache.Remove(player);

			return HookResult.Continue;
		}

	}
}
namespace SpawnProtection
{
    using CounterStrikeSharp.API;
    using CounterStrikeSharp.API.Core;
    using CounterStrikeSharp.API.Modules.Cvars;
    using CounterStrikeSharp.API.Modules.Extensions;
    using CounterStrikeSharp.API.Modules.Memory;
    using Microsoft.Extensions.Logging;

    public sealed partial class SpawnProt : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "SpawnProtection";
        public override string ModuleAuthor => "itsAudio @ Kitsune-Lab.com";
        public override string ModuleDescription => "https://github.com/audiomaster99/SpawnProtection";
        public override string ModuleVersion => "1.0.2";

        internal readonly Dictionary<uint, PlayerState> _playerStates = new();
        public static HashSet<CCSPlayerController> playerCache = [];
        public HashSet<CCSPlayerController> protectedPlayers = [];
        private CCSGameRules? gameRules;
        private float _freezeTime;

        public PluginConfig Config { get; set; } = new PluginConfig();
        public static SpawnProt Instance { get; private set; } = new();

        public void OnConfigParsed(PluginConfig config)
        {
            if (config.Version < Config.Version)
            {
                base.Logger.LogWarning("Plugin configuration is outdated! Please consider updating. [Expected: {0} | Current: {1}]", this.Config.Version, config.Version);
                config.Update<PluginConfig>();
            }

            this.Config = config;
        }

        public override void Load(bool hotReload)
        {
            Instance = this;

            RegisterEventsListeners();

            _freezeTime = ConVar.Find("mp_freezetime")!.GetPrimitiveValue<int>();

            if (hotReload)
            {
                _playerStates.Clear();
                playerCache.ToList().ForEach(p => StopSpawnProtection(p, new PlayerState()));
            }
        }

        public void OnTick(CCSPlayerController player)
        {
            if (!IsPlayerValid(player) || !player.IsProtected())
                return;

            DisplayProtectionStatus(player, _playerStates[player.Index]);
            CheckMovementViolation(player, _playerStates[player.Index]);
        }

        private bool IsPlayerValid(CCSPlayerController? player)
        {
            if (player is null || player.IsHLTV || !player.IsValid || player.PlayerPawn.Value is null)
            {
                if (player is not null)
                    protectedPlayers.Remove(player);
                return false;
            }
            return true;
        }

        private void DisplayProtectionStatus(CCSPlayerController player, PlayerState playerState)
        {
            float progressPercentage = playerState.ProtectionTimer / Config.SpawnProtTime;
            string displayMsg;

            if (playerState.ProtectionTimer > 0.1f)
            {
                string color = GetColorBasedOnProgress(progressPercentage);
                displayMsg =
                Localizer["phrases.center_isprotected"] +
                Localizer["phrases.seconds_left", color, (int)playerState.ProtectionTimer] +
                Localizer["progressbar", color, GenerateProgressBar(progressPercentage)];
            }
            else
                displayMsg = Localizer["phrases.protection_ended"];

            player.PrintToCenterHtml(displayMsg);
        }

        private string ProtectionEndedString
            => Localizer["phrases.protection_ended"];

        private void CheckMovementViolation(CCSPlayerController player, PlayerState playerState)
        {
            if (Config.StopProtectionOnMove && player.PlayerPawn.Value!.HasMovedSinceSpawn)
                StopSpawnProtection(player, playerState);
        }

        public void StartSpawnProtection(CCSPlayerController player)
        {
            protectedPlayers.Add(player);

            if (!_playerStates.TryGetValue(player.Index, out var state))
            {
                state = new PlayerState();
                _playerStates[player.Index] = state;
            }

            state.ShowCenterMessage = true;
            state.ProtectionState = ProtectionState.Protected;
            state.ProtectionTimer = Config.SpawnProtTime;
            Server.NextFrame(() => HandleTransparentModel(player));

            AddTimer(IsFreezeTime ? _freezeTime : 0f, () => CreateProtectionTimer(player, state));
        }

        public void StopSpawnProtection(CCSPlayerController player, PlayerState playerState, bool isDead = false)
        {
            protectedPlayers.Remove(player);

            playerState.ShowCenterMessage = false;
            playerState.ProtectionTimer = 0;
            playerState.SpawnTimer?.Kill();
            playerState.ProtectionState = ProtectionState.None;
            Server.NextFrame(() => HandleTransparentModel(player, true));
        }

        public override void Unload(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        }
    }
}

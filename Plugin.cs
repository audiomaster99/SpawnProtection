namespace SpawnProt
{
    using CounterStrikeSharp.API.Core;
    using CounterStrikeSharp.API.Modules.Memory;
    using Microsoft.Extensions.Logging;

    public sealed partial class SpawnProt : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "SpawnProt";
        public override string ModuleAuthor => "audio_brutalci";
        public override string ModuleDescription => "Simple spawn protection for CS2";
        public override string ModuleVersion => "0.0.5";

        public static SpawnProtectionState[] playerHasSpawnProt = new SpawnProtectionState[64];
        public static readonly bool[] CenterMessage = new bool[64];
        public static readonly float[] protTimer = new float[64];
        public static int FreezeTime;
        CCSGameRules? gameRules;
        public required PluginConfig Config { get; set; } = new PluginConfig();

        public void OnConfigParsed(PluginConfig config)
        {
            if (config.Version < Config.Version)
            {
                base.Logger.LogWarning("Plugin configuration is outdated! Please consider updating. [Expected: {0} | Current: {1}]", this.Config.Version, config.Version);
            }

            this.Config = config;
        }

        public override void Load(bool hotReload)
        {
            RegisterEventsListeners();

            if (hotReload)
            {
                SpawnTimer?.Kill();
            }
        }

        public void OnTick(CCSPlayerController player)
        {
            float progressPercentage = protTimer[player.Index] / Config.SpawnProtTime;
            string color = GetColorBasedOnProgress(progressPercentage);
            string progressBar = GenerateProgressBar(progressPercentage);

            player.PrintToCenterHtml(
                $"<font class='fontSize-m' color='orange'>{Localizer["center_isprotected"]}</font><br>" +
                $"[<font class='fontSize-m' color='{color}'>{(int)protTimer[player.Index]}</font><font color='white'>] SECONDS LEFT!</font><br>" +
                $"<font class='fontSize-l' color='{color}'>{progressBar}</font>"
            );
        }

        public void HandleSpawnProt(CCSPlayerController player)
        {
            int freezeTimer;

            if (IsFreezeTime)
                freezeTimer = FreezeTime;
            else
                freezeTimer = 0;

            AddTimer(freezeTimer, () =>
            {
                playerHasSpawnProt[player.Index] = SpawnProtectionState.Protected;

                AddTimer(Config.SpawnProtTime, () =>
                {
                    playerHasSpawnProt[player.Index] = SpawnProtectionState.None;
                    AddTimer(0.8f, () => { player.PrintToCenterAlert($" {Localizer["player_isnotprotected"]} "); });
                });
            });
        }

        public void HandlePlayerModel(CCSPlayerController player)
        {
            if (player is null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value is null)
                return;

            int freezeTimer;

            if (IsFreezeTime)
                freezeTimer = FreezeTime;
            else
                freezeTimer = 0;

            AddTimer(freezeTimer, () =>
            {
                SetPlayerColor(player);
                AddTimer(Config.SpawnProtTime, () => { ResetPlayerColor(player); });
            });
        }

        public void HandleCenterMessage(CCSPlayerController player)
        {
            int freezeTimer;

            if (IsFreezeTime)
                freezeTimer = FreezeTime;
            else
                freezeTimer = 0;

            AddTimer(freezeTimer, () =>
            {
                CenterMessage[player.Index] = true;
                AddTimer(Config.SpawnProtTime, () => { CenterMessage[player.Index] = false; });
            });
        }

        public override void Unload(bool hotReload)
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        }
    }
}

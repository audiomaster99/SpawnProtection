namespace SpawnProt
{
    using CounterStrikeSharp.API.Core;
    using Microsoft.Extensions.Logging;

    public sealed partial class SpawnProt : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "SpawnProt";
        public override string ModuleAuthor => "audio_brutalci";
        public override string ModuleDescription => "Simple spawn protection for CS2";
        public override string ModuleVersion => "0.0.3";

        public static SpawnProtectionState[] playerHasSpawnProt = new SpawnProtectionState[64];
        public static readonly bool[] CenterMessage = new bool[64];
        public static int FreezeTime;
        CCSGameRules? gameRules;

        public required Config Config { get; set; }

        public void OnConfigParsed(Config config)
        {
            if (config.Version < Config.Version)
            {
                base.Logger.LogWarning("Plugin config is outdated. Please consider updating the configuration file. [Expected: {0} | Current: {1}]", this.Config.Version, config.Version);
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
            float progressPercentage = CountdownTimer / Config.SpawnProtTime;
            string color = GetColorBasedOnProgress(progressPercentage);
            string progressBar = GenerateProgressBar(progressPercentage);

            player.PrintToCenterHtml(
                $"<font class='fontSize-m' color='orange'>{Localizer["center_isprotected"]}</font><br>" +
                $"[<font class='fontSize-m' color='{color}'>{(int)CountdownTimer}</font><font color='white'>] SECONDS LEFT!</font><br>" +
                $"<font class='fontSize-l' color='{color}'>{progressBar}</font>"
            );
        }

        public void HandleSpawnProt(CCSPlayerController player)
        {
            playerHasSpawnProt[player.Index] = SpawnProtectionState.Protected;

            AddTimer(Config.SpawnProtTime, () =>
            {
                playerHasSpawnProt[player.Index] = SpawnProtectionState.None;
                AddTimer(1.0f, () => { player.PrintToCenterAlert($" {Localizer["player_isnotprotected"]} "); });
            });
        }

        public void HandlePlayerModel(CCSPlayerController player)
        {
            if (player is null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value is null)
                return;

            SetPlayerColor(player);
            AddTimer(Config.SpawnProtTime, () => { ResetPlayerColor(player); });
        }

        public void HandleCenterMessage(CCSPlayerController player)
        {
            CenterMessage[player.Index] = true;
            AddTimer(Config.SpawnProtTime + 1.0f, () => { CenterMessage[player.Index] = false; });
        }
    }
}

namespace SpawnProt;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json.Serialization;

public class Config : BasePluginConfig
{
    [JsonPropertyName("spawn-prot-time")]
    public float SpawnProtTime { get; set; } = 5.0f;
    [JsonPropertyName("spawn-prot-center-message")]
    public bool SpawnProtCenterMsg { get; set; } = true;
    [JsonPropertyName("spawn-prot-end-announce")]
    public bool SpawnProtEndAnnouce { get; set; } = true;
    [JsonPropertyName("spawn-prot-end-message")]
    public string SpawnProtEndMsg { get; set; } = $" {ChatColors.Red}[SpawnProtection] {ChatColors.Default}You're no longer spawn protected!";
    [JsonPropertyName("attacker-center-message")]
    public bool AttackerCenterMsg { get; set; } = true;
    [JsonPropertyName("spawn-prot-transparent-model")]
    public bool TransparentModel { get; set; } = true;
    [JsonPropertyName("ct-protection-only")]
    public bool CTProtOnly { get; set; } = true;
}
public partial class SpawnProt : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "SpawnProt";
    public override string ModuleAuthor => "audio_brutalci";
    public override string ModuleDescription => "Simple spawn protection for CS2";
    public override string ModuleVersion => "0.0.2";


    public required Config Config { get; set; }
    private static readonly int?[] playerHasSpawnProt = new int?[65];

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }
    public override void Load(bool hotReload)
    {
        Logger.LogInformation("SpawnProt Plugin has started!");
    }

    public void HandleSpawnProt(CCSPlayerController player)
    {
        playerHasSpawnProt[player.Index] = 1;

        AddTimer(Config.SpawnProtTime, () => { 
            playerHasSpawnProt[player.Index] = 0;
            player.PrintToChat($" {Config.SpawnProtEndMsg}");
            });
    }
    public void HandlePlayerModel(CCSPlayerController? player)
    {
        Color transparentColor = Color.FromArgb(170, 255, 255, 255);
        Color defaultColor = Color.FromArgb(255, 255, 255, 255);

        if (player == null) { return; }
        player.PlayerPawn.Value!.Render = transparentColor;
        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");

        int playerIndex = (int)player.Index;

        AddTimer(Config.SpawnProtTime, () =>
        {
            player.PlayerPawn.Value!.Render = defaultColor;
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
        });
    }

    //---- P L U G I N - H O O O K S ----
    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        int playerIndex = (int)player.Index;

        if (Config.CTProtOnly && player.TeamNum == (byte)CsTeam.Terrorist)
        {
            return HookResult.Continue;
        }

        if (IsValid(player) && IsConnected(player) && IsAlive(player))
        {
            HandleSpawnProt(player);

            if (Config.TransparentModel)
            {
                HandlePlayerModel(player);
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        var Attacker = @event.Attacker;

        if (playerHasSpawnProt[player.Index] == 1 && IsValid(player) && IsAlive(player))
        {
            if (@event.DmgHealth > 0 && player.PlayerPawn.Value != null)
            {
                player.PlayerPawn.Value.Health += @event.DmgHealth;
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
                if (Config.SpawnProtCenterMsg)
                {
                    player.PrintToCenter("You are invulnerable for a short time");
                }
                Logger.LogInformation($" {Attacker.PlayerName} inflicted {@event.DmgHealth} DMG to {player.PlayerName}. HP {player.PlayerPawn.Value.Health}");
            }
            if (@event.DmgArmor > 0 && player.PlayerPawn.Value != null)
            {
                player.PlayerPawn.Value.ArmorValue += @event.DmgArmor;
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CCSPlayerPawnBase", "m_ArmorValue");
            }
            if (Attacker != null && Config.AttackerCenterMsg) 
            {
                Attacker.PrintToCenter($" {player.PlayerName} is spawn protected and cannot be damaged.");
            }
        }
        return HookResult.Continue; 
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        playerHasSpawnProt[player.Index] = 0;
        return HookResult.Continue; 
    }

    //---- P L U G I N - H E L P E R S ----
    static bool IsValid(CCSPlayerController? player)
    {
        return player?.IsValid == true && player.PlayerPawn?.IsValid == true;
    }
    static bool IsConnected(CCSPlayerController? player)
    {
        return player?.Connected == PlayerConnectedState.PlayerConnected;
    }
    static bool IsAlive(CCSPlayerController player)
    {
        return player?.PlayerPawn.Value?.LifeState == (byte)LifeState_t.LIFE_ALIVE;
    }
}

using System;
using System.ComponentModel.Design;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using VFXToggler.Windows;

namespace VFXToggler;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    
    [PluginService] internal static IChatGui Chat { get; private set; } = null!;

    private const string CommandName = "/vfxtoggler";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("VFXToggler");
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        MainWindow = new MainWindow(this);
        
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        ClientState.TerritoryChanged += OnTerritoryChanged;
        
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    private void OnTerritoryChanged(ushort territoryId)
    {
        try
        {
            var cfcSheet = DataManager.GetExcelSheet<ContentFinderCondition>();
            var cfc = cfcSheet?.GetRow(territoryId);
        
            if (cfc != null && cfc.Value.ContentType.Value.RowId != 0)
            {
                string contentType = null;
                    
                switch(cfc.Value.ContentType.Value.Name.ExtractText())
                {
                    case "Trials":
                        contentType = "Trials";
                        break;
                    case "Dungeons":
                    case "Guildhests":
                    case "V&C Dungeon Finder":
                    case "Deep Dungeons":
                        contentType = "Dungeons";
                        break;
                    case "Raids":
                    case "Chaotic Alliance Raid":
                    case "Ultimate Raids":
                        contentType = "Raids";
                        break;
                    case "PvP":
                        contentType = "PvP";
                        break;
                    default:
                        return;
                }
            
                if (Configuration.ContextualBattleFx.TryGetValue(contentType, out var settingDict))
                {
                    foreach (var keyValuePair in settingDict)
                    {
                        GameConfig.UiConfig.Set(keyValuePair.Key, (uint)keyValuePair.Value);
                    }
                }
            }
            else
            {
                if (Configuration.ContextualBattleFx.TryGetValue("World", out var settingDict))
                {
                    foreach (var keyValuePair in settingDict)
                    {
                        GameConfig.UiConfig.Set(keyValuePair.Key, (uint)keyValuePair.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error($"Failed to get Content Type: {ex}");
        }
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();
    
    public void ToggleMainUI() => MainWindow.Toggle();
}

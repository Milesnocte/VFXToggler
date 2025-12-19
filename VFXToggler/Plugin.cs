using System;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Hosting;
using VFXToggler.Services;
using VFXToggler.UI;

namespace VFXToggler;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/vfxtoggler";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("VFXToggler");
    private MainWindow MainWindow { get; init; }

    [Experimental("PendingExcelSchema")]
    public Plugin(IDalamudPluginInterface pluginInt)
    {
        pluginInt.Create<BaseServices>();
 
        Configuration = BaseServices.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        
        pluginInt.Create<TerritoryChange>(Configuration);
        UiService uiService = pluginInt.Create<UiService>();
        
        MainWindow = new MainWindow(this, uiService, Configuration);
        
        WindowSystem.AddWindow(MainWindow);

        BaseServices.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration window"
        });

        BaseServices.PluginInterface.UiBuilder.Draw += DrawUI;
        BaseServices.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        BaseServices.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();
    
    public void ToggleMainUI() => MainWindow.Toggle();
}

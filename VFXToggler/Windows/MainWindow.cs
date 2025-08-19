using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace VFXToggler.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Dictionary<string, Dictionary<string, uint>> ContextualBattleFx = new();
    
    public MainWindow(Plugin plugin)
        : base("VFX toggler", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 350),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        
        Plugin = plugin;
    }
    
    private readonly string[] battleFxOptions = { "Show All", "Limited", "None" };

    public void Dispose() { }

    public override void Draw()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            if (child.Success)
            {
                ImGui.Text("## Battle Effect Settings");
                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.BeginTabBar("BattleEffectTabs"))
                {
                    if (ImGui.BeginTabItem("Open World (Default)"))
                    {
                        DrawBattleFxRadios("Self", "BattleEffectSelf", "World");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Party", "BattleEffectParty", "World");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Other", "BattleEffectOther", "World");
                        ImGui.Spacing();
                        DrawBattleFxRadios("PvP", "BattleEffectPvPEnemyPc", "World");
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Dungeons"))
                    {
                        DrawBattleFxRadios("Self", "BattleEffectSelf", "Dungeons");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Party", "BattleEffectParty", "Dungeons");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Trials"))
                    {
                        DrawBattleFxRadios("Self", "BattleEffectSelf", "Trials");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Party", "BattleEffectParty", "Trials");
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Raids"))
                    {
                        DrawBattleFxRadios("Self", "BattleEffectSelf", "Raids");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Party", "BattleEffectParty", "Raids");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Other", "BattleEffectOther", "Raids");
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("PvP"))
                    {
                        DrawBattleFxRadios("Self", "BattleEffectSelf", "PvP");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Party", "BattleEffectParty", "PvP");
                        ImGui.Spacing();
                        DrawBattleFxRadios("Other", "BattleEffectOther", "PvP");
                        ImGui.Spacing();
                        DrawBattleFxRadios("PvP Opponents", "BattleEffectPvPEnemyPc", "PvP");
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

            }
        }
    }
    
    private void DrawBattleFxRadios(string label, string key, string type)
    {
        // Get or create the dictionary for the current type (e.g., "Dungeons")
        if (!Plugin.Configuration.ContextualBattleFx.TryGetValue(type, out var settingDict))
        {
            settingDict = new Dictionary<string, uint>();
            Plugin.Configuration.ContextualBattleFx[type] = settingDict;
        }

        // Try to get the stored value; if missing, default to current system config
        if (!settingDict.TryGetValue(key, out var value))
        {
            value = Plugin.GameConfig.UiConfig.GetUInt(key);
            settingDict[key] = value; // store it for later use
        }

        int current = (int)value;

        ImGui.SetWindowFontScale(1.25f);
        ImGui.Text(label);
        ImGui.SetWindowFontScale(1.0f);

        for (int i = 0; i < battleFxOptions.Length; i++)
        {
            ImGui.RadioButton($"{battleFxOptions[i]}##{label}{i}_{type}", current == i);
            if (ImGui.IsItemClicked())
            {
                settingDict[key] = (uint)i;
                Plugin.Configuration.Save(); // optional: save immediately
            }

            if (i < battleFxOptions.Length - 1)
            {
                ImGui.SameLine();
            }
        }
    }

}

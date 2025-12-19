using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets.Experimental;
using VFXToggler;
using VFXToggler.Services;
using VFXToggler.UI.Helper;
using InstanceContent = FFXIVClientStructs.FFXIV.Client.Game.UI.InstanceContent;

namespace VFXToggler.UI;

public class MainWindow : Window
{
    private Plugin Plugin;
    private UiService UiService;
    private Configuration Config;

    private enum InstanceTab
    {
        World,
        Dungeons,
        Trials,
        Raids,
        PvP
    }

    private InstanceTab _instanceTab = InstanceTab.World;

    private readonly (InstanceTab tab, string label, string typeKey)[] _instancePages =
    {
        (InstanceTab.World, "World (Default)", "World"),
        (InstanceTab.Dungeons, "Dungeons", "Dungeons"),
        (InstanceTab.Trials, "Trials", "Trials"),
        (InstanceTab.Raids, "Raids", "Raids"),
        (InstanceTab.PvP, "PvP", "PvP"),
    };

    private enum TopTab
    {
        InstanceType,
        Custom
    }

    private TopTab topTab = TopTab.InstanceType;

    private string _instanceSearch = string.Empty;
    private int _instanceIndex = 0;

    private readonly List<(uint RowId, string Name, string Category)> _duties = new();
    private readonly List<string> _dutyNames = new();
    private readonly Dictionary<string, uint> _nameToRowId = new();
    private string _dutySearch = "";
    private int _dutyIndex = -1;

    [Experimental("PendingExcelSchema")]
    public MainWindow(Plugin plugin, UiService uiService, Configuration config)
        : base("VFX toggler", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 350),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Config = config;
        UiService = uiService;
        Plugin = plugin;

        BuildDutyIndex();
    }

    private readonly string[] battleFxOptions = { "Show All", "Limited", "None" };

    public override void Draw()
    {
        UiService.HeaderText("Battle Effects Settings");
        ImGui.Spacing();

        if (ImGui.BeginTabBar("TopTabs"))
        {
            if (ImGui.BeginTabItem("Instance Type"))
            {
                topTab = TopTab.InstanceType;
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Custom"))
            {
                topTab = TopTab.Custom;
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.Spacing();

        if (topTab == TopTab.InstanceType)
        {
            float spacing = ImGui.GetStyle().ItemSpacing.X;

            ImGui.BeginChild("InstanceTypeNav", new Vector2(150f, 0), true);
            DrawNavItem("World (Default)", ref _instanceTab, InstanceTab.World);
            DrawNavItem("Dungeons", ref _instanceTab, InstanceTab.Dungeons);
            DrawNavItem("Trials", ref _instanceTab, InstanceTab.Trials);
            DrawNavItem("Raids", ref _instanceTab, InstanceTab.Raids);
            DrawNavItem("PvP", ref _instanceTab, InstanceTab.PvP);
            ImGui.EndChild();

            ImGui.SameLine(0, spacing);
            ImGui.BeginChild("InstanceTypeContent", new Vector2(0, 0), false);
            var page = Array.Find(_instancePages, p => p.tab == _instanceTab);
            switch (_instanceTab)
            {
                case InstanceTab.World:
                    DrawVfxRadios("Self", "BattleEffectSelf", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Party", "BattleEffectParty", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Other", "BattleEffectOther", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("PvP", "BattleEffectPvPEnemyPc", page.typeKey);
                    break;

                case InstanceTab.Dungeons:
                    DrawVfxRadios("Self", "BattleEffectSelf", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Party", "BattleEffectParty", page.typeKey);
                    break;

                case InstanceTab.Trials:
                    DrawVfxRadios("Self", "BattleEffectSelf", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Party", "BattleEffectParty", page.typeKey);
                    break;

                case InstanceTab.Raids:
                    DrawVfxRadios("Self", "BattleEffectSelf", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Party", "BattleEffectParty", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Other", "BattleEffectOther", page.typeKey);
                    break;

                case InstanceTab.PvP:
                    DrawVfxRadios("Self", "BattleEffectSelf", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Party", "BattleEffectParty", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("Other", "BattleEffectOther", page.typeKey);
                    ImGui.Spacing();
                    DrawVfxRadios("PvP Opponents", "BattleEffectPvPEnemyPc", page.typeKey);
                    break;
            }

            ImGui.EndChild();
        }
        else
        {
            if (ImGui.Button("Add"))
            {
                AddSelectedDutyToCustom();
            }
            ImGui.SameLine();
            if (ImGuiExtras.SearchableCombo("Instance", ref _instanceIndex, _dutyNames, ref _instanceSearch))
            {
                _instanceTab = (InstanceTab)_instanceIndex;
            }
            
            ImGui.Separator();
            
            foreach (var keyValuePair in Config.CustomBattleFx)
            {
                if (ImGui.CollapsingHeader(keyValuePair.Key))
                {
                    DrawCustomFxRadios("Self", "BattleEffectSelf", keyValuePair.Value, keyValuePair.Key);
                    DrawCustomFxRadios("Party", "BattleEffectParty", keyValuePair.Value, keyValuePair.Key);
                    DrawCustomFxRadios("Other", "BattleEffectOther", keyValuePair.Value, keyValuePair.Key);
                    DrawCustomFxRadios("PvP Opponents", "BattleEffectPvPEnemyPc", keyValuePair.Value, keyValuePair.Key);
                }
            }
        }


    }

    private void DrawNavItem(string label, ref InstanceTab current, InstanceTab me)
    {
        bool selected = current == me;
        if (ImGui.Selectable(label, selected, ImGuiSelectableFlags.None, new Vector2(250f, 0)))
            current = me;
    }
    
    private void DrawCustomFxRadios(string heading, string key, Dictionary<string, uint> dict, string dutyName)
    {
        UiService.SubHeaderText(heading);
        ImGui.Spacing();

        int current = (int)dict[key];

        for (int i = 0; i < battleFxOptions.Length; i++)
        {
            // unique id per duty/key/option
            if (ImGui.RadioButton($"{battleFxOptions[i]}##{dutyName}_{key}_{i}", current == i))
            {
                current = i;
                dict[key] = (uint)i;
                Plugin.Configuration.Save(); // persist immediately on change
            }

            if (i < battleFxOptions.Length - 1)
                ImGui.SameLine();
        }

        ImGui.Spacing();
        ImGui.Separator();
    }

    private void DrawVfxRadios(string label, string key, string type)
    {
        if (!Config.InstancedBattleFx.TryGetValue(type, out var settingDict))
        {
            settingDict = new Dictionary<string, uint>();
            Config.InstancedBattleFx[type] = settingDict;
        }

        if (!settingDict.TryGetValue(key, out var value))
        {
            value = Services.BaseServices.GameConfig.UiConfig.GetUInt(key);
            settingDict[key] = value; // store it for later use
        }

        int current = (int)value;

        UiService.SubHeaderText(label);
        ImGui.Spacing();

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

        ImGui.Spacing();
        ImGui.Separator();
    }
    
    private void AddSelectedDutyToCustom()
    {
        if (_instanceIndex < 0 || _instanceIndex >= _dutyNames.Count)
            return;

        var dutyName = _dutyNames[_instanceIndex];

        // create (or get) the inner dict
        if (!Config.CustomBattleFx.TryGetValue(dutyName, out var dict))
        {
            dict = new Dictionary<string, uint>(4)
            {
                ["BattleEffectSelf"]        = 0u,
                ["BattleEffectParty"]       = 0u,
                ["BattleEffectOther"]       = 0u,
                ["BattleEffectPvPEnemyPc"]  = 0u,
            };
            Config.CustomBattleFx[dutyName] = dict;
        }

        Plugin.Configuration.Save();
    }


    
    private static readonly HashSet<string> AllowedCfcTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Dungeons",
            "Trials",
            "Raids",
            "Alliance Raids",
            "Ultimate Raids",
            "Deep Dungeons",
            "V&C Dungeon Finder",
            "Variant Dungeons",
            "Criterion Dungeons",
        };
    
    [Experimental("PendingExcelSchema")]
    private void BuildDutyIndex()
    {
        _duties.Clear();
        _dutyNames.Clear();
        _nameToRowId.Clear();

        var sheet = BaseServices.DataManager.GetExcelSheet<ContentFinderCondition>();
        if (sheet == null) return;

        foreach (var row in sheet)
        {
            var typeName = row.ContentType.Value.Name.ExtractText();
            if (!AllowedCfcTypes.Contains(typeName)) continue;

            var name = row.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name)) continue;
            
            if (name.StartsWith("Duty Roulette", StringComparison.OrdinalIgnoreCase))
                continue;

            _duties.Add((row.RowId, name, typeName));
        }

        foreach (var g in _duties.OrderBy(d => d.Name).GroupBy(d => d.Name))
        {
            var first = g.First();
            _dutyNames.Add(first.Name);
            _nameToRowId[first.Name] = first.RowId;
        }
    }
}
using System;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace VFXToggler.Services;

public class TerritoryChange
{
    private Configuration _config;
    public TerritoryChange(Configuration config)
    {
        _config = config;
        BaseServices.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

      private void OnTerritoryChanged(uint u)
    {
        try
        {
            uint id;
            unsafe
            {
                id = GameMain.Instance()->CurrentContentFinderConditionId;
            }
            
            var cfc = BaseServices.DataManager.GetExcelSheet<ContentFinderCondition>()!.GetRow(id);

            if (cfc.ContentType.Value.RowId != 0)
            {
                
                if (_config.CustomBattleFx.ContainsKey(cfc.Name.ExtractText()))
                {
                    foreach (var keyValuePair in _config.CustomBattleFx[cfc.Name.ExtractText()])
                    {
                        BaseServices.GameConfig.UiConfig.Set(keyValuePair.Key, (uint)keyValuePair.Value);
                    }
                    return;
                }
                
                string contentType = null;
                    
                switch(cfc.ContentType.Value.Name.ExtractText())
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
            
                if (_config.InstancedBattleFx.TryGetValue(contentType, out var settingDict))
                {
                    foreach (var keyValuePair in settingDict)
                    {
                        BaseServices.GameConfig.UiConfig.Set(keyValuePair.Key, (uint)keyValuePair.Value);
                    }
                }
            }
            else
            {
                if (_config.InstancedBattleFx.TryGetValue("World", out var settingDict))
                {
                    foreach (var keyValuePair in settingDict)
                    {
                        BaseServices.GameConfig.UiConfig.Set(keyValuePair.Key, (uint)keyValuePair.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            BaseServices.Logger.Error($"[TerritoryChange] {ex}");
        }
    }
}
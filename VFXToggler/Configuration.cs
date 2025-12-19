using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace VFXToggler;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public Dictionary<string, Dictionary<string, uint>> InstancedBattleFx = new();
    public Dictionary<string, Dictionary<string, uint>> CustomBattleFx = new();

    public void Save()
    {
        Services.BaseServices.PluginInterface.SavePluginConfig(this);
    }
}

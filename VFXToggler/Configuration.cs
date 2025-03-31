﻿using System;
using Dalamud.Configuration;
using SamplePlugin;

namespace VFXToggler;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

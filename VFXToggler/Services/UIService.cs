using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;

namespace VFXToggler.Services;

public class UiService
{
    public IFontHandle HeaderFont { get; }
    public IFontHandle SubHeaderFont { get; }

    public UiService()
    {
        var atlas = BaseServices.PluginInterface.UiBuilder.FontAtlas;
        
        HeaderFont = atlas.NewDelegateFontHandle(e =>
        {
            e.OnPreBuild(tk => tk.AddDalamudAssetFont(Dalamud.DalamudAsset.NotoSansJpMedium, new()
            {
                SizePx = 36,
            }));
        });

        SubHeaderFont = atlas.NewDelegateFontHandle(e =>
        {
            e.OnPreBuild(tk => tk.AddDalamudAssetFont(Dalamud.DalamudAsset.NotoSansJpMedium, new()
            {
                SizePx = 30,
            }));
        });
    }

    public void HeaderText(string text)
    {
        using var _ = HeaderFont.Push();
        ImGui.TextUnformatted(text);
    }

    public void SubHeaderText(string text)
    {
        using var _ = SubHeaderFont.Push();
        ImGui.TextUnformatted(text);
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace VFXToggler.UI.Helper;

// Put this in a utility class/file you use for UI helpers.
public static class ImGuiExtras
{
    public static bool SearchableCombo(
        string label,
        ref int currentIndex,
        IReadOnlyList<string> items,
        ref string search,
        int maxSearchLen = 64)
    {
        string preview = (currentIndex >= 0 && currentIndex < items.Count)
            ? items[currentIndex]
            : "<None>";

        bool changed = false;

        if (ImGui.BeginCombo(label, preview))
        {
            // search box
            if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputTextWithHint("##search", "Search...", ref search, maxSearchLen);
            ImGui.Spacing();

            // height = whatever space is left in the popup (avoids outer scrollbar)
            float avail = 100f;
            if (avail < 1f) avail = 1f;

            ImGui.BeginChild("##combo_list", new Vector2(0, avail), true, ImGuiWindowFlags.NoSavedSettings);

            var filter = search?.Trim() ?? string.Empty;
            int visibleCount = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var txt = items[i];
                if (filter.Length > 0 &&
                    txt.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                visibleCount++;

                bool isSelected = (i == currentIndex);
                if (ImGui.Selectable(txt, isSelected))
                {
                    currentIndex = i;
                    changed = true;
                    ImGui.CloseCurrentPopup();
                }
                // don’t call SetItemDefaultFocus here; it can nudge scroll
            }

            if (visibleCount == 0)
                ImGui.TextDisabled("No results");

            ImGui.EndChild();
            ImGui.EndCombo();
        }

        return changed;
    }

}
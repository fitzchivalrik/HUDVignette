using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using FFXIVAction = Lumina.Excel.GeneratedSheets.Action;

namespace HUDVignette
{
    internal static class ConfigurationUi
    {
        internal static (bool shouldBuildConfigUi,bool changedConfig) DrawConfigUi(Configuration config)
        {
            var shouldBuildConfigUi = true;
            var changed = false;
            var scale = ImGui.GetIO().FontGlobalScale;
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowSize(new Vector2(400 * scale, 410 * scale), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(410 * scale, 200 * scale),
                new Vector2(float.MaxValue, float.MaxValue));
            if (!ImGui.Begin($"{HUDVignette.PluginName} Configuration", ref shouldBuildConfigUi,
                ImGuiWindowFlags.NoCollapse))
            {
                ImGui.End();
                return (shouldBuildConfigUi, changed);
            }
            

            if(ImGui.BeginTabBar("ConfigTabBar", ImGuiTabBarFlags.NoTooltip))
            {

                changed |= DrawGeneralConfigurationTab(config, scale);
                DrawCreditsTab(config, scale);
                ImGui.EndTabBar();
            }
            
            ImGui.End();
            return (shouldBuildConfigUi, changed);
        }

        private static void DrawCreditsTab(Configuration config, float scale)
        {
            if (!ImGui.BeginTabItem("Credits")) return;
            ImGui.Text("Simple Blood Vignette Overlay:");
            ImGui.Text("CC0 texture");
            ImGui.Text("Fancy Blood Vignette Overlay modified from:");
            ImGui.Text("7he1ndigo https://www.deviantart.com/7he1ndigo/art/Blood-Vignette-704205045");
        }

        private static bool DrawTreeCheckbox(string label, ref bool open, Func<bool> drawConfig)
        {
            var changed = ImGui.Checkbox($"###{label.GetHashCode()}", ref open);
            ImGui.SameLine();
            if (open)
            {
                if (!ImGui.TreeNodeEx(label)) return changed;
                changed |= drawConfig();
                ImGui.TreePop();
            }
            else
            {
                DummyTreeNode(label);
            }
            return changed;
        }

       
        private static bool DrawGeneralConfigurationTab(Configuration config, float scale)
        {
            if (!ImGui.BeginTabItem("Settings")) return false;
            var changed = false;
            ImGui.Indent();
            ImGui.PushItemWidth(300f * scale);
            changed |= ImGui.Checkbox("Enabled", ref config.Enabled);
            changed |= ImGui.SliderInt("###StartHealth", ref config.StartHealthPercentage, 10, 100,
                "Start Blood Vignette when health under %d %%");
            var currentSelectedBloodOverlay = (int)config.SelectedOverlay;
            changed |= ImGui.RadioButton("Simple Blood Vignette", ref currentSelectedBloodOverlay, 0);
            ImGui.SameLine();
            changed |= ImGui.RadioButton("Fancy Blood Vignette", ref currentSelectedBloodOverlay, 1);
            if (changed) config.SelectedOverlay = (BloodOverlay) currentSelectedBloodOverlay;
            ImGui.PopItemWidth();
            ImGui.Unindent();
            

            ImGui.EndTabItem();
            return changed;

        }
        

        private static void DummyTreeNode(string label, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen)
        {
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, 0x0);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, 0x0);
            ImGui.TreeNodeEx(label, flags);
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
        }
    }
}
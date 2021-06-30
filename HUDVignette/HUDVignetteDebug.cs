#if DEBUG
using Dalamud.Game.Command;
using ImGuiNET;
using SimpleTweaksPlugin.Helper;

namespace HUDVignette
{
    public partial class HUDVignette
    {

        partial void DebugCtor()
        {

            _pluginInterface.CommandManager.AddHandler($"{Command}debug", new CommandInfo((_, _) =>
            {
                _pluginInterface.UiBuilder.OnBuildUi -= BuildDebugUi;
                _pluginInterface.UiBuilder.OnBuildUi += BuildDebugUi;
            })
            {
                HelpMessage = $"Open {PluginName} Debug menu.",
                ShowInHelp = false
            });
            if (_pluginInterface.ClientState.LocalPlayer is not null)
            {
                OnLogin(null!, null!);
                _buildingConfigUi = true;
                _pluginInterface.UiBuilder.OnBuildUi += BuildConfigUi;
            }
            
            //_pluginInterface.UiBuilder.OnBuildUi += BuildDebugUi;
            
            UiHelper.Setup(_pluginInterface.TargetModuleScanner);
        }
        
        private void BuildDebugUi()
        {
            
            ImGui.SetNextWindowBgAlpha(1);
            if(!ImGui.Begin($"{PluginName} Debug")) { ImGui.End(); return;}
            
            ImGui.End();
        }
        

        partial void DebugDtor()
        {
            _pluginInterface.UiBuilder.OnBuildUi -= BuildDebugUi;
            _pluginInterface.UiBuilder.OnBuildUi -= BuildOverlay;
            _pluginInterface.CommandManager.RemoveHandler($"{Command}debug");
        }
    }
}
#endif
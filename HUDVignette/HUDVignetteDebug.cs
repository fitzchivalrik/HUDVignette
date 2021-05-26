#if DEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Component.GUI.ULD;
using ImGuiNET;
using ImGuiScene;
using SimpleTweaksPlugin;
using SimpleTweaksPlugin.Helper;
using static HUDVignette.Extensions;

namespace HUDVignette
{
    public partial class HUDVignette
    {

        partial void DebugCtor()
        {

            var p = Path.Combine(PluginBridge.AssemblyLocation,
                "/res/BloodOverlay.png");
            _bloodOverlayCC0 = _pluginInterface.UiBuilder.LoadImage(
                "C:\\Development\\chivalrik\\hudvignette\\HUDVignette\\bin\\Debug\\net472\\res\\BloodOverlay.png");
            _bloodOverlay7he1ndigo = _pluginInterface.UiBuilder.LoadImage(
                "C:\\Development\\chivalrik\\hudvignette\\HUDVignette\\bin\\Debug\\net472\\res\\BloodVignette_Deviantart_7he1ndigo.png");
            
            
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
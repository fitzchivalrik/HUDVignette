using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using SimpleTweaksPlugin;

namespace HUDVignette
{
    
    public partial class HUDVignette : IDisposable
    {
        public const string PluginName = "HUD Vignette";
        private const string Command = "/hudvignette";
        
        private readonly Configuration _config;
        private readonly DalamudPluginInterface _pluginInterface;
        
        private bool _isDisposed;
        private bool _buildingConfigUi;
        

        private TextureWrap _bloodOverlayCC0;
        private TextureWrap _bloodOverlay7he1ndigo;
        private TextureWrap _selectedOverlay = null!;
        private float _startHealth = 0.8f;
        private Vector2 _uvMin;
        private Vector2 _uvMax;
        private int _maxAlpha;

        public HUDVignette(DalamudPluginInterface pi, Configuration config)
        {
            
            
            #region Signatures

            #endregion


            _pluginInterface = pi;
            _config = config;

            #region Configuration Setup

            #endregion
            
            
            _pluginInterface.ClientState.OnLogin += OnLogin;
            _pluginInterface.ClientState.OnLogout += OnLogout;

            #region Hooks, Functions and Addresses

            #endregion

            #region Excel Data

            #endregion
            
            _bloodOverlayCC0 = _pluginInterface.UiBuilder.LoadImageRaw(
                ReadEmbedded("HUDVignette.res.BloodOverlay.rgba"), 1280, 800, 4);
            _bloodOverlay7he1ndigo = _pluginInterface.UiBuilder.LoadImageRaw(
                ReadEmbedded("HUDVignette.res.BloodVignette_Deviantart_7he1ndigo.rgba"), 960, 540, 4);

            pi.CommandManager.AddHandler(Command, new CommandInfo((_, args) =>
            {
                switch (args)
                {
                    case "toggle":
                        config.Enabled ^= true;
                        SetupOverlay();
                        _pluginInterface.SavePluginConfig(_config);
                        break;
                    case "on":
                        config.Enabled = true;
                        SetupOverlay();
                        _pluginInterface.SavePluginConfig(_config);
                        break;
                    case "off":
                        config.Enabled = false;
                        SetupOverlay();
                        _pluginInterface.SavePluginConfig(_config);
                        break;
                    default:
                        OnOpenConfigUi(null!, null!);
                        break;
                }
                
            })
            {
                HelpMessage = $"Open {PluginName} configuration menu. User \"{Command} toggle|on|off\" to enable/disable.",
                ShowInHelp = true
            });
            
            DebugCtor();
#if RELEASE

            if (_pluginInterface.Reason == PluginLoadReason.Installer
                || _pluginInterface.ClientState.LocalPlayer is not null
            )
            {
                 OnLogin(null!, null!);
            }
#endif
        }

        private static byte[] ReadEmbedded(string qualifiedName)
        {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(qualifiedName);
            using var ms = new MemoryStream();
            s?.CopyTo(ms);
            return ms.ToArray();
        }
        
        private void OnLogout(object sender, EventArgs e)
        {
            _pluginInterface.UiBuilder.OnOpenConfigUi -= OnOpenConfigUi;
            _pluginInterface.UiBuilder.OnBuildUi -= BuildConfigUi;
            _pluginInterface.UiBuilder.OnBuildUi -= BuildOverlay;
        }

        private void OnLogin(object sender, EventArgs e)
        {
            _pluginInterface.UiBuilder.OnOpenConfigUi += OnOpenConfigUi;
            SetupOverlay();
        }

        private void SetupOverlay()
        {
            _pluginInterface.UiBuilder.OnBuildUi -= BuildOverlay;
            _selectedOverlay = _config.SelectedOverlay switch
            {
                BloodOverlay.CC0 => _bloodOverlayCC0,
                BloodOverlay.TheIndigo => _bloodOverlay7he1ndigo,
                _ => throw new ArgumentOutOfRangeException()
            };
            _startHealth = _config.StartHealthPercentage / 100f;
            _uvMin = _config.UVMin;
            _uvMax = _config.UVMax;
            _maxAlpha = _config.MaxAlpha;
            if(_config.Enabled) _pluginInterface.UiBuilder.OnBuildUi += BuildOverlay;
        }

        #region UI
        
        private void BuildOverlay()
        {
            const ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration
                                           | ImGuiWindowFlags.NoMove
                                           | ImGuiWindowFlags.NoMouseInputs
                                           | ImGuiWindowFlags.NoFocusOnAppearing
                                           | ImGuiWindowFlags.NoBackground
                                           | ImGuiWindowFlags.NoNav
                                           | ImGuiWindowFlags.NoInputs
                                           | ImGuiWindowFlags.NoCollapse
                                           | ImGuiWindowFlags.NoSavedSettings;
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero, ImGuiCond.Always);
            if (!ImGui.Begin("###HUDVignetteOverlay", flags)
            )
            {
                ImGui.End();
                return;
            }

            var drawlist = ImGui.GetForegroundDrawList();
            drawlist.PushClipRectFullScreen();

            var player = _pluginInterface.ClientState.LocalPlayer;
            if (player is null) return;
            var healthPercentage = (float)player.CurrentHp / player.MaxHp;
            if (healthPercentage > _startHealth) return;
            var alpha = (int)(_maxAlpha * (1f - healthPercentage));
            drawlist.AddImage(
                _selectedOverlay.ImGuiHandle,
                Vector2.Zero, 
                new Vector2(ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y),
                _uvMin,
                _uvMax,
                (uint) (alpha << 24) | 0xFFFFFF
            );
            drawlist.PopClipRect();
            ImGui.End();
        }
        
        private void BuildConfigUi()
        {
            var (shouldBuildConfigUi, changedConfig) = ConfigurationUi.DrawConfigUi(_config);
            if (changedConfig)
            {
                _pluginInterface.SavePluginConfig(_config);
                SetupOverlay();
                
            }

            if (shouldBuildConfigUi)
            {
                const ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration
                                               | ImGuiWindowFlags.NoMove
                                               | ImGuiWindowFlags.NoMouseInputs
                                               | ImGuiWindowFlags.NoFocusOnAppearing
                                               | ImGuiWindowFlags.NoBackground
                                               | ImGuiWindowFlags.NoNav
                                               | ImGuiWindowFlags.NoInputs
                                               | ImGuiWindowFlags.NoCollapse
                                               | ImGuiWindowFlags.NoSavedSettings;
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero, ImGuiCond.Always);
                ImGui.Begin("###HUDVignetteOverlayPreview", flags);
                var drawlist = ImGui.GetForegroundDrawList();
                drawlist.PushClipRectFullScreen();
                drawlist.AddImage(
                    _selectedOverlay.ImGuiHandle,
                    Vector2.Zero, 
                    new Vector2(ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y),
                    _uvMin,
                    _uvMax,
                    (uint) (_maxAlpha << 24) | 0xFFFFFF
                );
                drawlist.PopClipRect();
                ImGui.End();
                return;
            }
            _pluginInterface.UiBuilder.OnBuildUi -= BuildConfigUi;
            _buildingConfigUi = false;
        }
        
        private void OnOpenConfigUi(object sender, EventArgs e)
        {
            _buildingConfigUi = !_buildingConfigUi;
            if(_buildingConfigUi)
                _pluginInterface.UiBuilder.OnBuildUi += BuildConfigUi;
            else
                _pluginInterface.UiBuilder.OnBuildUi -= BuildConfigUi;
            
        }

        #endregion
        
        #region Debug Partials
        
        partial void DebugCtor();
        partial void DebugDtor();
        
        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                // TODO (Chiv) Still not quite sure about correct dispose
                // NOTE (Chiv) Explicit, non GC? call - remove managed thingies too.
                OnLogout(null!, null!);
                _pluginInterface.ClientState.OnLogin -= OnLogin;
                _pluginInterface.ClientState.OnLogout -= OnLogout;
                _pluginInterface.CommandManager.RemoveHandler(Command);
                DebugDtor();
            }

            _isDisposed = true;
        }

        ~HUDVignette()
        {
            Dispose(false);
        }

        #endregion
    }
}
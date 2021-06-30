using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Dalamud.Configuration;
using Newtonsoft.Json;

namespace HUDVignette
{
    public class Configuration : IPluginConfiguration
    {

        public bool Enabled = true;
        public int StartHealthPercentage = 60;
        public BloodOverlay SelectedOverlay = BloodOverlay.CC0;
        public Vector2 UVMin = new Vector2(0.02f, 0.02f);
        public Vector2 UVMax = new Vector2(0.97f, 0.97f);
        public int MaxAlpha = 0x96;
        public int Version { get; set; } = 0;
    }

    public enum BloodOverlay
    {
        CC0,
        TheIndigo,
    }

}
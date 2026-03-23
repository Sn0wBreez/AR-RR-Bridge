using Dalamud.Configuration;
using Dalamud.Plugin;

namespace AutoRepricerBridge;

public class BridgeConfig : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // All the settings you want to save/load
    public bool Enabled { get; set; } = true;
    public string TriggerPhrase { get; set; } = "AutoRetainer";
    public bool OnlyReprice { get; set; } = false;
    public int DelayMs { get; set; } = 1500;

    // Reference to the plugin interface (set once during init)
    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    // Called once when the plugin loads (usually in your main Plugin.cs constructor)
    public void Initialize(IDalamudPluginInterface pi)
    {
        pluginInterface = pi;
    }

    // Save the current config to disk
    public void Save()
    {
        pluginInterface?.SavePluginConfig(this);
    }
}

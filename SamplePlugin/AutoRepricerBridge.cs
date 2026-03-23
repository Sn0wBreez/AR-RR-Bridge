using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;

namespace AutoRepricerBridge;

public sealed class AutoRepricerBridge : IDalamudPlugin
{
    public string Name => "AutoRepricer Bridge";

    private readonly WindowSystem windowSystem;
    private ConfigWindow configWindow;

    // ================== CONFIG ==================
    public bool Enabled { get; set; } = true;
    public string TriggerPhrase { get; set; } = "AutoRetainer"; // change if your exact message is different
    public bool OnlyReprice { get; set; } = false; // true = /rr start price instead of full start
    public int DelayMs { get; set; } = 1500; // safety delay after AutoRetainer finishes

    private DateTime lastTrigger = DateTime.MinValue;

    public AutoRepricerBridge(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;

        // Load or create config
        Config = pluginInterface.GetPluginConfig() as BridgeConfig ?? new BridgeConfig();
        Config.Initialize(pluginInterface);

        // Sync config
        Enabled = Config.Enabled;
        TriggerPhrase = Config.TriggerPhrase;
        OnlyReprice = Config.OnlyReprice;
        DelayMs = Config.DelayMs;

        windowSystem = new WindowSystem(Name);
        configWindow = new ConfigWindow(this);
        windowSystem.AddWindow(configWindow);

        // Register chat listener
        Service.ChatGui.ChatMessage += OnChatMessage;

        // Slash command to open config
        pluginInterface.CommandManager.AddHandler("/arrb", new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = "Open AutoRepricer Bridge config"
        });
    }

    private void OnChatMessage(XivChatType type, uint senderId, SeString sender, SeString message)
    {
        if (!Enabled) return;
        if (DateTime.UtcNow - lastTrigger < TimeSpan.FromSeconds(10)) return; // anti-spam

        var msg = message.ToString().ToLower();

        if (msg.Contains(TriggerPhrase.ToLower()) &&
            (msg.Contains("venture") || msg.Contains("ventures")) &&
            (msg.Contains("assign") || msg.Contains("sent") || msg.Contains("complete")))
        {
            lastTrigger = DateTime.UtcNow;
            Service.Framework.RunOnFrameworkThread(async () =>
            {
                await Task.Delay(DelayMs);
                var cmd = OnlyReprice ? "/rr start price" : "/rr start";
                Service.CommandManager.ProcessCommand(cmd);
                Service.ChatGui.Print($"[AutoRepricer Bridge] Triggered → {cmd}");
            });
        }
    }

    private void OnCommand(string command, string args) => configWindow.IsOpen = true;

    public void Dispose()
    {
        Service.ChatGui.ChatMessage -= OnChatMessage;
        PluginInterface.CommandManager.RemoveHandler("/arrb");
        windowSystem.RemoveAllWindows();
    }

    // Static access
    public static AutoRepricerBridge Plugin { get; private set; }
    public static IDalamudPluginInterface PluginInterface { get; private set; }
    public static BridgeConfig Config { get; private set; }
}

// Simple config class (add as BridgeConfig.cs in the same folder)
public class BridgeConfig : Dalamud.Configuration.IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    public string TriggerPhrase { get; set; } = "AutoRetainer";
    public bool OnlyReprice { get; set; } = false;
    public int DelayMs { get; set; } = 1500;

    public void Initialize(IDalamudPluginInterface pi) => pi.Create<BridgeConfig>().Save();
}

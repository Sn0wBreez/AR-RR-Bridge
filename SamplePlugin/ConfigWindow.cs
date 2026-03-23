using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AutoRepricerBridge;

public class ConfigWindow : Window, IDisposable
{
    private readonly AutoRepricerBridge plugin;

    public ConfigWindow(AutoRepricerBridge plugin) 
        : base("AutoRepricer Bridge Config", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar)
    {
        this.plugin = plugin;

        Size = new Vector2(380, 0);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudYellow, "AutoRepricer Bridge Settings");
        ImGui.Separator();
        ImGui.Spacing();

        var enabled = plugin.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            plugin.Enabled = enabled;
            AutoRepricerBridge.Config.Enabled = enabled;
            AutoRepricerBridge.Config.Save();
        }

        ImGui.Spacing();

        var onlyReprice = plugin.OnlyReprice;
        if (ImGui.Checkbox("Only Reprice (use /rr start price)", ref onlyReprice))
        {
            plugin.OnlyReprice = onlyReprice;
            AutoRepricerBridge.Config.OnlyReprice = onlyReprice;
            AutoRepricerBridge.Config.Save();
        }
        ImGui.SameLine();
        ImGui.TextDisabled("(unchecked = full /rr start with new listings)");

        ImGui.Spacing();

        var delay = plugin.DelayMs;
        if (ImGui.SliderInt("Delay after AutoRetainer (ms)", ref delay, 500, 5000, "%d ms"))
        {
            plugin.DelayMs = delay;
            AutoRepricerBridge.Config.DelayMs = delay;
            AutoRepricerBridge.Config.Save();
        }
        ImGui.TextColored(ImGuiColors.DalamudGrey, "Recommended: 1500–2500 ms to let menus settle");

        ImGui.Spacing();

        var trigger = plugin.TriggerPhrase ?? "AutoRetainer";
        ImGui.Text("Trigger phrase in chat message:");
        if (ImGui.InputText("##trigger", ref trigger, 128))
        {
            plugin.TriggerPhrase = trigger;
            AutoRepricerBridge.Config.TriggerPhrase = trigger;
            AutoRepricerBridge.Config.Save();
        }
        ImGui.TextColored(ImGuiColors.DalamudGrey, "Usually 'AutoRetainer' works – change if your log says something else");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Save & Close"))
        {
            IsOpen = false;
        }
        ImGui.SameLine();
        if (ImGui.Button("Test Trigger Now"))
        {
            // Fake a trigger for testing (without real delay)
            Service.CommandManager.ProcessCommand(plugin.OnlyReprice ? "/rr start price" : "/rr start");
            ImGui.GetWindowDrawList().AddText(ImGui.GetCursorScreenPos() + new Vector2(0, 4), 
                ImGui.GetColorU32(ImGuiColors.DalamudYellow), "Test command sent!");
        }
    }
}

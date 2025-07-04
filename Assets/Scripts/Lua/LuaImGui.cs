using ImGuiNET;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class LuaImGui
{
    public static void Text(string text) => ImGui.Text(text);
    public static bool Button(string label) => ImGui.Button(label);
    
    public static float GetWindowWidth() => ImGui.GetWindowWidth();
    public static float GetWindowHeight() => ImGui.GetWindowHeight();

    public static void SetCursorPosX(float x) => ImGui.SetCursorPosX(x);
    public static void SetCursorPosY(float y) => ImGui.SetCursorPosY(y);

    public static void SetNextWindowBgAlpha(float alpha) => ImGui.SetNextWindowBgAlpha(alpha);

    public static void BeginWindow(string title, ref bool open, ImGuiWindowFlags flags)
        => ImGui.Begin(title, ref open, flags);
    public static void EndWindow() => ImGui.End();

    public static int WindowFlags_NoMove => (int)ImGuiWindowFlags.NoMove;
    public static int WindowFlags_NoCollapse => (int)ImGuiWindowFlags.NoCollapse;
    public static int WindowFlags_NoResize => (int)ImGuiWindowFlags.NoResize;
    public static int WindowFlags_AlwaysAutoResize => (int)ImGuiWindowFlags.AlwaysAutoResize;
    public static int WindowFlags_NoFocusOnAppearing => (int)ImGuiWindowFlags.NoFocusOnAppearing;
    public static int WindowFlags_NoNav => (int)ImGuiWindowFlags.NoNav;
    public static int WindowFlags_NoDecoration => (int)(ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
}

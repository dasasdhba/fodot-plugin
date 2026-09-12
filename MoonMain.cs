#if DEBUG

using System;
using Godot.Editor;

namespace Godot.MoonPlugin;

[Tool]
public partial class MoonMain : EditorPlugin, ISerializationListener
{

    private static string MainSceneKey => MoonEditor.MainSceneKey;
    private const string AssemblyListKey = "moon/general/assembly_list";
    private const string AssemblyListCache = "res://.godot/moon_assembly_list";
    private const string LibraryKey = "moon/general/library_schedule_time";
    private const string BridgeKey = "Moon";
    private const string BridgeFile = "res://MoonEntry.cs";

    private static float LibraryScheduleTime => Plugin.GetProjectSetting(LibraryKey, 3.0f);

    public void OnAfterDeserialize()
    {
        Plugin.AddProjectSetting(MainSceneKey, "", Variant.Type.String,
            PropertyHint.File, "*.tscn,*.scn,*.res");
        Plugin.AddProjectSetting(AssemblyListKey, new string[0],
            Variant.Type.PackedStringArray);
        Plugin.AddProjectSetting(LibraryKey, 3.0, Variant.Type.Float,
            PropertyHint.Range, "0,60,0.5");

        LibInit();
    }

    public void OnBeforeSerialize()
    {
        LibExit();
    }

    public override void _EnterTree()
    {
        OnAfterDeserialize();
    }

    public override void _ExitTree()
    {
        OnBeforeSerialize();
    }

    public override void _EnablePlugin()
    {
        AddAutoloadSingleton(BridgeKey, BridgeFile);
    }

    public override void _DisablePlugin()
    {
        ProjectSettings.Clear(MainSceneKey);
        ProjectSettings.Clear(LibraryKey);
        RemoveAutoloadSingleton(BridgeKey);
    }

    public override void _Process(double delta)
    {
        UpdateDebugScene();
        ProcessLib(delta);
    }

    private void ProcessAssemblyList()
    {
        if (!FileAccess.FileExists(AssemblyListCache)) return;

        using var file = FileAccess.Open(AssemblyListCache, FileAccess.ModeFlags.Read);
        if (file == null) return;

        var assemblies = file.GetAsText()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        ProjectSettings.SetSetting(AssemblyListKey, assemblies);
        if (ProjectSettings.Save() == Error.Ok)
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(AssemblyListCache));
        }
    }

#endif    
}

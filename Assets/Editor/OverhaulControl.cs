using UnityEditor;
using System.IO;
[InitializeOnLoad]
public static class OverhaulControl
{
    static OverhaulControl(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        const string path="Artifacts/editor-command.txt";
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(path))return;
        string command=File.ReadAllText(path).Trim();File.Delete(path);
        if(command=="lighter")OverhaulAssets.PrepareLighter();
        if(command=="school")OverhaulAssets.OpenSchool();
        if(command=="school-art")OverhaulAssets.PrepareSchool();
        if(command=="prepare")OverhaulAssets.Prepare();
        if(command=="exit")EditorApplication.Exit(0);
    }
}

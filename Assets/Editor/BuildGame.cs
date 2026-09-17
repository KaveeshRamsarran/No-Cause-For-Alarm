using System;
using System.IO;
using System.Linq;
using NoCauseForAlarm;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class BuildGame
{
    public static void InspectPortrait()
    {
        var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Characters/portrait.fbx");
        foreach(var t in model.GetComponentsInChildren<Transform>())Debug.Log("PORTRAIT "+t.name+" rotation="+t.localEulerAngles+" scale="+t.localScale);
        foreach(var m in model.GetComponentsInChildren<MeshFilter>())Debug.Log("PORTRAIT MESH "+m.sharedMesh.bounds);
    }
    [MenuItem("NO CAUSE FOR ALARM/Configure and build Windows")]
    public static void Build()
    {
        if(Resources.Load<GameObject>("LocalLicensed/People/Person0")==null||Resources.Load<GameObject>("LocalLicensed/wall_default")==null)
            throw new Exception("Restore the free graphics packs with Tools/setup_graphics.ps1 before building. See Documentation/GRAPHICS_OVERHAUL.md.");
        Configure();Validate();
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/EastWing.unity"},locationPathName="Builds/Windows/NO CAUSE FOR ALARM.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
        Debug.Log("NCFA_BUILD_OK size="+report.summary.totalSize);
    }
    [MenuItem("NO CAUSE FOR ALARM/Configure project")]
    public static void Configure()
    {
        PlayerSettings.companyName="Bellwether Games";PlayerSettings.productName="NO CAUSE FOR ALARM";PlayerSettings.bundleVersion="1.1.0";
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
        PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
        // The project uses the stable built-in Input API so there are no package-specific input bindings.
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;settings.ApplyModifiedPropertiesWithoutUndo();}
        Directory.CreateDirectory("Assets/Settings");
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/CampusURP.asset");
        if(pipeline==null)
        {
            var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Settings/CampusRenderer.asset");
            pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Settings/CampusURP.asset");
        }
        pipeline.renderScale=1;pipeline.msaaSampleCount=2;pipeline.supportsHDR=true;
        pipeline.maxAdditionalLightsCount=8;pipeline.shadowDistance=22;
        var pipelineSettings=new SerializedObject(pipeline);
        pipelineSettings.FindProperty("m_AdditionalLightShadowsSupported").boolValue=true;
        pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
        var atlas=AssetImporter.GetAtPath("Assets/Resources/Characters/FaceAtlas.png") as TextureImporter;
        if(atlas!=null){atlas.filterMode=FilterMode.Point;atlas.wrapMode=TextureWrapMode.Clamp;atlas.maxTextureSize=2048;atlas.SaveAndReimport();}
        GraphicsSettings.defaultRenderPipeline=pipeline;
        if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Surface.mat")==null)
            AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/Resources/Surface.mat");
        if(AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SurfaceEmission.mat")==null)
        {
            var emission=new Material(Shader.Find("Universal Render Pipeline/Lit"));emission.EnableKeyword("_EMISSION");emission.SetColor("_EmissionColor",Color.white*3);
            AssetDatabase.CreateAsset(emission,"Assets/Resources/SurfaceEmission.mat");
        }
        for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i,false);QualitySettings.renderPipeline=pipeline;}
        QualitySettings.vSyncCount=1;QualitySettings.SetQualityLevel(1,false);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("NO CAUSE FOR ALARM / Bootstrap").AddComponent<GameDirector>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/EastWing.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/EastWing.unity",true)};
        AssetDatabase.SaveAssets();Debug.Log("NCFA_CONFIGURED");
    }
    [MenuItem("NO CAUSE FOR ALARM/Validate story logic")]
    public static void Validate()
    {
        int assertions=0;
        Action<bool,string> check=(ok,message)=>{assertions++;if(!ok)throw new Exception("Validation failed: "+message);};
        for(int seed=0;seed<1000;seed++)
        {
            var s=GameState.New(seed);check(s.people.Count(p=>p.infiltrator)==3,"three infiltrators");check(!s.people[0].infiltrator&&!s.people[2].infiltrator,"fixed human anchors");
            foreach(var id in new[]{"register","files","cctv","analysis"})check(!string.IsNullOrWhiteSpace(EvidenceText.Describe(id,s)),"coherent generated clue "+id);
            check(JsonUtility.FromJson<GameState>(JsonUtility.ToJson(s)).Valid(),"save serialization");
        }
        var clock=GameState.New(4);for(int i=0;i<32;i++)clock.Spend();check(clock.hour==18&&clock.actions==0,"32-action day");clock.Spend();check(clock.hour==18,"end time clamped");check(!clock.power,"power event");
        var test=GameState.New(5);test.evacuation=Enumerable.Range(0,12).ToList();check(test.ResolveEnding(false)=="OVERRUN","overrun");
        int first=Array.FindIndex(test.people,p=>p.infiltrator);test.Accuse(first);check(test.ResolveEnding(false)=="INFILTRATION","infiltration");
        test.evacuation=Enumerable.Range(0,12).Where(i=>!test.people[i].infiltrator).ToList();check(test.ResolveEnding(false)=="THE BUILDING REMAINS","infestation");
        test.ceilingSealed=true;check(test.ResolveEnding(false)=="THE LAST BUS","survival");
        foreach(int id in Enumerable.Range(0,12).Where(i=>!test.people[i].infiltrator).Take(3))test.Accuse(id);
        check(test.ResolveEnding(false)=="A CLEAN REGISTER","paranoia");
        var secret=GameState.New(9);secret.ceilingSealed=true;secret.lecturerConfessed=true;check(secret.ResolveEnding(true)=="THE PREVIOUS COHORT","secret");
        secret.health=0;check(secret.ResolveEnding(true)=="OVERRUN","death overrides secret");
        Directory.CreateDirectory("Artifacts");File.WriteAllText("Artifacts/logic-validation.txt",assertions+" assertions passed / 1000 seeds / six endings / save roundtrip / day progression\n");
        Debug.Log("NCFA_LOGIC_OK "+assertions+" assertions");
    }
}

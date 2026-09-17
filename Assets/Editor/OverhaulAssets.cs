using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class OverhaulAssets
{
    public static void OpenCity()
    {
        UnityEditor.PackageManager.UI.Window.Open("260446");
    }
    public static void OpenSchool()
    {
        UnityEditor.PackageManager.UI.Window.Open("146253");
    }
    public static void PrepareCity()
    {
        const string root="Assets/LocalLicensed/CityPeople";
        Directory.CreateDirectory(Local+"/People");AssetDatabase.Refresh();
        string[] models={"professions/Doctor_Male_B","professions/police_Female_A","elder/elder_Female_A","downtown/casual_Female_K","city/casual_Female_G","downtown/casual_Male_K","city/casual_Female_G","city/casual_Male_G","downtown/casual_Female_K","construction/tradesperson_man","elder/elder_Female_A","construction/worker_Male_constructor_B"};
        for(int i=0;i<models.Length;i++)
        {
            var g=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(root+"/Meshes/"+models[i]+".fbx"));g.name="City People / "+NoCauseForAlarm.Cast.All[i].name;
            var animator=g.GetComponentInChildren<Animator>();if(animator==null||!animator.avatar.isHuman)throw new Exception("City People humanoid missing: "+models[i]);
            string texture=root+"/Textures/people_pal"+(i>=6?"_s"+new[]{1,3,6,11}[i%4]:"")+".png";
            var mat=Textured("Campus palette "+i,texture,null,Local+"/People");mat.SetColor("_BaseColor",new Color(.76f,.80f,.70f));
            foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mat).ToArray();
            PrefabUtility.SaveAsPrefabAsset(g,Local+"/People/Person"+i+".prefab");UnityEngine.Object.DestroyImmediate(g);
        }
        foreach(var pair in new[]{("idle_m_1_200f","CityIdleM"),("idle_f_1_150f","CityIdleF"),("locom_m_basicWalk_30f","CityWalkM"),("locom_f_basicWalk_30f","CityWalkF"),("locom_m_jogging_30f","CityRunM"),("locom_f_jogging_30f","CityRunF"),("idle_m_2_220f","CityTalkM"),("idle_f_2_190f","CityTalkF"),("idle_selfcheck_1_300f","CityFlinchM"),("idle_selfcheck_1_300f","CityFlinchF")})
        {
            string file=root+"/Animations/"+pair.Item1+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(file);
            var clips=importer.clipAnimations;if(clips.Length==0)clips=importer.defaultClipAnimations;
            foreach(var clip in clips){clip.loopTime=true;clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;clip.keepOriginalPositionXZ=true;}
            importer.clipAnimations=clips;importer.SaveAndReimport();
            var animation=AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__"));
            string target=Local+"/"+pair.Item2+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(target);
            if(existing==null)AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(animation),target);else EditorUtility.CopySerialized(animation,existing);
        }
        AssetDatabase.SaveAssets();Debug.Log("CITY_PEOPLE_READY / twelve avatars and ten authored clips");
    }
    const string Local="Assets/Resources/LocalLicensed";
    const string Art="Assets/Resources/Overhaul";
    public static void PrepareLighter()
    {
        AssetDatabase.Refresh();
        const string root="Assets/Art/Lighter/";
        foreach(string name in new[]{"LighterBase","LighterMetalSmoothness","LighterNormal"})
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(root+name+".png");
            importer.sRGBTexture=name=="LighterBase";importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.maxTextureSize=1024;
            if(name=="LighterNormal")importer.textureType=TextureImporterType.NormalMap;
            importer.SaveAndReimport();
        }
        Directory.CreateDirectory(Art);
        var solid=Textured("Vintage lighter",root+"LighterBase.png",root+"LighterNormal.png",Art);
        solid.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(root+"LighterMetalSmoothness.png"));solid.EnableKeyword("_METALLICSPECGLOSSMAP");solid.SetFloat("_Smoothness",.8f);
        var cutout=Textured("Vintage lighter perforations",root+"LighterBase.png",root+"LighterNormal.png",Art);
        cutout.CopyPropertiesFromMaterial(solid);cutout.SetFloat("_AlphaClip",1);cutout.SetFloat("_Cutoff",.5f);cutout.EnableKeyword("_ALPHATEST_ON");cutout.renderQueue=2450;
        var g=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(root+"VintageLighter.fbx"));g.name="Vintage Lighter / Poly Haven CC0";
        foreach(var renderer in g.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m.name.Contains("alpha")?cutout:solid).ToArray();
        PrefabUtility.SaveAsPrefabAsset(g,Art+"/VintageLighter.prefab");
        foreach(var t in g.GetComponentsInChildren<Transform>())Debug.Log("LIGHTER_TRANSFORM "+t.name+" position="+t.localPosition+" rotation="+t.localEulerAngles);
        UnityEngine.Object.DestroyImmediate(g);AssetDatabase.SaveAssets();Debug.Log("LIGHTER_READY");
    }
    public static void PrepareSchool()
    {
        const string source="Assets/LocalLicensed/School/";
        string folder=Local+"/School";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var mat=Textured("School palette",source+"material/1.png",null,folder);mat.SetColor("_BaseColor",new Color(.78f,.80f,.72f));
        foreach(string name in new[]{"chair","chair1","table1","table2","table3","locker","locker1","locker_1","locker_2","locker_3","locker_4","locker_5","rack","rack1","showcase","board2","computer","computer1","computer2","computer3","book","fire","fire1","speaker","projector"})
        {
            var root=new GameObject(name);var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source+"props/"+name+".fbx"),root.transform);
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mat).ToArray();
            var bounds=NoCauseForAlarm.CampusArt.LocalBounds(root.transform);
            model.transform.localPosition-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            // Normalize the chair's real backrest to -Z, so +Z always means seated facing direction.
            if(name=="chair")
            {
                Vector3 back=Vector3.zero;int count=0;
                foreach(var mesh in model.GetComponentsInChildren<MeshFilter>())foreach(var v in mesh.sharedMesh.vertices)
                {var p=root.transform.InverseTransformPoint(mesh.transform.TransformPoint(v));if(p.y>bounds.size.y*.7f){back+=p;count++;}}
                back/=Mathf.Max(1,count);back.y=0;
                model.transform.RotateAround(root.transform.position,Vector3.up,Vector3.SignedAngle(back,Vector3.back,Vector3.up));
            }
            PrefabUtility.SaveAsPrefabAsset(root,folder+"/"+name+".prefab");
            Debug.Log("SCHOOL_MODEL "+name+" bounds="+bounds.size+" rotation="+model.transform.localEulerAngles);
            UnityEngine.Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();Debug.Log("SCHOOL_READY");
    }
    public static void Inspect()
    {
        foreach(string path in AssetDatabase.FindAssets("t:Model").Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.StartsWith("Assets/Art")||p.Contains("LocalLicensed")))
        {
            if(!path.Contains("LighterGrip")&&!path.Contains("small_door"))continue;
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.Log("ART_MODEL "+path+" rotation="+model.transform.localEulerAngles+" scale="+model.transform.localScale);
            foreach(var r in model.GetComponentsInChildren<Renderer>())Debug.Log("ART_RENDERER "+r.name+" bounds="+r.bounds+" materials="+string.Join(",",r.sharedMaterials.Select(m=>m?m.name:"null")));
            foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())Debug.Log("ART_CLIP "+clip.name+" length="+clip.length);
        }
    }
    [MenuItem("NO CAUSE FOR ALARM/Prepare graphics overhaul assets")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Local);Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/LocalLicensed","Assets/Art"}).Select(AssetDatabase.GUIDToAssetPath))
        {
            var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(imp==null)continue;
            bool changed=imp.maxTextureSize!=1024;imp.maxTextureSize=1024;
            if(path.ToLower().Contains("normal")&&imp.textureType!=TextureImporterType.NormalMap){imp.textureType=TextureImporterType.NormalMap;changed=true;}
            if(changed)imp.SaveAndReimport();
        }
        string vintage="Assets/LocalLicensed/Vintage/ZNS3D/Vintage Living Room Game Pack";
        foreach(var name in new[]{"wall_default","floor_small","ceiling_small","wall_with_small_door","wall_with_one_window","bookshelf","sofa_small","light_desk","curtain_1","carpet_1"})
        {
            var g=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(vintage+"/Meshes/"+name+".fbx"));g.name=name;
            foreach(var r in g.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(m=>VintageMaterial(m.name,vintage)).ToArray();
            }
            if(name=="wall_with_small_door")
            {
                foreach(var r in g.GetComponentsInChildren<Renderer>())if(r.name=="small_door"||r.name=="small_door_frame")
                {
                    var door=UnityEngine.Object.Instantiate(r.gameObject);door.transform.rotation=r.transform.rotation;door.transform.localScale=r.transform.lossyScale;
                    PrefabUtility.SaveAsPrefabAsset(door,Local+(r.name=="small_door"?"/DoorLeaf.prefab":"/DoorFrame.prefab"));UnityEngine.Object.DestroyImmediate(door);
                }
            }
            PrefabUtility.SaveAsPrefabAsset(g,Local+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);
        }
        var grip=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Hands/LighterGrip.fbx"));
        foreach(var r in grip.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Textured("WRAD skin","Assets/Art/Hands/arm_albedo_pale.png",null,Art)).ToArray();
        PrefabUtility.SaveAsPrefabAsset(grip,Art+"/LighterGrip.prefab");UnityEngine.Object.DestroyImmediate(grip);
        PrepareCity();AssetDatabase.SaveAssets();Inspect();Debug.Log("OVERHAUL_PREPARED");
    }
    static Material Plain(string name,string folder)
    {
        var color=name.Contains("Skin")?new Color(.55f,.40f,.29f):name.Contains("Shirt")?new Color(.34f,.36f,.27f):new Color(.08f,.09f,.08f);
        var mat=Textured(name,null,null,folder);mat.SetColor("_BaseColor",color);return mat;
    }
    static Material Textured(string name,string texture,string normal,string folder)
    {
        string path=folder+"/"+name.Replace("/","_")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetFloat("_Smoothness",.23f);
        if(texture!=null)mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texture));
        if(normal!=null){mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normal));mat.EnableKeyword("_NORMALMAP");}
        EditorUtility.SetDirty(mat);return mat;
    }
    static Material VintageMaterial(string name,string root)
    {
        string category=name.Contains("Wall")&&!name.Contains("Window")?"Walls":name.Contains("Floor")?"Floors":name.ToLower().Contains("ceiling")?"Ceiling":name.Contains("Window")?"Windows_Doors":name.Contains("Furnitures_2")?"Furnitures_2":name.Contains("Furnitures")?"Furnitures_1":name.Contains("Decors_2")?"Decors_2":"Decors_1";
        if(name.Contains("Glass")){var glass=Plain("Dark glass",Local);glass.SetColor("_BaseColor",new Color(.08f,.13f,.14f));return glass;}
        string directory=root+"/Textures/"+category;
        string albedo=Directory.GetFiles(directory,"*BaseColor.png").FirstOrDefault();string normal=Directory.GetFiles(directory,"*Normal.png").FirstOrDefault();
        return Textured(category,albedo?.Replace('\\','/'),normal?.Replace('\\','/'),Local);
    }
}

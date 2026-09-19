using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace NoCauseForAlarm
{
    public class GraphicsReview:MonoBehaviour
    {
        string output;int errors;readonly List<string> results=new List<string>();
        void Awake(){Application.logMessageReceived+=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception){errors++;results.Add(message+"\n"+trace);}}
        void Check(bool ok,string label){results.Add((ok?"PASS ":"FAIL ")+label);if(!ok)errors++;}
        IEnumerator Shot(string name)
        {
            yield return new WaitForSeconds(.7f);yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        IEnumerator Start()
        {
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/overhaul"));Directory.CreateDirectory(output);
            var g=GameDirector.I;yield return new WaitForSeconds(2);g.NewGame();for(int i=0;i<7;i++)g.IntroNext();yield return null;
            Check(g.Campus.actors.All(a=>a.GetComponentInChildren<Animator>().avatar.isHuman),"all twelve actors have valid imported humanoid avatars");
            Check(g.Campus.actors.All(a=>a.GetComponentInChildren<SkinnedMeshRenderer>()!=null),"all people use weighted character meshes");
            Check(Resources.Load<GameObject>("LocalLicensed/Priest")==null,"priest model excluded from game resources");
            Check(g.Audio.FootstepVariations>=3,"supplied MP3 provides distinct footstep contacts");
            Check(FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Count(r=>r.transform.root==g.Campus.transform)>100,"modular architecture is instantiated");
            Check(Resources.Load<GameObject>("Overhaul/LighterGrip")==null,"hand model excluded from game resources");
            var lighter=g.Player.lighter.GetComponentsInChildren<MeshFilter>().FirstOrDefault(f=>f.sharedMesh.vertexCount>100);
            Check(lighter!=null,"imported Poly Haven lighter mesh");
            Check(g.Player.flame.GetComponent<FlameVfx>()!=null,"animated lighter flame");
            Check(g.Campus.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("School assets / "))>50,"requested School assets furnish the campus");
            Check(g.UI.Portraits.Ready&&g.UI.Portraits.images.All(p=>p!=null),"all twelve cast portraits are generated from game models");
            Check(FindObjectsByType<Door>(FindObjectsSortMode.None).Count(d=>d.isStall)==3,"three working bathroom stall doors");
            foreach(var dressing in FindObjectsByType<WindowDressing>(FindObjectsSortMode.None))
            {
                var b=new Bounds(dressing.curtains.position,Vector3.zero);foreach(var renderer in dressing.curtains.GetComponentsInChildren<Renderer>())b.Encapsulate(renderer.bounds);
                var frame=dressing.frame.bounds;
                Check(Mathf.Abs(b.center.z-frame.center.z)<.02f,"curtains centered on the actual window opening at "+frame.center);
                Check(Mathf.Abs(b.max.y-frame.max.y-.15f)<.02f&&b.min.y<frame.min.y,"curtain rail and hem aligned to frame");
                Check(b.size.z>frame.size.z&&b.size.z<frame.size.z+.45f,"curtain width follows window frame");
                Check(frame.center.x<0?b.min.x>frame.max.x:b.max.x<frame.min.x,"curtains hang inside the window without clipping its frame");
            }
            Physics.SyncTransforms();
            foreach(var actor in g.Campus.actors)
            {
                var body=actor.GetComponent<CapsuleCollider>();
                bool clear=g.Campus.GetComponentsInChildren<BoxCollider>().Where(b=>SchoolFurniture.Models.ContainsKey(b.name)).All(b=>!Physics.ComputePenetration(body,actor.transform.position,actor.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out _));
                Check(clear,"character home clears furniture: "+Cast.All[actor.id].name);
            }
            foreach(var clock in g.Campus.GetComponentsInChildren<Transform>().Where(t=>t.name=="Wall clock"))
            {
                var bounds=new Bounds(clock.position,Vector3.zero);foreach(var r in clock.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
                Check(bounds.size.z<.15f&&bounds.size.x>.35f&&bounds.size.y>.35f,"clock face parallel to wall: "+clock.position);
                Check(bounds.min.z>Mathf.Round(clock.position.z/10)*10-4.89f,"clock clears wall: "+clock.position);
            }
            foreach(var chair in g.Campus.GetComponentsInChildren<Transform>().Where(t=>t.name=="schoolChair"||t.name=="chairDesk"))
            {
                var b=CampusArt.LocalBounds(chair);Vector3 back=Vector3.zero;int count=0;
                foreach(var mesh in chair.GetComponentsInChildren<MeshFilter>())foreach(var v in mesh.sharedMesh.vertices)
                {var p=chair.InverseTransformPoint(mesh.transform.TransformPoint(v));if(p.y>b.max.y*.7f){back+=p;count++;}}
                back/=Mathf.Max(1,count);Vector3 worldBack=chair.TransformDirection(new Vector3(back.x,0,back.z));
                Check(Vector3.Dot(worldBack.normalized,Vector3.forward)>.8f,"chair back behind seated person at "+chair.position);
            }
            foreach(var a in g.Campus.actors)
            {
                var anim=a.GetComponentInChildren<Animator>();var arm=anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);var chest=anim.GetBoneTransform(HumanBodyBones.Chest);var pose=arm.localRotation;var chestPose=chest.localRotation;float motion=0;
                for(int sample=0;sample<6;sample++){yield return new WaitForSeconds(.25f);motion=Mathf.Max(motion,Quaternion.Angle(pose,arm.localRotation),Quaternion.Angle(chestPose,chest.localRotation));}
                Check(motion>.01f,"authored idle animation moves "+Cast.All[a.id].name);
            }
            g.Player.Teleport(new Vector3(0,.05f,4),0);yield return Shot("corridor");
            var podium=GameObject.Find("lectern");Check(podium.GetComponent<BoxCollider>().bounds.size.y<1.6f,"classroom podium has appropriate height");
            foreach(var mount in g.Campus.GetComponentsInChildren<Transform>().Where(t=>t.name=="Emergency wall sconce"))
                Check(!Physics.CheckBox(mount.position,new Vector3(.21f,.11f,.032f),mount.rotation,~0,QueryTriggerInteraction.Ignore),"emergency sconce clears wall at "+mount.position);
            g.Player.Teleport(new Vector3(0,.05f,12),90);g.Player.pitch=-15;yield return Shot("emergency-sconce");
            g.Player.Teleport(new Vector3(-8,.05f,3.6f),180);yield return Shot("classroom");
            g.Player.Teleport(new Vector3(-4.4f,.05f,-2.8f),180);g.Player.pitch=-24;yield return Shot("wall-clock");
            g.Player.Teleport(new Vector3(-8,.05f,3.6f),180);
            g.Player.lighterRaised=true;g.Player.ToggleFlame();yield return Shot("lighter");
            foreach(int id in new[]{0,2,3,5}){g.Talk(g.Campus.actors[id]);yield return Shot("person-"+id);g.LeaveTalk();}
            g.Player.Teleport(new Vector3(-8,.05f,33.5f),180);yield return Shot("office");
            g.Player.Teleport(new Vector3(8,.05f,21.8f),180);g.Player.pitch=4;yield return Shot("bathroom");
            var openStall=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.isStall&&d.room=="STALL 2");openStall.open=true;yield return Shot("bathroom-open");openStall.open=false;
            g.Player.Teleport(new Vector3(8,.05f,3.7f),180);yield return Shot("cafeteria");
            g.Player.Teleport(new Vector3(-8,.05f,21),310);g.Player.pitch=19;
            var candle=FindObjectsByType<TestCandle>(FindObjectsSortMode.None).First(c=>c.haunted);candle.Ignite();yield return Shot("candle");
            g.Mode=ScreenMode.Settings;yield return Shot("settings");g.Mode=ScreenMode.Play;
            var door=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room=="LECTURE 01");door.open=false;
            g.Player.Teleport(new Vector3(0,.05f,0),270);yield return Shot("door-closed");door.open=true;yield return Shot("door-open");
            g.Player.Teleport(new Vector3(-10.5f,.05f,0),270);g.Player.pitch=-5;yield return Shot("window-curtains");
            var npc=g.Campus.actors[5];npc.transform.position=new Vector3(0,0,15);npc.OnHour(12);g.Player.Teleport(new Vector3(0,.05f,11),0);yield return Shot("walking");
            g.BeginChase(new Vector3(0,0,16),true);yield return Shot("pursuer");
            foreach(var a in g.Campus.actors){var anim=a.GetComponentInChildren<Animator>();Check(anim.GetBoneTransform(HumanBodyBones.LeftHand).position.y<anim.GetBoneTransform(HumanBodyBones.Head).position.y+.2f,"natural resting arm pose: "+Cast.All[a.id].name);}
            g.Talk(g.Campus.actors[2]);g.Question(4);g.LeaveTalk();
            g.Interact(FindObjectsByType<Interactable>(FindObjectsSortMode.None).First(i=>i.kind=="supply"&&i.id=="keyledger"));
            g.Talk(g.Campus.actors[1]);g.Question(4);g.LeaveTalk();g.UI.OpenRequests();yield return Shot("requests");
            foreach(string clue in new[]{"key","register","bag","tissue","photo","report","maintenance","fuel","medical","cctv","files","analysis","flame","attendance","recording","protocol"})g.State.AddEvidence(clue);
            g.UI.OpenEvidence();yield return Shot("evidence-list");
            g.State.hour=18;g.State.evacuation=Enumerable.Range(0,12).Where(g.State.Available).ToList();g.Mode=ScreenMode.Evacuation;yield return Shot("manifest");
            var savedState=g.State;
            foreach(var ending in new[]{"THE LAST BUS","INFILTRATION","A CLEAN REGISTER","THE BUILDING REMAINS","THE PREVIOUS COHORT","OVERRUN"})
            {
                var state=GameState.New(5);state.ceilingSealed=ending!="THE BUILDING REMAINS";state.lecturerConfessed=ending=="THE PREVIOUS COHORT";
                state.evacuation=Enumerable.Range(0,12).Where(i=>!state.people[i].infiltrator).ToList();state.ending=ending;state.ended=true;
                if(ending=="INFILTRATION")state.evacuation.Add(Array.FindIndex(state.people,p=>p.infiltrator));
                if(ending=="A CLEAN REGISTER")state.evacuation.RemoveRange(0,3);
                g.State=state;g.Mode=ScreenMode.Ending;yield return Shot("ending-"+ending.ToLower().Replace(' ','-'));
            }
            g.State=savedState;
            foreach(var supply in FindObjectsByType<SupplyPickup>(FindObjectsSortMode.None))
            {
                bool supported=Physics.Raycast(supply.transform.position-Vector3.up*.002f,Vector3.down,out var hit,.06f)&&hit.collider.GetComponentInParent<SupplyPickup>()==null;
                Check(supported,"supply sits on furniture: "+supply.id);
            }
            var walls=g.Campus.GetComponentsInChildren<BoxCollider>().Where(c=>c.name=="Outer wall"||c.name=="Partition"||c.name=="Corridor partition").ToArray();
            foreach(var furniture in g.Campus.GetComponentsInChildren<BoxCollider>().Where(c=>SchoolFurniture.Models.ContainsKey(c.name)&&c.transform.parent==g.Campus.transform))
            {
                bool clear=walls.All(w=>!Physics.ComputePenetration(furniture,furniture.transform.position,furniture.transform.rotation,w,w.transform.position,w.transform.rotation,out _,out _));
                Check(clear,"furniture clears walls: "+furniture.name+" "+furniture.transform.position);
            }
            g.Mode=ScreenMode.Settings;
            string savedPreferences=PlayerPrefs.GetString("preferences","");int previousDisplay=g.Settings.displayMode;
            foreach(int mode in new[]{0,1,2})
            {
                g.Settings.displayMode=mode;g.Settings.Save();yield return new WaitForSeconds(1.5f);
                var expected=mode==0?FullScreenMode.ExclusiveFullScreen:mode==1?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed;
                Check(Screen.fullScreenMode==expected,"display mode applied: "+expected);
                Check(Preferences.Load().displayMode==mode,"display mode persisted: "+expected);
            }
            PlayerPrefs.SetString("preferences",savedPreferences);PlayerPrefs.Save();g.Settings.displayMode=previousDisplay;
            results.Add("ERRORS "+errors);File.WriteAllLines(Path.Combine(output,"graphics-validation.txt"),results);Debug.Log("GRAPHICS_REVIEW_COMPLETE errors="+errors);Application.Quit(errors==0?0:1);
        }
    }
}

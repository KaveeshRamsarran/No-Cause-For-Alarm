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
            var hand=g.Player.hand.GetComponentsInChildren<MeshFilter>().FirstOrDefault(f=>f.sharedMesh.vertexCount>100);
            Check(hand!=null,"imported WRAD hand replaces capsule hand");
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
            g.Player.lighterRaised=true;g.Player.ToggleFlame();yield return Shot("hand");
            foreach(int id in new[]{0,2,3,5}){g.Talk(g.Campus.actors[id]);yield return Shot("person-"+id);g.LeaveTalk();}
            g.Player.Teleport(new Vector3(-8,.05f,33.5f),180);yield return Shot("office");
            var door=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room=="LECTURE 01");door.open=false;
            g.Player.Teleport(new Vector3(0,.05f,0),270);yield return Shot("door-closed");door.open=true;yield return Shot("door-open");
            var npc=g.Campus.actors[5];npc.transform.position=new Vector3(0,0,15);npc.OnHour(12);g.Player.Teleport(new Vector3(0,.05f,11),0);yield return Shot("walking");
            g.BeginChase(new Vector3(0,0,16),true);yield return Shot("pursuer");
            foreach(var a in g.Campus.actors){var anim=a.GetComponentInChildren<Animator>();Check(anim.GetBoneTransform(HumanBodyBones.LeftHand).position.y<anim.GetBoneTransform(HumanBodyBones.Head).position.y+.2f,"natural resting arm pose: "+Cast.All[a.id].name);}
            results.Add("ERRORS "+errors);File.WriteAllLines(Path.Combine(output,"graphics-validation.txt"),results);Debug.Log("GRAPHICS_REVIEW_COMPLETE errors="+errors);Application.Quit(errors==0?0:1);
        }
    }
}

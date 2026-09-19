using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NoCauseForAlarm
{
    // Explicit opt-in standalone integration harness. Never active in an ordinary playthrough.
    public class SmokeRunner:MonoBehaviour
    {
        readonly List<string> results=new List<string>();readonly List<float> frameTimes=new List<float>();string output;int errors;
        void Update(){if(Time.realtimeSinceStartup>3&&Mathf.Approximately(Time.timeScale,1))frameTimes.Add(Time.unscaledDeltaTime*1000);}
        void Awake(){Application.logMessageReceived+=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;results.Add("ERROR "+message+"\n"+trace);}}
        void Check(bool condition,string message){results.Add((condition?"PASS ":"FAIL ")+message);if(!condition)errors++;}
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();
            float signal=0;for(int y=20;y<shot.height;y+=80)for(int x=20;x<shot.width;x+=80)signal+=shot.GetPixel(x,y).grayscale;
            Check(signal>.05f,"rendered screenshot: "+name);File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());Destroy(shot);yield return null;
        }
        IEnumerator Start()
        {
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Artifacts/Smoke"));Directory.CreateDirectory(output);
            var g=GameDirector.I;yield return new WaitForSeconds(2);Check(g.Campus.rooms.Count==12,"twelve connected rooms");Check(g.Campus.actors.Count==12,"twelve NPCs");
            yield return Capture("01-menu");g.NewGame();yield return new WaitForSeconds(1);yield return Capture("02-opening");
            for(int i=0;i<7;i++){g.IntroNext();yield return null;}Check(g.Mode==ScreenMode.Play,"opening reaches gameplay");
            yield return new WaitForSeconds(1);yield return Capture("03-lecture");
            g.Player.Teleport(new Vector3(0,.05f,-4),0);
            foreach(var actor in g.Campus.actors){actor.ResetBody();actor.transform.position=g.Campus.HomePosition(actor.id);}
            Physics.SyncTransforms();var beforeWalk=g.Campus.actors.Select(a=>a.transform.position).ToArray();
            foreach(var actor in g.Campus.actors)Check(actor.TryBeginWander(),"clear wandering destination for "+Cast.All[actor.id].name);
            yield return new WaitForSeconds(2.8f);
            foreach(var actor in g.Campus.actors)
            {
                Check(Vector3.Distance(beforeWalk[actor.id],actor.transform.position)>.2f,"authored walking moves "+Cast.All[actor.id].name);
                var body=actor.GetComponent<CapsuleCollider>();
                // Grounded capsules touch the floor; ignore PhysX's sub-millimetre contact tolerance.
                var collisions=g.Campus.GetComponentsInChildren<BoxCollider>().Where(b=>Physics.ComputePenetration(body,actor.transform.position,actor.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out float depth)&&depth>.005f).Select(b=>b.name).ToArray();
                Check(collisions.Length==0,"wander destination clears environment: "+Cast.All[actor.id].name+(collisions.Length==0?"":" / "+string.Join(", ",collisions)));
            }
            var talker=g.Campus.actors[2];talker.ResetBody();talker.TryBeginWander();g.Talk(talker);var talkPosition=talker.transform.position;
            yield return new WaitForSeconds(.4f);Check(!talker.IsWandering&&Vector3.Distance(talkPosition,talker.transform.position)<.01f,"conversation stops wandering");
            var towardPlayer=g.Player.transform.position-talker.transform.position;towardPlayer.y=0;Check(Vector3.Dot(talker.transform.forward,towardPlayer.normalized)>.99f,"interrupted walker turns toward the player for conversation");g.LeaveTalk();
            int[] trips=g.Campus.actors.Select(a=>a.WanderTrips).ToArray();Time.timeScale=4;yield return new WaitForSeconds(36);Time.timeScale=1;
            Check(g.Campus.actors.Count(a=>a.WanderTrips>trips[a.id])>=8,"independent idle timers trigger occasional automatic walks");
            var pausedPositions=g.Campus.actors.Select(a=>a.transform.position).ToArray();g.Mode=ScreenMode.Pause;yield return new WaitForSeconds(.5f);
            Check(g.Campus.actors.All(a=>Vector3.Distance(a.transform.position,pausedPositions[a.id])<.01f),"wandering pauses with the game");g.Mode=ScreenMode.Play;
            foreach(var actor in g.Campus.actors){actor.ResetBody();actor.transform.position=g.Campus.HomePosition(actor.id);}
            foreach(var r in g.Campus.rooms)
            {
                var door=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room==r.name);door.open=true;
                g.Player.Teleport(new Vector3(0,.05f,r.center.z),0);yield return new WaitForSeconds(.65f);
                float until=Time.time+1.8f;while(Time.time<until){g.Player.body.Move(new Vector3(r.side*2.5f,-2,0)*Time.deltaTime);yield return null;}
                Check(Mathf.Abs(g.Player.transform.position.x)>3.5f,"walkable doorway: "+r.name);
            }
            var archive=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room=="ARCHIVE");
            Check(archive.Locked,"archive starts access-locked");
            g.Player.Teleport(new Vector3(0,.05f,50),0);archive.open=true;g.State.power=false;
            archive.Toggle();Check(!archive.open&&archive.manualClosed,"archive closes during a power failure and remembers manual closure");
            archive.Toggle();Check(!archive.open,"locked archive still prevents entry from corridor");
            g.Player.Teleport(g.Campus.FindRoom("ARCHIVE").center+new Vector3(0,.05f,0),0);
            archive.Toggle();Check(archive.open,"archive permits emergency exit without power or clearance");g.State.power=true;
            foreach(var stall in FindObjectsByType<Door>(FindObjectsSortMode.None).Where(d=>d.isStall))
            {
                g.Player.Teleport(stall.transform.position+new Vector3(.65f,.05f,1.85f),180);
                stall.open=false;stall.Toggle();yield return new WaitForSeconds(.9f);
                Check(stall.open&&Quaternion.Angle(stall.transform.localRotation,Quaternion.identity)>95,"stall opens: "+stall.room);
                float until=Time.time+1.65f;while(Time.time<until){g.Player.body.Move(new Vector3(0,-2,-1.6f)*Time.deltaTime);yield return null;}
                Check(g.Player.transform.position.z<stall.transform.position.z-.3f,"stall doorway is traversable: "+stall.room);
                stall.Toggle();yield return new WaitForSeconds(.9f);
                Check(!stall.open&&Quaternion.Angle(stall.transform.localRotation,Quaternion.identity)<1,"stall closes from inside: "+stall.room);
                stall.Toggle();
            }
            var testStall=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.isStall);
            testStall.open=true;testStall.transform.localRotation=Quaternion.Euler(0,testStall.sign*testStall.swing,0);
            g.Player.Teleport(testStall.transform.position+new Vector3(.65f,.05f,0),0);testStall.Toggle();yield return new WaitForSeconds(1);
            Check(Quaternion.Angle(testStall.transform.localRotation,Quaternion.identity)>5,"closing door waits while player occupies its swing");
            g.Player.Teleport(testStall.transform.position+new Vector3(.65f,.05f,1.85f),0);yield return new WaitForSeconds(1);
            Check(Quaternion.Angle(testStall.transform.localRotation,Quaternion.identity)<1,"door finishes closing once player steps clear");
            var storesDoor=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room=="STORES");
            storesDoor.open=false;storesDoor.manualClosed=true;storesDoor.transform.localRotation=Quaternion.identity;
            var ada=g.Campus.actors[2];ada.transform.position=new Vector3(-4,0,40);ada.OnHour(12);yield return new WaitForSeconds(2);
            Check(!storesDoor.open&&ada.transform.position.x<-3,"scheduled NPC waits at a manually closed door");
            ada.ResetBody();ada.transform.position=g.Campus.HomePosition(2);storesDoor.open=true;storesDoor.manualClosed=false;
            foreach(var plate in g.Campus.GetComponentsInChildren<Interactable>().Where(i=>i.name=="Door operating plate"))
            {
                float direction=Mathf.Abs(plate.transform.position.x)<2.8f?Mathf.Sign(plate.transform.position.x):-Mathf.Sign(plate.transform.position.x);
                var origin=plate.transform.position-Vector3.right*direction*.6f;
                Check(Physics.Raycast(origin,Vector3.right*direction,out var hit,.7f)&&hit.collider.gameObject==plate.gameObject,"door plate reachable: "+plate.door.room+" "+(Mathf.Abs(plate.transform.position.x)<2.8f?"outside":"inside"));
            }
            var room=g.Campus.FindRoom("CLASSROOM 03");g.Player.Teleport(new Vector3(0,.1f,20),0);yield return new WaitForSeconds(.5f);yield return Capture("04-corridor");
            int actionCount=g.State.actions;g.Talk(g.Campus.actors[2]);g.Question(4);g.LeaveTalk();
            Check(!g.State.Has("key")&&g.State.actions==actionCount,"help request waits for physical supplies without spending an action");
            foreach(var supply in HelpRequests.Supplies){g.Interact(FindObjectsByType<Interactable>(FindObjectsSortMode.None).First(i=>i.kind=="supply"&&i.id==supply.id));Check(g.State.inventory.Contains(supply.id),"collectible enters inventory: "+supply.id);}
            Check(g.State.actions==actionCount,"collecting supplies costs no investigation actions");
            g.Save();var carried=SaveStore.Load();Check(carried.inventory.Count==9&&carried.collected.Count==9,"inventory survives save and load");
            g.Talk(g.Campus.actors[2]);g.Question(0);g.Question(4);Check(g.State.Has("key"),"caretaker grants key");yield return new WaitForSeconds(.8f);yield return Capture("05-dialogue");g.LeaveTalk();
            g.Talk(g.Campus.actors[1]);g.Question(4);g.LeaveTalk();Check(g.State.archiveUnlocked,"security grants clearance");
            foreach(string clue in new[]{"photo","report","tissue","register","medical","maintenance"})g.AddClue(clue);
            g.Talk(g.Campus.actors[4]);g.Question(4);g.LeaveTalk();Check(g.State.Has("analysis"),"sample analysis unlocks evidence");
            Check(!g.State.inventory.Contains("keyledger")&&!g.State.inventory.Contains("batteries")&&!g.State.inventory.Contains("samplekit"),"delivered supplies are consumed");
            foreach(int helper in new[]{3,5,6,7,8,9,10,11})
            {
                if(helper==10)g.AddClue("bag");
                g.Mode=ScreenMode.Play;g.Talk(g.Campus.actors[helper]);g.Question(4);g.LeaveTalk();Check(g.State.people[helper].helped,"completed help delivery for "+Cast.All[helper].name);
            }
            int clock=g.State.hour*4-g.State.actions;g.Talk(g.Campus.actors[2]);g.Question(4);g.LeaveTalk();Check(g.State.hour*4-g.State.actions==clock,"repeating a completed request grants no extra action cost or reward");
            Check(g.State.inventory.Count==0,"all nine delivered supplies leave inventory");
            g.ShowNotebook();yield return Capture("06-notebook");g.Mode=ScreenMode.Play;
            g.Talk(g.Campus.actors[0]);g.Question(6);g.LeaveTalk();Check(g.State.lecturerConfessed,"document-gated lecturer confession");
            g.State.hour=13;g.State.actions=4;g.Mode=ScreenMode.Play;g.Player.Teleport(room.center+new Vector3(-3.8f,.1f,1.8f),0);g.Player.pitch=24;
            var control=FindObjectsByType<TestCandle>(FindObjectsSortMode.None).First(c=>!c.haunted);control.Ignite();
            var candle=FindObjectsByType<TestCandle>(FindObjectsSortMode.None).First(c=>c.haunted);candle.Ignite();Time.timeScale=10;
            yield return new WaitForSeconds(48);Time.timeScale=1;Check(candle.lit&&g.State.ceilingDiscovered,"infinite flame survives calibrated duration and reveals ceiling");
            Check(!control.lit,"reference candle extinguishes after thirty seconds");yield return Capture("07-haunted-candle");
            Time.timeScale=8;yield return new WaitForSeconds(12);Time.timeScale=1;Check(g.Chase!=null,"ceiling encounter spawns pursuer");yield return Capture("08-encounter");
            g.State.ceilingSealed=true;yield return null;Check(g.Chase==null,"sealing ends chase");
            g.State.hour=14;g.State.power=false;g.State.actions=4;g.Mode=ScreenMode.Play;
            var fuse=FindObjectsByType<Interactable>(FindObjectsSortMode.None).First(i=>i.kind=="power");g.Interact(fuse);Check(g.State.power&&g.State.repaired,"power repair restores supply");
            g.State.hour=17;g.State.actions=1;g.Mode=ScreenMode.Play;g.Spend("Smoke final action");Check(g.State.hour==18,"day reaches evacuation");g.Mode=ScreenMode.Play;
            var exit=FindObjectsByType<Interactable>(FindObjectsSortMode.None).First(i=>i.kind=="evacuation");g.Interact(exit);Check(g.Mode==ScreenMode.Evacuation,"exit opens manifest");yield return Capture("09-manifest");
            int excludedPassenger=g.State.evacuation[0];g.State.evacuation.Remove(excludedPassenger);g.ShowNotebook();g.Mode=ScreenMode.Play;g.Interact(exit);
            Check(!g.State.evacuation.Contains(excludedPassenger),"manifest selections survive reviewing case notes and reopening exit");
            g.Save();var manifestSave=SaveStore.Load();Check(manifestSave.manifestPrepared&&!manifestSave.evacuation.Contains(excludedPassenger),"manifest selections persist through save and load");
            int first=Array.FindIndex(g.State.people,p=>p.infiltrator);g.State.Accuse(first);g.State.evacuation=Enumerable.Range(0,12).Where(i=>!g.State.people[i].infiltrator).ToList();g.End(false);
            Check(g.State.ending=="THE LAST BUS","integrated survival ending");yield return Capture("10-ending");
            g.Save();var loaded=SaveStore.Load();Check(loaded!=null&&loaded.ending==g.State.ending,"save/load roundtrip");g.MainMenu();g.NewGame();Check(g.Mode==ScreenMode.Intro&&!g.State.ended,"new game resets ending");
            // A second run follows the actual action economy without assigning the clock or granting clues directly.
            for(int i=0;i<7;i++)g.IntroNext();
            Action<int,int> ask=(id,choice)=>{g.Mode=ScreenMode.Play;g.Talk(g.Campus.actors[id]);g.Question(choice);g.LeaveTalk();};
            Action<string,string> use=(kind,id)=>{g.Mode=ScreenMode.Play;g.Interact(FindObjectsByType<Interactable>(FindObjectsSortMode.None).First(t=>t.kind==kind&&(id==""||t.id==id)));g.Mode=ScreenMode.Play;};
            use("supply","keyledger");ask(2,4);use("supply","batteries");ask(1,4);use("evidence","photo");use("evidence","report");ask(0,6);
            use("evidence","register");use("cctv","");use("computer","");use("evidence","tissue");use("supply","samplekit");ask(4,4);
            ask(7,3);use("evidence","maintenance");
            control=FindObjectsByType<TestCandle>(FindObjectsSortMode.None).First(c=>!c.haunted);control.Ignite();
            candle=FindObjectsByType<TestCandle>(FindObjectsSortMode.None).First(c=>c.haunted);candle.Ignite();
            g.Player.Teleport(new Vector3(0,.05f,4),0);g.Mode=ScreenMode.Play;Time.timeScale=10;yield return new WaitForSeconds(60);Time.timeScale=1;
            Check(g.State.ceilingDiscovered,"chronological run finds ceiling without injected evidence");use("seal","");yield return null;
            int[] culprits=Enumerable.Range(0,12).Where(i=>g.State.people[i].infiltrator).ToArray();
            foreach(int id in culprits){g.Talk(g.Campus.actors[id]);g.Accuse();g.Mode=ScreenMode.Play;yield return null;}
            if(!g.State.power)use("power","");use("evidence","fuel");
            while(g.State.hour<18){g.WaitHour();g.Mode=ScreenMode.Play;yield return null;}
            use("evacuation","");g.End(false);
            Check(g.State.ending=="THE LAST BUS"&&g.State.mistakes==0,"full chronological investigation reaches survival through ordinary actions");
            Check(g.State.Has("files")&&g.State.Has("analysis")&&g.State.lecturerConfessed,"full run retains combined clues and secret branch");
            g.MainMenu();g.Continue();Check(g.Mode==ScreenMode.Ending&&g.State.ending=="THE LAST BUS","continue restores completed case");
            frameTimes.Sort();if(frameTimes.Count>0)results.Add("PERFORMANCE "+SystemInfo.graphicsDeviceName+" / "+Screen.width+"x"+Screen.height+" / median "+frameTimes[frameTimes.Count/2].ToString("F2")+" ms / p95 "+frameTimes[(int)(frameTimes.Count*.95f)].ToString("F2")+" ms / allocated "+(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576)+" MiB");
            results.Add("TOTAL ERRORS: "+errors);File.WriteAllLines(Path.Combine(output,"results.txt"),results);Debug.Log("NCFA_SMOKE_COMPLETE errors="+errors);Application.Quit(errors==0?0:1);
        }
    }
}

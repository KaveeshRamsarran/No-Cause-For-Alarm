using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NoCauseForAlarm
{
    public enum ScreenMode {Menu,Intro,Play,Dialogue,Notebook,Map,Pause,Settings,Credits,Notice,Inspect,CCTV,Accuse,Evacuation,Ending}
    public class GameDirector:MonoBehaviour
    {
        public static GameDirector I;
        public GameState State;public Preferences Settings;public ScreenMode Mode=ScreenMode.Menu,ReturnMode;
        public Campus Campus;public FirstPerson Player;public Soundscape Audio;public GameUI UI;public Pursuer Chase;
        public NpcActor Talking;public string dialogue="",noticeTitle="",noticeText="",inspected="",toast="",subtitle="";
        public float toastUntil,subtitleUntil;public int introStep,notebookPerson;public bool confessionChoice;
        public string[] IntroLines={
            "09:07 / TUESDAY\n\nDR. VENN\nWe were discussing continuity. Whether remembering a life is the same thing as having lived it.",
            "[The lecturer stops. The lock turns.]\n\nDR. VENN\nBefore anyone leaves, I need you to listen. Someone in this room may remember a life that is not theirs.",
            "BEN\nIs this part of the assessment?\n\nDR. VENN\nNo. And being afraid does not make you guilty. Remember that.",
            "[Venn strikes a lighter.]\n\nDR. VENN\nWatch the flame. Watch what they watch. And if a flame keeps burning after it should have died... leave the room.",
            "[A chair scrapes. A neck turns too far.]\n\nA VOICE\nPut. That. Out.",
            "CAMPUS OPERATIONS\nThe east wing is under temporary lockdown. Please remain inside.\n\nThere is no cause for alarm.",
            "NO CAUSE FOR ALARM\n\n10:00 / EAST WING\n\nFour actions each hour. Talk. Investigate. Test. Decide.\nTransport is expected at 18:00. It is not a promise."
        };
        void Awake()
        {
            I=this;Settings=Preferences.Load();Settings.Apply(!Environment.GetCommandLineArgs().Contains("-screen-fullscreen"));State=GameState.New(1979);
            Campus=new GameObject("East Wing / Campus").AddComponent<Campus>();Campus.Build();
            Player=new GameObject("Student").AddComponent<FirstPerson>();Player.Build();Player.Teleport(new Vector3(0,.1f,-4),0);
            Audio=gameObject.AddComponent<Soundscape>();UI=gameObject.AddComponent<GameUI>();
            if(Environment.GetCommandLineArgs().Contains("-ncfa-smoke"))gameObject.AddComponent<SmokeRunner>();
            if(Environment.GetCommandLineArgs().Contains("-ncfa-art"))gameObject.AddComponent<GraphicsReview>();
        }
        void Update()
        {
            bool free=Mode==ScreenMode.Play;Cursor.lockState=free?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!free;
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(Mode==ScreenMode.Play)Pause();
                else if(Mode==ScreenMode.Dialogue||Mode==ScreenMode.Accuse)LeaveTalk();
                else if(Mode==ScreenMode.Pause)Mode=ScreenMode.Play;
                else if(Mode==ScreenMode.Notebook||Mode==ScreenMode.Map||Mode==ScreenMode.Inspect||Mode==ScreenMode.CCTV){Mode=ScreenMode.Play;Campus.securityCamera.enabled=false;}
                else if(Mode==ScreenMode.Settings){Settings.Save();Mode=ReturnMode;}
                else if(Mode==ScreenMode.Credits)Mode=ScreenMode.Menu;
            }
            if(Mode==ScreenMode.Menu){Player.view.transform.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.12f)*4,0);}
        }
        public void NewGame()
        {
            ClearTransient();State=GameState.New(unchecked((int)DateTime.UtcNow.Ticks));ResetActors();
            foreach(var door in FindObjectsByType<Door>(FindObjectsSortMode.None))door.open=door.room!="ARCHIVE"&&door.room!="STAFF OFFICE";
            Campus.actors[0].transform.position=new Vector3(-8,0,-3.3f);Campus.actors[0].transform.rotation=Quaternion.identity;
            for(int i=1;i<10;i++){Campus.actors[i].transform.position=i==9?new Vector3(-6,0,4.2f):new Vector3(i<=4?-11.8f:-4.2f,0,-2+((i-1)%4)*1.7f);Campus.actors[i].transform.rotation=Quaternion.Euler(0,180,0);}
            Player.Teleport(new Vector3(-8,.05f,3.6f),180);introStep=0;Mode=ScreenMode.Intro;Audio.Voice("intro0");
        }
        public void IntroNext()
        {
            introStep++;
            if(introStep==1){var door=FindObjectsByType<Door>(FindObjectsSortMode.None).First(d=>d.room=="LECTURE 01");door.open=false;Audio.OneShot("door",door.transform.position,1);}
            if(introStep==3){Player.lighterRaised=true;Player.ToggleFlame();}
            if(introStep==4)
            {
                Audio.OneShot("impact",Player.transform.position,.8f);
                // The opening glimpse is an unidentified visitor, not a reliable reveal of a randomized identity.
                BeginChase(new Vector3(-8,0,-2),false);if(Chase!=null)Chase.enabled=false;
            }
            if(introStep==5){if(Chase!=null){Destroy(Chase.gameObject);Chase=null;}Audio.Voice("lockdown");Audio.OneShot("alarm",Player.transform.position,.6f);}
            if(introStep>=IntroLines.Length)
            {
                ResetActors();Player.flameOn=false;Player.flame.SetActive(false);Mode=ScreenMode.Play;
                foreach(var d in FindObjectsByType<Door>(FindObjectsSortMode.None))if(d.room=="LECTURE 01")d.open=true;
                Save();Toast("WASD move  /  E interact  /  F ignite  /  TAB notebook  /  M map",12);
            }
        }
        void ResetActors()
        {
            for(int i=0;i<Campus.actors.Count;i++){var a=Campus.actors[i];a.ResetBody();a.transform.position=Campus.HomePosition(i);}
        }
        void ClearTransient()
        {
            if(Talking!=null){Player.Unfocus();Talking=null;}if(Chase!=null){Destroy(Chase.gameObject);Chase=null;}
            foreach(var c in FindObjectsByType<TestCandle>(FindObjectsSortMode.None)){c.lit=false;c.elapsed=0;}
            foreach(var f in GameObject.FindGameObjectsWithTag("Untagged"))if(f.name=="Candle flame")Destroy(f);
            Player.flameOn=false;Player.flame.SetActive(false);Campus.ceilingPanel.localRotation=Quaternion.identity;Campus.securityCamera.enabled=false;
        }
        public void Continue()
        {
            var s=SaveStore.Load();if(s==null){Toast("No valid case file found.");return;}ClearTransient();State=s;ResetActors();
            Player.Teleport(new Vector3(s.px,s.py,s.pz),s.yaw);Mode=s.ended?ScreenMode.Ending:ScreenMode.Play;
        }
        public void Save()
        {
            State.px=Player.transform.position.x;State.py=Player.transform.position.y;State.pz=Player.transform.position.z;State.yaw=Player.transform.eulerAngles.y;
            try{SaveStore.Save(State);}catch(Exception e){Debug.LogWarning("Could not save case: "+e.Message);Toast("Save failed. Check available disk space.");}
        }
        public void MainMenu(){ClearTransient();Mode=ScreenMode.Menu;Player.Teleport(new Vector3(0,.1f,-4),0);}
        public void Pause(){Mode=ScreenMode.Pause;Save();}
        public void ShowNotebook(){notebookPerson=0;Mode=ScreenMode.Notebook;}
        public void Toast(string text,float seconds=5){toast=text;toastUntil=Time.unscaledTime+seconds;}
        public void Subtitle(string text,float seconds=7){subtitle=text;subtitleUntil=Time.unscaledTime+seconds;}
        public void Notice(string title,string text){noticeTitle=title;noticeText=text;Mode=ScreenMode.Notice;}
        public void AddClue(string id)
        {
            if(State.AddEvidence(id)){State.journal.Add(State.hour.ToString("00")+":00 — "+EvidenceText.Name(id));Toast("Filed in notebook: "+EvidenceText.Name(id));Audio.OneShot("paper",Player.transform.position,.5f);}
        }
        public void Spend(string record)
        {
            if(State.hour>=18)return;State.journal.Add(State.hour.ToString("00")+":00 — "+record);
            if(State.Spend())
            {
                if(Talking!=null){Player.Unfocus();Talking=null;}
                if(State.hour==15)
                {
                    int victim=Enumerable.Range(4,8).FirstOrDefault(i=>State.Available(i)&&!State.people[i].infiltrator);
                    if(victim>0&&!State.ceilingSealed){State.people[victim].missing=true;State.journal.Add(Cast.All[victim].name+" did not answer the afternoon roll call.");}
                }
                foreach(var a in Campus.actors)a.OnHour(State.hour);
                string text=Cast.Hours[State.hour-11];int split=text.IndexOf('\n');Notice(text.Substring(0,split),text.Substring(split+1));
                Audio.Voice(State.hour==14?"power":State.hour==18?"evacuation":"remain");
                if(State.hour==16&&!State.ceilingSealed)BeginChase(new Vector3(0,0,40),true);
            }
            Save();
        }
        public void WaitHour()
        {int count=State.actions;for(int i=0;i<count;i++)Spend("Waited for the next campus update.");}
        public void Interact(Interactable item)
        {
            if(State.hour>=18&&item.kind!="door"&&item.kind!="map"&&item.kind!="evacuation"&&!(item.kind=="evidence"&&State.Has(item.id)))
            {Toast("The investigation period has ended. Review your manifest at the east exit.");return;}
            switch(item.kind)
            {
                case "supply":
                    if(State.collected.Contains(item.id)){Toast("You already picked that up.");break;}
                    State.collected.Add(item.id);State.inventory.Add(item.id);
                    State.journal.Add("Picked up "+HelpRequests.Name(item.id)+" in "+Campus.Location(item.transform.position)+".");
                    Toast("Collected: "+HelpRequests.Name(item.id)+" / TAB: supplies and requests");Audio.OneShot("paper",item.transform.position,.5f);Save();break;
                case "door":item.door.Toggle();break;
                case "map":Mode=ScreenMode.Map;break;
                case "candle":item.GetComponent<TestCandle>().Ignite();break;
                case "evidence":
                    if(!State.Has(item.id))
                    {
                        if(State.hour>=18){Toast("The investigation period has ended. Review your manifest.");return;}
                        AddClue(item.id);if(item.id=="fuel")State.fuel=Mathf.Min(100,State.fuel+70);
                        Spend("Inspected "+EvidenceText.Name(item.id)+".");if(Mode==ScreenMode.Notice)return;
                    }
                    inspected=item.id;Mode=ScreenMode.Inspect;break;
                case "cctv":
                    if(!State.power){Toast("Camera archive offline. Restore power in Utility.");break;}
                    if(!State.people[1].helped){Toast("Ruth Calder must authorise camera access. Ask her for help.");break;}
                    if(!State.Has("cctv")){AddClue("cctv");Spend("Reviewed the east-corridor recording.");if(Mode==ScreenMode.Notice)break;}
                    Campus.securityCamera.enabled=true;Mode=ScreenMode.CCTV;break;
                case "computer":
                    if(!State.power){Toast("The local cache terminal has no power.");break;}
                    if(!State.Available(5)){Toast("Noah is unavailable. Recovering the cache manually takes two actions.");}
                    if(!State.Has("files")){AddClue("files");int cost=State.Available(5)?1:2;for(int i=0;i<cost;i++)Spend("Recovered an access-cache fragment.");}
                    if(Mode!=ScreenMode.Notice){inspected="files";Mode=ScreenMode.Inspect;}break;
                case "power":
                    if(State.power){Toast("The supply is stable.");break;}
                    State.power=true;State.repaired=true;
                    int repairs=(State.HumanHelper(3)&&State.people[3].helped)||(State.HumanHelper(11)&&State.people[11].helped)?1:2;
                    for(int i=0;i<repairs;i++)Spend("Restored a fuse bank.");Toast("Power restored. Camera archive and electronic locks online.");break;
                case "seal":
                    if(!State.ceilingDiscovered){Toast("Four ducts. You need to locate the source before closing one.");break;}
                    if(!State.power&&!State.Has("key")){Toast("Actuator offline. Restore power or use Ada's brass override key.");break;}
                    if(State.ceilingSealed){Toast("Damper E-03 is sealed. Nothing is moving behind it.");break;}
                    State.ceilingSealed=true;Spend("Closed and secured extraction damper E-03.");Audio.OneShot("impact",item.transform.position,.8f);Toast("The scratching stops.");break;
                case "evacuation":
                    if(State.hour<18){Notice("TRANSPORT / NOT YET CLEARED","Transport is expected at 18:00. Use your remaining actions to investigate. You can wait for the next hour from the notebook.");break;}
                    State.evacuation=Enumerable.Range(0,12).Where(State.Available).ToList();Mode=ScreenMode.Evacuation;break;
            }
        }
        public void Talk(NpcActor actor)
        {
            if(!State.Available(actor.id))return;Talking=actor;Player.Focus(actor);Mode=ScreenMode.Dialogue;
            dialogue=Cast.Greeting(actor.id,State);
        }
        public void LeaveTalk(){if(Talking!=null){Player.Unfocus();Talking=null;}Mode=ScreenMode.Play;}
        public void Question(int choice)
        {
            if(Talking==null)return;int id=Talking.id;var p=Cast.All[id];var s=State.people[id];
            if(State.hour>=18&&choice>=2){dialogue="There is no time left. The manifest is at the east exit.";return;}
            switch(choice)
            {
                case 0:dialogue=p.alibi;s.observations.Add("Alibi: "+p.alibi);break;
                case 1:dialogue=p.witness;if(!s.observations.Contains(p.witness))s.observations.Add(p.witness);break;
                case 2:
                    dialogue=p.secret;s.conversations++;s.observations.Add("Under questioning: "+p.secret);Spend("Questioned "+p.name+" about a contradiction.");break;
                case 3:
                    if(s.tested){dialogue="You already watched me do this. What answer are you waiting for?";break;}
                    if(State.fuel<8){dialogue="Your lighter is empty. Find the refill in Stores.";break;}
                    s.tested=true;State.fuel-=8;Player.lighterRaised=true;if(!Player.flameOn)Player.ToggleFlame();
                    string reaction=s.infiltrator?(id%3==0?"Their jaw holds open a moment too long. Then: 'Put that away.'":id%3==1?"Their eyes follow the flame while their head stays perfectly still.":"They take one step back. Their hands are steady. 'Is this enough?' "):(id==7?"Ben recoils hard enough to strike the wall. His breathing becomes ragged.":"They watch your hand and ask you to put it out. Irritation, or effort. You cannot tell.");
                    dialogue=reaction;s.observations.Add("Controlled flame: "+reaction);if(s.infiltrator&&id%3==0)Talking.TransformBody();
                    Spend("Performed a controlled flame observation with "+p.name+".");break;
                case 4:Help(id);break;
                case 5:
                    if(State.evidence.Count<2){dialogue="An accusation needs more than a bad feeling. Collect at least two pieces of evidence.";break;}
                    Mode=ScreenMode.Accuse;break;
                case 6:
                    if(id==0&&State.Has("photo")&&State.Has("report"))
                    {State.lecturerConfessed=true;dialogue=p.secret+" The bus belongs to the same programme. Seal the duct. At six, stay with me and transmit the names.";s.observations.Add("Venn admitted directing the previous study.");Spend("Confronted Venn with Cohort 19.");}
                    break;
            }
        }
        void Help(int id)
        {
            var p=Cast.All[id];var s=State.people[id];
            if(s.helped){dialogue=HelpRequests.Thanks(id)+" Let me know if anything changes.";return;}
            s.helpRequested=true;
            if(!HelpRequests.Ready(id,State)){dialogue=HelpRequests.Request(id);Save();return;}
            if(id==5&&!State.power){dialogue="You found it, good. Keep hold of it until the power's back. I can't read the drive on a dead terminal.";Save();return;}
            string item=HelpRequests.Item(id);if(item!="")State.inventory.Remove(item);
            s.helped=true;dialogue=HelpRequests.Thanks(id);
            s.observations.Add("Help completed: "+dialogue);
            if(id==0)State.lecturerConfessed=true;
            if(id==1)State.archiveUnlocked=true;
            if(id==2)AddClue("key");
            if(id==3&&!s.infiltrator){State.power=true;State.repaired=true;}
            if(id==3&&s.infiltrator)dialogue="I've fitted it. I think that'll hold. If it trips again, try the cabinet in Utility.";
            if(id==4){AddClue("analysis");State.health=Mathf.Min(100,State.health+30);}
            if(id==5)AddClue("files");
            if(id==6)AddClue("attendance");
            if(id==7)AddClue("medical");
            if(id==8)AddClue("recording");
            if(id==9)State.health=Mathf.Min(100,State.health+25);
            if(id==10)AddClue("protocol");
            if(id==11)AddClue("maintenance");
            Spend("Completed a request for "+p.name+".");
        }

        public void Accuse()
        {
            if(Talking==null)return;int id=Talking.id;bool trueCase=State.people[id].infiltrator;Vector3 pos=Talking.transform.position;
            if(trueCase)Talking.TransformBody();State.Accuse(id);LeaveTalk();
            Spend("Detained "+Cast.All[id].name+". Their skills are no longer available.");
            if(trueCase){BeginChase(pos,false,id);Subtitle("[A familiar voice breaks into two. The restraint does not hold its shape.]",7);}
            else Subtitle(Cast.All[id].name+": You can lock me in. It won't make you right.",7);
            Save();
        }
        public void BeginChase(Vector3 pos,bool hidden,int identity=-1)
        {
            if(Chase!=null)return;var g=new GameObject(hidden?"Ceiling organism":"Exposed mimic");g.transform.position=new Vector3(pos.x,.1f,pos.z);Chase=g.AddComponent<Pursuer>();Chase.fromCeiling=hidden;Chase.Build(identity);
            if(Mode==ScreenMode.Play)Toast("Something is following. Sprint, use the lighter, or shut a door.",8);
        }
        public void End(bool stay)
        {
            if(Talking!=null)LeaveTalk();State.ending=State.ResolveEnding(stay);State.ended=true;Mode=ScreenMode.Ending;Save();Audio.Voice("ending");
        }
    }
}

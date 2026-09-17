using System;
using UnityEngine;

namespace NoCauseForAlarm
{
    public class GameUI:MonoBehaviour
    {
        GameDirector G=>GameDirector.I;
        public CastPortraits Portraits {get;private set;}
        void Awake(){Portraits=gameObject.AddComponent<CastPortraits>();}
        public void OpenRequests(){tab=3;scroll=Vector2.zero;G.Mode=ScreenMode.Notebook;}
        public void OpenEvidence(){tab=1;scroll=evidenceScroll=Vector2.zero;G.Mode=ScreenMode.Notebook;}
        readonly Color cream=new Color(.94f,.92f,.84f),muted=new Color(.72f,.76f,.69f),amber=new Color(.86f,.72f,.42f),ink=new Color(.035f,.048f,.047f,.97f);
        GUIStyle label,small,title,button,body;int tab,evidenceIndex;Vector2 scroll,evidenceScroll;Texture2D pixel;bool ready,hasSave;float nextSaveCheck;
        void Init()
        {
            if(ready)return;ready=true;pixel=Texture2D.whiteTexture;
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label=new GUIStyle(GUI.skin.label){font=font,fontSize=19,wordWrap=true};label.normal.textColor=cream;
            small=new GUIStyle(label){fontSize=13};small.normal.textColor=muted;
            title=new GUIStyle(label){fontSize=55,fontStyle=FontStyle.Bold};
            body=new GUIStyle(label){fontSize=21,richText=false};
            button=new GUIStyle(GUI.skin.button){font=font,fontSize=17,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,12,8,8),wordWrap=true};
            button.normal.background=Texture(ink);button.normal.textColor=cream;button.hover.background=Texture(new Color(.19f,.25f,.22f));button.hover.textColor=Color.white;
            button.active.background=Texture(amber);button.active.textColor=ink;button.focused=button.hover;
        }
        Texture2D Texture(Color c){var t=new Texture2D(1,1);t.SetPixel(0,0,c);t.Apply();return t;}
        void Fill(Rect r,Color c){var previous=GUI.color;GUI.color=c;GUI.DrawTexture(r,pixel);GUI.color=previous;}
        void Text(string s,float x,float y,float w,float h,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),s,style??label);}
        bool Button(string s,float x,float y,float w=340,float h=44){return GUI.Button(new Rect(x,y,w,h),s,button);}
        void Rule(float x,float y,float w){Fill(new Rect(x,y,w,1),new Color(.37f,.42f,.36f,.55f));}
        void Panel(string eyebrow,string heading)
        {Fill(new Rect(0,0,1280,720),ink);Text(eyebrow,60,34,1100,25,small);Text(heading,60,77,1130,75,title);Rule(60,167,1160);}
        void OnGUI()
        {
            if(G==null)return;Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
            var mode=G.Mode;
            if(mode==ScreenMode.Play)HUD();
            else if(mode==ScreenMode.Menu)Menu();
            else if(mode==ScreenMode.Intro)Intro();
            else if(mode==ScreenMode.Dialogue||mode==ScreenMode.Accuse)Dialogue();
            else if(mode==ScreenMode.Notebook)Notebook();
            else if(mode==ScreenMode.Map)Map();
            else if(mode==ScreenMode.Pause)Pause();
            else if(mode==ScreenMode.Settings)Settings();
            else if(mode==ScreenMode.Credits)Credits();
            else if(mode==ScreenMode.Notice)Notice();
            else if(mode==ScreenMode.Inspect)Inspect();
            else if(mode==ScreenMode.CCTV)CCTV();
            else if(mode==ScreenMode.Evacuation)Evacuation();
            else if(mode==ScreenMode.Ending)Ending();
            if(G.Settings.effects>.01f)
            {
                for(int y=0;y<720;y+=4)Fill(new Rect(0,y,1280,1),new Color(0,0,0,G.Settings.effects*.075f));
                Fill(new Rect(0,0,1280,2),new Color(.65f,.74f,.61f,.12f));
            }
            if(Time.unscaledTime<G.toastUntil&&(mode==ScreenMode.Play||mode==ScreenMode.Dialogue||mode==ScreenMode.Inspect))
            {float y=mode==ScreenMode.Dialogue?665:22;Fill(new Rect(260,y,550,40),new Color(.035f,.048f,.047f,.93f));Text(G.toast,278,y+8,514,30,small);}
            if(Time.unscaledTime<G.subtitleUntil&&G.Settings.subtitles&&mode==ScreenMode.Play)
            {Fill(new Rect(220,554,840,65),new Color(.025f,.032f,.029f,.88f));Text(G.subtitle,245,566,790,49,label);}
        }
        void Menu()
        {
            Fill(new Rect(0,0,670,720),new Color(.022f,.032f,.029f,.91f));Fill(new Rect(58,61,34,4),amber);
            Text("BELLWETHER COLLEGE  /  EAST WING",60,84,560,24,small);
            var hero=new GUIStyle(title){fontSize=68};Text("NO CAUSE\nFOR ALARM",55,132,610,172,hero);
            Text("A record of an ordinary day.",62,320,510,32,label);Rule(60,375,482);
            if(Button("01   BEGIN A NEW DAY",60,401,480))G.NewGame();
            if(Time.unscaledTime>=nextSaveCheck){hasSave=SaveStore.Load()!=null;nextSaveCheck=Time.unscaledTime+1;}
            GUI.enabled=hasSave;if(Button("02   CONTINUE CASE FILE",60,453,480))G.Continue();GUI.enabled=true;
            if(Button("03   SETTINGS",60,505,230)){G.ReturnMode=ScreenMode.Menu;G.Mode=ScreenMode.Settings;}
            if(Button("04   CREDITS",306,505,234))G.Mode=ScreenMode.Credits;
            if(Button("05   QUIT",60,557,480))Application.Quit();
            Text("PSYCHOLOGICAL HORROR  /  HEADPHONES RECOMMENDED",60,663,690,22,small);
            Text("CAMPUS STATUS\nTEMPORARILY CLOSED",987,633,230,50,small);
        }
        void HUD()
        {
            Fill(new Rect(32,26,235,63),new Color(.02f,.03f,.025f,.72f));
            Text(G.State.hour.ToString("00")+":00   /   "+(G.State.hour<18?G.State.actions+" ACTIONS":"TRANSPORT"),48,36,245,25,label);
            Text(G.Campus.Location(G.Player.transform.position),48,64,400,23,small);
            Fill(new Rect(638,358,4,4),cream);
            string prompt=G.Player.npcTarget!=null?"E  /  TALK TO "+Cast.All[G.Player.npcTarget.id].name.ToUpper():G.Player.target!=null?"E  /  "+(G.Player.target.door!=null?G.Player.target.door.Prompt:G.Player.target.prompt):"";
            if(prompt!=""){Fill(new Rect(335,628,610,44),new Color(.02f,.03f,.025f,.82f));Text(prompt,355,639,570,27,label);}
            Text("TAB  CASE NOTES     M  DIRECTORY     F  FLAME     R  RAISE / LOWER",34,682,800,25,small);
            if(G.Player.lighterRaised)Text("FUEL  "+Mathf.CeilToInt(G.State.fuel)+"%",1070,638,180,25,small);
            if(G.Player.stamina<98){Fill(new Rect(1090,682,150,3),muted);Fill(new Rect(1090,682,G.Player.stamina*1.5f,3),cream);}
            if(G.State.health<100)Text("CONDITION  "+Mathf.Max(0,(int)G.State.health)+"%",1060,606,200,27,small);
            if(G.Chase!=null)Text("KEEP MOVING",1030,36,220,30,label);
        }
        void Intro()
        {
            Fill(new Rect(0,0,1280,85),new Color(.01f,.017f,.015f,.9f));Fill(new Rect(0,395,1280,325),new Color(.015f,.025f,.022f,.95f));
            Text("BELLWETHER / TUESDAY / 09:07",60,30,1000,25,small);
            Text(G.IntroLines[Mathf.Min(G.introStep,G.IntroLines.Length-1)],80,426,1060,210,body);
            Text((G.introStep+1).ToString("00")+" / 07",80,665,200,25,small);
            if(Button(G.introStep==6?"ENTER LOCKDOWN  →":"CONTINUE  →",930,650,270))G.IntroNext();
        }
        void Dialogue()
        {
            if(G.Talking==null){G.Mode=ScreenMode.Play;return;}
            int id=G.Talking.id;var p=Cast.All[id];Fill(new Rect(0,0,1280,102),new Color(.018f,.027f,.023f,.87f));
            Text(p.name.ToUpper(),55,25,900,42,new GUIStyle(title){fontSize=29});Text(p.role+"  /  "+p.room,57,68,800,25,small);
            if(G.Mode==ScreenMode.Accuse)
            {
                Fill(new Rect(0,380,1280,340),new Color(.02f,.03f,.025f,.97f));
                Text("DETENTION IS A DECISION, NOT A TEST.",60,410,1110,45,new GUIStyle(title){fontSize=29});
                Text("Remove "+p.name+" from the group? Their help will become unavailable. If you are wrong, this will count against you. Review the evidence before you commit.",60,470,1080,92,body);
                if(Button("CONFIRM DETENTION",60,616,550))G.Accuse();
                if(Button("RETURN TO CONVERSATION",632,616,580))G.Mode=ScreenMode.Dialogue;return;
            }
            Fill(new Rect(805,102,475,618),new Color(.016f,.025f,.015f,.90f));
            Fill(new Rect(0,524,805,196),new Color(.015f,.024f,.012f,.91f));
            Text(G.dialogue,42,549,727,129,body);
            Text("ASK. WATCH. REMEMBER.",842,129,370,27,small);
            if(Button("Where were you?  /  free",842,175,370))G.Question(0);
            if(Button("Who did you see?  /  free",842,230,370))G.Question(1);
            string help=G.State.people[id].helped?"Follow up / free":HelpRequests.Ready(id,G.State)?"Complete request / 1 action":"Ask for help / free";
            if(Button(help,842,285,370))G.Question(4);
            GUI.enabled=G.State.evidence.Count>0;if(Button("Press a contradiction  /  1 action",842,340,370))G.Question(2);GUI.enabled=true;
            if(Button("Controlled flame  /  1 action",842,395,370))G.Question(3);
            if(Button("Accuse  /  1 action",842,450,370))G.Question(5);
            if(id==0&&G.State.Has("photo")&&G.State.Has("report")){if(Button("Show Cohort 19 documents",842,505,370))G.Question(6);}
            Text("A reaction is an observation.\nIt is not proof.",842,568,370,50,small);
            if(Button("LEAVE CONVERSATION",842,645,370))G.LeaveTalk();
        }
        void Notebook()
        {
            Panel("STUDENT CASE FILE  /  SUBJECTIVE NOTES ARE NOT VERDICTS","The attendance of the living");
            if(Button("PEOPLE",60,185,180)) {tab=0;scroll=Vector2.zero;}
            if(Button("EVIDENCE / "+G.State.evidence.Count,254,185,205))OpenEvidence();
            if(Button("CHRONOLOGY",473,185,210)){tab=2;scroll=Vector2.zero;}
            if(Button("SUPPLIES / REQUESTS",697,185,285))OpenRequests();
            if(Button("CLOSE  /  ESC",1000,185,220))G.Mode=ScreenMode.Play;
            if(tab==0)
            {
                for(int i=0;i<12;i++)if(Button((G.State.people[i].detained?"[D] ":G.State.people[i].missing?"[?] ":"")+Cast.All[i].name,60,242+i*33,310,30)){G.notebookPerson=i;scroll=Vector2.zero;}
                var p=Cast.All[G.notebookPerson];var s=G.State.people[G.notebookPerson];
                Text(p.name.ToUpper(),406,246,800,46,new GUIStyle(title){fontSize=32});
                Text(p.role+" / "+(s.detained?"DETAINED":s.missing?"MISSING":s.dead?"DECEASED":"ACCOUNTED FOR"),408,292,800,27,small);
                string[] tags={"UNSURE","TRUST","SUSPICIOUS"};for(int j=0;j<3;j++)if(Button((s.trustTag==j?"● ":"○ ")+tags[j],408+j*258,334,242,37))s.trustTag=j;
                scroll=GUI.BeginScrollView(new Rect(408,393,790,216),scroll,new Rect(0,0,756,Mathf.Max(216,s.observations.Count*105)));
                if(s.observations.Count==0)Text("No observations recorded. Begin with their account of the morning.",0,0,735,100,body);
                for(int i=0;i<s.observations.Count;i++)Text("— "+s.observations[i],0,i*105,735,102,label);GUI.EndScrollView();
            }
            else if(tab==1)
            {
                evidenceScroll=GUI.BeginScrollView(new Rect(60,242,332,379),evidenceScroll,new Rect(0,0,310,Mathf.Max(378,G.State.evidence.Count*32)));
                for(int i=0;i<G.State.evidence.Count;i++)if(Button(EvidenceText.Name(G.State.evidence[i]),0,i*32,309,30)){evidenceIndex=i;scroll=Vector2.zero;}
                GUI.EndScrollView();
                if(G.State.evidence.Count>0)
                {evidenceIndex=Mathf.Clamp(evidenceIndex,0,G.State.evidence.Count-1);string id=G.State.evidence[evidenceIndex];Text(EvidenceText.Name(id).ToUpper(),410,254,780,65,new GUIStyle(title){fontSize=29});Text(EvidenceText.Describe(id,G.State),410,345,765,270,body);}
                else Text("No physical evidence filed yet.",410,266,780,100,body);
            }
            else if(tab==3)
            {
                Text("CARRYING",60,248,320,30,small);
                if(G.State.inventory.Count==0)Text("No supplies in your bag. Ask people what they need, then search the rooms they mention.",60,291,315,130,body);
                for(int i=0;i<G.State.inventory.Count;i++)Text(HelpRequests.Name(G.State.inventory[i]),60,290+i*32,320,31,label);
                int count=0;foreach(var person in G.State.people)if(person.helpRequested||person.helped)count++;
                scroll=GUI.BeginScrollView(new Rect(410,250,790,368),scroll,new Rect(0,0,755,Mathf.Max(366,count*104)));
                int row=0;
                for(int i=0;i<12;i++)
                {
                    var person=G.State.people[i];if(!person.helpRequested&&!person.helped)continue;
                    string item=HelpRequests.Item(i);string status=person.helped?"COMPLETED":!G.State.Available(i)?"PERSON UNAVAILABLE":HelpRequests.Ready(i,G.State)?"READY TO RETURN":"LOOKING FOR SUPPLIES";
                    Text(Cast.All[i].name+" / "+status,0,row*104,740,29,label);
                    Text(person.helped?"Your delivery and their response are recorded under People.":HelpRequests.Ready(i,G.State)?"Return to "+Cast.All[i].name+" / "+G.Campus.Location(G.Campus.actors[i].transform.position)+". Choose Complete request.":item!=""?HelpRequests.Name(item)+" - "+HelpRequests.Location(item)+(i==4?"; also collect the bathroom tissue.":"."):i==0?"Photograph in Staff Office and report in Archive.":i==6?"Attendance register in Lecture 01.":"Candle sleeve in the Classroom 03 bag.",0,row*104+33,740,63,small);row++;
                }
                if(count==0)Text("Ask someone for help to record their request here. Collecting supplies is free; completing a request costs one action.",0,0,735,105,body);
                GUI.EndScrollView();
            }
            else
            {
                scroll=GUI.BeginScrollView(new Rect(60,250,1158,356),scroll,new Rect(0,0,1120,Mathf.Max(355,G.State.journal.Count*51)));
                for(int i=0;i<G.State.journal.Count;i++)Text(G.State.journal[i],0,i*51,1100,48,label);GUI.EndScrollView();
            }
            Rule(60,638,1160);Text("CURRENT HOUR  "+G.State.hour+":00     /     "+G.State.actions+" ACTIONS REMAIN",60,665,700,29,small);
            GUI.enabled=G.State.hour<18;if(Button("WAIT UNTIL NEXT HOUR",900,653,320))G.WaitHour();GUI.enabled=true;
        }
        void Map()
        {
            Panel("BELLWETHER COLLEGE / EMERGENCY DIRECTORY","East teaching wing");
            Text("NORTH  /  EVACUATION EXIT ↑",490,185,650,30,small);
            Fill(new Rect(603,224,74,389),new Color(.25f,.31f,.28f));
            for(int i=0;i<6;i++)
            {
                var left=G.Campus.rooms[i*2];var right=G.Campus.rooms[i*2+1];float y=566-i*64;
                Fill(new Rect(330,y,259,51),new Color(.15f,.21f,.18f));Fill(new Rect(691,y,259,51),new Color(.15f,.21f,.18f));
                Text(left.name,345,y+15,240,30,small);Text(right.name,709,y+15,235,30,small);
            }
            var p=G.Player.transform.position;float py=588-Mathf.Clamp(p.z,0,50)*6.4f;float px=p.x<-3?457:p.x>3?822:636;
            Fill(new Rect(px,py,9,9),amber);Text("YOU",px+15,py-7,80,24,small);
            Text("●  YOU\n\nSTAFF OFFICE\nBrass key required\n\nARCHIVE\nSecurity clearance + power",61,240,237,250,label);
            Text("Movement costs no actions.\nRoom searches and controlled tests do.\n\nThe east exit opens at 18:00.",1000,280,210,250,label);
            if(Button("RETURN TO CORRIDOR",900,651,320))G.Mode=ScreenMode.Play;
        }
        void Pause()
        {
            Panel("CASE FILE SAVED","A moment of quiet");
            if(Button("RESUME",60,210,480))G.Mode=ScreenMode.Play;
            if(Button("CASE NOTES",60,266,480))G.ShowNotebook();
            if(Button("SETTINGS",60,322,480)){G.ReturnMode=ScreenMode.Pause;G.Mode=ScreenMode.Settings;}
            if(Button("SAVE AND RETURN TO MENU",60,378,480)){G.Save();G.MainMenu();}
            if(Button("QUIT TO DESKTOP",60,434,480)){G.Save();Application.Quit();}
            Text("WASD  Move\nMOUSE  Look\nE  Interact\nSHIFT  Sprint\nCTRL  Crouch\nF  Ignite / extinguish\nR  Raise / lower lighter\nTAB  Notebook\nM  Campus directory\nESC  Pause / close",720,214,480,380,body);
        }
        void Settings()
        {
            Panel("PREFERENCES / SAVED ON RETURN","Adjust the signal");var s=G.Settings;
            s.master=Slider("MASTER VOLUME",s.master,0,1,60,214);s.music=Slider("MUSIC / DRONE",s.music,0,1,60,284);s.sfx=Slider("EFFECTS / VOICE",s.sfx,0,1,60,354);
            s.sensitivity=Slider("MOUSE SENSITIVITY",s.sensitivity,.3f,3,60,424);s.brightness=Slider("BRIGHTNESS",s.brightness,.65f,1.8f,60,494);
            s.fov=Slider("FIELD OF VIEW",s.fov,60,100,680,214);s.effects=Slider("ANALOG / GRAIN / FLICKER",s.effects,0,1,680,284);s.shake=Slider("CAMERA MOVEMENT",s.shake,0,1,680,354);
            if(Button("SUBTITLES   "+(s.subtitles?"ON":"OFF"),680,430,520))s.subtitles=!s.subtitles;
            if(Button("GRAPHICS   "+(s.quality==0?"LOW":s.quality==1?"MEDIUM":"HIGH"),680,494,520))s.quality=(s.quality+1)%3;
            if(Button("DISPLAY   "+(s.displayMode==0?"FULLSCREEN":s.displayMode==1?"BORDERLESS WINDOWED":"WINDOWED"),680,558,520))s.displayMode=(s.displayMode+1)%3;
            Text("Critical clues stay in the notebook. Zero analog/camera movement reduces motion.\nDisplay changes take effect when you apply.",60,604,1100,44,small);
            if(Button("APPLY AND RETURN",900,650,320)){s.Save();G.Mode=G.ReturnMode;}
            AudioListener.volume=s.master;
        }
        float Slider(string name,float value,float min,float max,float x,float y)
        {Text(name,x,y,440,26,small);Text(value.ToString("0.00"),x+443,y,85,28,small);return GUI.HorizontalSlider(new Rect(x,y+34,520,20),value,min,max);}
        void Notice()
        {
            Panel("CAMPUS OPERATIONS / PUBLIC ADDRESS",G.noticeTitle);Text(G.noticeText,80,245,1080,190,new GUIStyle(body){fontSize=27});
            Text("You may continue to investigate. The official account is not the only account.",80,525,1090,55,small);
            if(Button("ACKNOWLEDGE",900,650,320))G.Mode=ScreenMode.Play;
        }
        void Inspect()
        {
            Panel("EVIDENCE / "+G.Campus.Location(G.Player.transform.position),EvidenceText.Name(G.inspected));
            Fill(new Rect(65,215,12,330),amber);Text(EvidenceText.Describe(G.inspected,G.State),111,225,1010,355,new GUIStyle(body){fontSize=26});
            Text("A copy has been filed in your notebook.",110,599,800,30,small);
            if(Button("PUT DOWN",900,650,320))G.Mode=ScreenMode.Play;
        }
        void CCTV()
        {
            Panel("SECURITY / INCOMPLETE RECORDING","East corridor. 09:02.");
            GUI.DrawTexture(new Rect(60,207,640,360),G.Campus.securityTexture,ScaleMode.ScaleToFit,false);
            Text("LIVE REFERENCE VIEW / ARCHIVE NOTES →",66,581,665,30,small);
            Text(EvidenceText.Describe("cctv",G.State),750,217,450,335,body);
            if(Button("LEAVE TERMINAL",900,650,320)){G.Campus.securityCamera.enabled=false;G.Mode=ScreenMode.Play;}
        }
        void Evacuation()
        {
            Panel("18:00 / FINAL PASSENGER MANIFEST","Who leaves with you?");
            Text("Click a card to select a passenger. Your trust notes are not proof of identity.\nLeaving someone off this list does not detain them. Review your evidence before departure.",60,183,1140,49,label);
            for(int i=0;i<12;i++)
            {
                bool available=G.State.Available(i),selected=available&&G.State.evacuation.Contains(i);int col=i%3,row=i/3;
                float x=60+col*391,y=237+row*92;var state=G.State.people[i];
                GUI.enabled=available;
                if(Button("",x,y,376,84)){if(selected)G.State.evacuation.Remove(i);else G.State.evacuation.Add(i);G.Save();}
                GUI.enabled=true;
                if(Portraits.images[i]!=null)GUI.DrawTexture(new Rect(x+6,y+5,63,74),Portraits.images[i],ScaleMode.ScaleToFit);
                if(selected)Fill(new Rect(x,y,3,84),amber);
                Text(Cast.All[i].name,x+80,y+6,287,26,new GUIStyle(label){fontSize=18});
                Text(Cast.All[i].role,x+80,y+32,280,23,small);
                string status=state.dead?"DECEASED":state.missing?"MISSING":state.detained?"DETAINED":selected?"[X] BOARDING":"[ ] LEFT BEHIND";
                Text(status+(available?" / "+(state.trustTag==1?"TRUST":state.trustTag==2?"SUSPICIOUS":"UNSURE"):""),x+80,y+58,283,23,small);
            }
            Text(G.State.evacuation.FindAll(G.State.Available).Count+" passengers selected. Excluded people remain at the college.",60,609,1130,27,small);
            if(Button("RETURN TO CASE NOTES",60,645,350)){G.ShowNotebook();}
            if(G.State.lecturerConfessed&&G.State.ceilingSealed&&G.State.HumanHelper(0)){if(Button("STAY / TRANSMIT THE EVIDENCE",426,645,405))G.End(true);}
            if(Button("AUTHORISE DEPARTURE",850,645,370))G.End(false);
        }
        void Ending()
        {
            Panel("NO CAUSE FOR ALARM / FINAL REPORT",G.State.ending);
            Text(EndingReport.Narrative(G.State),80,195,1090,262,new GUIStyle(body){fontSize=23});
            Rule(80,474,1100);Text(EndingReport.Consequences(G.State),80,490,1090,138,new GUIStyle(small){fontSize=16});
            if(Button("RETURN TO MENU",60,650,350))G.MainMenu();if(Button("ANOTHER DAY",438,650,350))G.NewGame();
            if(Button("CREDITS",816,650,400))G.Mode=ScreenMode.Credits;
        }
        void Credits()
        {
            Panel("A GAME ABOUT CERTAINTY","NO CAUSE FOR ALARM");
            Text("Created for Kaveesh Ramsarran\nCity People models and animations: Denys Almaral (free Unity Store pack)\nVintage Living Room architecture: ZNS3D (free Unity Store pack)\nSchool assets: A.R.S|T. (free Unity Store pack)\nVintage Lighter: Slinc / Poly Haven (CC0)\nAdditional fixtures: Styloo / Kenney (CC0)\nConcrete footsteps: supplied by project owner, free-use confirmation\nOriginal ambience and local system-voice PA\nBuilt with Unity 6 / URP — full sources: ASSET_CREDITS.txt\n\nAll characters, institutions and events are fictional.",60,205,1130,388,body);
            Text("THERE IS NO CAUSE FOR ALARM.",60,593,1100,40,small);
            if(Button("RETURN TO MENU",900,650,320))G.MainMenu();
        }
    }
}

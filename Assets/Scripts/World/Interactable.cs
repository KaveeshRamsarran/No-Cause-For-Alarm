using UnityEngine;

namespace NoCauseForAlarm
{
    public class Interactable:MonoBehaviour
    {
        public string kind,prompt,id; public Door door;
        public void Setup(string k,string p,string i){kind=k;prompt=p;id=i;}
        public void Use(){GameDirector.I.Interact(this);}
    }
    public class Door:MonoBehaviour
    {
        public string room;public bool open,manualClosed,isStall;public float sign;
        public float swing=94;BoxCollider leaf;bool waitingForPlayer;
        public string Prompt => (open?"CLOSE ":"OPEN ")+room+(isStall?"":" DOOR");
        public bool Locked
        {
            get{var s=GameDirector.I.State;return (room=="ARCHIVE"&&(!s.archiveUnlocked||!s.power))||(room=="STAFF OFFICE"&&!s.Has("key"));}
        }
        public void Toggle()
        {
            var g=GameDirector.I;
            // A lock controls entry, never closing or escape from inside the room.
            bool inside=!isStall&&g.Campus.Location(g.Player.transform.position)==room;
            if(!open&&Locked&&!inside){g.Toast(room=="ARCHIVE"?"Archive lock: power and security clearance required.":"Brass lock. Ada Moss carries the staff key.");return;}
            open=!open;manualClosed=!open;g.Audio.OneShot("door",transform.position,.7f);
        }
        void Update()
        {
            if(leaf==null)leaf=GetComponentInChildren<BoxCollider>();
            var next=Quaternion.RotateTowards(transform.localRotation,Quaternion.Euler(0,open?sign*swing:0,0),Time.deltaTime*150);
            // Stop at the player instead of pushing them through the wall with a rotating collider.
            var player=GameDirector.I.Player;
            if(leaf!=null&&player!=null&&Quaternion.Angle(next,transform.localRotation)>.01f)
            {
                var rotation=transform.parent.rotation*next;
                var position=transform.position+rotation*Vector3.Scale(leaf.transform.localPosition,transform.lossyScale);
                var leafRotation=rotation*leaf.transform.localRotation;
                var center=position+leafRotation*Vector3.Scale(leaf.center,leaf.transform.lossyScale);
                var local=Quaternion.Inverse(leafRotation)*(player.transform.TransformPoint(player.body.center)-center);
                var half=Vector3.Scale(leaf.size,leaf.transform.lossyScale)*.5f;
                float dx=Mathf.Max(0,Mathf.Abs(local.x)-half.x),dz=Mathf.Max(0,Mathf.Abs(local.z)-half.z);
                float radius=player.body.radius+player.body.skinWidth;
                // Door hinges only rotate around Y; this capsule/rectangle clearance check
                // includes the controller's skin, before its next physics movement can depenetrate.
                if(dx*dx+dz*dz<radius*radius&&Mathf.Abs(local.y)<half.y+player.body.height*.5f)
                {
                    if(!waitingForPlayer&&GameDirector.I.Mode==ScreenMode.Play)GameDirector.I.Toast("Step clear of the door so it can finish moving.",3);
                    waitingForPlayer=true;return;
                }
            }
            waitingForPlayer=false;
            transform.localRotation=next;
        }
    }
    public class TestCandle:MonoBehaviour
    {
        public bool haunted,lit;public float elapsed;GameObject flame;Light glow;bool heard,discovered,released;
        public void Ignite()
        {
            var g=GameDirector.I;if(lit){g.Toast("The calibration mark reads: 30 SECONDS. Observe the flame.");return;}
            if(g.State.hour>=18){g.Toast("Transport is waiting. Finish the manifest at the east exit.");return;}
            lit=true;elapsed=0;heard=discovered=released=false;g.Spend("Lit a calibration candle in "+g.Campus.Location(transform.position)+".");
            flame=FlameVfx.Create("Candle flame",null,transform.position+Vector3.up*.155f,.068f,.17f,1.35f,4);
            glow=flame.GetComponent<FlameVfx>().glow;
            g.Audio.OneShot("lighter",transform.position,.8f);g.Toast("30-second calibration candle lit.");
        }
        void Update()
        {
            if(!lit||GameDirector.I.Mode!=ScreenMode.Play)return;
            elapsed+=Time.deltaTime;var g=GameDirector.I;
            bool endless=haunted&&!g.State.ceilingSealed;
            if(elapsed>=30&&!endless){lit=false;Destroy(flame);g.Audio.OneShot("extinguish",transform.position,.5f);return;}
            if(endless&&elapsed>37&&!heard){heard=true;g.Audio.OneShot("scratch",transform.position+Vector3.up*2.5f,1);g.Subtitle("[A slow scrape travels across the ceiling.]",8);}
            if(endless&&elapsed>45&&!discovered)
            {
                discovered=true;g.State.ceilingDiscovered=true;g.AddClue("flame");g.State.journal.Add("The candle in Classroom 03 continued past its calibration mark. Something moved above it.");
            }
            if(endless&&elapsed>40&&g.Campus.ceilingPanel!=null)g.Campus.ceilingPanel.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*2)*6,0,0);
            if(endless&&elapsed>58&&!released)
            {
                released=true;g.Audio.OneShot("impact",transform.position,1);g.Subtitle("[The ceiling folds. Something drops behind you.]",6);
                g.BeginChase(transform.position+Vector3.up*1.5f,true);
                for(int i=0;i<12;i++){var dust=Geometry.Shape(PrimitiveType.Cube,"Plaster dust",transform.position+new Vector3(Random.Range(-1f,1f),2,Random.Range(-1f,1f)),Vector3.one*.06f,Geometry.paper,null,false);dust.AddComponent<Rigidbody>();Destroy(dust,3);}
            }
        }
        void OnDestroy(){if(flame!=null)Destroy(flame);}
    }
}

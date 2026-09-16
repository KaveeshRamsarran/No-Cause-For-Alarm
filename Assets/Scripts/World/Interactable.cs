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
        public string room;public bool open;public float sign;
        public bool Locked
        {
            get{var s=GameDirector.I.State;return (room=="ARCHIVE"&&(!s.archiveUnlocked||!s.power))||(room=="STAFF OFFICE"&&!s.Has("key"));}
        }
        public void Toggle()
        {
            if(Locked){GameDirector.I.Toast(room=="ARCHIVE"?"Archive lock: power and security clearance required.":"Brass lock. Ada Moss carries the staff key.");return;}
            open=!open;GameDirector.I.Audio.OneShot("door",transform.position,.7f);
        }
        void Update(){transform.localRotation=Quaternion.Slerp(transform.localRotation,Quaternion.Euler(0,open?sign*94:0,0),Time.deltaTime*5);}
    }
    public class TestCandle:MonoBehaviour
    {
        public bool haunted,lit;public float elapsed;GameObject flame;Light glow;bool heard,discovered,released;
        public void Ignite()
        {
            var g=GameDirector.I;if(lit){g.Toast("The calibration mark reads: 30 SECONDS. Observe the flame.");return;}
            if(g.State.hour>=18){g.Toast("Transport is waiting. Finish the manifest at the east exit.");return;}
            lit=true;elapsed=0;heard=discovered=released=false;g.Spend("Lit a calibration candle in "+g.Campus.Location(transform.position)+".");
            flame=Geometry.Shape(PrimitiveType.Sphere,"Candle flame",transform.position+Vector3.up*.18f,new Vector3(.065f,.17f,.065f),Geometry.Mat("Candle emission",new Color(1,.48f,.08f),5),null,false);
            glow=flame.AddComponent<Light>();glow.color=new Color(1,.58f,.24f);glow.intensity=1.8f;glow.range=4;
            g.Audio.OneShot("lighter",transform.position,.8f);g.Toast("30-second calibration candle lit.");
        }
        void Update()
        {
            if(!lit||GameDirector.I.Mode!=ScreenMode.Play)return;
            elapsed+=Time.deltaTime;var g=GameDirector.I;
            glow.intensity=1.4f+Mathf.Sin(Time.time*21)*.2f;
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

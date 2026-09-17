using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NoCauseForAlarm
{
    public class Room
    {
        public string name; public Vector3 center, door; public int index; public float side;
        public Room(string n,int i,float side,float z){name=n;index=i;this.side=side;center=new Vector3(side*8,0,z);door=new Vector3(side*2.8f,0,z);}
    }
    public class Campus:MonoBehaviour
    {
        public List<Room> rooms=new List<Room>(); public List<NpcActor> actors=new List<NpcActor>();
        public Transform ceilingPanel; public Camera securityCamera; public RenderTexture securityTexture;
        public Vector3 HomePosition(int id)
        {
            var center=FindRoom(Cast.All[id].room).center;
            if(id==6)return center+new Vector3(-2,0,-3.4f);
            if(id==5)return center+new Vector3(0,0,-4.35f);
            return center+new Vector3(id==7?2:0,0,id==9?3.8f:-3.4f);
        }
        public Room FindRoom(string name)=>rooms.Find(r=>r.name==name);
        public string Location(Vector3 p)
        {
            if(Mathf.Abs(p.x)<3)return "EAST WING / MAIN CORRIDOR";
            Room nearest=null;float d=999;
            foreach(var r in rooms){float n=(p-r.center).sqrMagnitude;if(n<d){d=n;nearest=r;}}
            return nearest==null?"EAST WING":nearest.name;
        }
        public void Build()
        {
            Geometry.Materials();
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.21f,.26f,.18f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.055f,.071f,.071f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.021f;
            Geometry.Box("Corridor floor",new Vector3(0,-.15f,25),new Vector3(5.6f,.3f,62),Geometry.floor,transform);
            Geometry.Box("Corridor ceiling",new Vector3(0,3.4f,25),new Vector3(5.6f,.2f,62),Geometry.wall,transform);
            for(int z=-4;z<55;z+=2)
            {
                Geometry.Box("Tile seam",new Vector3(0,.006f,z),new Vector3(5.6f,.008f,.018f),Geometry.metal,transform,false);
                Geometry.Box("Ceiling rail",new Vector3(0,3.27f,z),new Vector3(5.6f,.035f,.035f),Geometry.trim,transform,false);
                if(z%6==0)Geometry.Lamp(new Vector3(0,3.23f,z),false,transform);
                if(z%12==0)Geometry.Lamp(new Vector3(2.5f,2.65f,z),true,transform);
            }
            string[] left={"LECTURE 01","CLASSROOM 02","CLASSROOM 03","STAFF OFFICE","STORES","ARCHIVE"};
            string[] right={"CAFETERIA","COMPUTER LAB","BATHROOM","SECURITY","MAINTENANCE","UTILITY"};
            for(int i=0;i<6;i++){BuildRoom(new Room(left[i],i,-1,i*10));BuildRoom(new Room(right[i],i,1,i*10));}
            Geometry.Box("South bulkhead",new Vector3(0,1.6f,-5.7f),new Vector3(5.6f,3.4f,.3f),Geometry.wall,transform);
            Geometry.Box("Exit bulkhead",new Vector3(0,1.6f,55.5f),new Vector3(5.6f,3.4f,.3f),Geometry.trim,transform);
            var exit=Geometry.Box("Evacuation doors",new Vector3(0,1.3f,55.25f),new Vector3(2.8f,2.6f,.18f),Geometry.metal,transform);
            exit.AddComponent<Interactable>().Setup("evacuation","REVIEW EVACUATION MANIFEST","");
            Geometry.Text("EXIT / TRANSPORT",new Vector3(0,2.95f,55.10f),.14f,new Color(.7f,.9f,.68f),transform,180);
            Geometry.Text("EAST WING\nFACULTY OF HUMAN SCIENCES",new Vector3(0,2.15f,-5.48f),.13f,Color.white,transform);
            var map=Geometry.Box("Directory",new Vector3(2.62f,1.5f,-3),new Vector3(.08f,1.2f,1.4f),Geometry.paper,transform);
            map.AddComponent<Interactable>().Setup("map","READ CAMPUS DIRECTORY","");
            for(int i=0;i<12;i++)
            {
                var room=FindRoom(Cast.All[i].room);var g=new GameObject(Cast.All[i].name);g.transform.SetParent(transform);
                Vector3 p=HomePosition(i);g.transform.position=p;
                var a=g.AddComponent<NpcActor>();a.Build(i,room);actors.Add(a);
            }
            PlaceEvidence("register","Attendance register", "LECTURE 01",new Vector3(2,.81f,0));
            PlaceEvidence("bag","Unclaimed student bag", "CLASSROOM 03",new Vector3(2.65f,.18f,2));
            PlaceEvidence("tissue","Translucent tissue", "BATHROOM",new Vector3(1.45f,.81f,1));
            PlaceEvidence("photo","Photograph: Cohort 19", "STAFF OFFICE",new Vector3(1,.83f,1));
            PlaceEvidence("report","Sealed ventilation study", "ARCHIVE",new Vector3(1,.83f,1));
            PlaceEvidence("maintenance","Duct maintenance report", "MAINTENANCE",new Vector3(1,.83f,1));
            PlaceEvidence("fuel","Lighter refill tin", "STORES",new Vector3(1,.83f,1));
            PlaceEvidence("medical","First-aid incident card", "CLASSROOM 02",new Vector3(.35f,.81f,0));
            PlaceDevice("cctv","REVIEW CAMERA ARCHIVE","SECURITY",new Vector3(1,1.05f,1));
            PlaceDevice("computer","RECOVER DELETED FILES","COMPUTER LAB",new Vector3(1,1.05f,1));
            PlaceDevice("power","REPAIR FUSE CABINET","UTILITY",new Vector3(2,1.2f,2));
            PlaceDevice("seal","CLOSE EXTRACTION DAMPER","MAINTENANCE",new Vector3(2,1.2f,2));
            foreach(var supply in HelpRequests.Supplies)SupplyPickup.Place(supply,transform,FindRoom(supply.room).center+supply.position);
            Candle("STAFF OFFICE",false);Candle("CLASSROOM 03",true);
            var r3=FindRoom("CLASSROOM 03");ceilingPanel=Geometry.Box("Loose ceiling panel",r3.center+new Vector3(0,3.26f,1),new Vector3(1.7f,.08f,1.7f),Geometry.trim,transform,false).transform;
            var cg=new GameObject("CCTV / East corridor");cg.transform.SetParent(transform);cg.transform.position=new Vector3(2.4f,2.9f,32);cg.transform.rotation=Quaternion.Euler(15,185,0);
            securityCamera=cg.AddComponent<Camera>();securityCamera.fieldOfView=65;securityCamera.farClipPlane=70;
            securityTexture=new RenderTexture(640,360,16);securityCamera.targetTexture=securityTexture;securityCamera.enabled=false;
            var vol=new GameObject("Atmosphere / accessible post processing").AddComponent<Volume>();vol.isGlobal=true;vol.profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var tone=vol.profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var vignette=vol.profile.Add<Vignette>();vignette.intensity.Override(.23f);vignette.smoothness.Override(.6f);
            var bloom=vol.profile.Add<Bloom>();bloom.intensity.Override(.25f);bloom.threshold.Override(1.1f);
            var grain=vol.profile.Add<FilmGrain>();grain.intensity.Override(.15f);grain.response.Override(.7f);
            var grade=vol.profile.Add<ColorAdjustments>();grade.contrast.Override(19);grade.saturation.Override(-16);grade.colorFilter.Override(new Color(.86f,1,.77f));grade.postExposure.Override(.3f);
            gameObject.AddComponent<Atmosphere>().profile=vol.profile;
        }
        void BuildRoom(Room r)
        {
            rooms.Add(r);var c=r.center;float s=r.side;float z=c.z;
            Geometry.Box(r.name+" floor",c+new Vector3(0,-.15f,0),new Vector3(10.4f,.3f,10),Geometry.floor,transform);
            Geometry.Box(r.name+" ceiling",c+new Vector3(0,3.4f,0),new Vector3(10.4f,.2f,10),Geometry.wall,transform);
            Geometry.Box("Outer wall",new Vector3(s*13.2f,1.6f,z),new Vector3(.25f,3.4f,10),Geometry.wall,transform);
            for(int end=-1;end<=1;end+=2)Geometry.Box("Partition",c+new Vector3(0,1.6f,end*5),new Vector3(10.4f,3.4f,.22f),Geometry.wall,transform);
            for(int end=-1;end<=1;end+=2)
            {
                Geometry.Box("Corridor partition",new Vector3(s*2.8f,1.6f,z+end*3.05f),new Vector3(.24f,3.4f,3.9f),Geometry.wall,transform);
                Geometry.Box("Dado rail",new Vector3(s*2.66f,.95f,z+end*3.05f),new Vector3(.06f,.10f,3.9f),Geometry.trim,transform,false);
                Geometry.Box("Wall base",new Vector3(s*2.65f,.28f,z+end*3.05f),new Vector3(.05f,.55f,3.9f),Geometry.trim,transform,false);
            }
            Geometry.Box("Door lintel",new Vector3(s*2.8f,2.97f,z),new Vector3(.24f,.86f,2.2f),Geometry.wall,transform);
            Geometry.Text(r.name,new Vector3(s*2.62f,2.79f,z),.12f,Color.white,transform,s<0?90:-90);
            var d=new GameObject(r.name+" door hinge");d.transform.SetParent(transform);d.transform.position=new Vector3(s*2.79f,0,z-1.05f);
            var leaf=Geometry.Box("Door",new Vector3(0,1.27f,1.05f),new Vector3(.13f,2.54f,2.1f),Geometry.wood,d.transform);
            CampusArt.Door(leaf);
            CampusArt.Fit("DoorFrame",transform,new Vector3(s*2.79f,1.31f,z),new Vector3(2.26f,2.65f,.18f),90);
            var door=d.AddComponent<Door>();door.room=r.name;door.sign=s;door.open=r.name!="ARCHIVE"&&r.name!="STAFF OFFICE";
            var interact=leaf.AddComponent<Interactable>();interact.Setup("door","OPEN / CLOSE "+r.name,"");interact.door=door;
            Geometry.Lamp(c+new Vector3(-2,3.18f,-2),false,transform);Geometry.Lamp(c+new Vector3(2,3.18f,2),false,transform);
            Geometry.Lamp(c+new Vector3(s*4,2.7f,0),true,transform);
            if(r.name.Contains("CLASSROOM")||r.name=="LECTURE 01")
            {
                for(int x=-2;x<=2;x+=2)for(int row=-2;row<=2;row+=2)
                { Geometry.Prop("schoolTable",c+new Vector3(x,0,row),1.35f,0,transform);Geometry.Prop("schoolChair",c+new Vector3(x,0,row+ .9f),.55f,180,transform); }
                CampusArt.Fit("School/board2",transform,c+new Vector3(0,1.9f,-4.78f),new Vector3(4.8f,1.5f,.055f));
                Geometry.Text(r.name=="LECTURE 01"?"IDENTITY / CONTINUITY\nWhat makes a person the same person?":"PLEASE LEAVE THE ROOM\nAS YOU FOUND IT",c+new Vector3(0,1.9f,-4.75f),.11f,new Color(.75f,.78f,.65f),transform);
            }
            else if(r.name=="CAFETERIA")
            {
                for(int a=-2;a<=2;a+=4){Geometry.Prop("tableRound",c+new Vector3(a,0,2),1.8f,0,transform);}
                Geometry.Prop("kitchenCabinet",c+new Vector3(1,0,-3.6f),3,0,transform);Geometry.Prop("displayCounter",c+new Vector3(3.7f,0,-3.6f),1.6f,0,transform);
            }
            else if(r.name=="BATHROOM")
            {
                for(int a=-2;a<=2;a+=2){Geometry.Prop("toilet",c+new Vector3(a,0,-3.5f),.7f,180,transform);Geometry.Box("Stall",c+new Vector3(a+ .8f,1.1f,-3),new Vector3(.1f,2.2f,2.5f),Geometry.trim,transform);}
                Geometry.Prop("bathroomSink",c+new Vector3(1,0,1),1.6f,0,transform);
            }
            else
            {
                Geometry.Prop("desk",c+new Vector3(1,0,1),1.8f,0,transform);
                Geometry.Prop("chairDesk",c+new Vector3(1,0,2.3f),.65f,180,transform);
                Geometry.Prop("bookcaseOpen",c+new Vector3(-3,0,-4.3f),.85f,0,transform);
                if(r.name=="COMPUTER LAB")for(int a=-3;a<=3;a+=3){Geometry.Prop("desk",c+new Vector3(a,0,-3),1.8f,0,transform);Geometry.Prop("computerScreen",c+new Vector3(a,.8f,-3),.65f,0,transform);}
                if(r.name=="STORES"||r.name=="MAINTENANCE")for(int a=0;a<3;a++)Geometry.Prop("cardboardBoxClosed",c+new Vector3(-2+a,0,3),.8f,0,transform);
            }
            Geometry.Prop("trashcan",c+new Vector3(-s*3.8f,0,3.8f),.45f,0,transform);
            Geometry.Prop("extinguisher",c+new Vector3(-s*4.83f,.85f,-2.3f),.65f,s<0?270:90,transform);
            if(r.name=="CAFETERIA")Geometry.Prop("vending",c+new Vector3(3.9f,0,3.9f),1.25f,180,transform);
            if(r.name=="LECTURE 01")Geometry.Prop("lectern",c+new Vector3(-2.7f,0,-3.7f),1.05f,0,transform);
            if(r.name.Contains("CLASSROOM")||r.name=="LECTURE 01")
            {Geometry.Prop("clock",c+new Vector3(3.6f,2.55f,-4.79f),.38f,0,transform);Geometry.Prop("book",c+new Vector3(0,.785f,0),.30f,0,transform);}
            if(r.name=="SECURITY")Geometry.Prop("radio",c+new Vector3(1,.82f,1.5f),.45f,0,transform);
            if(r.name=="STAFF OFFICE")Geometry.Prop("telephone",c+new Vector3(.3f,.82f,1),.28f,0,transform);
            if(r.index%2==0)for(int l=0;l<3;l++)Geometry.Prop("locker",new Vector3(s*2.35f,0,z+2.1f+l*.65f),.5f,s<0?90:270,transform);
            for(int rail=-4;rail<=4;rail+=2)Geometry.Box("Ceiling tile joint",c+new Vector3(0,3.285f,rail),new Vector3(10.2f,.018f,.025f),Geometry.metal,transform,false);
            Geometry.Box("Emergency notice",new Vector3(s*2.63f,1.65f,z+3),new Vector3(.045f,.7f,.52f),Geometry.paper,transform,false);
            Geometry.Text("REMAIN\nINSIDE",new Vector3(s*2.595f,1.68f,z+3),.08f,new Color(.18f,.2f,.17f),transform,s<0?90:-90);
            CampusArt.RoomDetails(r,transform);
        }
        void PlaceEvidence(string id,string label,string room,Vector3 offset)
        {
            var p=FindRoom(room).center+offset;
            // Clues sit on the existing school desks and washbasin; no overlapping pedestal.
            GameObject g;
            if(id=="fuel")g=Geometry.Shape(PrimitiveType.Cylinder,label,p+Vector3.up*.06f,new Vector3(.09f,.06f,.09f),Geometry.metal,transform,true);
            else if(id=="bag")g=Geometry.Box(label,p,new Vector3(.32f,.34f,.22f),Geometry.wood,transform);
            else g=Geometry.Box(label,p,id=="tissue"?new Vector3(.12f,.025f,.11f):new Vector3(.28f,.025f,.20f),Geometry.paper,transform);
            g.AddComponent<Interactable>().Setup("evidence","INSPECT "+label.ToUpper(),id);
        }
        void PlaceDevice(string id,string label,string room,Vector3 offset)
        {
            var p=FindRoom(room).center+offset;
            bool terminal=id=="cctv"||id=="computer";
            if(!terminal)p=FindRoom(room).center+new Vector3(2,1.35f,4.82f);
            var g=terminal?Geometry.Prop("monitor",FindRoom(room).center+new Vector3(1,.78f,1),.65f,0,transform):Geometry.Box(label,p,new Vector3(.65f,.46f,.18f),Geometry.metal,transform);
            if(!terminal)Geometry.Text(id=="power"?"FUSE BANK / E-03":"EXTRACTION / E-03",p+new Vector3(0,.36f,-.11f),.06f,Color.white,transform,180);
            g.AddComponent<Interactable>().Setup(id,label,"");
        }
        void Candle(string room,bool haunted)
        {
            var p=FindRoom(room).center+new Vector3(-3.8f,0,3.5f);Geometry.Prop("desk",p,1.05f,0,transform);
            var g=Geometry.Shape(PrimitiveType.Cylinder,"30-second calibration candle",p+new Vector3(0,.93f,0),new Vector3(.14f,.15f,.14f),Geometry.paper,transform,true);
            var c=g.AddComponent<TestCandle>();c.haunted=haunted;g.AddComponent<Interactable>().Setup("candle","LIGHT CALIBRATION CANDLE","");
        }
    }
    public class Atmosphere:MonoBehaviour
    {
        public VolumeProfile profile;
        void Update(){if(GameDirector.I==null)return;var s=GameDirector.I.Settings;
            if(profile.TryGet<FilmGrain>(out var grain))grain.intensity.value=s.effects*.2f;
            if(profile.TryGet<Vignette>(out var v))v.intensity.value=.12f+s.effects*.13f;}
    }
}

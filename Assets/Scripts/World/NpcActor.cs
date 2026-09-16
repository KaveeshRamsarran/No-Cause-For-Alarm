using UnityEngine;

namespace NoCauseForAlarm
{
    public class NpcActor:MonoBehaviour
    {
        public int id;public Room room;public Transform head,leftArm,rightArm,leftLeg,rightLeg,jaw;public bool transformed;
        public Vector3 home;float phase,reactionCooldown;Material skin;Vector3 target;bool relocating;int scheduledHour;
        readonly System.Collections.Generic.Queue<Vector3> route=new System.Collections.Generic.Queue<Vector3>();
        Renderer[] renderers;Collider actorCollider;bool lastVisible=true;
        public PersonState State=>GameDirector.I.State?.people[id];
        public void Build(int index,Room start)
        {
            id=index;room=start;home=transform.position;target=home;phase=id*.71f;transform.rotation=Quaternion.Euler(0,room.side<0?90:270,0);
            var clothes=Geometry.Mat(Cast.All[id].name+" clothing",Cast.All[id].color);
            float skinShade=.44f+(id%4)*.10f;skin=Geometry.Mat("Skin "+id,new Color(skinShade,skinShade*.76f,skinShade*.59f));
            Geometry.Shape(PrimitiveType.Capsule,"Torso",new Vector3(0,1.05f,0),new Vector3(.43f,.39f,.25f),clothes,transform);
            Geometry.Box("Collar",new Vector3(0,1.36f,.10f),new Vector3(.20f,.04f,.045f),Geometry.paper,transform,false);
            leftLeg=Limb("Left leg",new Vector3(-.12f,.69f,0),new Vector3(.15f,.35f,.17f),Geometry.dark);
            rightLeg=Limb("Right leg",new Vector3(.12f,.69f,0),new Vector3(.15f,.35f,.17f),Geometry.dark);
            leftArm=Limb("Left arm",new Vector3(-.28f,1.32f,0),new Vector3(.12f,.31f,.14f),clothes);
            rightArm=Limb("Right arm",new Vector3(.28f,1.32f,0),new Vector3(.12f,.31f,.14f),clothes);
            Geometry.Shape(PrimitiveType.Sphere,"Hand",new Vector3(0,-.6f,0),new Vector3(.1f,.15f,.09f),skin,leftArm);
            Geometry.Shape(PrimitiveType.Sphere,"Hand",new Vector3(0,-.6f,0),new Vector3(.1f,.15f,.09f),skin,rightArm);
            head=new GameObject("Head / transform pivot").transform;head.SetParent(transform,false);head.localPosition=new Vector3(0,1.58f,0);
            var face=Resources.Load<GameObject>("Characters/portrait");
            bool textured=Resources.Load<Texture2D>("Characters/FaceAtlas")!=null;
            if(face!=null){var f=Instantiate(face,head);f.transform.localPosition=Vector3.zero;
                var renderer=f.GetComponentInChildren<Renderer>();float width=renderer.bounds.size.x;f.transform.localScale*=.28f/Mathf.Max(.0001f,width);
                var portrait=Geometry.Mat("Portrait / "+Cast.All[id].name,new Color(.92f,1,.86f));
                portrait.SetTexture("_BaseMap",Resources.Load<Texture2D>("Characters/FaceAtlas"));portrait.SetTextureScale("_BaseMap",new Vector2(1f/3,1f/4));portrait.SetTextureOffset("_BaseMap",new Vector2((id%3)/3f,(3-id/3)/4f));
                foreach(var r in f.GetComponentsInChildren<Renderer>())r.sharedMaterials=textured?new[]{portrait,Geometry.dark}:new[]{skin,skin};}
            else Geometry.Shape(PrimitiveType.Sphere,"Face",Vector3.zero,new Vector3(.28f,.34f,.27f),skin,head);
            if(!textured)
            {
            Geometry.Shape(PrimitiveType.Sphere,"Hair",new Vector3(0,.115f,-.025f),new Vector3(.30f,.17f,.27f),id%3==0?Geometry.metal:Geometry.dark,head);
            if(id==2||id==4||id==8)Geometry.Shape(PrimitiveType.Sphere,"Tied hair",new Vector3(0,.08f,-.14f),new Vector3(.19f,.23f,.15f),Geometry.dark,head);
            if(id==3||id==9)Geometry.Box("Work cap",new Vector3(0,.17f,.015f),new Vector3(.31f,.065f,.33f),clothes,head,false);
            for(int e=-1;e<=1;e+=2)
            {
                Geometry.Shape(PrimitiveType.Sphere,"Eye",new Vector3(e*.064f,.033f,.121f),new Vector3(.051f,.028f,.023f),Geometry.paper,head);
                Geometry.Shape(PrimitiveType.Sphere,"Pupil",new Vector3(e*.064f,.034f,.136f),new Vector3(.014f,.022f,.007f),Geometry.dark,head);
                Geometry.Box("Brow",new Vector3(e*.064f,.065f,.124f),new Vector3(.057f,.012f,.014f),Geometry.dark,head,false);
                if(id==0||id==5||id==10)Geometry.Box("Spectacle rim",new Vector3(e*.068f,.041f,.144f),new Vector3(.08f,.045f,.007f),Geometry.metal,head,false);
            }
            Geometry.Shape(PrimitiveType.Sphere,"Nose",new Vector3(.004f,-.012f,.137f),new Vector3(.044f,.078f,.065f),skin,head);
            jaw=Geometry.Shape(PrimitiveType.Sphere,"Jaw",new Vector3(0,-.115f,.022f),new Vector3(.22f,.105f,.22f),skin,head).transform;
            Geometry.Box("Mouth",new Vector3(0,-.085f,.142f),new Vector3(.095f,.012f,.008f),Geometry.dark,head,false);
            }
            else jaw=new GameObject("Jaw deformation anchor").transform;
            if(textured){jaw.SetParent(head,false);jaw.localPosition=new Vector3(0,-.10f,.07f);}
            if(id==0||id==10)Geometry.Box("Tie",new Vector3(0,1.19f,.14f),new Vector3(.055f,.30f,.015f),Geometry.dark,transform,false);
            var col=gameObject.AddComponent<CapsuleCollider>();col.height=1.9f;col.radius=.34f;col.center=Vector3.up*.95f;
            actorCollider=col;renderers=GetComponentsInChildren<Renderer>();
        }
        Transform Limb(string n,Vector3 p,Vector3 size,Material mat)
        {var pivot=new GameObject(n).transform;pivot.SetParent(transform,false);pivot.localPosition=p;Geometry.Shape(PrimitiveType.Capsule,n+" mesh",new Vector3(0,-size.y,0),size,mat,pivot);return pivot;}
        public void TransformBody()
        {transformed=true;head.localScale=new Vector3(.86f,1.35f,1);jaw.localScale=new Vector3(.27f,.34f,.22f);leftArm.localScale=new Vector3(1,1.6f,1);rightArm.localRotation=Quaternion.Euler(0,0,58);skin.SetColor("_BaseColor",new Color(.55f,.57f,.42f));}
        public void ResetBody(){transformed=false;relocating=false;route.Clear();head.localScale=Vector3.one;leftArm.localScale=Vector3.one;rightArm.localRotation=Quaternion.identity;leftArm.localRotation=Quaternion.identity;jaw.localScale=new Vector3(.22f,.105f,.22f);
            float shade=.44f+(id%4)*.10f;skin.SetColor("_BaseColor",new Color(shade,shade*.76f,shade*.59f));}
        public void OnHour(int hour)
        {
            if(State==null||!GameDirector.I.State.Available(id))return;
            scheduledHour=hour;
            // Route through the actual doorway and keep to the clear perimeter around classroom desks.
            Room next=(hour==12||hour==16)?GameDirector.I.Campus.FindRoom("CAFETERIA"):room;
            home=next.center+new Vector3((id%3-1)*1.5f,0,-3.4f+(hour==12||hour==16?(id/3)*.55f:0));
            route.Clear();float side=Mathf.Sign(transform.position.x);float fromZ=Mathf.Round(transform.position.z/10)*10;
            if(Mathf.Abs(transform.position.x)>2.5f)
            {route.Enqueue(new Vector3(side*4.0f,0,transform.position.z));route.Enqueue(new Vector3(side*4.0f,0,fromZ));route.Enqueue(new Vector3(0,0,fromZ));}
            route.Enqueue(new Vector3(0,0,next.center.z));route.Enqueue(new Vector3(next.side*4,0,next.center.z));
            route.Enqueue(new Vector3(next.side*4,0,home.z));route.Enqueue(home);
            target=route.Dequeue();relocating=true;
        }
        void Update()
        {
            var g=GameDirector.I;if(g==null||g.State==null)return;var s=State;
            bool available=g.State.Available(id);
            if(available!=lastVisible){foreach(var r in renderers)r.enabled=available;actorCollider.enabled=available;lastVisible=available;}
            if(!available||g.Mode!=ScreenMode.Play)return;
            float dist=Vector3.Distance(transform.position,g.Player.transform.position);reactionCooldown-=Time.deltaTime;
            if(g.Player.flameOn&&dist<2.8f&&reactionCooldown<=0)
            {
                reactionCooldown=20;g.Subtitle(Cast.All[id].name+": "+Cast.All[id].fire,6);
                if(!s.observations.Contains("Watched the lighter at close range."))s.observations.Add("Watched the lighter at close range.");
                if(s.infiltrator||id==7){leftArm.localRotation=Quaternion.Euler(-35,0,-25);target=transform.position-transform.forward*.6f;}
                if(s.infiltrator&&g.State.hour>=15&&id%3==0){TransformBody();g.BeginChase(transform.position,false,id);}
            }
            if(relocating)
            {
                Vector3 delta=target-transform.position;delta.y=0;
                if(delta.magnitude<.18f)
                {
                    if(route.Count>0)target=route.Dequeue();else relocating=false;
                }
                else
                {
                    // Closed doors stop a schedule. An NPC waits rather than walking through it.
                    bool blocked=Physics.Raycast(transform.position+Vector3.up,delta.normalized,out var obstacle,.65f);
                    if(blocked){var door=obstacle.collider.GetComponentInParent<Door>();if(door!=null&&!door.Locked)door.open=true;}
                    if(!blocked)transform.position+=delta.normalized*Time.deltaTime*.8f;
                    transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*4);
                    leftLeg.localRotation=Quaternion.Euler(Mathf.Sin(Time.time*6+phase)*22,0,0);rightLeg.localRotation=Quaternion.Euler(-Mathf.Sin(Time.time*6+phase)*22,0,0);
                }
            }
            else if(dist<4){var dir=g.Player.transform.position-transform.position;dir.y=0;if(dir.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),Time.deltaTime*.8f);}
            head.localPosition=new Vector3(transformed?.035f:0,1.58f+Mathf.Sin(Time.time*1.5f+phase)*.005f+(transformed?.12f:0),0);
        }
    }
    public class Pursuer:MonoBehaviour
    {
        public bool fromCeiling;
        float life=28,hitCooldown;CharacterController body;Transform torso;
        public void Build(int identity=-1)
        {
            body=gameObject.AddComponent<CharacterController>();body.height=1.55f;body.center=Vector3.up*.8f;body.radius=.3f;
            var skin=Geometry.Mat("Exposed organism",new Color(.45f,.39f,.29f));
            Material clothes=identity>=0?Geometry.Mat("Recognisable clothing",Cast.All[identity].color):skin;
            torso=Geometry.Shape(PrimitiveType.Capsule,"Bent torso",new Vector3(0,.9f,0),new Vector3(.4f,.55f,.25f),clothes,transform).transform;torso.localEulerAngles=new Vector3(30,0,15);
            var portraitModel=Resources.Load<GameObject>("Characters/portrait");
            if(identity>=0&&portraitModel!=null)
            {
                var facePivot=new GameObject("Distorted portrait pivot").transform;facePivot.SetParent(transform,false);facePivot.localPosition=new Vector3(.12f,1.5f,.18f);
                var f=Instantiate(portraitModel,facePivot);f.transform.localPosition=Vector3.zero;
                float width=f.GetComponentInChildren<Renderer>().bounds.size.x;f.transform.localScale*=.31f/Mathf.Max(.0001f,width);
                facePivot.localScale=new Vector3(.85f,1.7f,1);facePivot.localRotation=Quaternion.Euler(0,0,12);
                var material=Geometry.Mat("Distorted familiar face",new Color(.8f,.95f,.64f));material.SetTexture("_BaseMap",Resources.Load<Texture2D>("Characters/FaceAtlas"));
                material.SetTextureScale("_BaseMap",new Vector2(1f/3,1f/4));material.SetTextureOffset("_BaseMap",new Vector2((identity%3)/3f,(3-identity/3)/4f));
                foreach(var renderer in f.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=new[]{material,Geometry.dark};
            }
            else
            {
                Geometry.Shape(PrimitiveType.Sphere,"Unfamiliar head",new Vector3(.12f,1.5f,.18f),new Vector3(.32f,.5f,.28f),skin,transform);
                Geometry.Box("Open mouth",new Vector3(.12f,1.42f,.32f),new Vector3(.17f,.27f,.02f),Geometry.dark,transform,false);
            }
            for(int s=-1;s<=1;s+=2){var arm=Geometry.Shape(PrimitiveType.Capsule,"Too-long arm",new Vector3(s*.36f,.61f,.1f),new Vector3(.10f,.64f,.11f),skin,transform);arm.transform.localEulerAngles=new Vector3(-18,0,s*15);}
        }
        void Update()
        {
            var g=GameDirector.I;if(g.Mode!=ScreenMode.Play)return;life-=Time.deltaTime;hitCooldown-=Time.deltaTime;
            if(life<=0||(fromCeiling&&g.State.ceilingSealed)){g.Chase=null;Destroy(gameObject);g.Subtitle("[The footsteps stop. The ventilation does not.]",5);return;}
            Vector3 player=g.Player.transform.position,dir=player-transform.position;dir.y=0;float dist=dir.magnitude;
            bool blocked=Physics.Linecast(transform.position+Vector3.up,player+Vector3.up,out var hit)&&hit.collider.gameObject!=g.Player.gameObject;
            if(blocked)
            {
                float doorZ=Mathf.Round(transform.position.z/10)*10;
                Vector3 waypoint=Mathf.Abs(transform.position.x)>2.5f?
                    (Mathf.Abs(transform.position.z-doorZ)>.2f?new Vector3(transform.position.x,0,doorZ):new Vector3(0,0,doorZ)):
                    new Vector3(0,0,Mathf.Round(player.z/10)*10);
                dir=waypoint-transform.position;dir.y=0;
            }
            bool repelled=g.Player.flameOn&&dist<3.1f&&!blocked;
            float speed=repelled?-.75f:2.9f;
            if(dir.sqrMagnitude>.05f){transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),Time.deltaTime*8);body.Move((dir.normalized*speed+Vector3.down*5)*Time.deltaTime);}
            torso.localRotation=Quaternion.Euler(30+Mathf.Sin(Time.time*13)*12,0,15);
            if(dist<1.15f&&!repelled&&!blocked&&hitCooldown<=0){hitCooldown=2;g.State.health-=34;g.Audio.OneShot("impact",transform.position,1);g.Toast("RUN. Close a door or hold the flame between you.");if(g.State.health<=0)g.End(false);}
        }
    }
}

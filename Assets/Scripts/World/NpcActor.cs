using UnityEngine;

namespace NoCauseForAlarm
{
    public class NpcActor:MonoBehaviour
    {
        public int id;public Room room;public Transform head;public bool transformed;
        public Vector3 home;float reactionCooldown;Vector3 target;bool relocating;int scheduledHour;
        readonly System.Collections.Generic.Queue<Vector3> route=new System.Collections.Generic.Queue<Vector3>();
        Renderer[] renderers;Collider actorCollider;bool lastVisible=true;CharacterVisual visual;
        public PersonState State=>GameDirector.I.State?.people[id];
        public void Build(int index,Room start)
        {
            id=index;room=start;home=transform.position;target=home;transform.rotation=Quaternion.Euler(0,room.side<0?90:270,0);
            visual=gameObject.AddComponent<CharacterVisual>();visual.Build(id);head=visual.face;
            var col=gameObject.AddComponent<CapsuleCollider>();col.height=1.9f;col.radius=.34f;col.center=Vector3.up*.95f;
            actorCollider=col;renderers=GetComponentsInChildren<Renderer>();
        }
        public void TransformBody(){transformed=true;visual.distorted=true;visual.React();}
        public void ResetBody(){transformed=false;relocating=false;route.Clear();visual.distorted=false;}
        public void OnHour(int hour)
        {
            if(State==null||!GameDirector.I.State.Available(id))return;
            scheduledHour=hour;
            // Route through the actual doorway and keep to the clear perimeter around classroom desks.
            Room next=(hour==12||hour==16)?GameDirector.I.Campus.FindRoom("CAFETERIA"):room;
            bool gathering=hour==12||hour==16;
            // Waiting positions occupy the cafeteria's three clear aisles, away from counters and stools.
            home=gathering?next.center+(id<6?new Vector3(0,0,-2.1f+id*1.2f):id<9?new Vector3(4.1f,0,-1.7f+(id-6)*1.2f):new Vector3(-4.1f,0,-1.5f+(id-9)*1.5f)):GameDirector.I.Campus.HomePosition(id);
            route.Clear();float side=Mathf.Sign(transform.position.x);float fromZ=Mathf.Round(transform.position.z/10)*10;
            if(Mathf.Abs(transform.position.x)>2.5f)
            {route.Enqueue(new Vector3(side*4.0f,0,transform.position.z));route.Enqueue(new Vector3(side*4.0f,0,fromZ));route.Enqueue(new Vector3(0,0,fromZ));}
            route.Enqueue(new Vector3(0,0,next.center.z));route.Enqueue(new Vector3(next.side*4,0,next.center.z));
            route.Enqueue(gathering?new Vector3(home.x,0,next.center.z):new Vector3(next.side*4,0,home.z));route.Enqueue(home);
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
                if(s.infiltrator||id==7){visual.React();target=transform.position-transform.forward*.6f;}
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
                    bool blocked=Physics.CapsuleCast(transform.position+Vector3.up*.35f,transform.position+Vector3.up*1.5f,.30f,delta.normalized,out var obstacle,.38f,~0,QueryTriggerInteraction.Ignore);
                    if(blocked){var door=obstacle.collider.GetComponentInParent<Door>();if(door!=null&&!door.Locked&&!door.manualClosed)door.open=true;}
                    if(!blocked)transform.position+=delta.normalized*Time.deltaTime*.8f;
                    transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*4);

                }
            }
            else if(dist<4){var dir=g.Player.transform.position-transform.position;dir.y=0;if(dir.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(dir),Time.deltaTime*.8f);}

        }
    }
    public class Pursuer:MonoBehaviour
    {
        public bool fromCeiling;
        float life=28,hitCooldown;CharacterController body;
        public void Build(int identity=-1)
        {
            body=gameObject.AddComponent<CharacterController>();body.height=1.55f;body.center=Vector3.up*.8f;body.radius=.3f;
            var visual=gameObject.AddComponent<CharacterVisual>();visual.Build(identity<0?9:identity,identity<0);visual.distorted=true;
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

            if(dist<1.15f&&!repelled&&!blocked&&hitCooldown<=0){hitCooldown=2;g.State.health-=34;g.Audio.OneShot("impact",transform.position,1);g.Toast("RUN. Close a door or hold the flame between you.");if(g.State.health<=0)g.End(false);}
        }
    }
}

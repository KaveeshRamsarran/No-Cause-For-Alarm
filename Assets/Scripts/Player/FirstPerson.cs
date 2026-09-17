using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NoCauseForAlarm
{
    public class FirstPerson:MonoBehaviour
    {
        public Camera view;public CharacterController body;public float pitch,stamina=100;public bool lighterRaised,flameOn,crouched;
        public Interactable target; public NpcActor npcTarget;public Transform hand;public GameObject flame;public Light flameLight;
        float velocityY,stepTimer,bob,igniteKick;Transform grip,lid;Light portraitLight;public Vector3 savedPosition;public Quaternion savedRotation;
        public void Build()
        {
            body=gameObject.AddComponent<CharacterController>();body.height=1.8f;body.radius=.27f;body.center=new Vector3(0,.9f,0);body.stepOffset=.25f;
            var cam=new GameObject("Player camera");cam.tag="MainCamera";cam.transform.SetParent(transform,false);cam.transform.localPosition=Vector3.up*1.62f;
            view=cam.AddComponent<Camera>();view.nearClipPlane=.045f;view.farClipPlane=90;view.fieldOfView=75;view.backgroundColor=RenderSettings.fogColor;view.clearFlags=CameraClearFlags.SolidColor;
            view.GetUniversalAdditionalCameraData().renderPostProcessing=true;cam.AddComponent<AudioListener>();
            portraitLight=cam.AddComponent<Light>();portraitLight.type=LightType.Point;portraitLight.range=3;portraitLight.intensity=.4f;portraitLight.color=new Color(.83f,.88f,.68f);portraitLight.enabled=false;
            hand=new GameObject("Lighter hand").transform;hand.SetParent(cam.transform,false);hand.localPosition=new Vector3(.25f,-.36f,.46f);
            var handAsset=Resources.Load<GameObject>("Overhaul/LighterGrip");
            if(handAsset==null)throw new System.InvalidOperationException("Missing CC0 WRAD hand. Prepare graphics assets.");
            var gripPivot=new GameObject("Animated WRAD wrist").transform;gripPivot.SetParent(hand,false);gripPivot.localPosition=new Vector3(.015f,-.02f,-.015f);gripPivot.localRotation=Quaternion.Euler(0,180,-8);
            grip=Instantiate(handAsset,gripPivot).transform;grip.localPosition=Vector3.zero;
            Geometry.Box("Lighter body",Vector3.zero,new Vector3(.066f,.115f,.028f),Geometry.metal,hand,false);
            Geometry.Box("Lighter hood",new Vector3(0,.069f,0),new Vector3(.065f,.029f,.03f),Geometry.dark,hand,false);
            lid=Geometry.Box("Hinged lighter lid",new Vector3(-.038f,.066f,0),new Vector3(.06f,.035f,.033f),Geometry.metal,hand,false).transform;
            flame=Geometry.Shape(PrimitiveType.Sphere,"Lighter flame",new Vector3(0,.12f,0),new Vector3(.018f,.068f,.018f),Geometry.Mat("Lighter flame",new Color(1,.55f,.1f),6),hand);
            flameLight=flame.AddComponent<Light>();flameLight.range=5;flameLight.intensity=2;flameLight.color=new Color(1,.62f,.3f);flameLight.shadows=LightShadows.Soft;flame.SetActive(false);
        }
        public void Teleport(Vector3 p,float yaw)
        {body.enabled=false;transform.position=p;transform.rotation=Quaternion.Euler(0,yaw,0);body.enabled=true;pitch=0;view.transform.localRotation=Quaternion.identity;velocityY=0;}
        public void Focus(NpcActor npc)
        {
            savedPosition=view.transform.position;savedRotation=view.transform.rotation;
            Vector3 face=npc.head.position+Vector3.up*.015f;Vector3 facing=npc.transform.forward;
            view.transform.position=face+facing*.74f+Vector3.up*.015f;view.transform.LookAt(face-Vector3.up*.025f-npc.transform.right*.18f);
        }
        public void Unfocus(){view.transform.position=savedPosition;view.transform.rotation=savedRotation;}
        public void ToggleFlame()
        {
            var g=GameDirector.I;if(g.State==null)return;
            if(g.State.fuel<=0){g.Toast("The lighter is empty. There is a refill tin in Stores.");return;}
            igniteKick=1;lighterRaised=true;flameOn=!flameOn;flame.SetActive(flameOn);g.Audio.OneShot(flameOn?"lighter":"extinguish",transform.position,.6f);
        }
        void Update()
        {
            var g=GameDirector.I;if(g==null)return;
            portraitLight.enabled=g.Mode==ScreenMode.Dialogue||g.Mode==ScreenMode.Accuse;
            view.fieldOfView=Mathf.Lerp(view.fieldOfView,g.Mode==ScreenMode.Dialogue?51:g.Settings.fov,Time.unscaledDeltaTime*5);
            if(g.State!=null&&flameOn&&(g.Mode==ScreenMode.Play||g.Mode==ScreenMode.Dialogue))
            {
                g.State.fuel=Mathf.Max(0,g.State.fuel-Time.deltaTime*.48f);if(g.State.fuel<=0){flameOn=false;flame.SetActive(false);}
                flameLight.intensity=(.32f+Mathf.Sin(Time.time*29)*.04f)*g.Settings.brightness;
            }
            igniteKick=Mathf.MoveTowards(igniteKick,0,Time.unscaledDeltaTime*3.5f);
            float sway=Mathf.Sin(bob)*.005f*g.Settings.shake;
            hand.localPosition=Vector3.Lerp(hand.localPosition,new Vector3(.25f+sway,(lighterRaised?-.18f:-.83f)+sway,.46f),Time.unscaledDeltaTime*9);
            hand.localRotation=Quaternion.Slerp(hand.localRotation,Quaternion.Euler(igniteKick*-10,Mathf.Clamp(-Input.GetAxisRaw("Mouse X"),-3,3),igniteKick*5),Time.unscaledDeltaTime*10);
            lid.localRotation=Quaternion.Slerp(lid.localRotation,Quaternion.Euler(0,0,flameOn?115:0),Time.unscaledDeltaTime*14);
            hand.gameObject.SetActive(g.Mode==ScreenMode.Play||g.Mode==ScreenMode.Intro);

            if(g.Mode!=ScreenMode.Play){target=null;npcTarget=null;return;}
            Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;
            transform.Rotate(0,Input.GetAxisRaw("Mouse X")*g.Settings.sensitivity*1.6f,0);
            pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*g.Settings.sensitivity*1.6f,-82,82);
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            crouched=Input.GetKey(KeyCode.LeftControl);float axisX=Input.GetAxisRaw("Horizontal"),axisZ=Input.GetAxisRaw("Vertical");
            bool moving=Mathf.Abs(axisX)+Mathf.Abs(axisZ)>.01f;bool sprint=Input.GetKey(KeyCode.LeftShift)&&stamina>1&&moving&&!crouched;
            stamina=Mathf.Clamp(stamina+Time.deltaTime*(sprint?-18:13),0,100);
            float speed=crouched?1.5f:sprint?4.2f:2.65f;Vector3 dir=Vector3.ClampMagnitude(transform.right*axisX+transform.forward*axisZ,1);
            if(body.isGrounded&&velocityY<0)velocityY=-2;velocityY-=Time.deltaTime*18;
            body.Move((dir*speed+Vector3.up*velocityY)*Time.deltaTime);
            bob+=Time.deltaTime*(sprint?12:8);float height=crouched?1.05f:1.62f;
            view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,new Vector3(0,height+(moving?Mathf.Sin(bob)*.018f*g.Settings.shake:0),0),Time.deltaTime*10);
            if(moving&&body.isGrounded){stepTimer-=Time.deltaTime;if(stepTimer<=0){stepTimer=sprint?.31f:.52f;g.Audio.Footstep(crouched?.16f:sprint?.55f:.38f,sprint);}}
            if(Input.GetKeyDown(KeyCode.F))ToggleFlame();
            if(Input.GetKeyDown(KeyCode.R)){lighterRaised=!lighterRaised;if(!lighterRaised){flameOn=false;flame.SetActive(false);}}
            target=null;npcTarget=null;
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,3.2f,~0,QueryTriggerInteraction.Ignore))
            {
                target=hit.collider.GetComponentInParent<Interactable>();npcTarget=hit.collider.GetComponentInParent<NpcActor>();
                if(Input.GetKeyDown(KeyCode.E)){if(npcTarget!=null)g.Talk(npcTarget);else if(target!=null)target.Use();}
            }
            if(Input.GetKeyDown(KeyCode.Tab))g.ShowNotebook();if(Input.GetKeyDown(KeyCode.M))g.Mode=ScreenMode.Map;
        }
    }
}

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace NoCauseForAlarm
{
    // Imported weighted meshes, humanoid retargeting and actual authored animation clips.
    public class CharacterVisual:MonoBehaviour
    {
        public Transform face;public Animator animator;public bool distorted;
        Transform skull;Vector3 previous,skullScale;PlayableGraph graph;AnimationMixerPlayable mixer;
        float moveWeight,talkWeight,flinchTime;
        public void Build(int id,bool creature=false)
        {
            bool female=id==1||id==2||id==3||id==4||id==6||id==8||id==10;
            var prefab=Resources.Load<GameObject>("LocalLicensed/People/Person"+id);
            if(prefab==null)throw new System.InvalidOperationException("Run the graphics asset preparation step: missing character prefab.");
            var model=Instantiate(prefab,transform);model.name="Imported animated character";
            // The import's axis conversion belongs to the model, not the actor root.
            var bounds=new Bounds();bool first=true;
            foreach(var r in model.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
            float height=creature?2.03f:id==0?1.89f:female?1.70f:1.80f;
            float factor=height/Mathf.Max(.01f,bounds.size.y);model.transform.localScale*=factor;
            model.transform.localPosition-=Vector3.up*(bounds.min.y-transform.position.y)*factor;
            animator=model.GetComponent<Animator>();if(animator==null)animator=model.GetComponentInChildren<Animator>();
            animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            skull=animator.GetBoneTransform(HumanBodyBones.Head);skullScale=skull.localScale;
            face=new GameObject("Face / animated head anchor").transform;face.SetParent(transform,false);
            graph=PlayableGraph.Create("Humanoid motion / "+id);graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            mixer=AnimationMixerPlayable.Create(graph,5);
            string[] states={"Idle","Walk","Flinch","Talk","Run"};
            for(int i=0;i<states.Length;i++)
            {
                var clip=Resources.Load<AnimationClip>("LocalLicensed/City"+states[i]+(female?"F":"M"));if(clip==null)throw new System.InvalidOperationException("Missing animation: "+states[i]);
                var playable=AnimationClipPlayable.Create(graph,clip);playable.SetApplyFootIK(true);playable.SetTime(id*.137);graph.Connect(playable,0,mixer,i);
            }
            var output=AnimationPlayableOutput.Create(graph,"Humanoid",animator);output.SetSourcePlayable(mixer);mixer.SetInputWeight(0,1);graph.Play();previous=transform.position;
        }
        public void React(){flinchTime=.7f;mixer.GetInput(2).SetTime(0);}
        void Update()
        {
            if(!graph.IsValid())return;
            var g=GameDirector.I;bool running=g!=null&&(g.Mode==ScreenMode.Play||g.Mode==ScreenMode.Dialogue||g.Mode==ScreenMode.Accuse||g.Mode==ScreenMode.Intro);
            graph.GetRootPlayable(0).SetSpeed(running?1:0);
            float speed=(transform.position-previous).magnitude/Mathf.Max(.001f,Time.deltaTime);previous=transform.position;
            moveWeight=Mathf.MoveTowards(moveWeight,running&&speed>.08f?1:0,Time.deltaTime*4);
            flinchTime=Mathf.Max(0,flinchTime-Time.deltaTime);float reaction=Mathf.Clamp01(flinchTime*3);
            bool speaking=g!=null&&g.Talking!=null&&g.Talking.gameObject==gameObject&&(g.Mode==ScreenMode.Dialogue||g.Mode==ScreenMode.Accuse);
            talkWeight=Mathf.MoveTowards(talkWeight,speaking?1:0,Time.deltaTime*3);
            float run=Mathf.Clamp01((speed-1.7f)/.7f);float rest=(1-moveWeight)*(1-reaction);
            mixer.SetInputWeight(0,rest*(1-talkWeight));mixer.SetInputWeight(3,rest*talkWeight);
            mixer.SetInputWeight(1,moveWeight*(1-reaction)*(1-run));mixer.SetInputWeight(4,moveWeight*(1-reaction)*run);mixer.SetInputWeight(2,reaction);
            mixer.GetInput(1).SetSpeed(Mathf.Clamp(speed/1.35f,.65f,2.3f));
        }
        void LateUpdate()
        {
            if(skull==null)return;
            if(distorted){skull.localScale=Vector3.Scale(skullScale,new Vector3(.83f,1.28f,1));skull.localRotation*=Quaternion.Euler(12,0,17);}
            else skull.localScale=skullScale;
            face.position=skull.position+transform.up*.035f;face.rotation=transform.rotation*Quaternion.Euler(0,0,distorted?14:0);
            face.localScale=distorted?new Vector3(.86f,1.3f,1):Vector3.one;
        }
        void OnDestroy(){if(graph.IsValid())graph.Destroy();}
    }
}

using System.Collections.Generic;
using UnityEngine;
namespace NoCauseForAlarm
{
    public class Soundscape:MonoBehaviour
    {
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();readonly List<AudioSource> ballasts=new List<AudioSource>();AudioSource ambience,music,voice;float nextEvent=20;
        AudioClip[] footsteps;AudioSource stepSource;int lastStep=-1;
        public int FootstepVariations=>footsteps==null?0:footsteps.Length;
        AudioSource Source(string name,bool loop,float spatial=0)
        {var g=new GameObject(name);g.transform.SetParent(transform);var a=g.AddComponent<AudioSource>();a.loop=loop;a.spatialBlend=spatial;a.rolloffMode=AudioRolloffMode.Linear;a.maxDistance=18;a.minDistance=1;return a;}
        AudioClip Clip(string name){if(!clips.ContainsKey(name))clips[name]=Resources.Load<AudioClip>("Audio/"+name);return clips[name];}
        void Start()
        {
            ambience=Source("HVAC / rain",true);ambience.clip=Clip("ambience");ambience.Play();
            music=Source("Low frequency score",true);music.clip=Clip("drone");music.Play();voice=Source("Public address",false);
            footsteps=Resources.LoadAll<AudioClip>("Audio/UserFootsteps");stepSource=Source("Player concrete footfalls",false,0);
            for(int i=0;i<5;i++){var a=Source("Fluorescent ballast",true,.85f);a.transform.position=new Vector3(0,3,i*12);a.clip=Clip("buzz");a.volume=.1f;a.Play();ballasts.Add(a);}
        }
        void Update()
        {
            var g=GameDirector.I;if(ambience==null)return;
            ambience.volume=g.Settings.sfx*(g.State.power?.32f:.12f);music.volume=g.Settings.music*(g.Chase!=null?.5f:g.Mode==ScreenMode.Menu||g.Mode==ScreenMode.Ending?.3f:.065f);voice.volume=g.Settings.sfx*.85f;
            foreach(var ballast in ballasts)ballast.volume=g.State.power?g.Settings.sfx*.1f:0;
            if(g.Mode!=ScreenMode.Play)return;nextEvent-=Time.deltaTime;
            if(nextEvent<=0){nextEvent=Random.Range(35,65);Vector3 p=g.Player.transform.position+g.Player.transform.forward*Random.Range(8,14);p.y=3;OneShot(g.State.hour>=13?"scratch":"door",p,.25f);}
        }
        public void OneShot(string name,Vector3 pos,float volume)
        {
            var clip=Clip(name);if(clip==null)return;var a=Source(name,false,.65f);a.transform.position=pos;a.clip=clip;a.volume=volume*GameDirector.I.Settings.sfx;a.pitch=Random.Range(.95f,1.04f);a.Play();Destroy(a.gameObject,clip.length+.2f);
        }
        public void Voice(string name){if(voice==null)return;var c=Clip(name);if(c==null)return;voice.clip=c;voice.Play();}
        public void Footstep(float volume,bool sprint)
        {
            if(footsteps==null||footsteps.Length==0){OneShot("step",GameDirector.I.Player.transform.position,volume);return;}
            int next=Random.Range(0,footsteps.Length);if(next==lastStep)next=(next+1)%footsteps.Length;lastStep=next;
            stepSource.pitch=Random.Range(.95f,1.04f)*(sprint?1.06f:1);stepSource.volume=volume*GameDirector.I.Settings.sfx;
            stepSource.PlayOneShot(footsteps[next]);
        }
    }
}

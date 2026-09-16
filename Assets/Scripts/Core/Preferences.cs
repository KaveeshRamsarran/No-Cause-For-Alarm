using System;
using UnityEngine;
namespace NoCauseForAlarm
{
    [Serializable] public class Preferences
    {
        public float master=.75f,music=.4f,sfx=.8f,sensitivity=1.3f,brightness=1.1f,fov=75,effects=.4f,shake=.4f;
        public bool subtitles=true;public int quality=1;
        public static Preferences Load(){try{return JsonUtility.FromJson<Preferences>(PlayerPrefs.GetString("preferences",""))??new Preferences();}catch{return new Preferences();}}
        public void Save(){PlayerPrefs.SetString("preferences",JsonUtility.ToJson(this));PlayerPrefs.Save();Apply();}
        public void Apply(){AudioListener.volume=master;QualitySettings.SetQualityLevel(Mathf.Clamp(quality,0,QualitySettings.names.Length-1),true);Application.targetFrameRate=60;
            if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipeline)
                pipeline.renderScale=quality==0?.7f:quality==1?.85f:1f;
        }
    }
}

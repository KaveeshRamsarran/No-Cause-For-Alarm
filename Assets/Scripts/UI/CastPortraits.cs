using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NoCauseForAlarm
{
    // Cached ID photos use precisely the same prefabs and idle poses as the people in the rooms.
    public class CastPortraits:MonoBehaviour
    {
        public readonly Texture2D[] images=new Texture2D[12];
        public bool Ready {get;private set;}
        IEnumerator Start()
        {
            var rig=new GameObject("Passenger portrait studio");rig.transform.position=new Vector3(2000,0,0);
            var cameraObject=new GameObject("ID camera");cameraObject.transform.SetParent(rig.transform,false);
            var camera=cameraObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.17f,.21f,.19f);camera.cullingMask=1<<30;
            camera.orthographic=true;camera.orthographicSize=.34f;camera.nearClipPlane=.05f;camera.farClipPlane=5;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            var target=new RenderTexture(192,224,24);camera.targetTexture=target;
            var lampObject=new GameObject("Portrait soft light");lampObject.transform.SetParent(rig.transform,false);lampObject.transform.localPosition=new Vector3(-.4f,2.2f,1.4f);
            var lamp=lampObject.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=5;lamp.intensity=2.3f;lamp.color=new Color(1,.94f,.84f);lamp.cullingMask=1<<30;
            for(int id=0;id<images.Length;id++)
            {
                var subject=new GameObject("ID subject");subject.transform.SetParent(rig.transform,false);
                var visual=subject.AddComponent<CharacterVisual>();visual.Build(id);
                foreach(var child in subject.GetComponentsInChildren<Transform>())child.gameObject.layer=30;
                yield return null;yield return null;
                var head=visual.animator.GetBoneTransform(HumanBodyBones.Head).position;
                camera.transform.position=head+new Vector3(0,-.08f,1.6f);camera.transform.LookAt(head-Vector3.up*.08f);
                yield return new WaitForEndOfFrame();
                var previous=RenderTexture.active;RenderTexture.active=target;
                var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();image.name=Cast.All[id].name+" portrait";images[id]=image;
                RenderTexture.active=previous;subject.SetActive(false);Destroy(subject);
            }
            camera.targetTexture=null;target.Release();Destroy(target);Destroy(rig);Ready=true;
        }
        void OnDestroy(){foreach(var image in images)if(image!=null)Destroy(image);}
    }
}

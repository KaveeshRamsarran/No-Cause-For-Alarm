using UnityEngine;
namespace NoCauseForAlarm
{
    public class FlameVfx:MonoBehaviour
    {
        public Light glow;public float strength;
        Transform ribbon;float phase;
        public static GameObject Create(string name,Transform parent,Vector3 position,float width,float height,float intensity,float range)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;
            var effect=g.AddComponent<FlameVfx>();effect.strength=intensity;effect.phase=Random.value*7;
            var sheet=new GameObject("Animated flame ribbon");sheet.transform.SetParent(g.transform,false);sheet.transform.localScale=new Vector3(width,height,1);
            var mesh=new Mesh{name="Flame quad"};mesh.vertices=new[]{new Vector3(-.5f,0,0),new Vector3(.5f,0,0),new Vector3(-.5f,1,0),new Vector3(.5f,1,0)};
            mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};mesh.triangles=new[]{0,2,1,2,3,1};mesh.RecalculateBounds();
            sheet.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=sheet.AddComponent<MeshRenderer>();renderer.sharedMaterial=new Material(Resources.Load<Shader>("Flame"));
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;effect.ribbon=sheet.transform;
            effect.glow=g.AddComponent<Light>();effect.glow.type=LightType.Point;effect.glow.color=new Color(1,.55f,.20f);effect.glow.range=range;effect.glow.intensity=intensity;
            return g;
        }
        void LateUpdate()
        {
            var g=GameDirector.I;if(g==null||g.Player==null)return;
            Vector3 forward=g.Player.view.transform.position-transform.position;forward.y=0;
            if(forward.sqrMagnitude>.0001f)ribbon.rotation=Quaternion.LookRotation(forward,Vector3.up);
            float shimmer=Mathf.Sin(Time.time*19+phase)*.08f+Mathf.Sin(Time.time*31+phase)*.035f;
            glow.intensity=strength*(1+shimmer*g.Settings.effects)*g.Settings.brightness;
        }
        void OnDestroy()
        {
            if(ribbon==null)return;Destroy(ribbon.GetComponent<MeshFilter>().sharedMesh);Destroy(ribbon.GetComponent<Renderer>().sharedMaterial);
        }
    }
}

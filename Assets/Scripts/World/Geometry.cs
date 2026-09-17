using UnityEngine;
using UnityEngine.Rendering;

namespace NoCauseForAlarm
{
    public static class Geometry
    {
        static readonly System.Collections.Generic.Dictionary<string,Material> propMaterials=new System.Collections.Generic.Dictionary<string,Material>();
        static Font worldFont;static Material textMaterial;
        public static Material wall, trim, floor, wood, metal, dark, paper, glow, red, glass;
        public static void Materials()
        {
            wall = Mat("Aged plaster", new Color(.54f,.53f,.43f)); trim = Mat("Institutional green", new Color(.22f,.29f,.27f));
            floor = Mat("Terrazzo", new Color(.29f,.31f,.29f)); wood = Mat("Varnished plywood", new Color(.34f,.24f,.15f));
            metal = Mat("Powder coated steel", new Color(.25f,.28f,.28f)); dark = Mat("Rubber", new Color(.045f,.055f,.053f));
            paper = Mat("Ivory paper", new Color(.75f,.73f,.61f)); glow = Mat("Fluorescent diffuser", new Color(.70f,.82f,.68f),2);
            red = Mat("Emergency red",new Color(.48f,.045f,.025f), .3f); glass = Mat("Rain-dark glass",new Color(.08f,.14f,.16f),.2f);
            AddTexture(wall,91,.07f);AddTexture(floor,42,.22f);AddTexture(wood,9,.16f);
        }
        static void AddTexture(Material material,int seed,float strength)
        {
            var rng=new System.Random(seed);var texture=new Texture2D(64,64);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Repeat;
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float v=1-(float)rng.NextDouble()*strength;if(material==floor&&(x<1||y<1))v=.64f;texture.SetPixel(x,y,new Color(v,v,v));}
            texture.Apply();material.SetTexture("_BaseMap",texture);material.SetTextureScale("_BaseMap",new Vector2(4,4));
        }
        public static Material Mat(string name, Color color, float emission=0)
        {
            var template=Resources.Load<Material>(emission>0?"SurfaceEmission":"Surface");
            var m = emission>0?new Material(Resources.Load<Shader>("Emissive")):template!=null?new Material(template):new Material(Shader.Find("Universal Render Pipeline/Lit")); m.name=name;
            m.SetColor("_BaseColor",color); m.SetFloat("_Smoothness",.15f);
            if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}
            return m;
        }
        public static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, Transform parent=null, bool solid=true)
        { var box=Shape(PrimitiveType.Cube,name,pos,size,mat,parent,solid);CampusArt.DressBox(box,size,mat);return box; }
        public static GameObject Shape(PrimitiveType type,string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool solid=false)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;
            g.GetComponent<Renderer>().sharedMaterial=mat;
            if(!solid) Object.Destroy(g.GetComponent<Collider>());
            return g;
        }
        public static TextMesh Text(string text,Vector3 pos,float size,Color color,Transform parent=null,float yaw=0)
        {
            if(worldFont==null)
            {
                worldFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");textMaterial=new Material(Resources.Load<Shader>("WorldText"));
                Font.textureRebuilt+=font=>{if(font==worldFont)textMaterial.mainTexture=font.material.mainTexture;};
            }
            worldFont.RequestCharactersInTexture(text,64);
            var g=new GameObject("Sign: "+text);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localEulerAngles=new Vector3(0,yaw+180,0);
            var t=g.AddComponent<TextMesh>();t.font=worldFont;t.text=text;t.characterSize=size*.25f;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;
            textMaterial.mainTexture=worldFont.material.mainTexture;g.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
            return t;
        }
        public static GameObject Prop(string resource, Vector3 pos, float width, float yaw=0, Transform parent=null)
        {
            var school=SchoolFurniture.Create(resource,pos,width,yaw,parent);if(school!=null)return school;
            var model=Resources.Load<GameObject>("Props/"+resource);
            if(model==null)return Box(resource,pos+Vector3.up*.4f,new Vector3(width,.8f,width*.6f),wood,parent);
            var root=new GameObject(resource);root.transform.SetParent(parent,false);root.transform.localPosition=pos;
            var g=Object.Instantiate(model,root.transform);g.transform.localPosition=Vector3.zero;
            var bounds=new Bounds(); bool first=true;
            foreach(var r in g.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
            float scale=width/Mathf.Max(.01f,bounds.size.x);g.transform.localScale*=scale;
            g.transform.localPosition-=new Vector3(bounds.center.x-root.transform.position.x,bounds.min.y-root.transform.position.y,bounds.center.z-root.transform.position.z)*scale;
            foreach(var r in g.GetComponentsInChildren<Renderer>())
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    Color original=mats[i]!=null&&mats[i].HasProperty("_Color")?mats[i].color:new Color(.45f,.43f,.36f);
                    Color.RGBToHSV(original,out var hue,out var saturation,out var value);
                    Color aged=Color.HSVToRGB(hue,saturation*.40f,Mathf.Clamp(value*.67f,.16f,.7f));
                    string key=resource+" aged surface "+i;
                    if(!propMaterials.TryGetValue(key,out var shared))
                    {
                        shared=Mat(key,aged);
                        if(resource=="schoolTable"||resource=="schoolChair"||resource=="lectern"||resource=="locker"||resource=="clock"||resource=="book"||resource=="radio"||resource=="monitor"||resource=="vending"||resource=="extinguisher"||resource=="telephone")
                        {shared.SetTexture("_BaseMap",Resources.Load<Texture2D>("Props/SchoolPalette"));shared.SetColor("_BaseColor",new Color(.72f,.77f,.66f));}
                        propMaterials[key]=shared;
                    }
                    mats[i]=shared;
                }
                r.sharedMaterials=mats;
            }
            root.transform.localEulerAngles=new Vector3(0,yaw,0);
            var col=root.AddComponent<BoxCollider>();col.center=new Vector3(0,bounds.size.y*scale*.5f,0);col.size=new Vector3(width,bounds.size.y*scale,bounds.size.z*scale);
            return root;
        }
        public static Light Lamp(Vector3 pos,bool emergency=false,Transform parent=null)
        {
            if(emergency)
            {
                // Model the sconce in its own wall-facing frame. The old ceiling fixture
                // extended along X and intersected the corridor partition.
                float side=Mathf.Sign(pos.x);
                if(Mathf.Abs(pos.x)>3){pos.x=side*13.04f;pos.z+=3.5f;}else pos.x=side*2.64f;
                var mount=new GameObject("Emergency wall sconce").transform;mount.SetParent(parent,false);mount.localPosition=pos;
                mount.localRotation=Quaternion.Euler(0,side>0?-90:90,0);
                Box("Sconce backplate",Vector3.zero,new Vector3(.42f,.22f,.065f),metal,mount,false);
                Box("Red diffuser",new Vector3(0,0,.055f),new Vector3(.34f,.135f,.065f),red,mount,false);
                for(int i=-1;i<=1;i++)Box("Protective lens bar",new Vector3(i*.13f,0,.093f),new Vector3(.018f,.17f,.025f),metal,mount,false);
                var source=new GameObject("Emergency light");source.transform.SetParent(mount,false);source.transform.localPosition=new Vector3(0,0,.24f);
                var light=source.AddComponent<Light>();light.type=LightType.Point;light.range=6;light.intensity=.9f;light.color=new Color(1,.18f,.08f);light.shadows=LightShadows.None;
                source.AddComponent<CampusLight>().emergency=true;return light;
            }
            Box("Fixture",pos,new Vector3(1.4f,.12f,.28f),metal,parent,false);
            Box("Tube",pos-Vector3.up*.07f,new Vector3(1.25f,.045f,.20f),emergency?red:glow,parent,false);
            var g=new GameObject(emergency?"Emergency light":"Fluorescent light");g.transform.SetParent(parent,false);g.transform.localPosition=pos-Vector3.up*.2f;
            var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=emergency?7:10;l.intensity=emergency?.9f:2.5f;l.color=emergency?new Color(1,.18f,.08f):new Color(.64f,.87f,.55f);
            l.shadows=LightShadows.None;
            if(!emergency&&Mathf.Abs(pos.x)>3&&pos.z%10<0)
            {l.type=LightType.Spot;g.transform.localRotation=Quaternion.Euler(90,0,0);l.spotAngle=145;l.innerSpotAngle=95;l.intensity=5;l.shadows=LightShadows.Hard;l.shadowResolution=UnityEngine.Rendering.LightShadowResolution.Low;}
            g.AddComponent<CampusLight>().emergency=emergency; return l;
        }
    }
    public class CampusLight:MonoBehaviour
    {
        public bool emergency; Light lamp; float original,phase;
        void Start(){lamp=GetComponent<Light>();original=lamp.intensity;phase=Random.value*100;}
        void Update(){var game=GameDirector.I;if(game==null)return;bool on=game.State==null||game.State.power;
            float flutter=(Mathf.Sin(Time.time*2.1f+phase)>.985f && game.Settings.effects>.1f)?.3f:1;
            lamp.intensity=original*(emergency?(on?.17f:1):(on?flutter:.025f))*game.Settings.brightness;}
    }
}

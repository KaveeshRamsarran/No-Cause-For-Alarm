using UnityEngine;
namespace NoCauseForAlarm
{
    public class SupplyPickup:MonoBehaviour
    {
        public string id;Renderer[] meshes;Collider trigger;bool visible=true;
        public static void Place(Supply supply,Transform parent,Vector3 position)
        {
            var root=new GameObject(supply.name);root.transform.SetParent(parent,false);root.transform.localPosition=position;
            var pickup=root.AddComponent<SupplyPickup>();pickup.id=supply.id;
            var col=root.AddComponent<BoxCollider>();col.center=Vector3.up*.065f;col.size=new Vector3(.29f,.13f,.24f);
            root.AddComponent<Interactable>().Setup("supply","PICK UP "+supply.name.ToUpper(),supply.id);
            if(supply.id=="batteries"||supply.id=="fuse")
            {
                for(int i=0;i<(supply.id=="fuse"?1:2);i++)
                {Geometry.Shape(PrimitiveType.Cylinder,"Battery casing",new Vector3(i*.06f-.03f,.04f,0),new Vector3(.037f,.04f,.037f),Geometry.metal,root.transform);Geometry.Shape(PrimitiveType.Cylinder,"Contact",new Vector3(i*.06f-.03f,.085f,0),new Vector3(.024f,.006f,.024f),Geometry.wood,root.transform);}
            }
            else if(supply.id=="keyledger")Geometry.Prop("book",new Vector3(0,0,0),.23f,0,root.transform);
            else if(supply.id=="inhaler")
            {Geometry.Box("Inhaler",new Vector3(0,.045f,0),new Vector3(.045f,.09f,.035f),Geometry.glass,root.transform,false);Geometry.Box("Mouthpiece",new Vector3(0,.015f,-.025f),new Vector3(.045f,.03f,.035f),Geometry.paper,root.transform,false);}
            else if(supply.id=="gloves")
            {
                for(int side=-1;side<=1;side+=2)
                {
                    Geometry.Shape(PrimitiveType.Sphere,"Glove palm",new Vector3(side*.065f,.015f,0),new Vector3(.085f,.03f,.09f),Geometry.wood,root.transform);
                    for(int finger=0;finger<4;finger++)Geometry.Shape(PrimitiveType.Sphere,"Glove finger",new Vector3(side*.065f+(finger-1.5f)*.018f,.015f,.06f),new Vector3(.018f,.024f,.08f),Geometry.wood,root.transform);
                    Geometry.Shape(PrimitiveType.Sphere,"Glove thumb",new Vector3(side*.015f,.015f,.005f),new Vector3(.026f,.025f,.06f),Geometry.wood,root.transform);
                }
            }
            else
            {
                var size=supply.id=="usb"?new Vector3(.04f,.02f,.10f):supply.id=="canopener"?new Vector3(.035f,.035f,.15f):new Vector3(.21f,.04f,.14f);
                Geometry.Box(supply.name,size.y*.5f*Vector3.up,size,supply.id=="samplekit"?Geometry.paper:Geometry.metal,root.transform,false);
                if(supply.id=="usb")Geometry.Box("USB connector",new Vector3(0,.01f,.07f),new Vector3(.03f,.014f,.04f),Geometry.paper,root.transform,false);
                if(supply.id=="cassette")for(int x=-1;x<=1;x+=2)Geometry.Shape(PrimitiveType.Cylinder,"Tape spool",new Vector3(x*.055f,.023f,0),new Vector3(.035f,.004f,.035f),Geometry.dark,root.transform);
                if(supply.id=="samplekit")
                {Geometry.Box("Medical cross",new Vector3(0,.021f,0),new Vector3(.025f,.003f,.08f),Geometry.red,root.transform,false);Geometry.Box("Medical cross",new Vector3(0,.021f,0),new Vector3(.08f,.003f,.025f),Geometry.red,root.transform,false);}
                if(supply.id=="canopener")
                {Geometry.Shape(PrimitiveType.Cylinder,"Cutting wheel",new Vector3(0,.022f,.09f),new Vector3(.06f,.009f,.06f),Geometry.metal,root.transform);Geometry.Box("Crank",new Vector3(.035f,.04f,.065f),new Vector3(.06f,.015f,.015f),Geometry.dark,root.transform,false);}
            }
            pickup.meshes=root.GetComponentsInChildren<Renderer>();pickup.trigger=col;
        }
        void Update()
        {
            var state=GameDirector.I?.State;if(state==null)return;bool show=!state.collected.Contains(id);
            if(show==visible)return;visible=show;foreach(var r in meshes)r.enabled=show;trigger.enabled=show;
            // Books from the school pack also have a physical collider.
            foreach(var c in GetComponentsInChildren<Collider>())c.enabled=show;
        }
    }
}

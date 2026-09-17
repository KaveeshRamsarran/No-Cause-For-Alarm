using System.Collections.Generic;
using UnityEngine;
namespace NoCauseForAlarm
{
    public static class SchoolFurniture
    {
        public static readonly Dictionary<string,string> Models=new Dictionary<string,string>{
            {"schoolTable","table3"},{"desk","table3"},{"lectern","table3"},{"tableRound","table2"},
            {"schoolChair","chair"},{"chair","chair"},{"chairDesk","chair"},{"teacherChair","chair"},
            {"kitchenCabinet","table1"},{"bathroomSink","table1"},{"bookcaseOpen","rack1"},
            {"locker","locker_1"},{"computerScreen","computer"},{"monitor","computer"},
            {"book","book"},{"extinguisher","fire"},{"displayCounter","showcase"}};
        public static GameObject Create(string resource,Vector3 position,float width,float yaw,Transform parent)
        {
            if(!Models.TryGetValue(resource,out var modelName))return null;
            var prefab=Resources.Load<GameObject>("LocalLicensed/School/"+modelName);
            if(prefab==null)throw new System.InvalidOperationException("Prepare School assets with Tools/setup_graphics.ps1.");
            var root=new GameObject(resource);root.transform.SetParent(parent,false);root.transform.localPosition=position;
            var model=Object.Instantiate(prefab,root.transform);model.name="School assets / "+modelName;
            var b=CampusArt.LocalBounds(model.transform);float scale=width/b.size.x;
            float height=b.size.y*scale;
            if(resource=="desk"||resource=="schoolTable"||resource=="kitchenCabinet"||resource=="bathroomSink")height=.78f;
            if(resource=="lectern")height=1.05f;
            if(resource=="kitchenCabinet"||resource=="bathroomSink")height=1.15f;
            if(resource=="tableRound")height=.78f;
            if(resource=="locker")height=1.8f;
            if(resource=="bookcaseOpen")height=2.05f;
            model.transform.localScale=new Vector3(scale,height/b.size.y,scale);
            float solidHeight=resource=="kitchenCabinet"||resource=="bathroomSink"?.78f:height;
            var collider=root.AddComponent<BoxCollider>();collider.center=Vector3.up*(solidHeight*.5f);collider.size=new Vector3(width,solidHeight,b.size.z*scale);
            root.transform.localRotation=Quaternion.Euler(0,yaw,0);return root;
        }
    }
}

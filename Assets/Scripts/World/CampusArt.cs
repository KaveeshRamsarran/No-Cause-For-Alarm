using UnityEngine;
namespace NoCauseForAlarm
{
    public static class CampusArt
    {
        public static Bounds LocalBounds(Transform root)
        {
            var bounds=new Bounds();bool first=true;
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var b=filter.sharedMesh.bounds;var matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int i=0;i<8;i++)
                {
                    var p=matrix.MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                    if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);
                }
            }
            return bounds;
        }
        public static GameObject Fit(string resource,Transform parent,Vector3 center,Vector3 size,float yaw=0)
        {
            var prefab=Resources.Load<GameObject>("LocalLicensed/"+resource);if(prefab==null)return null;
            var root=new GameObject("Imported architecture / "+resource);root.transform.SetParent(parent,false);
            var model=Object.Instantiate(prefab,root.transform);var bounds=LocalBounds(root.transform);
            model.transform.localPosition-=bounds.center;
            root.transform.localScale=new Vector3(size.x/Mathf.Max(.001f,bounds.size.x),size.y/Mathf.Max(.001f,bounds.size.y),size.z/Mathf.Max(.001f,bounds.size.z));
            root.transform.localPosition=center;root.transform.localRotation=Quaternion.Euler(0,yaw,0);
            if(resource=="sofa_small"||resource=="bookshelf"){var collider=root.AddComponent<BoxCollider>();collider.size=bounds.size;}
            return root;
        }
        public static void DressBox(GameObject box,Vector3 size,Material material)
        {
            if(Resources.Load<GameObject>("LocalLicensed/wall_default")==null)return;
            bool wall=material==Geometry.wall&&size.y>3&&Mathf.Min(size.x,size.z)<.5f;
            bool floor=material==Geometry.floor&&size.y<.4f;
            bool ceiling=material==Geometry.wall&&size.y<.3f;
            if(!wall&&!floor&&!ceiling)return;
            // Visual modules sit on the existing collision shell, preserving every doorway.
            var parent=box.transform.parent;var center=box.transform.localPosition;
            if(wall)
            {
                bool alongX=size.x>size.z;float length=alongX?size.x:size.z;int count=Mathf.CeilToInt(length/2.5f);float tile=length/count;
                for(int i=0;i<count;i++)
                {
                    var pos=center+(alongX?Vector3.right:Vector3.forward)*(-length*.5f+tile*(i+.5f));
                    Fit("wall_default",parent,pos,new Vector3(tile,size.y,.25f),alongX?0:90);
                }
            }
            else
            {
                int nx=Mathf.CeilToInt(size.x/3),nz=Mathf.CeilToInt(size.z/3);float tx=size.x/nx,tz=size.z/nz;
                for(int x=0;x<nx;x++)for(int z=0;z<nz;z++)
                {
                    var pos=center+new Vector3(-size.x*.5f+tx*(x+.5f),floor?size.y*.5f-.018f:-size.y*.5f+.018f,-size.z*.5f+tz*(z+.5f));
                    Fit(floor?"floor_small":"ceiling_small",parent,pos,new Vector3(tx,.035f,tz));
                }
            }
            box.GetComponent<Renderer>().enabled=false;
        }
        public static void Door(GameObject collisionLeaf)
        {
            var art=Fit("DoorLeaf",collisionLeaf.transform,Vector3.zero,Vector3.one);
            if(art==null)return;
            // Door asset width X -> collision width Z; compensate parent nonuniform scale.
            art.transform.localRotation=Quaternion.Euler(0,90,0);
            var bounds=LocalBounds(art.transform);art.transform.localScale=new Vector3(1/Mathf.Max(.001f,bounds.size.x),1/Mathf.Max(.001f,bounds.size.y),1/Mathf.Max(.001f,bounds.size.z));
            collisionLeaf.GetComponent<Renderer>().enabled=false;
        }
        public static void RoomDetails(Room room,Transform parent)
        {
            var c=room.center;float s=room.side;
            Fit("wall_with_one_window",parent,c+new Vector3(s*4.96f,1.65f,1.8f),new Vector3(2.1f,3.2f,.30f),s<0?90:270);
            if(room.name=="STAFF OFFICE"||room.name=="SECURITY"||room.name=="ARCHIVE")
            {
                Fit("bookshelf",parent,c+new Vector3(3.4f,1.15f,-4.35f),new Vector3(1.5f,2.3f,.45f),180);
                Fit("light_desk",parent,c+new Vector3(1.5f,1.09f,1.1f),new Vector3(.35f,.55f,.32f));
                if(room.name=="STAFF OFFICE")Fit("sofa_small",parent,c+new Vector3(2.5f,.48f,3.7f),new Vector3(2.1f,.96f,.85f),180);
            }
            if(room.name=="LECTURE 01"||room.name=="STAFF OFFICE")Fit("curtain_1",parent,c+new Vector3(s*4.75f,1.8f,-1.5f),new Vector3(2.5f,2.6f,.1f),90);
        }
    }
}

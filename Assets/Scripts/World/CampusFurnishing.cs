using UnityEngine;

namespace NoCauseForAlarm
{
    // Reuse the owner's free School pack; additions leave the door approach and clue surfaces clear.
    public static class CampusFurnishing
    {
        public static void Clock(Vector3 center,Transform parent)
        {
            var root=new GameObject("Wall clock");root.transform.SetParent(parent,false);root.transform.localPosition=center;
            var model=Object.Instantiate(Resources.Load<GameObject>("Props/clock"),root.transform);
            var b=CampusArt.LocalBounds(root.transform);
            // The source clock's thin axis is not its forward axis. Orient before measuring/scaling.
            if(b.size.x< b.size.z&&b.size.x<b.size.y)model.transform.Rotate(0,90,0,Space.World);
            else if(b.size.y<b.size.z)model.transform.Rotate(90,0,0,Space.World);
            model.transform.Rotate(0,180,0,Space.World); // Clock dial faces into the room, not into the plaster.
            b=CampusArt.LocalBounds(root.transform);float scale=.46f/Mathf.Max(b.size.x,b.size.y);
            model.transform.localPosition-=b.center;root.transform.localScale=Vector3.one*scale;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())
            {
                var material=new Material(Resources.Load<Material>("Surface"));material.SetTexture("_BaseMap",Resources.Load<Texture2D>("Props/SchoolPalette"));
                material.SetColor("_BaseColor",new Color(.85f,.88f,.77f));renderer.sharedMaterial=material;
            }
        }
        public static void Stalls(Room room,Transform parent)
        {
            var c=room.center;
            for(int i=0;i<4;i++)Geometry.Box("Stall partition",c+new Vector3(-3+i*2,1.17f,-3.15f),new Vector3(.10f,2.06f,3.1f),Geometry.trim,parent);
            for(int i=0;i<3;i++)
            {
                float x=-2+i*2;
                Geometry.Prop("toilet",c+new Vector3(x,0,-3.7f),.7f,180,parent);
                foreach(float dx in new[]{-.82f,.82f})Geometry.Box("Stall front panel",c+new Vector3(x+dx,1.17f,-1.60f),new Vector3(.30f,2.06f,.10f),Geometry.trim,parent);
                var hinge=new GameObject("Stall "+(i+1)+" hinge");hinge.transform.SetParent(parent,false);hinge.transform.localPosition=c+new Vector3(x-.65f,0,-1.60f);
                var leaf=Geometry.Box("Stall door",new Vector3(.65f,1.17f,0),new Vector3(1.28f,1.96f,.08f),Geometry.wood,hinge.transform);
                // School cubicles use the same free imported door leaf, fitted to the privacy panel.
                var art=CampusArt.Fit("DoorLeaf",leaf.transform,Vector3.zero,Vector3.one);
                if(art!=null)leaf.GetComponent<Renderer>().enabled=false;
                // Outward swing leaves enough room to shut the door while standing inside.
                var door=hinge.AddComponent<Door>();door.room="STALL "+(i+1);door.isStall=true;door.sign=-1;door.swing=100;
                var interaction=leaf.AddComponent<Interactable>();interaction.Setup("door","OPEN STALL "+(i+1),"");interaction.door=door;
                foreach(float z in new[]{-.075f,.075f})Geometry.Box("Stall latch",new Vector3(1.14f,1.1f,z),new Vector3(.09f,.16f,.06f),Geometry.metal,hinge.transform,false);
                Geometry.Text((i+1).ToString("00"),c+new Vector3(x,2.38f,-1.52f),.08f,Color.white,parent);
            }
            Geometry.Text("WASH HANDS BEFORE RETURNING TO CLASS",c+new Vector3(0,1.80f,4.78f),.055f,Color.white,parent,180);
            Geometry.Box("Washbasin notice board",c+new Vector3(0,1.8f,4.82f),new Vector3(1.7f,.34f,.045f),Geometry.trim,parent,false);
        }
        public static void Dress(Room room,Transform parent)
        {
            var c=room.center;string name=room.name;
            bool classroom=name.Contains("CLASSROOM")||name=="LECTURE 01";
            if(classroom)
            {
                Geometry.Prop("desk",c+new Vector3(2,0,-3.85f),1.65f,180,parent);
                Geometry.Prop("teacherChair",c+new Vector3(2,0,-4.48f),.55f,0,parent);
                for(int i=0;i<2;i++)Geometry.Prop("bookcaseOpen",c+new Vector3(i*2,0,4.38f),1.3f,180,parent);
                Geometry.Prop("book",c+new Vector3(2,.79f,-3.85f),.34f,10,parent);
                Geometry.Prop("book",c+new Vector3(2.1f,.84f,-3.85f),.30f,-8,parent);
                for(int row=-2;row<=2;row+=2)
                {
                    Geometry.Prop("book",c+new Vector3(-2.25f,.79f,row),.26f,row*5,parent);
                    Geometry.Box("Student worksheet",c+new Vector3(.25f,.786f,row),new Vector3(.20f,.006f,.27f),Geometry.paper,parent,false);
                }
                var projector=CampusArt.Fit("School/projector",parent,c+new Vector3(0,2.85f,.8f),new Vector3(.42f,.18f,.34f),180);
                if(projector!=null)Geometry.Box("Projector ceiling bracket",c+new Vector3(0,3.08f,.8f),new Vector3(.055f,.3f,.055f),Geometry.metal,parent,false);
                Geometry.Box("Board chalk tray",c+new Vector3(0,1.12f,-4.66f),new Vector3(4.8f,.055f,.20f),Geometry.metal,parent,false);
                Geometry.Text(name=="CLASSROOM 03"?"CLASS 03 / DO NOT MOVE CALIBRATION EQUIPMENT":name=="CLASSROOM 02"?"CLASS 02 / SEMINAR MATERIALS":"LECTURE 01 / RETURN BORROWED BOOKS",c+new Vector3(1,2.52f,4.83f),.065f,Color.white,parent,180);
            }
            else if(name=="CAFETERIA")
            {
                Geometry.Prop("displayCounter",c+new Vector3(-3.4f,0,-4.2f),1.65f,0,parent);
                Geometry.Prop("tableRound",c+new Vector3(-2,0,-1.5f),1.8f,0,parent);
                Geometry.Text("RETURN TRAYS / KEEP THE EXIT CLEAR",c+new Vector3(-2.8f,2.1f,-4.83f),.075f,Color.white,parent);
            }
            else if(name!="BATHROOM")
            {
                Geometry.Prop("bookcaseOpen",c+new Vector3(3.3f,0,4.35f),1.45f,180,parent);
                Geometry.Prop("locker",c+new Vector3(-3.3f,0,3.1f),.75f,0,parent);
                if(name=="COMPUTER LAB")
                {
                    for(int a=-3;a<=3;a+=3)Geometry.Prop("chairDesk",c+new Vector3(a,0,-2.12f),.55f,180,parent);
                    Geometry.Text("COMPUTER LAB / SAVE YOUR WORK LOCALLY",c+new Vector3(0,2.3f,-4.83f),.08f,Color.white,parent);
                }
                else
                {
                    Geometry.Prop("desk",c+new Vector3(-2,0,-1.4f),1.65f,0,parent);
                    Geometry.Prop("chairDesk",c+new Vector3(-2,0,-.5f),.55f,180,parent);
                    Geometry.Prop(name=="SECURITY"?"computerScreen":"book",c+new Vector3(-2,.79f,-1.4f),name=="SECURITY"?.65f:.32f,0,parent);
                }
                if(name=="ARCHIVE"||name=="STORES")
                {
                    Geometry.Prop("bookcaseOpen",c+new Vector3(1.4f,0,4.35f),1.45f,180,parent);
                    Geometry.Text(name=="ARCHIVE"?"ARCHIVE / COHORT RECORDS":"STORES / RETURN ALL BORROWED ITEMS",c+new Vector3(2.35f,2.65f,4.83f),.07f,Color.white,parent,180);
                }
            }
        }
    }
}

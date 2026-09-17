using UnityEngine;
namespace NoCauseForAlarm
{
    public class Supply
    {
        public string id,name,room;public Vector3 position;
        public Supply(string i,string n,string r,Vector3 p){id=i;name=n;room=r;position=p;}
    }
    public static class HelpRequests
    {
        public static readonly Supply[] Supplies={
            new Supply("batteries","Radio batteries","UTILITY",new Vector3(1.5f,.79f,1)),
            new Supply("keyledger","Key sign-out book","MAINTENANCE",new Vector3(.45f,.79f,1)),
            new Supply("fuse","Replacement fuse","STORES",new Vector3(1.5f,.79f,1)),
            new Supply("samplekit","Sealed sample kit","COMPUTER LAB",new Vector3(-2.45f,.79f,-3)),
            new Supply("usb","Noah's USB drive","CAFETERIA",new Vector3(1.8f,.79f,-3.6f)),
            new Supply("inhaler","Ben's inhaler","CLASSROOM 03",new Vector3(2.35f,.79f,2)),
            new Supply("canopener","Kitchen can opener","STORES",new Vector3(.45f,.79f,1)),
            new Supply("cassette","Cohort 19 cassette","ARCHIVE",new Vector3(1.55f,.79f,1)),
            new Supply("gloves","Insulated work gloves","MAINTENANCE",new Vector3(1.55f,.79f,1))};
        public static string Item(int id){switch(id){case 1:return "batteries";case 2:return "keyledger";case 3:return "fuse";case 4:return "samplekit";case 5:return "usb";case 7:return "inhaler";case 8:return "cassette";case 9:return "canopener";case 11:return "gloves";default:return "";}}
        public static string Name(string id){foreach(var item in Supplies)if(item.id==id)return item.name;return id;}
        public static string Location(string id){foreach(var item in Supplies)if(item.id==id)return item.room;return "";}
        public static string Request(int id)
        {
            switch(id){
                case 0:return "I owe you an explanation. Get the photograph from my office and the sealed report from Archive. I'd rather you had them in front of you.";
                case 1:return "My radio's dead. There are spare batteries on the desk in Utility. Bring them back and I'll get you cleared for the archive and cameras.";
                case 2:return "I can lend you the office key. But fetch my sign-out book from Maintenance first, would you? Someone's been using the spare, and I need to know who.";
                case 3:return "I can fix this. Probably. There's a replacement fuse on the desk in Stores. Get that, and please don't touch the cabinet while I'm working.";
                case 4:return "Bring me that tissue from the bathroom and the sealed sample kit in the computer lab. I'll take a look. Just... don't tell everyone it's a diagnosis.";
                case 5:return "I can pull the deleted access log, but I need my USB drive. I left it on the cafeteria counter. Of all the days to forget it.";
                case 6:return "Let me see the register from Lecture 01. I know whose handwriting is whose. I've spent all term chasing these people for signatures.";
                case 7:return "My inhaler's still in Classroom 03, on the desk beside my bag. Could you get it? I don't want to go back in there.";
                case 8:return "There's a cassette with my brother's cohort number on it. In Archive. If you find it, bring it to me before you give it to Venn.";
                case 9:return "We should eat something. The tins are here, but Ada borrowed the can opener. Try the desk in Stores. I'll put a plate aside for you.";
                case 10:return "Bring me the sleeve from that old bag in Classroom 03. I want to check the candle batch before anyone starts making claims about it.";
                default:return "I can show you how to shut that duct, but I need my insulated gloves. They're on the desk in Maintenance. Bare hands and that actuator are a bad combination.";
            }
        }
        public static string Thanks(int id)
        {
            switch(id){
                case 0:return "Yes. That's my signature. I kept telling myself someone else would stop it. Nobody did. We need to seal the duct before six.";
                case 1:return "There we go. Thanks. I've put your name through; the archive lock and camera terminal should accept you now. Come back if they don't.";
                case 2:return "Oh, thank goodness. Here, take the brass key. Office across from Security. And shut the door after you, please.";
                case 3:return "That's the right rating. Good. Give me a second... there. I've replaced the bridge. We shouldn't lose the cameras again.";
                case 4:return "Hold the light steady. See how the edge curls away? The control doesn't do that. I've written down what I can actually see. That's all I can promise.";
                case 5:return "Found it. Two readers, the same badge, six seconds apart. I've saved a copy for you. Could be a cloned card. I really hope it's a cloned card.";
                case 6:return "That's not one person's writing. And this line was added later. I've marked the differences, but don't go arresting someone because their pen ran out.";
                case 7:return "Thanks. Just give me a minute. I heard something above that desk before I ran out. Three scrapes, then a knock. I wasn't imagining it.";
                case 8:return "That's his voice. He says they asked to leave. Venn told us they'd volunteered to stay. Please keep a copy. I don't trust myself with the only one.";
                case 9:return "Here. Eat it while it's warm. You look like you're running on nerves. There's enough for the others too; I'll sort them out.";
                case 10:return "Same batch as the reference candle in my office. Thirty seconds, give or take. If that classroom candle keeps going, the wax isn't the explanation.";
                default:return "Right, now I can get a grip on it. I've marked E-03 for you. Locate the noise first, then use the damper in here. I'll help with the fuse bank if it trips.";
            }
        }
        public static bool Ready(int id,GameState state)
        {
            string item=Item(id);if(item!=""&&!state.inventory.Contains(item))return false;
            if(id==0)return state.Has("photo")&&state.Has("report");
            if(id==4)return state.Has("tissue");
            if(id==6)return state.Has("register");
            if(id==10)return state.Has("bag");
            return true;
        }
    }
}

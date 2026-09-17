using System.Linq;

namespace NoCauseForAlarm
{
    public static class EndingReport
    {
        public static int[] Boarded(GameState s)=>s.evacuation.Where(i=>i>=0&&i<s.people.Length&&s.Available(i)).Distinct().ToArray();
        public static string Narrative(GameState s)
        {
            int humans=Boarded(s).Count(i=>!s.people[i].infiltrator);
            string rescue=humans==0?"You leave without a single human passenger.":"You get "+humans+" human passenger"+(humans==1?"":"s")+" to the district checkpoint. They are taken into protective isolation.";
            switch(s.ending)
            {
                case "THE LAST BUS":return rescue+" No infiltrator is on the bus. The sealed E-03 damper keeps the ceiling organism inside the college.\n\nYou give the checkpoint staff your case notes and warn them to keep the campus closed. Everyone you brought is safe for tonight. Anyone left behind still needs to be found.\n\nThe driver turns off the radio. For the first time all day, nobody tells you that everything is fine.";
                case "INFILTRATION":return "At least one infiltrator passed your passenger review. Beyond the college gates, it attacks inside the bus. The driver stops short of the district checkpoint; the evacuation becomes another containment incident.\n\n"+(s.ceilingSealed?"You sealed the source in the building, but the passenger you admitted carried the threat outside.":"The ceiling source is still open, and your passenger choice has carried the threat outside as well.")+"\n\nYour notes helped you make a choice. They could not make that choice safe.";
                case "A CLEAN REGISTER":return rescue+" No infiltrator boards, and the ceiling source is sealed.\n\nBut at least three humans were detained, killed, or excluded from your manifest. The checkpoint cannot call this a successful rescue. It opens an inquiry into the people you left behind.\n\nYou hand over the register. Beside the crossed-out names, you have to write what you actually knew.";
                case "THE BUILDING REMAINS":return rescue+" You kept infiltrators off the bus, but never sealed damper E-03.\n\nThe organism can still move through the college ventilation. Rescue teams cannot enter safely, and the next intake must be cancelled. Escaping the wing did not contain its source.\n\nAt the checkpoint, you point to Classroom 03 on the plan and tell them not to send anyone under that ceiling.";
                case "THE PREVIOUS COHORT":return "You stay with Venn beside the sealed E-03 duct. He admits that the earlier exposure study continued after its students asked to leave. This time you make him give that account on the security radio.\n\nYou relay the names from the photograph, the study's findings and his confession"+(s.Has("recording")?", backed by June's recovered recording":"")+". The district records the evidence and suspends the next intake. Venn stays to answer to the responders.\n\nYou have exposed the experiment and contained the ceiling source. You have not made every person in the wing safe; the remaining occupants still need a guarded evacuation.";
                default:return s.health<=0?"You were caught before you could leave the wing. Your injuries stop you reaching the bus; no departure is authorised by you.\n\nThe investigation ends with the evidence you had gathered. Closing doors and keeping a flame between you and a pursuer can buy time, but neither replaces sealing the ceiling source.\n\nThe next announcement asks you to report to the exit. You cannot answer.":"All three infiltrators are still loose when you open the evacuation exit. They converge before the driver can finish boarding. The bus never leaves the college.\n\nThe passenger list cannot hold back an attack at the doorway. You needed to identify and contain at least one of them before departure, as well as decide who could board.\n\nThe driver calls for an armed containment team. Inside the wing, the announcement keeps promising transport.";
            }
        }
        public static string Consequences(GameState s)
        {
            var aboard=Boarded(s);bool leaves=s.ending!="OVERRUN"&&s.ending!="THE PREVIOUS COHORT";
            string names=aboard.Length==0?"None":string.Join(", ",aboard.Select(i=>Cast.All[i].name));
            int excluded=Enumerable.Range(0,12).Count(i=>!s.people[i].infiltrator&&s.Available(i)&&!aboard.Contains(i));
            int missing=s.people.Count(p=>!p.infiltrator&&p.missing),dead=s.people.Count(p=>!p.infiltrator&&p.dead);
            return (leaves?"PASSENGERS: ":"PLANNED MANIFEST (NOT A COMPLETED EVACUATION): ")+names+"\n"+
                "CEILING SOURCE: "+(s.ceilingSealed?"SEALED":"OPEN")+"   /   WRONGFUL DETENTIONS: "+s.mistakes+"   /   HUMAN DEATHS: "+dead+"\n"+
                "HUMANS EXCLUDED: "+excluded+"   /   HUMANS MISSING: "+missing+"   /   EVIDENCE FILED: "+s.evidence.Count+"\n"+
                (s.ending=="INFILTRATION"?"INFILTRATORS ADMITTED: "+string.Join(", ",aboard.Where(i=>s.people[i].infiltrator).Select(i=>Cast.All[i].name)):s.ending=="THE PREVIOUS COHORT"?"YOU AND VENN: remaining for the containment team.":s.ending=="OVERRUN"?"DEPARTURE: failed.":"DEPARTURE: reached the district checkpoint.");
        }
    }
}

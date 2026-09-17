using System.Linq;
namespace NoCauseForAlarm
{
    public static class EvidenceText
    {
        public static string Name(string id)
        {
            switch(id){case "attendance":return "Mara's register annotations";case "recording":return "Cohort 19 / cassette transcript";case "protocol":return "Matched candle calibration";case "key":return "Staff-office key";case "register":return "Attendance register";case "bag":return "The unclaimed bag";case "tissue":return "Translucent tissue";case "photo":return "Cohort 19 / photograph";case "report":return "Ventilation study / sealed report";case "maintenance":return "Unfinished work order";case "fuel":return "Refill tin / empty";case "medical":return "First-aid incident card";case "cctv":return "East corridor / camera archive";case "files":return "Recovered access log";case "analysis":return "Sample comparison";case "flame":return "Classroom 03 / flame observation";default:return id;}
        }
        public static string Describe(string id,GameState s)
        {
            int a=Enumerable.Range(1,4).First(i=>s.people[i].infiltrator);int b=Enumerable.Range(5,3).First(i=>s.people[i].infiltrator);int c=Enumerable.Range(8,4).First(i=>s.people[i].infiltrator);
            switch(id)
            {
                case "attendance":return "Mara compared the signatures beside you. The two entries for "+Cast.All[b].name+" use different letter shapes and pen pressure. She remembers only one arrival. Her own alteration concerns her sister; it does not explain the duplicate badge in the access log.";
                case "recording":return "June recognised her brother's voice on the cassette: 'We asked to leave. Dr. Venn said the doors would open after the next reading.' A second voice counts nineteen people. June wrote down the time and kept the original tape. This corroborates the sealed study's account of continued exposure.";
                case "protocol":return "Ellis matched the bag's candle sleeve to the office reference batch. Both are thirty-second calibration candles. Use the office candle as a control, then observe the candle in Classroom 03. A prolonged flame is grounds to investigate the ventilation, not to accuse a frightened person.";
                case "key":return "Ada's brass key. STAFF OFFICE is punched into its label. The archive uses a separate electronic lock.";
                case "register":return "Twelve names. "+Cast.All[b].name+" signed at 08:52 and again at 09:07, in different handwriting. Mara has crossed out a thirteenth name. A duplicate signature might be a favour, or a mistake.";
                case "bag":return "A damp bag beneath a dry ceiling. The timetable belongs to a course discontinued twenty years ago. Inside: a candle sleeve marked CALIBRATION / 30 SECONDS. There are fingernail marks on the zip.";
                case "tissue":return "A translucent strip caught in the washbasin grille. One side carries a partial fingerprint; the other has a pattern like leaf veins. Samira may be able to examine it.";
                case "photo":return "Nineteen people outside this building. Venn is younger, but the expression is the same. A student has June's surname. On the reverse: DO NOT COUNT THE REFLECTIONS. 2006.";
                case "report":return "VENTILATION STUDY / CONTROL GROUP 19. The exposure continued after the subjects requested evacuation. Lead investigator: E. Venn. A handwritten amendment says: Heat was never the stimulus. Recognition was.\n\nA second page authorises tomorrow's intake.";
                case "maintenance":return "Damper E-03: accessible from Maintenance. Close only after locating the source. Manual operation requires a powered actuator OR the caretaker's brass override key. Contractor signature: Sol Mercer. Completion box left blank.";
                case "fuel":return "The last maintenance refill. You emptied it into your lighter. Fuel does not return when the hour changes.";
                case "medical":return "Ben Harlow: severe anxiety around exposed flame, documented three months ago. He has old burns. The card proves a history, not an identity.";
                case "cctv":return "09:02 / EAST CORRIDOR. "+Cast.All[a].name+" enters the stairwell. The same clothes pass the other way at 09:02:03, before the door has closed. The frame between them is missing.\n\n09:11 / CLASSROOM 03. Something crosses the top of the image. The timestamp restarts.";
                case "files":return "LOCAL ACCESS CACHE. "+Cast.All[b].name+" used the library reader at 09:01. At that time the room was sealed for fumigation. A second badge with the same ID entered the lecture room six seconds later.\n\nThe cache also lists "+Cast.All[a].name+" in the basement during the east-stairwell recording. Badge cloning remains possible.";
                case "analysis":return "The washbasin fragment carries a partial print matching the cup labelled "+Cast.All[c].name+". Its cells fold away from the slide lamp. The control sample does not.\n\nSamira's margin note: I cannot call this a diagnosis. Do not let them make me.";
                case "flame":return "The Classroom 03 calibration candle remained lit beyond thirty seconds. Scraping crossed the ceiling. A panel lifted from above. The reference candle in Staff Office extinguishes normally.\n\nThe extraction damper is in Maintenance.";
                default:return "No further details recorded.";
            }
        }
    }
}

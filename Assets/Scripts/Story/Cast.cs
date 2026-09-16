using UnityEngine;

namespace NoCauseForAlarm
{
    public class Person
    {
        public string name, role, room, alibi, secret, witness, fire, help;
        public Color color;
        public Person(string n, string r, string loc, string a, string s, string w, string f, string h, Color c)
        { name=n; role=r; room=loc; alibi=a; secret=s; witness=w; fire=f; help=h; color=c; }
    }
    public static class Cast
    {
        public static readonly Person[] All = {
            new Person("Dr. Elias Venn", "Behavioural science lecturer", "LECTURE 01", "I was preparing this room from eight. Ask Ada. She let me in.", "I taught the previous cohort. There were nineteen of us. The report says eighteen. I signed it anyway.", "Mara knows what happened to the records. Ask her about the attendance sheet.", "A recoil is not a verdict. Some people have good reasons to fear a flame.", "Find my old photograph and the sealed report. Then I will explain.", new Color(.29f,.32f,.27f)),
            new Person("Ruth Calder", "Campus security", "SECURITY", "I checked the east stairwell at nine. The camera has a blind spot. I reported it last term.", "I copied a master card. My daughter studies here. I needed a way back in.", "Sol was near the archive. His trolley was empty. He usually has something to complain about.", "Put it away. I am not filing another incident report.", "I can open the camera archive. A timestamp is evidence, not a conviction.", new Color(.18f,.25f,.32f)),
            new Person("Ada Moss", "Caretaker", "STORES", "I unlocked the lecture room for Venn at eight. He asked me to leave the ceiling hatch closed.", "I sleep in the store some nights. Rent went up. Please don't write that part down.", "Someone used my keys while I was in the bathroom. They returned them warm.", "That flame's too steady. The draught in here should have taken it.", "I have the brass key for Venn's office. You can borrow it.", new Color(.38f,.33f,.23f)),
            new Person("Iris Chen", "Engineering student", "UTILITY", "I was in the computer lab until the alarm. The lab clock is slow; I don't know by how much.", "I bridged a fuse last week. The sanctioned replacement never arrived. This failure might be mine.", "Noah asked me whether a backup camera keeps recording when the main power dies.", "Do you want to ignite the insulation? Lower it. Please.", "Give me an hour and a clear fuse cabinet. I can get the cameras back.", new Color(.51f,.37f,.22f)),
            new Person("Samira Bell", "Nursing student", "CLASSROOM 02", "I was helping Ben breathe. He had an episode before the doors locked.", "I am not qualified yet. Everyone keeps looking at me as though I am.", "Ben has old burn scars on his wrist. He was frightened before Venn lit anything.", "Keep it away from his sleeves. You are making him worse.", "Bring me the tissue from the bathroom. I can compare it with the first-aid sample.", new Color(.48f,.53f,.50f)),
            new Person("Noah Pike", "IT technician", "COMPUTER LAB", "I came through the library at nine. The reader was broken, so I used the rear entrance.", "I erased a disciplinary email. Not the footage. It was about a friend, not me.", "Mara asked me to remove a name from yesterday's attendance. She wouldn't say why.", "You're holding it too close to the equipment.", "The archive terminal still has a local cache. I can recover the deleted file.", new Color(.23f,.35f,.37f)),
            new Person("Mara Vale", "Class representative", "CAFETERIA", "I checked everyone in at the lecture. There should be twelve of us. I counted twice.", "I put my sister's name on the register. She dropped out in March. Our parents don't know.", "Venn recognised June before she introduced herself. June says they have never met.", "Can we discuss this like adults? That isn't a laboratory test.", "I'll cross-check the attendance register. There is a spare copy in Lecture 01.", new Color(.42f,.23f,.25f)),
            new Person("Ben Harlow", "First-year student", "CLASSROOM 02", "Samira found me in the second classroom. I couldn't get a full breath.", "There was a fire at home. I don't want to explain it again. Look at my wrist if you must.", "I heard a chair scrape upstairs. This is the top teaching floor, isn't it?",
                "No. No, put it out. Please. I asked you.", "I saw a ceiling tile move in Classroom 03. I left my bag there.", new Color(.29f,.36f,.44f)),
            new Person("June Park", "Visiting student", "CLASSROOM 03", "I waited here for a seminar. The room number on my timetable doesn't exist.", "I'm looking for my brother. He was in the cohort that the college stopped mentioning.", "Venn looks older in person than in the photograph. The photograph is twenty years old.", "It smells like the room my brother described.", "The envelope in Venn's office has my brother's handwriting.", new Color(.48f,.43f,.32f)),
            new Person("Len Ortiz", "Cafeteria worker", "CAFETERIA", "I was taking delivery at the service door. No one signed for it. There wasn't a driver.", "I took food home last night. If the stock numbers don't match, that is why.", "Ruth came by twice wearing different shoes. She said she'd been here all morning.", "I work with hot oil. A lighter isn't going to tell you anything about me.", "There's refill fuel in Stores. Only one tin left.", new Color(.57f,.53f,.39f)),
            new Person("Dr. Mara Ellis", "Chemistry lecturer", "STAFF OFFICE", "I was marking in the staff office. Venn called, asked if I still had the old protocol.", "We called it a ventilation study. The grant required fewer questions than it paid for.", "The sealed report is in the restricted archive. Venn's signature is on every page.", "Fear of fire is common. A reaction without a control proves very little.", "The reference candle is calibrated to thirty seconds. Compare it with the one in Classroom 03.", new Color(.34f,.31f,.38f)),
            new Person("Sol Mercer", "Maintenance contractor", "MAINTENANCE", "I checked the ductwork at eight. There wasn't enough clearance for anyone to get inside.", "I invoiced for a sealed hatch. I never sealed it. Someone told me to leave it accessible.", "Ada knows which breaker controls the old extraction fans. They don't appear on the new plans.", "Don't bring it under the vent. Just don't.", "Close the extraction damper in Maintenance after you find the source.", new Color(.40f,.40f,.29f))
        };
        public static readonly string[] Hours = {
            "11:00 | ATTENDANCE DISCREPANCY\nA bag has been found in Classroom 03. Its owner is absent from every register.",
            "12:00 | INTERIM REVIEW\nSecurity requests a name. You may detain someone, or keep investigating. Silence also has a cost.",
            "13:00 | MAINTENANCE NOTICE\nDo not investigate sounds above the suspended ceiling. Maintenance has already attended.",
            "14:00 | LOCAL POWER INTERRUPTION\nBackup lighting is operating normally. Cameras and the archive lock are offline.",
            "15:00 | MOVEMENT RESTRICTION\nAn occupant has failed to report. Travel with caution. Keep doors between you and anything following.",
            "16:00 | CORRECTION\nThe person who vouched for you cannot be located. Please disregard previous assurances.",
            "17:00 | FINAL REVIEW\nEvacuation is expected at eighteen hundred. Prepare a passenger manifest at the east exit.",
            "18:00 | TRANSPORT HAS ARRIVED\nThe bus has no interior lights. Decide who gets on."
        };
        public static string EndingText(string title)
        {
            switch (title)
            {
                case "THE LAST BUS": return "The people you chose sit apart. Nobody sleeps. At the district boundary, the driver asks whether this is everyone.\n\nYou say yes.\n\nBehind you, someone counts the seats again.";
                case "INFILTRATION": return "At the first stop, a passenger thanks you for believing them. Their reflection keeps looking forward after they turn.\n\nThe bus joins the evening traffic.\n\nThe city has a great many classrooms.";
                case "A CLEAN REGISTER": return "Every suspicious name is crossed out. So are several others. The bus is almost empty.\n\nYou remember a voice asking you to put the lighter away. You cannot remember whose.\n\nThe report calls your actions proportionate.";
                case "THE BUILDING REMAINS": return "The last door is locked. The passengers are accounted for. The campus will reopen on Monday.\n\nIn Classroom 03, a candle is still burning.\n\nAbove it, something learns the sound of the morning bell.";
                case "THE PREVIOUS COHORT": return "Venn stays by the sealed duct. Twenty years ago he chose a control group and left them inside. Today you refuse to leave him alone.\n\nJune's envelope contains nineteen names. You transmit every one.\n\nFor the first time, the announcement stops mid-sentence.";
                default: return "The exit opens before the checks are complete. A voice outside calls each survivor by name. Then it calls names no one has given it.\n\nThe doors close again.\n\nAt 18:01 the college records a successful evacuation.";
            }
        }
    }
}

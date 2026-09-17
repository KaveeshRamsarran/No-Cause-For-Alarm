using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoCauseForAlarm
{
    [Serializable] public class PersonState
    {
        public bool infiltrator, detained, missing, dead, tested, helped,helpRequested;
        public int trustTag, conversations;
        public List<string> observations = new List<string>();
    }
    [Serializable] public class GameState
    {
        public int version = 1, seed, hour = 10, actions = 4, mistakes;
        public float fuel = 100, health = 100;
        public bool power = true, repaired, ceilingDiscovered, ceilingSealed, archiveUnlocked, lecturerConfessed;
        public bool ended;
        public string ending = "";
        public PersonState[] people;
        public List<string> evidence = new List<string>();
        public List<string> journal = new List<string>();
        public List<string> inventory = new List<string>(),collected = new List<string>();
        public List<int> evacuation = new List<int>();
        public float px = -8, py = 1.05f, pz = 3, yaw = 180;

        public static GameState New(int seed)
        {
            var s = new GameState { seed = seed, people = new PersonState[12] };
            for (int i = 0; i < 12; i++) s.people[i] = new PersonState();
            // One organism in each social group; the lecturer and janitor are fixed humans.
            var rng = new System.Random(seed);
            int[][] groups = { new[] { 1, 3, 4 }, new[] { 5, 6, 7 }, new[] { 8, 9, 10, 11 } };
            foreach (var group in groups) s.people[group[rng.Next(group.Length)]].infiltrator = true;
            s.journal.Add("10:00 — Lockdown. Four consequential actions per hour. Walking, reading the notebook and brief questions are free.");
            return s;
        }
        public bool Available(int id) => !people[id].detained && !people[id].dead && !people[id].missing;
        public bool HumanHelper(int id) => Available(id) && !people[id].infiltrator;
        public bool Has(string id) => evidence.Contains(id);
        public bool AddEvidence(string id) { if (Has(id)) return false; evidence.Add(id); return true; }
        public bool Spend()
        {
            if (ended || hour >= 18) return false;
            actions--;
            if (actions > 0) return false;
            hour++; actions = hour < 18 ? 4 : 0;
            if (hour == 14 && !repaired) power = false;
            return true;
        }
        public void Accuse(int id)
        {
            if (!Available(id)) return;
            people[id].detained = true;
            if (!people[id].infiltrator) mistakes++;
        }
        public string ResolveEnding(bool stayWithLecturer)
        {
            int active = people.Count(p => p.infiltrator && !p.detained && !p.dead);
            if (health <= 0) return "OVERRUN";
            if (stayWithLecturer && lecturerConfessed && ceilingSealed && HumanHelper(0)) return "THE PREVIOUS COHORT";
            if (active >= 3) return "OVERRUN";
            if (evacuation.Any(i => i >= 0 && i < people.Length && Available(i) && people[i].infiltrator)) return "INFILTRATION";
            if (!ceilingSealed) return "THE BUILDING REMAINS";
            if (mistakes >= 3 || people.Count(p => !p.infiltrator && (p.dead || p.detained)) >= 3) return "A CLEAN REGISTER";
            return "THE LAST BUS";
        }
        public bool Valid()
        {
            return version == 1 && people != null && people.Length == 12 && people.All(p => p != null && p.observations != null)
                && evidence != null && journal != null && evacuation != null && hour >= 10 && hour <= 18 && actions >= 0 && actions <= 4;
        }
    }
    public static class SaveStore
    {
        public static string Path => System.IO.Path.Combine(Application.persistentDataPath,
            Environment.GetCommandLineArgs().Contains("-ncfa-art") ? "graphics-save.json" : Environment.GetCommandLineArgs().Contains("-ncfa-smoke") ? "smoke-save.json" : "cohort-save.json");
        public static void Save(GameState state)
        {
            var temp = Path + ".tmp";
            System.IO.File.WriteAllText(temp, JsonUtility.ToJson(state, true));
            if (System.IO.File.Exists(Path)) System.IO.File.Copy(Path, Path + ".bak", true);
            System.IO.File.Copy(temp, Path, true); System.IO.File.Delete(temp);
        }
        public static GameState Load()
        {
            try { var s = JsonUtility.FromJson<GameState>(System.IO.File.ReadAllText(Path));if(s!=null){s.inventory??=new List<string>();s.collected??=new List<string>();} return s != null && s.Valid() ? s : null; }
            catch (Exception) { return null; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PF2E_Creature_Maker
{
    public enum ResistOrWeak
    {
        Resistance,
        Weakness,
        Both,
        None
    }
    public class Creature
    {
        public int Level = 0;
        public string Name = "";
        public string Size = "";
        public int HitPoints = 0;
        public int Regeneration = 0;
        public int ResistWeakValue = 0;
        public ResistOrWeak ResistOrWeakType = ResistOrWeak.None;
        public SkillPool SkillPool = new SkillPool();
        public bool IsStrengthExtreme = false;
        public CreatureType Type = CreatureType.Any;
        public List<Degree> DegreeList;
        public TraitPool TraitPool = new TraitPool();
        public Dictionary<AbilityScoreEnum, int> AbilityScoreDictionary = new Dictionary<AbilityScoreEnum, int>
        {
            {AbilityScoreEnum.Strength, 0 },
            {AbilityScoreEnum.Dexterity, 0 },
            {AbilityScoreEnum.Intelligence, 0 },
            {AbilityScoreEnum.Wisdom, 0 },
            {AbilityScoreEnum.Charisma, 0 }
        };

        public Creature()
        {
            DegreeList = new List<Degree>() { Degree.bad };

            for (int i = 0; i < 5; i++)
            {
                DegreeList.Add(Degree.low);
            }
            for (int i = 0; i < 10; i++)
            {
                DegreeList.Add(Degree.moderate);
            }
            for (int i = 0; i < 5; i++)
            {
                DegreeList.Add(Degree.high);
            }
        }
    }
}

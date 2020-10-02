using System;
using System.Collections.Generic;
using System.Linq;

namespace PF2E_Creature_Maker
{
    public class Creature
    {
        public int Level = 0;
        public int SaveNumber = 0;
        public string Name = "";
        public string Size = "";
        public bool IsStrengthExtreme = false;
        public List<Degree> DegreeList;
        public AbilityScore[] AbilityScores;

        public Creature()
        {
            DegreeList = new List<Degree>() { Degree.bad };

            for (int i = 0; i < 5; i++)
            {
                DegreeList.Add(Degree.low);
            }
            for (int i = 0; i < 10; i++)
            {
                DegreeList.Add(Degree.mod);
            }
            for (int i = 0; i < 5; i++)
            {
                DegreeList.Add(Degree.high);
            }

            AbilityScores = new AbilityScore[5];

            for (int i = 0; i < AbilityScores.Length; i++)
            {
                AbilityScores[i] = new AbilityScore();
                AbilityScores[i]._abilityBonus = 0;
                string[] abilityNames = new string[] { "Strength", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
                AbilityScores[i]._abilityName = abilityNames[i];
            }
        }
    }
}

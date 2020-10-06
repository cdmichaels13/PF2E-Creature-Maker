﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PF2E_Creature_Maker
{
    public class Creature
    {
        public int Level = 0;
        public string Name = "";
        public string Size = "";
        public int HitPoints = 0;
        public int Regeneration = 0;
        public bool IsStrengthExtreme = false;
        public CreatureType Type = CreatureType.Any;
        public List<Degree> DegreeList;
        public TraitPool traitPool = new TraitPool();
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
                DegreeList.Add(Degree.mod);
            }
            for (int i = 0; i < 5; i++)
            {
                DegreeList.Add(Degree.high);
            }
        }

        public Degree SelectAndRemoveDegree(int index, Degree[] validDegrees)
        {
            Degree selectedDegree = DegreeList[index];
            DegreeList.Remove(selectedDegree);
            return selectedDegree;
        }
    }
}

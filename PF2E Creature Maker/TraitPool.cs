using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PF2E_Creature_Maker
{
    public class TraitPool
    {
        public List<string> AllPossibleTraits = new List<string>();
        public List<string> SelectedTraits = new List<string>();
        public List<int> HumanoidIndexes = new List<int>();
        public TraitPool()
        {
            string[] creatureTraits = File.ReadAllLines(Program.FilePathByName("Menu Page All Creature Traits"));
            string[] creatureTraitsWithoutHumanoidTag = new string[creatureTraits.Length];

            for (int i = 0; i < creatureTraits.Length; i++)
            {
                creatureTraitsWithoutHumanoidTag[i] = creatureTraits[i];
                if (creatureTraitsWithoutHumanoidTag[i].Contains(','))
                {
                    HumanoidIndexes.Add(i);
                    int commaIndex = creatureTraitsWithoutHumanoidTag[i].IndexOf(',');
                    creatureTraitsWithoutHumanoidTag[i] = creatureTraitsWithoutHumanoidTag[i].Substring(0, commaIndex);
                }
                AllPossibleTraits.Add(creatureTraitsWithoutHumanoidTag[i].Trim());
            }
        }
    }
}

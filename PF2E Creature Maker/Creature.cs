using System;
using System.Collections.Generic;
using System.Linq;

namespace PF2E_Creature_Maker
{
    public class Creature
    {
        public int _level = 0;
        public int _saveNumber = 0;
        public string _name = "";
        public string _size = "";
        public Step _lastSaved;
        public bool _isStrengthExtreme = false;
        public List<Degree> _degreeList;
        public AbilityScore[] _abilityScores;

        public Creature()
        {
            _degreeList = new List<Degree>() { Degree.bad };

            for (int i = 0; i < 5; i++)
            {
                _degreeList.Add(Degree.low);
            }
            for (int i = 0; i < 10; i++)
            {
                _degreeList.Add(Degree.mod);
            }
            for (int i = 0; i < 5; i++)
            {
                _degreeList.Add(Degree.high);
            }

            _abilityScores = new AbilityScore[5];

            for (int i = 0; i < _abilityScores.Length; i++)
            {
                _abilityScores[i] = new AbilityScore();
                _abilityScores[i]._abilityBonus = 0;
                string[] abilityNames = new string[] { "Strength", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
                _abilityScores[i]._abilityName = abilityNames[i];
            }
        }
    }
}

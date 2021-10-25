using System.Collections.Generic;

namespace PF2E_Creature_Maker
{
    public struct Item
    {
        public string name;
        public int itemLevel;
        public double price;
        public string rarity;
        public string traits;
        public string category;
        public string subcategory;
        public double bulk;
    }
    public struct Spell
    {
        public string name;
        public string description;
        public string school;
        public int level;
    }
    public enum ResistOrWeak
    {
        Resistance,
        Weakness,
        Both,
        None
    }

    public enum SaveName
    {
        Fortitude,
        Reflex,
        Will
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
        public Dictionary<SaveName, int> SavingThrows = new Dictionary<SaveName, int>
        {
            { SaveName.Fortitude, 0 },
            { SaveName.Reflex, 0 },
            { SaveName.Will, 0 }
        };
        public int ArmorClass = 0;
        public int StrikeAttack = 0;
        public string StrikeDamage = "";
        public int Perception = 0;
        public bool HasSpells = true;
        public List<Spell> Spells = new List<Spell>();
        public int SpellsDC = 0;
        public int SpellsAttackBonus = 0;
        public string[] AreaDamageValues = new string[2];
        public List<Item> Gear = new List<Item>();
        public int BulkLimit = 0;
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

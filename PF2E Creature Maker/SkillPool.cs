using System.Collections.Generic;
using System.IO;

namespace PF2E_Creature_Maker
{
    public struct Skill
    {
        public string skillName;
        public string skillBonus;
    }
    public class SkillPool
    {
        public List<string> PossibleSkills = new List<string>();
        public List<Skill> SelectedSkills = new List<Skill>();

        public SkillPool()
        {
            string[] skillFile = File.ReadAllLines(Program.FilePathByName("Menu Page Skills"));
            foreach (var line in skillFile)
            {
                string skillName = line.Substring(0, line.IndexOf(','));
                PossibleSkills.Add(skillName);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using Microsoft.VisualBasic.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace PF2E_Creature_Maker
{
    public enum Degree
    {
        bad,
        low,
        moderate,
        high,
        extreme
    }

    public enum MinMax
    {
        hasMin,
        hasMax,
        hasBoth
    }

    public enum Step
    {
        NPCorMonster,
        Level,
        Size,
        Traits,
        Hit_Points,
        Resistances_Weaknesses,
        Skills,
        Armor_Class,
        Strike_Attack_Bonus,
        Strike_Damage,
        Ability_Scores,
        Perception,
        Saves,
        Spells,
        Spell_DC_And_Attack_Bonus,
        Area_Damage,
        Gear,
        End,
        SaveToFile
    }
    
    public enum CreatureType
    {
        NPC,
        Monster,
        Any
    }

    public enum AbilityScoreEnum
    {
        Strength,
        Dexterity,
        Intelligence,
        Wisdom,
        Charisma
    }

    class Program
    {
        public static Creature _creature = new Creature();
        public static Dictionary<Step, Creature> _creatureSaves = new Dictionary<Step, Creature>();
        static void Main()
        {
            Random random = new Random();
            bool endSteps = false;

            Step[] stepOrder = new Step[]
            {
                Step.NPCorMonster,
                Step.Level,
                Step.Traits,
                Step.Size,
                Step.Hit_Points,
                Step.Resistances_Weaknesses,
                Step.Skills,
                Step.Armor_Class,
                Step.Strike_Attack_Bonus,
                Step.Strike_Damage,
                Step.Ability_Scores,
                Step.Perception,
                Step.Saves,
                Step.Spells,
                Step.Spell_DC_And_Attack_Bonus,
                Step.Area_Damage,
                Step.Gear,
                Step.End,
                Step.SaveToFile
            };

            int partyLevel = Program.GetMinMaxInt("Please enter the party's current level: ", MinMax.hasBoth, 1, 20);

            for (int step = 0; endSteps == false; step++)
            {
                if (step >= 0 && step < stepOrder.Length)
                {
                    Console.WriteLine("Step: " + stepOrder[step]);
                }

                if (step > 0)
                {
                    step = ContinueOrBack(stepOrder, step);
                }

                if (_creatureSaves.ContainsKey(stepOrder[step]))
                {
                    _creatureSaves.Remove(stepOrder[step]);
                }
                _creatureSaves.Add(stepOrder[step], CopyCreature(_creature));

                switch (stepOrder[step])
                {
                    case Step.NPCorMonster:
                        {
                            Console.WriteLine("Enter NPC or Monster to select a creature type, or press Enter to randomly select");
                            string userInput = GetValidString("NPC", "Monster", "");
                            switch(userInput.ToUpper())
                            {
                                case "NPC":
                                    _creature.Type = CreatureType.NPC;
                                    break;
                                case "Monster":
                                    _creature.Type = CreatureType.Monster;
                                    break;
                                case "":
                                    _creature.Type = CreatureType.Any;
                                    break;
                            }
                            break;
                        }
                    case Step.Level:
                        {
                            ExecuteStep.LevelStep(random, _creature, partyLevel);
                            break;
                        }
                    case Step.Traits:
                        {
                            string userInput = "";
                            do
                            {
                                ExecuteStep.TraitsStep(_creature, random);
                                Console.WriteLine("Press enter to continue or ADD to add another trait");
                                userInput = GetValidString("", "ADD");
                            } while (userInput.ToUpper() == "ADD");
                            break;
                        }
                    case Step.Size:
                        {
                            ExecuteStep.SizeStep(_creature, random);
                            break;
                        }
                    case Step.Hit_Points:
                        {
                            ExecuteStep.HitPointStep(_creature, random);
                            break;
                        }
                    case Step.Resistances_Weaknesses:
                        {
                            Console.WriteLine("Press Enter to continue or NO to skip this step");
                            string userInput = GetValidString("", "NO");
                            if (userInput.ToUpper() != "NO")
                            {
                                ExecuteStep.ResistanceWeaknessStep(_creature, random);
                            }
                            break;
                        }
                    case Step.Skills:
                        {
                            do
                            {
                                List<Degree> savedDegrees = CopyDegreeList(_creature.DegreeList);
                                ExecuteStep.SkillStep(_creature, random);
                                Console.WriteLine("Press ENTER to continue or BACK to erase previous skill");
                                string userInput = GetValidString("", "BACK");
                                if (userInput.ToUpper() == "BACK")
                                {
                                    int latestSkillIndex = _creature.SkillPool.SelectedSkills.Count() - 1;
                                    _creature.SkillPool.PossibleSkills.Add(_creature.SkillPool.SelectedSkills[latestSkillIndex].skillName);
                                    _creature.SkillPool.PossibleSkills.Sort();
                                    _creature.SkillPool.SelectedSkills.RemoveAt(latestSkillIndex);
                                    _creature.DegreeList = CopyDegreeList(savedDegrees);
                                }
                            } while (_creature.SkillPool.SelectedSkills.Count < 6);
                            break;
                        }
                    case Step.Armor_Class:
                        {
                            ExecuteStep.ArmorClassStep(_creature, random);
                            break;
                        }
                    case Step.Strike_Attack_Bonus:
                        {
                            ExecuteStep.StrikeAttackStep(_creature, random);
                            break;
                        }
                    case Step.Strike_Damage:
                        {
                            ExecuteStep.StrikeDamageStep(_creature, random);
                            break;
                        }
                    case Step.Ability_Scores:
                        {
                            AbilityScoreEnum[] abilities = new AbilityScoreEnum[] { AbilityScoreEnum.Strength, AbilityScoreEnum.Dexterity, AbilityScoreEnum.Intelligence, AbilityScoreEnum.Wisdom, AbilityScoreEnum.Charisma };
                            foreach (AbilityScoreEnum ability in abilities)
                            {
                                string userInput = "";
                                do
                                {
                                    List<Degree> saveDegrees = CopyDegreeList(_creature.DegreeList);
                                    ExecuteStep.AbilityScoreStep(_creature, random, ability);
                                    Console.WriteLine("Press ENTER to continue or BACK to redo the {0} score", ability);
                                    userInput = GetValidString("", "BACK");
                                    if (userInput.ToUpper() == "BACK")
                                    {
                                        _creature.DegreeList = CopyDegreeList(saveDegrees);
                                    }
                                } while (userInput.ToUpper() == "BACK");
                            }
                            break;
                        }
                    case Step.Perception:
                        {
                            ExecuteStep.PerceptionStep(_creature, random);
                            break;
                        }
                    case Step.Saves:
                        {
                            SaveName[] saves = new SaveName[] { SaveName.Fortitude, SaveName.Reflex, SaveName.Will };
                            foreach (SaveName save in saves)
                            {
                                ExecuteStep.SavesStep(_creature, random, save);
                            }
                            break;
                        }
                    case Step.Spells:
                        {
                            string[] spellTypes = new string[] { "Arcane", "Occult", "Primal", "Divine", "Focus", "NONE" };
                            Console.WriteLine("Press Enter to randomly select this creature's magic type or one of the following types:");
                            foreach (string type in spellTypes)
                            {
                                Console.WriteLine(type);
                            }
                            string[] validInputs = new string[spellTypes.Length + 1];
                            for (int i = 0; i < validInputs.Length; i++)
                            {
                                if (i == 0)

                                {
                                    validInputs[i] = "";
                                }
                                else
                                {
                                    validInputs[i] = spellTypes[i - 1];
                                }
                            }
                            string typeInput = GetValidString(validInputs);
                            if (typeInput == "")
                            {
                                if (random.Next(2) == 1)
                                {
                                    typeInput = spellTypes[random.Next(spellTypes.Length)];
                                }
                                else
                                {
                                    _creature.HasSpells = false;
                                }
                            }
                            if (typeInput.ToUpper() == "NONE")
                            {
                                _creature.HasSpells = false;
                            }
                            if (_creature.HasSpells)
                            {
                                ExecuteStep.SpellsStep(_creature, random, CapitalizeString(typeInput));
                            }
                            break;
                        }
                    case Step.Spell_DC_And_Attack_Bonus:
                        {
                            if (!_creature.Spells.Any())
                            {
                                Console.WriteLine("No spells detected. Skipping step");
                            }
                            else
                            {
                                ExecuteStep.SpellStatsStep(_creature, random);
                            }
                            break;
                        }
                    case Step.Area_Damage:
                        {
                            ExecuteStep.AreaDamageStep(_creature);
                            break;
                        }
                    case Step.Gear:
                        {
                            Console.WriteLine("Press Enter to randomly determine if creature has gear or Y\\N:");
                            string userInput = GetValidString("", "Y", "N");
                            if (userInput == "" && random.Next(2) == 1)
                            {
                                userInput = "Y";
                            }
                            if (userInput.ToUpper() == "Y")
                            {
                                ExecuteStep.GearStep(_creature, random);
                            }
                            break;
                        }
                    case Step.End:
                        {
                            ExecuteStep.EndStep(_creature);
                            break;
                        }
                    case Step.SaveToFile:
                        {
                            Console.WriteLine("Would you like to save this creature to desktop? Y/N");
                            string saveOrNot = GetValidString("Y", "N");
                            if (saveOrNot.ToUpper() == "Y")
                            {
                                ExecuteStep.SaveToFileStep(_creature);
                            }
                            endSteps = true;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Step incomplete");
                            //endSteps = true;
                            break;
                        }
                }
            }
        }

        #region Method Region
        public static string FilePathByName(string fileName)
        {
            return @"Data Files\" + fileName + ".txt";
        }

        public static string CorrectedDash(string numberString)
        {
            return numberString.Replace('–', '-');
        }
        
        public static Creature CopyCreature(Creature creature)
        {
            Creature creatureCopy = new Creature
            {
                Level = creature.Level,
                Name = creature.Name,
                Size = creature.Size,
                HitPoints = creature.HitPoints,
                Regeneration = creature.Regeneration,
                ResistOrWeakType = creature.ResistOrWeakType,
                ArmorClass = creature.ArmorClass,
                StrikeAttack = creature.StrikeAttack,
                StrikeDamage = creature.StrikeDamage,
                Perception = creature.Perception,
                HasSpells = creature.HasSpells,
                SpellsDC = creature.SpellsDC,
                SpellsAttackBonus = creature.SpellsAttackBonus,
                IsStrengthExtreme = creature.IsStrengthExtreme,
                Type = creature.Type,
                DegreeList = CopyDegreeList(creature.DegreeList)
            };

            foreach (AbilityScoreEnum ability in creature.AbilityScoreDictionary.Keys)
            {
                creatureCopy.AbilityScoreDictionary[ability] = creature.AbilityScoreDictionary[ability];
            }

            foreach (SaveName save in creature.SavingThrows.Keys)
            {
                creatureCopy.SavingThrows[save] = creature.SavingThrows[save];
            }

            creatureCopy.TraitPool.AllPossibleTraits.Clear();
            creatureCopy.TraitPool.SelectedTraits.Clear();
            creatureCopy.TraitPool.HumanoidIndexes.Clear();

            foreach (string trait in creature.TraitPool.AllPossibleTraits)
            {
                creatureCopy.TraitPool.AllPossibleTraits.Add(trait);
            }
            foreach (string trait in creature.TraitPool.SelectedTraits)
            {
                creatureCopy.TraitPool.SelectedTraits.Add(trait);
            }
            foreach (int index in creature.TraitPool.HumanoidIndexes)
            {
                creatureCopy.TraitPool.HumanoidIndexes.Add(index);
            }

            creatureCopy.SkillPool.PossibleSkills.Clear();
            creatureCopy.SkillPool.SelectedSkills.Clear();

            foreach (string skill in creature.SkillPool.PossibleSkills)
            {
                creatureCopy.SkillPool.PossibleSkills.Add(skill);
            }
            foreach (Skill skill in creature.SkillPool.SelectedSkills)
            {
                creatureCopy.SkillPool.SelectedSkills.Add(skill);
            }

            foreach (Spell spell in creature.Spells)
            {
                creatureCopy.Spells.Add(spell);
            }

            for (int i = 0; i < creature.AreaDamageValues.Length; i++)
            {
                creatureCopy.AreaDamageValues[i] = creature.AreaDamageValues[i];
            }

            foreach (Item item in creature.Gear)
            {
                creatureCopy.Gear.Add(item);
            }

            return creatureCopy;
        }

        public static int WeightedRandom(Random random, int min, int max, int heavyValue, int weight)
        {
            int[] randoms = new int[weight + 1];

            for (int i = 0; i < randoms.Length; i++)
            {
                randoms[i] = random.Next(min, max + 1);
            }

            int difference = 0;

            do
            {
                foreach (int randValue in randoms)
                {
                    if (randValue == heavyValue - difference || randValue == heavyValue + difference)
                    {
                        return randValue;
                    }
                }
                difference++;
            } while (true);
        }

        public static int ContinueOrBack(Step[] stepOrder, int step)
        {
            string userInput;
            do
            {
                Console.WriteLine("Press Enter to continue or BACK to go back a step");
                userInput = GetValidString("", "BACK");
                if (userInput.ToUpper() == "BACK")
                {
                    if (step == 0)
                    {
                        Console.WriteLine("Cannot go back any further");
                    }
                    else
                    {
                        step -= 1;
                        _creature = CopyCreature(_creatureSaves[stepOrder[step]]);
                        Console.WriteLine("Returning to {0} Step", stepOrder[step]);
                    }
                }
            } while (userInput.ToUpper() == "BACK");

            return step;
        }

        public static List<Degree> CopyDegreeList(List<Degree> copiedList)
        {
            List<Degree> returningList = new List<Degree>();

            for (int i = 0; i < copiedList.Count(); i++)
            {
                returningList.Add(copiedList[i]);
            }

            return returningList;
        }

        public static string GetValidString(params string[] valids)
        {
            string userInput = "";
            bool passable = false;

            do
            {
                passable = false;
                userInput = Console.ReadLine();

                foreach (string validString in valids)
                {
                    if (userInput.ToLower() == validString.ToLower())
                    {
                        passable = true;
                        break;
                    }
                }

                if (passable == false)
                {
                    Console.WriteLine("Your entry was not valid. Please try again");
                }

            } while (passable == false);

            return userInput;
        }

        public static int DisadvantageRandom(Random random, int min, int max)
        {
            int[] randomInts = new int[2];
            for (int i = 0; i < randomInts.Length; i++)
            {
                randomInts[i] = random.Next(min, max);
            }

            return randomInts.Min();
        }

        public static int PinkRandom(Random random, int min, int max)
        {
            int[] randomInts = new int[2];
            for (int i = 0; i < randomInts.Length; i++)
            {
                randomInts[i] = random.Next(min, max + 1);
            }

            double pinkDouble = Convert.ToDouble(randomInts.Sum()) / Convert.ToDouble(randomInts.Length);

            return Convert.ToInt32(pinkDouble);
        }

        public static int GetMinMaxInt(string prompt, MinMax minMax, params int[] minMaxes)
        {
            bool passable = true;
            int userInt = 0;

            if (minMaxes.Length >= 1 || minMaxes.Length <= 2)
            {
                do
                {
                    passable = true;
                    userInt = GetUserInt(prompt);

                    if (minMax == MinMax.hasMin)
                    {
                        if (userInt < minMaxes[0])
                        {
                            passable = false;
                            Console.WriteLine("Value too low");
                        }
                    }
                    if (minMax == MinMax.hasMax)
                    {
                        if (userInt > minMaxes[0])
                        {
                            passable = false;
                            Console.WriteLine("Value too high");
                        }
                    }
                    if (minMax == MinMax.hasBoth)
                    {
                        if (userInt < minMaxes[0] || userInt > minMaxes[1])
                        {
                            passable = false;
                            Console.WriteLine("Value out of bounds");
                        }
                    }
                } while (passable == false);
            }
            else
            {
                Console.WriteLine("Tried to GetMinMaxInt with either no mins/maxes or more than 1 min and 1 max");
            }

            return userInt;
        }

        public static int GetUserInt(string prompt)
        {
            int userInt = 0;
            string userInput = "";

            do
            {
                Console.WriteLine(prompt);
                userInput = Console.ReadLine();

                try
                {
                    userInt = int.Parse(userInput);
                    return userInt;
                }
                catch
                {
                    Console.WriteLine("Invalid input. Please try again");
                }
            } while (true);
        }

        public static string CapitalizeString(string stringToCapitalize)
        {
            return stringToCapitalize.ToUpper()[0] + stringToCapitalize.ToLower().Substring(1);
        }

        #endregion
    }

}

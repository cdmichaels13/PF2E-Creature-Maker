using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PF2E_Creature_Maker
{
    public class ExecuteStep
    {
        public static void LevelStep(Random random, Creature creature, int partyLevel)
        {
            string userInput;
            bool passable;
            int creatureLevel = 0;

            do
            {
                userInput = "";
                Console.WriteLine("Enter the creature's level or enter PARTY to randomly select a level based on the party's level" +
                    "\nJust press Enter to randomly select level based on whether the creature is an NPC or a Monster");
                do
                {
                    string levelSelectionStyle = Console.ReadLine();
                    passable = true;
                    switch (levelSelectionStyle.ToUpper())
                    {
                        case "PARTY":
                            creatureLevel = Program.PinkRandom(random, partyLevel - 7, partyLevel + 7);
                            break;
                        case "":
                            if (creature.Type == CreatureType.NPC)
                            {
                                creatureLevel = Program.DisadvantageRandom(random, -1, 11);
                            }
                            else
                            {
                                creatureLevel = Program.DisadvantageRandom(random, -1, 25);
                            }
                            break;
                        default:
                            try
                            {
                                creatureLevel = int.Parse(levelSelectionStyle);
                            }
                            catch
                            {
                                passable = false;
                                Console.WriteLine("Invalid entry");
                            }
                            break;
                    }
                } while (passable == false);

                if (creatureLevel < partyLevel - 7 || creatureLevel > partyLevel + 7)
                {
                    Console.WriteLine("Creature level is {0}, which is out of normal PC encounter range. Proceed anyway? Y/N", creatureLevel);
                    userInput = Program.GetValidString("Y", "N").ToUpper();
                }
            } while (userInput == "N");

            Console.WriteLine("Creature Level: " + creatureLevel);
            creature.Level = creatureLevel;

            for (int i = 11; i <= creature.Level; i++)
            {
                int[] extremeLevels = new int[] { 11, 15, 20 };
                if (extremeLevels.Contains(i))
                {
                    int removingDegree;
                    do
                    {
                        removingDegree = random.Next(0, creature.DegreeList.Count);
                    } while (creature.DegreeList[removingDegree] == Degree.extreme);

                    Console.WriteLine("Since this creature is at least Level {0}, replacing a {1} degree with extreme", i, creature.DegreeList[removingDegree]);

                    creature.DegreeList.RemoveAt(removingDegree);
                    creature.DegreeList.Add(Degree.extreme);
                }
            }
        }
        public static void SizeStep(Creature creature, Random random)
        {
            string[] sizeOptions = File.ReadAllLines(Program.FilePathByName("Menu Page Size"));
            List<string[]> sizeOptionsSplit = new List<string[]>();

            foreach (string sizeOption in sizeOptions)
            {
                sizeOptionsSplit.Add(sizeOption.Split(';'));
            }

            SizeOption[] sizeOptionsArray = new SizeOption[sizeOptions.Length];
            int mediumSizeIndex = 0;
            for (int i = 0; i < sizeOptionsArray.Length; i++)
            {
                sizeOptionsArray[i] = new SizeOption
                {
                    _name = sizeOptionsSplit[i][0],
                    _minLevel = int.Parse(sizeOptionsSplit[i][1])
                };
                if (sizeOptionsSplit[i][2] != "" && sizeOptionsSplit[i][2] != null)
                {
                    sizeOptionsArray[i]._maxExStrForSizeAndLevel = int.Parse(sizeOptionsSplit[i][2]);
                }
                if (sizeOptionsArray[i]._name == "Medium")
                {
                    mediumSizeIndex = i;
                }
            }

            Console.WriteLine("Please type one of the following sizes or press Enter to randomly pick: ");
            foreach (SizeOption option in sizeOptionsArray)
            {
                Console.WriteLine(option._name + " ");
            }

            string[] validSizeOptions = new string[sizeOptionsArray.Length + 1];

            for (int i = 0; i < validSizeOptions.Length; i++)
            {
                if (i == validSizeOptions.Length - 1)
                {
                    validSizeOptions[i] = "";
                }
                else
                {
                    validSizeOptions[i] = sizeOptionsArray[i]._name;
                }
            }

            string userInput;
            bool tooLarge;
            do
            {
                tooLarge = false;
                userInput = Program.GetValidString(validSizeOptions);

                if (userInput == "")
                {
                    SizeOption randomSize;
                    do
                    {
                        randomSize = sizeOptionsArray[Program.WeightedRandom(random, 0, sizeOptionsArray.Length - 1, mediumSizeIndex, weight: 2)];
                    } while (creature.Level < randomSize._minLevel);
                    creature.Size = randomSize._name;
                }
                else
                {
                    for (int i = 0; i < sizeOptionsArray.Length; i++)
                    {
                        if (userInput.ToLower() == sizeOptionsArray[i]._name.ToLower())
                        {
                            if (creature.Level < sizeOptionsArray[i]._minLevel)
                            {
                                Console.WriteLine("Creatures of this level aren't usually so large. Are you sure? Y/N");
                                userInput = Program.GetValidString("Y", "N");
                                if (userInput.ToUpper() == "N")
                                {
                                    tooLarge = true;
                                    Console.WriteLine("Please enter a smaller size: ");
                                }
                            }
                            else
                            {
                                creature.Size = sizeOptionsArray[i]._name;
                            }
                        }
                    }
                }
            } while (tooLarge == true);

            Console.WriteLine("Creature Size: " + creature.Size);

            for (int i = 0; i < sizeOptionsArray.Length; i++)
            {
                if (sizeOptionsArray[i]._name.ToLower() == creature.Size.ToLower())
                {
                    if (creature.Level <= sizeOptionsArray[i]._maxExStrForSizeAndLevel)
                    {
                        creature.IsStrengthExtreme = true;
                    }
                    break;
                }
            }

            Console.WriteLine("Creature's Strength is Extreme: " + creature.IsStrengthExtreme);
        }
        public static void TraitsStep(Creature creature, Random random)
        {
            if (creature.Type == CreatureType.NPC)
            {
                for (int i = creature.TraitPool.AllPossibleTraits.Count - 1; i >= 0; i--)
                {
                    if (!creature.TraitPool.HumanoidIndexes.Contains(i))
                    {
                        creature.TraitPool.AllPossibleTraits.RemoveAt(i);
                    }
                }
            }
            if (creature.Type == CreatureType.Monster)
            {
                for (int i = creature.TraitPool.AllPossibleTraits.Count - 1; i >= 0; i--)
                {
                    if (creature.TraitPool.HumanoidIndexes.Contains(i))
                    {
                        creature.TraitPool.AllPossibleTraits.RemoveAt(i);
                    }
                }
            }

            Console.WriteLine("Press Enter to randomly select trait, or enter SELECT");
            string userInput = Program.GetValidString("SELECT", "");

            string selectedTrait = "";

            switch (userInput.ToUpper())
            {
                case "":
                    {
                        int traitIndex = random.Next(creature.TraitPool.AllPossibleTraits.Count);
                        selectedTrait = creature.TraitPool.AllPossibleTraits[traitIndex];
                        break;
                    }
                case "SELECT":
                    {
                        Console.WriteLine("Please enter a trait from the list below:");
                        string[] validTraits = creature.TraitPool.AllPossibleTraits.ToArray();
                        foreach (string trait in validTraits)
                        {
                            Console.WriteLine(trait);
                        }
                        selectedTrait = Program.GetValidString(validTraits);
                        break;
                    }
                default:
                    Console.WriteLine("No correct user input detected");
                    break;
            }

            List<string> queuedTraits = new List<string>();

            do
            {
                if (queuedTraits.Any())
                {
                    selectedTrait = queuedTraits.First();
                    queuedTraits.RemoveAt(0);
                }

                selectedTrait = Program.CapitalizeString(selectedTrait.Trim());

                string[] traitPackages = File.ReadAllLines(Program.FilePathByName("Trait Packages"));
                string traitRuleIfExists = "";
                foreach (string traitLine in traitPackages)
                {
                    string traitName = traitLine.Substring(0, traitLine.IndexOf(':')).Trim();
                    string traitRule = traitLine.Substring(traitLine.IndexOf(':') + 2).Trim();
                    if (Program.CapitalizeString(traitName) == selectedTrait)
                    {
                        traitRuleIfExists = traitRule;
                        break;
                    }
                }

                if (traitRuleIfExists != "")
                {
                    creature.TraitPool.SelectedTraits.Add(selectedTrait + ": " + traitRuleIfExists);
                }
                else
                {
                    creature.TraitPool.SelectedTraits.Add(selectedTrait);
                }

                if (creature.TraitPool.AllPossibleTraits.Contains(selectedTrait))
                {
                    creature.TraitPool.AllPossibleTraits.Remove(selectedTrait);
                }

                foreach (string trait in creature.TraitPool.AllPossibleTraits)
                {
                    if (traitRuleIfExists != "" && traitRuleIfExists.ToLower().Contains(trait.ToLower() + " trait"))
                    {
                        queuedTraits.Add(trait);
                    }
                }

                Console.WriteLine(creature.TraitPool.SelectedTraits.Last());
            } while (queuedTraits.Any());

        }
        public static void HitPointStep(Creature creature, Random random)
        {
            Console.WriteLine("Should creature have regeneration? Y/N, or Enter to pick randomly");
            string userInput = Program.GetValidString("Y", "N", "");
            if (userInput == "" && random.Next(10) == 0)
            {
                userInput = "Y";
            }
            if (userInput.ToUpper() == "Y")
            {
                string[] strikeDamageFileLines = File.ReadAllLines(Program.FilePathByName("Strike Damage"));
                foreach (string line in strikeDamageFileLines)
                {
                    string[] splitLine = line.Split(',');
                    if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                    {
                        int[] parenthesesIndexes = new int[] { splitLine[2].IndexOf('('), splitLine[2].IndexOf(')') };
                        creature.Regeneration = int.Parse(splitLine[2].Substring(parenthesesIndexes[0] + 1, parenthesesIndexes[1] - parenthesesIndexes[0] - 1));
                        Console.WriteLine("Regeneration: " + creature.Regeneration);
                    }
                }
            }

            string[] hitPointsTable = File.ReadAllLines(Program.FilePathByName("Hit Points"));
            foreach (string line in hitPointsTable)
            {
                string[] splitLine = line.Split(',');
                if (creature.Level == int.Parse(splitLine[0]))
                {
                    Dictionary<Degree, string> lineDegreeValues = new Dictionary<Degree, string>
                    {
                        {Degree.high, splitLine[1] },
                        {Degree.moderate, splitLine[2] },
                        {Degree.low, splitLine[3] }
                    };

                    Degree chosenDegree;

                    do
                    {
                        Console.WriteLine("Press Enter to randomly determine HP or enter LOW, MOD, or HIGH");
                        string hpDegreeInput = Program.GetValidString("", "LOW", "MOD", "HIGH");
                        switch (hpDegreeInput.ToUpper())
                        {
                            case "LOW":
                                {
                                    chosenDegree = Degree.low;
                                    break;
                                }
                            case "MOD":
                                {
                                    chosenDegree = Degree.moderate;
                                    break;
                                }
                            case "HIGH":
                                {
                                    chosenDegree = Degree.high;
                                    break;
                                }
                            case "":
                                {
                                    do
                                    {
                                        chosenDegree = creature.DegreeList.ElementAt(random.Next(creature.DegreeList.Count));
                                    } while (!lineDegreeValues.ContainsKey(chosenDegree) || !creature.DegreeList.Contains(chosenDegree));
                                    Console.WriteLine("Random degree: " + chosenDegree);
                                    break;
                                }
                            default:
                                Console.WriteLine("Broken at hpDegreeInput interpretation, continued with Degree.mod");
                                chosenDegree = Degree.moderate;
                                break;
                        }
                    } while (DegreeIsMissing(creature, chosenDegree));

                    creature.DegreeList.Remove(chosenDegree);

                    string hpRange = lineDegreeValues[chosenDegree];
                    if (hpRange.Contains('-'))
                    {
                        string[] hpRangeSplit = hpRange.Split('-');
                        int[] minMaxHP = new int[] { int.Parse(hpRangeSplit[1]), int.Parse(hpRangeSplit[0]) };
                        creature.HitPoints = Program.PinkRandom(random, minMaxHP[0], minMaxHP[1]);
                    }
                    else
                    {
                        creature.HitPoints = int.Parse(hpRange);
                    }

                    creature.HitPoints -= (creature.Regeneration * 2);
                    Console.WriteLine("Hit Points: " + creature.HitPoints);

                    break;
                }
            }
        }

        public static void ResistanceWeaknessStep(Creature creature, Random random)
        {
            Degree selectedDegree;
            Degree[] acceptableDegrees = new Degree[] { Degree.low, Degree.moderate, Degree.high };

            Console.WriteLine("Press Enter to randomly determine Resistance/Weakness values or RESISTANCE, WEAKNESS, or BOTH");

            do
            {
                string degreeInput = Program.GetValidString("", "RESISTANCE", "WEAKNESS", "BOTH");
                switch (degreeInput.ToUpper())
                {
                    case "WEAKNESS":
                        {
                            selectedDegree = Degree.low;
                            break;
                        }
                    case "BOTH":
                        {
                            selectedDegree = Degree.moderate;
                            break;
                        }
                    case "RESISTANCE":
                        {
                            selectedDegree = Degree.high;
                            break;
                        }
                    case "":
                        {
                            do
                            {
                                selectedDegree = creature.DegreeList[random.Next(creature.DegreeList.Count)];
                            } while (!acceptableDegrees.Contains(selectedDegree) || !creature.DegreeList.Contains(selectedDegree));
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("degreeInput broken");
                            selectedDegree = Degree.moderate;
                            break;
                        }
                }
            } while (DegreeIsMissing(creature, selectedDegree));
            
            creature.DegreeList.Remove(selectedDegree);

            switch(selectedDegree)
            {
                case Degree.low:
                    {
                        creature.ResistOrWeakType = ResistOrWeak.Weakness;
                        break;
                    }
                case Degree.moderate:
                    {
                        creature.ResistOrWeakType = ResistOrWeak.Both;
                        break;
                    }
                case Degree.high:
                    {
                        creature.ResistOrWeakType = ResistOrWeak.Resistance;
                        break;
                    }
            }

            string[] resistWeaknessFile = File.ReadAllLines(Program.FilePathByName("Weakness-Resistance"));
            foreach (string line in resistWeaknessFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int maxValue = int.Parse(splitLine[1]);
                    int minValue = int.Parse(splitLine[2]);
                    creature.ResistWeakValue = Program.PinkRandom(random, minValue, maxValue);
                    break;
                }
            }

            if (creature.ResistOrWeakType == ResistOrWeak.None)
            {
                Console.WriteLine("No Resistances or Weaknesses");
            }
            else
            {
                Console.WriteLine(creature.ResistOrWeakType + ": " + creature.ResistWeakValue);
            }
        }

        public static void SkillStep(Creature creature, Random random)
        {
            Skill skillStruct = new Skill();
            Console.WriteLine("Press Enter to randomly select a skill or type one of the following skills:");
            string[] validSkills = new string[creature.SkillPool.PossibleSkills.Count];
            for (int i = 0; i < validSkills.Length; i++)
            {
                validSkills[i] = creature.SkillPool.PossibleSkills[i];
                Console.WriteLine(validSkills[i]);
            }
            string userInput = Console.ReadLine();
            string selectedSkill;
            if (userInput == "")
            {
                selectedSkill = validSkills[random.Next(validSkills.Length)];
                Console.WriteLine("Random skill: " + Program.CapitalizeString(selectedSkill));
            }
            else
            {
                selectedSkill = Program.GetValidString(validSkills);
            }

            foreach (string skill in creature.SkillPool.PossibleSkills)
            {
                if (skill.ToLower().Contains(selectedSkill.ToLower()))
                {
                    creature.SkillPool.PossibleSkills.Remove(skill);
                    break;
                }
            }

            skillStruct.skillName = Program.CapitalizeString(selectedSkill);

            Degree[] validDegrees = new Degree[] { Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            Console.WriteLine("Press Enter to randomly select degree of skill bonus or LOW, MOD, HIGH, or EX");
            string[] validDegreePlusEnter = new string[] { "LOW", "MOD", "HIGH", "EX", "" };
            Degree selectedDegree = Degree.moderate;

            do
            {
                userInput = Program.GetValidString(validDegreePlusEnter);
                if (userInput == "")
                {
                    do
                    {
                        selectedDegree = validDegrees[random.Next(validDegrees.Length)];
                    } while (!creature.DegreeList.Contains(selectedDegree));
                    Console.WriteLine("Random degree: " + selectedDegree);
                }
                else
                {
                    switch (userInput.ToUpper())
                    {
                        case "LOW":
                            {
                                selectedDegree = Degree.low;
                                break;
                            }
                        case "MOD":
                            {
                                selectedDegree = Degree.moderate;
                                break;
                            }
                        case "HIGH":
                            {
                                selectedDegree = Degree.high;
                                break;
                            }
                        case "EX":
                            {
                                selectedDegree = Degree.extreme;
                                break;
                            }
                    }
                }
            } while (DegreeIsMissing(creature, selectedDegree));

            creature.DegreeList.Remove(selectedDegree);

            string[] skillBonusFile = File.ReadAllLines(Program.FilePathByName("Skill Bonuses"));
            foreach (string line in skillBonusFile)
            {
                string[] splitLine = line.Split(',');
                int[] splitLineParsed = new int[splitLine.Length];
                for (int i = 0; i < splitLineParsed.Length; i++)
                {
                    splitLineParsed[i] = int.Parse(Program.CorrectedDash(splitLine[i]));
                }

                if (splitLineParsed[0] == creature.Level)
                {
                    int bonusIndex = 0;
                    switch(selectedDegree)
                    {
                        case Degree.low:
                            {
                                bonusIndex = 4;
                                break;
                            }
                        case Degree.moderate:
                            {
                                bonusIndex = 3;
                                break;
                            }
                        case Degree.high:
                            {
                                bonusIndex = 2;
                                break;
                            }
                        case Degree.extreme:
                            {
                                bonusIndex = 1;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Couldn't match selectedDegree to a corresponding index");
                                break;
                            }
                    }

                    int selectedBonus = splitLineParsed[bonusIndex] - creature.Level;

                    if (selectedBonus > 0)
                    {
                        skillStruct.skillBonus = "+";
                    }
                     
                    skillStruct.skillBonus += selectedBonus;

                    creature.SkillPool.SelectedSkills.Add(skillStruct);
                    Console.WriteLine("Bonus: " + creature.SkillPool.SelectedSkills.Last().skillBonus);

                    break;
                }
            }
        }

        public static void ArmorClassStep(Creature creature, Random random)
        {
            Console.WriteLine("Press Enter to randomly determine AC or LOW, MOD, HIGH, EX");
            Degree chosenDegree = Degree.moderate;
            Degree[] validDegrees = new Degree[] { Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            string armorInput;
            do
            {
                armorInput = Program.GetValidString("", "LOW", "MOD", "HIGH", "EX");
                switch (armorInput.ToUpper())
                {
                    case "":
                        {
                            do
                            {
                                chosenDegree = validDegrees[random.Next(validDegrees.Length)];
                            } while (!creature.DegreeList.Contains(chosenDegree));
                            break;
                        }
                    case "LOW":
                        {
                            chosenDegree = Degree.low;
                            break;
                        }
                    case "MOD":
                        {
                            chosenDegree = Degree.moderate;
                            break;
                        }
                    case "HIGH":
                        {
                            chosenDegree = Degree.high;
                            break;
                        }
                    case "EX":
                        {
                            chosenDegree = Degree.extreme;
                            break;
                        }
                }
            } while (DegreeIsMissing(creature, chosenDegree));

            creature.DegreeList.Remove(chosenDegree);

            string[] armorFile = File.ReadAllLines(Program.FilePathByName("Armor Class"));
            foreach (string line in armorFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch(chosenDegree)
                    {
                        case Degree.low:
                            {
                                degreeIndex = 4;
                                break;
                            }
                        case Degree.moderate:
                            {
                                degreeIndex = 3;
                                break;
                            }
                        case Degree.high:
                            {
                                degreeIndex = 2;
                                break;
                            }
                        case Degree.extreme:
                            {
                                degreeIndex = 1;
                                break;
                            }
                    }
                    creature.ArmorClass = int.Parse(Program.CorrectedDash(splitLine[degreeIndex])) - creature.Level;
                    Console.WriteLine("{0} Armor Class: {1}", chosenDegree, creature.ArmorClass);
                    break;
                }
            }
        }

        public static void EndStep(Creature creature)
        {
            Console.WriteLine("__Final Creature__" +
                "\nName: " + creature.Name + 
                "\nLevel: " + creature.Level +
                "\nSize: " + creature.Size + 
                "\nHit Points: " + creature.HitPoints);
            if (creature.Regeneration != 0)
            {
                Console.WriteLine("Regeneration: " + creature.Regeneration);
            }

            if (creature.ResistOrWeakType == ResistOrWeak.Resistance || creature.ResistOrWeakType == ResistOrWeak.Both)
            {
                Console.WriteLine("Resistance: " + creature.ResistWeakValue);
            }
            if (creature.ResistOrWeakType == ResistOrWeak.Weakness || creature.ResistOrWeakType == ResistOrWeak.Both)
            {
                Console.WriteLine("Weakness: " + creature.ResistWeakValue);
            }

            Console.WriteLine("Armor Class: " + creature.ArmorClass);

            Console.WriteLine("\n_Skills_");
            foreach (Skill skill in creature.SkillPool.SelectedSkills)
            {
                Console.Write("{0}: {1}  ", skill.skillName, skill.skillBonus);
            }
            Console.WriteLine();

            foreach (KeyValuePair<AbilityScoreEnum, int> ability in creature.AbilityScoreDictionary)
            {
                Console.Write(ability.Key + " = " + ability.Value + "  ");
                if (ability.Key == creature.AbilityScoreDictionary.Last().Key)
                {
                    Console.WriteLine("");
                }
            }

            Console.WriteLine("\n_Traits_");
            foreach (string trait in creature.TraitPool.SelectedTraits)
            {
                Console.WriteLine(trait);
            }
        }

        public static bool DegreeIsMissing(Creature creature, Degree degree)
        {
            bool degreeMissing = false;

            if (!creature.DegreeList.Contains(degree))
            {
                degreeMissing = true;
                Console.WriteLine("You've chosen too many {0} values for this creature. Please select a different value", degree);
            }

            return degreeMissing;
        }
    }
}

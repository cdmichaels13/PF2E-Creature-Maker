using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

        public static void StrikeAttackStep(Creature creature, Random random)
        {
            Console.WriteLine("Press Enter to randomly determine Strike Attack Bonus or LOW, MOD, HIGH, EX");
            Degree chosenDegree = Degree.moderate;
            Degree[] validDegrees = new Degree[] { Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            string strikeInput;
            do
            {
                strikeInput = Program.GetValidString("", "LOW", "MOD", "HIGH", "EX");
                switch (strikeInput.ToUpper())
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

            string[] strikeAttackFile = File.ReadAllLines(Program.FilePathByName("Strike Attack Bonus"));
            foreach (string line in strikeAttackFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch (chosenDegree)
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
                    creature.StrikeAttack = int.Parse(Program.CorrectedDash(splitLine[degreeIndex])) - creature.Level;
                    Console.WriteLine("{0} Strike Attack Bonus: {1}", chosenDegree, creature.StrikeAttack);
                    break;
                }
            }
        }

        public static void StrikeDamageStep(Creature creature, Random random)
        {
            Console.WriteLine("Press Enter to randomly determine Strike Damage or LOW, MOD, HIGH, EX");
            Degree chosenDegree = Degree.moderate;
            Degree[] validDegrees = new Degree[] { Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            string damageInput;
            do
            {
                damageInput = Program.GetValidString("", "LOW", "MOD", "HIGH", "EX");
                switch (damageInput.ToUpper())
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

            string[] damageFile = File.ReadAllLines(Program.FilePathByName("Strike Damage"));
            foreach (string line in damageFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch (chosenDegree)
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
                    creature.StrikeDamage = Program.CorrectedDash(splitLine[degreeIndex]);
                    Console.WriteLine("{0} Strike Damage: {1}", chosenDegree, creature.StrikeDamage);
                    break;
                }
            }
        }

        public static void AbilityScoreStep(Creature creature, Random random, AbilityScoreEnum ability)
        {
            Degree chosenDegree = Degree.moderate;
            if (ability == AbilityScoreEnum.Strength && creature.IsStrengthExtreme)
            {
                Console.WriteLine("This creature's size makes its Strength extreme");
                chosenDegree = Degree.extreme;

                Degree removingDegree = Degree.moderate;
                for (int i = 0;; i++)
                {
                    switch(i)
                    {
                        case 0:
                            {
                                removingDegree = Degree.extreme;
                                break;
                            }
                        case 1:
                            {
                                removingDegree = Degree.high;
                                break;
                            }
                        case 2:
                            {
                                removingDegree = Degree.moderate;
                                break;
                            }
                        case 3:
                            {
                                removingDegree = Degree.low;
                                break;
                            }
                        case 4:
                            {
                                removingDegree = Degree.bad;
                                break;
                            }
                    }
                    if (creature.DegreeList.Contains(removingDegree))
                    {
                        creature.DegreeList.Remove(removingDegree);
                        break;
                    }
                }
            }
            else
            {
                Degree[] validDegrees = new Degree[] { Degree.bad, Degree.low, Degree.moderate, Degree.high, Degree.extreme };
                if (creature.Level < 1)
                {
                    Console.WriteLine("For {0}, press Enter to randomly determine its score or BAD, LOW, MOD, HIGH", ability);
                    validDegrees = new Degree[] { Degree.bad, Degree.low, Degree.moderate, Degree.high };
                }
                else
                {
                    Console.WriteLine("For {0}, press Enter to randomly determine its score or BAD, LOW, MOD, HIGH, EX", ability);
                }

                string scoreInput;
                do
                {
                    if (creature.Level < 1)
                    {
                        scoreInput = Program.GetValidString("", "BAD", "LOW", "MOD", "HIGH");
                    }
                    else
                    {
                        scoreInput = Program.GetValidString("", "BAD", "LOW", "MOD", "HIGH", "EX");
                    }
                    switch (scoreInput.ToUpper())
                    {
                        case "":
                            {
                                do
                                {
                                    chosenDegree = validDegrees[random.Next(validDegrees.Length)];
                                } while (!creature.DegreeList.Contains(chosenDegree));
                                break;
                            }
                        case "BAD":
                            {
                                chosenDegree = Degree.bad;
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
            }

            string[] abilityScoreFile = File.ReadAllLines(Program.FilePathByName("Ability Modifier Scales"));
            foreach (string line in abilityScoreFile)
            {
                string[] splitLine = line.Split(',');
                bool skipParse = false;
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch (chosenDegree)
                    {
                        case Degree.bad:
                            {
                                creature.AbilityScoreDictionary[ability] = random.Next(-5, -1);
                                skipParse = true;
                                break;
                            }
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
                    if (!skipParse)
                    {
                        creature.AbilityScoreDictionary[ability] = int.Parse(Program.CorrectedDash(splitLine[degreeIndex]));
                    }
                    Console.WriteLine("{0} {1} Score: {2}", chosenDegree, ability, creature.AbilityScoreDictionary[ability]);
                    break;
                }
            }
        }

        public static void PerceptionStep(Creature creature, Random random)
        {
            Console.WriteLine("Press Enter to randomly determine Perception or BAD, LOW, MOD, HIGH, EX");
            Degree chosenDegree = Degree.moderate;
            Degree[] validDegrees = new Degree[] { Degree.bad, Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            string perceptionInput;
            do
            {
                perceptionInput = Program.GetValidString("", "BAD", "LOW", "MOD", "HIGH", "EX");
                switch (perceptionInput.ToUpper())
                {
                    case "":
                        {
                            do
                            {
                                chosenDegree = validDegrees[random.Next(validDegrees.Length)];
                            } while (!creature.DegreeList.Contains(chosenDegree));
                            break;
                        }
                    case "BAD":
                        {
                            chosenDegree = Degree.bad;
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

            string[] perceptionFile = File.ReadAllLines(Program.FilePathByName("Perception Bonuses"));
            foreach (string line in perceptionFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch (chosenDegree)
                    {
                        case Degree.bad:
                            {
                                degreeIndex = 5;
                                break;
                            }
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
                    creature.Perception = int.Parse(Program.CorrectedDash(splitLine[degreeIndex])) - creature.Level;
                    Console.WriteLine("{0} Perception Bonus: +{1}", chosenDegree, creature.Perception);
                    break;
                }
            }
        }

        public static void SavesStep(Creature creature, Random random, SaveName save)
        {
            Console.WriteLine("For {0}, press Enter to randomly determine bonus or BAD, LOW, MOD, HIGH, EX", save);
            Degree chosenDegree = Degree.moderate;
            Degree[] validDegrees = new Degree[] { Degree.bad, Degree.low, Degree.moderate, Degree.high, Degree.extreme };
            string saveInput;
            do
            {
                saveInput = Program.GetValidString("", "BAD", "LOW", "MOD", "HIGH", "EX");
                switch (saveInput.ToUpper())
                {
                    case "":
                        {
                            do
                            {
                                chosenDegree = validDegrees[random.Next(validDegrees.Length)];
                            } while (!creature.DegreeList.Contains(chosenDegree));
                            break;
                        }
                    case "BAD":
                        {
                            chosenDegree = Degree.bad;
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

            string[] savingThrowFile = File.ReadAllLines(Program.FilePathByName("Saving Throws"));
            foreach (string line in savingThrowFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    int degreeIndex = 0;
                    switch (chosenDegree)
                    {
                        case Degree.bad:
                            {
                                degreeIndex = 5;
                                break;
                            }
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
                    creature.SavingThrows[save] = int.Parse(Program.CorrectedDash(splitLine[degreeIndex])) - creature.Level;
                    Console.WriteLine("{0} {1} Bonus: +{2}", chosenDegree, save, creature.SavingThrows[save]);
                    break;
                }
            }
        }

        public static void SpellsStep(Creature creature, Random random, string spellType)
        {
            bool forceSchool = true;
            Console.WriteLine("Choosing spells of type {0}", spellType);
            string[] spellSchools = new string[] { "Abjuration", "Conjuration", "Necromancy", "Transmutation", "Enchantment", "Divination", "Illusion", "Evocation" };

            Console.WriteLine("Press Enter to randomly pick a school of magic, or type one of the following:");
            foreach (string school in spellSchools)
            {
                Console.WriteLine(school);
            }

            string[] validSchoolSelection = new string[spellSchools.Length + 1];
            for (int i = 0; i < validSchoolSelection.Length; i++)
            {
                if (i == 0)
                {
                    validSchoolSelection[i] = "";
                }
                else
                {
                    validSchoolSelection[i] = spellSchools[i - 1];
                }
            }

            string schoolSelection = Program.GetValidString(validSchoolSelection);
            if (schoolSelection == "")
            {
                schoolSelection = spellSchools[random.Next(spellSchools.Length)];
                Console.WriteLine(schoolSelection);
            }
            schoolSelection = Program.CapitalizeString(schoolSelection);

            string[] schoolSpellFile = File.ReadAllLines(Program.FilePathByName(spellType + " Spells"));
            List<string[]> availableSpellLines = new List<string[]>();
            List<Spell> allSpells = new List<Spell>();
            
            for (int i = 1; i < schoolSpellFile.Length; i++)
            {
                string[] splitLine = schoolSpellFile[i].Split(',');
                Spell newSpell = new Spell();
                newSpell.name = splitLine[0];
                newSpell.school = splitLine[1];
                newSpell.description = splitLine[2];
                newSpell.level = int.Parse(splitLine[3]);
                allSpells.Add(newSpell);
            }

            List<Spell> availableCantrips = new List<Spell>();
            foreach (Spell spell in allSpells)
            {
                if (spell.level == 0)
                {
                    availableCantrips.Add(spell);
                }
            }

            Spell chosenSpell = new Spell();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Press Enter to randomly select a cantrip or SELECT to choose from a list");
                string userInput = Program.GetValidString("", "SELECT");
                switch(userInput.ToUpper())
                {
                    case "":
                        {
                            do
                            {
                                forceSchool = true;
                                chosenSpell = availableCantrips[random.Next(availableCantrips.Count)];
                                if (random.Next(2) == 1)
                                {
                                    forceSchool = false;
                                }
                            } while (!chosenSpell.school.ToLower().Contains(schoolSelection.ToLower()) && forceSchool);
                            break;
                        }
                    case "SELECT":
                        {
                            List<string> validSpells = new List<string>();
                            foreach (Spell spell in availableCantrips)
                            {
                                Console.WriteLine(spell.name);
                                validSpells.Add(spell.name);
                            }
                            string chosenSpellName = Program.GetValidString(validSpells.ToArray());
                            chosenSpell = availableCantrips.FirstOrDefault(Spell => Spell.name.ToLower() == chosenSpellName.ToLower());
                            break;
                        }
                }

                creature.Spells.Add(chosenSpell);
                availableCantrips.Remove(chosenSpell);
                Console.WriteLine("Selected the {0} spell", chosenSpell.name);
            }

            int distanceFromTopSpells = 0;
            for (int spellLevelSelection = Convert.ToInt32(Math.Round(Convert.ToDouble(creature.Level / 2), MidpointRounding.ToPositiveInfinity)); spellLevelSelection > 0; spellLevelSelection--)
            {
                Console.WriteLine("spellLevelSelection = " + spellLevelSelection);

                List<Spell> spellsOfLevel = new List<Spell>();
                foreach (Spell spell in allSpells)
                {
                    if (spell.level == spellLevelSelection)
                    {
                        spellsOfLevel.Add(spell);
                    }
                }

                int uniqueSpellsOfLevel = 0;
                int copiesOfUniques = 1;
                switch(distanceFromTopSpells)
                {
                    case 0:
                        uniqueSpellsOfLevel = 3;
                        break;
                    case 1:
                        uniqueSpellsOfLevel = 2;
                        copiesOfUniques = 2;
                        break;
                    default:
                        uniqueSpellsOfLevel = 1;
                        copiesOfUniques = 4;
                        break;
                }

                for (; uniqueSpellsOfLevel > 0; uniqueSpellsOfLevel--)
                {
                    Console.WriteLine("Press Enter to randomly select a spell of Level {0} or SELECT to choose for yourself", spellLevelSelection);
                    string userInput = Program.GetValidString("", "SELECT");
                    if (userInput == "")
                    {
                        do
                        {
                            forceSchool = true;
                            if (random.Next(2) == 1)
                            {
                                forceSchool = false;
                            }
                            chosenSpell = spellsOfLevel[random.Next(spellsOfLevel.Count)];
                        } while ((chosenSpell.school != schoolSelection && forceSchool) || creature.Spells.Contains(chosenSpell));
                    }
                    else
                    {
                        Console.WriteLine("\nSpells of {0} school first:\n", schoolSelection);
                        foreach (Spell spell in spellsOfLevel)
                        {
                            if (spell.school.ToLower().Contains(schoolSelection.ToLower()))
                            {
                                Console.WriteLine(spell.name);
                            }
                        }
                        Console.WriteLine("\nOther spells:\n");
                        foreach (Spell spell in spellsOfLevel)
                        {
                            if (!spell.school.ToLower().Contains(schoolSelection.ToLower()))
                            {
                                Console.WriteLine(spell.name);
                            }
                        }
                        List<string> validSpellNames = new List<string>();
                        foreach (Spell spell in spellsOfLevel)
                        {
                            validSpellNames.Add(spell.name);
                        }
                        string spellNameString = Program.GetValidString(validSpellNames.ToArray());
                        chosenSpell = spellsOfLevel.FirstOrDefault(Spell => Spell.name.ToLower() == spellNameString.ToLower());
                    }

                    for (int uniques = copiesOfUniques; uniques > 0; uniques--)
                    {
                        creature.Spells.Add(chosenSpell);
                        Console.WriteLine("Added spell {0} at Level {1}", chosenSpell.name, chosenSpell.level);
                    }
                }

                distanceFromTopSpells++;
            }
        }

        public static void SpellStatsStep(Creature creature, Random random)
        {
            Degree[] additionalSpellDegrees = new Degree[] { Degree.moderate, Degree.high, Degree.high };
            foreach (Degree degree in additionalSpellDegrees)
            {
                creature.DegreeList.Add(degree);
            }
            if (creature.Level >= 20)
            {
                creature.DegreeList.Add(Degree.extreme);
            }

            Console.WriteLine("Press enter to randomly select degree of spell DCs and attack bonuses or MOD, HIGH, EX");
            string degreeSelection = Program.GetValidString("", "MOD", "HIGH", "EX");

            Degree chosenDegree = Degree.high;
            bool repeat;
            do
            {
                repeat = false;
                switch (degreeSelection)
                {
                    case "":
                        {
                            Degree[] validDegrees = new Degree[] { Degree.moderate, Degree.high, Degree.extreme };
                            do
                            {
                                chosenDegree = creature.DegreeList[random.Next(creature.DegreeList.Count)];
                            } while (!validDegrees.Contains(chosenDegree));
                            Console.WriteLine("Random degree: " + chosenDegree);
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
                    default:
                        {
                            Console.WriteLine("No valid input detected. Defaulting to Degree.high");
                            break;
                        }
                }

                if (!creature.DegreeList.Contains(chosenDegree))
                {
                    Console.WriteLine("This creature has used too many {0} degrees in previous stats. Please choose a different degree.", chosenDegree);
                    repeat = true;
                }
            } while (repeat);

            creature.DegreeList.Remove(chosenDegree);

            string[] spellDCsFile = File.ReadAllLines(Program.FilePathByName("Spell DCs"));
            foreach (string line in spellDCsFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    switch(chosenDegree)
                    {
                        case Degree.moderate:
                            {
                                creature.SpellsDC = int.Parse(splitLine[3]);
                                break;
                            }
                        case Degree.high:
                            {
                                creature.SpellsDC = int.Parse(splitLine[2]);
                                break;
                            }
                        case Degree.extreme:
                            {
                                creature.SpellsDC = int.Parse(splitLine[1]);
                                break;
                            }
                    }
                    break;
                }
            }

            creature.SpellsDC -= creature.Level;
            creature.SpellsAttackBonus = creature.SpellsDC - 8;

            Console.WriteLine("Spells DC: " + creature.SpellsDC + 
                "\nSpells Attack Bonus: " + creature.SpellsAttackBonus);
        }

        public static void AreaDamageStep(Creature creature)
        {
            Console.WriteLine("If this creature has abilities that do area damage, use these damage values:");

            string[] areaDamageFile = File.ReadAllLines(Program.FilePathByName("Area Damage"));
            foreach (string line in areaDamageFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    creature.AreaDamageValues[0] = splitLine[1];
                    creature.AreaDamageValues[1] = splitLine[2];
                    break;
                }
            }

            Console.WriteLine("Area Damage for at-will abilities: " + creature.AreaDamageValues[0]);
            Console.WriteLine("Area Damage for limited abilities: " + creature.AreaDamageValues[1]);
        }

        public static void GearStep(Creature creature, Random random)
        {
            creature.BulkLimit = 5 + creature.AbilityScoreDictionary[AbilityScoreEnum.Strength];
            double currentBulk = 0;
            bool tryAnotherItem = false;

            int safeItemLevel = 0;
            string[] safeItemFile = File.ReadAllLines(Program.FilePathByName("Safe Item Level"));
            foreach (string line in safeItemFile)
            {
                string[] splitLine = line.Split(',');
                if (int.Parse(Program.CorrectedDash(splitLine[0])) == creature.Level)
                {
                    safeItemLevel = int.Parse(splitLine[1]);
                }
            }

            int selectingItemLevel = safeItemLevel;

            string[] gearFile = File.ReadAllLines(Program.FilePathByName("Gear"));
            List<Item> validGear = new List<Item>();
            Item selectedItem = new Item();

            Console.WriteLine("Enter Y before continuing to next step");
            string continueY = Program.GetValidString("Y");
            string repeatOrContinue = "";

            do
            {
                do
                {
                    tryAnotherItem = false;
                    List<string> validCategories = new List<string>();

                    for (int line = 1; line < gearFile.Length; line++)
                    {
                        string[] splitLine = gearFile[line].Split(',');
                        if (int.Parse(splitLine[1]) > safeItemLevel)
                        {
                            break;
                        }
                        else
                        {
                            Item item = new Item();
                            item.name = splitLine[0];
                            item.itemLevel = int.Parse(splitLine[1]);
                            item.rarity = splitLine[3];
                            item.traits = splitLine[4];
                            item.category = splitLine[5];
                            item.subcategory = splitLine[6];

                            string[] currencies = new string[] { " cp", " sp", " gp" };
                            double price = 0;
                            foreach (string currency in currencies)
                            {
                                if (splitLine[2].Contains(currency))
                                {
                                    price = double.Parse(splitLine[2].Split(' ')[0]);
                                    switch (currency)
                                    {
                                        case " cp":
                                            {
                                                price *= .01;
                                                break;
                                            }
                                        case " sp":
                                            {
                                                price *= .1;
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                            }
                            item.price = price;

                            try
                            {
                                item.bulk = int.Parse(splitLine[7]);
                            }
                            catch (Exception)
                            {
                                if (splitLine[7] == "L")
                                {
                                    item.bulk = .1;
                                }
                                else
                                {
                                    item.bulk = 0;
                                }
                            }

                            validGear.Add(item);
                            if (!validCategories.Contains(item.category))
                            {
                                validCategories.Add(item.category);
                            }
                        }
                    }

                    string selectedCategory = "";
                    Console.WriteLine("Press Enter to randomly select an item category or SELECT");
                    string categorySelect = Program.GetValidString("", "SELECT");
                    if (categorySelect == "")
                    {
                        selectedCategory = validCategories[random.Next(validCategories.Count)];
                        Console.WriteLine(selectedCategory);
                    }
                    else
                    {
                        foreach (string category in validCategories)
                        {
                            Console.WriteLine(category);
                        }
                        selectedCategory = Program.GetValidString(validCategories.ToArray());
                    }

                    List<string> validSubcats = new List<string>();
                    foreach (Item item in validGear)
                    {
                        if (item.category.ToLower() == selectedCategory.ToLower() && !validSubcats.Contains(item.subcategory))
                        {
                            validSubcats.Add(item.subcategory);
                        }
                    }

                    string selectedSubcat = "";
                    Console.WriteLine("Press Enter to randomly select an item subcategory or SELECT");
                    string subcatSelect = Program.GetValidString("", "SELECT");
                    if (subcatSelect == "")
                    {
                        selectedSubcat = validSubcats[random.Next(validSubcats.Count)];
                        Console.WriteLine(selectedSubcat);
                    }
                    else
                    {
                        foreach (string subcat in validSubcats)
                        {
                            Console.WriteLine(subcat);
                        }
                        selectedSubcat = Program.GetValidString(validSubcats.ToArray());
                    }

                    string selectedItemName = "";
                    Console.WriteLine("Press Enter to randomly select an item or SELECT");
                    string itemSelect = Program.GetValidString("", "SELECT");
                    if (itemSelect == "")
                    {
                        do
                        {
                            selectedItemName = validGear[random.Next(validGear.Count)].name;
                            selectedItem = validGear.FirstOrDefault(Item => Item.name.ToLower() == selectedItemName.ToLower());
                        } while (selectedItem.category.ToLower() != selectedCategory.ToLower() || selectedItem.subcategory.ToLower() != selectedSubcat.ToLower());
                        Console.WriteLine(selectedItem.name);
                    }
                    else
                    {
                        List<string> itemNames = new List<string>();
                        foreach (Item item in validGear)
                        {
                            if (item.category.ToLower() == selectedCategory.ToLower() && item.subcategory.ToLower() == selectedSubcat.ToLower())
                            {
                                Console.WriteLine(item.name);
                                itemNames.Add(item.name);
                            }
                        }
                        selectedItemName = Program.GetValidString(itemNames.ToArray());
                        selectedItem = validGear.FirstOrDefault(Item => Item.name.ToLower() == selectedItemName.ToLower());
                    }

                    if (selectedItem.bulk + currentBulk > creature.BulkLimit)
                    {
                        Console.WriteLine("Adding this item to the creature would encumber it. Add anyway? Y/N");
                        string addAnyway = Program.GetValidString("Y", "N");
                        if (addAnyway.ToUpper() == "N")
                        {
                            tryAnotherItem = true;
                        }
                    }
                } while (tryAnotherItem);

                creature.Gear.Add(selectedItem);
                currentBulk += selectedItem.bulk;

                Console.WriteLine("Added {0} to gear list", selectedItem.name);

                if (selectingItemLevel == safeItemLevel)
                {
                    selectingItemLevel--;
                }
                else
                {
                    selectingItemLevel = random.Next(safeItemLevel);
                }

                Console.WriteLine("Press Enter to add more items or STOP to finish the Gear Step");
                repeatOrContinue = Program.GetValidString("", "STOP");
            } while (repeatOrContinue.ToUpper() != "STOP");
        }

        public static void EndStep(Creature creature)
        {
            Console.WriteLine("Give this creature a name:");
            do
            {
                creature.Name = Console.ReadLine();
                if (creature.Name.Trim() == "")
                {
                    Console.WriteLine("Please enter a name");
                }
            } while (creature.Name.Trim() == "");

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

            Console.WriteLine("Armor Class: " + creature.ArmorClass + 
                "\nStrike Attack Bonus: +" + creature.StrikeAttack + 
                "\nStrike Damage: " + creature.StrikeDamage + 
                "\nPerception Bonus: +" + creature.Perception);

            Console.WriteLine("\n_Ability Scores_");
            foreach (var ability in creature.AbilityScoreDictionary)
            {
                Console.Write(ability.Key + ": ");
                if (ability.Value > 0)
                {
                    Console.Write("+");
                }
                Console.Write(ability.Value + "  ");
            }
            Console.WriteLine();

            Console.WriteLine("\n_Saving Throws_");
            foreach (var save in creature.SavingThrows)
            {
                Console.Write(save.Key + ": ");
                if (save.Value > 0)
                {
                    Console.Write("+");
                }
                Console.Write(save.Value + "  ");
            }
            Console.WriteLine();

            Console.WriteLine("\n_Skills_");
            foreach (Skill skill in creature.SkillPool.SelectedSkills)
            {
                Console.Write("{0}: {1}  ", skill.skillName, skill.skillBonus);
            }
            Console.WriteLine();

            Console.WriteLine("\n_Traits_");
            foreach (string trait in creature.TraitPool.SelectedTraits)
            {
                Console.WriteLine(trait);
            }

            if (creature.HasSpells)
            {
                Console.WriteLine("\n_Spells_");
                foreach (var spell in creature.Spells)
                {
                    Console.WriteLine(spell.name + " at level " + spell.level);
                }

                Console.WriteLine("Spells DC: " + creature.SpellsDC + 
                    "\nSpells Attack Bonus: " + creature.SpellsAttackBonus);
            }

            Console.WriteLine("\nArea Damage for at-will abilities: " + creature.AreaDamageValues[0]);
            Console.WriteLine("Area Damage for limited abilities: " + creature.AreaDamageValues[1]);

            if (creature.Gear.Any())
            {
                double totalBulk = 0;
                double totalPrice = 0;
                Console.WriteLine("\n_Gear_");
                Dictionary<Item, int> uniqueItems = new Dictionary<Item, int>();
                foreach (Item item in creature.Gear)
                {
                    if (uniqueItems.ContainsKey(item))
                    {
                        uniqueItems[item]++;
                    }
                    else
                    {
                        uniqueItems.Add(item, 1);
                    }
                }
                foreach (var item in uniqueItems)
                {
                    Console.Write(item.Key.name);
                    if (item.Value > 1)
                    {
                        Console.Write(" x" + item.Value);
                    }
                    Console.WriteLine();
                    Console.WriteLine("\tBulk per: " + item.Key.bulk + 
                        "\n\tPrice per: " + item.Key.price);

                    totalBulk += item.Key.bulk * item.Value;
                    totalPrice += item.Key.price * item.Value;
                }

                Console.WriteLine("\nTotal Bulk: " + totalBulk + 
                    "\nTotal Price: " + totalPrice);
            }
        }

        public static void SaveToFileStep(Creature creature)
        {
            string overwrite = "Y";
            if (Directory.GetFiles(@"C:\Users\cdmic\Desktop").Contains(@"C:\Users\cdmic\Desktop\" + creature.Name + ".txt"))
            {
                Console.WriteLine("A file with this name already exists on the desktop. Overwrite? Y/N");
                overwrite = Program.GetValidString("Y", "N");
            }
            if (overwrite.ToUpper() == "Y")
            {
                StreamWriter saveFile = new StreamWriter(File.Create(@"C:\Users\cdmic\Desktop\" + creature.Name + ".txt"));

                saveFile.WriteLine("__Final Creature__" +
                "\nName: " + creature.Name +
                "\nLevel: " + creature.Level +
                "\nSize: " + creature.Size +
                "\nHit Points: " + creature.HitPoints);
                if (creature.Regeneration != 0)
                {
                    saveFile.WriteLine("Regeneration: " + creature.Regeneration);
                }

                if (creature.ResistOrWeakType == ResistOrWeak.Resistance || creature.ResistOrWeakType == ResistOrWeak.Both)
                {
                    saveFile.WriteLine("Resistance: " + creature.ResistWeakValue);
                }
                if (creature.ResistOrWeakType == ResistOrWeak.Weakness || creature.ResistOrWeakType == ResistOrWeak.Both)
                {
                    saveFile.WriteLine("Weakness: " + creature.ResistWeakValue);
                }

                saveFile.WriteLine("Armor Class: " + creature.ArmorClass +
                    "\nStrike Attack Bonus: +" + creature.StrikeAttack +
                    "\nStrike Damage: " + creature.StrikeDamage +
                    "\nPerception Bonus: +" + creature.Perception);

                saveFile.WriteLine("\n_Ability Scores_");
                foreach (var ability in creature.AbilityScoreDictionary)
                {
                    saveFile.Write(ability.Key + ": ");
                    if (ability.Value > 0)
                    {
                        saveFile.Write("+");
                    }
                    saveFile.Write(ability.Value + "  ");
                }
                saveFile.WriteLine();

                saveFile.WriteLine("\n_Saving Throws_");
                foreach (var save in creature.SavingThrows)
                {
                    saveFile.Write(save.Key + ": ");
                    if (save.Value > 0)
                    {
                        saveFile.Write("+");
                    }
                    saveFile.Write(save.Value + "  ");
                }
                saveFile.WriteLine();

                saveFile.WriteLine("\n_Skills_");
                foreach (Skill skill in creature.SkillPool.SelectedSkills)
                {
                    saveFile.Write("{0}: {1}  ", skill.skillName, skill.skillBonus);
                }
                saveFile.WriteLine();

                saveFile.WriteLine("\n_Traits_");
                foreach (string trait in creature.TraitPool.SelectedTraits)
                {
                    saveFile.WriteLine(trait);
                }

                if (creature.HasSpells)
                {
                    saveFile.WriteLine("\n_Spells_");
                    foreach (var spell in creature.Spells)
                    {
                        saveFile.WriteLine(spell.name + " at level " + spell.level);
                    }

                    saveFile.WriteLine("Spells DC: " + creature.SpellsDC +
                        "\nSpells Attack Bonus: " + creature.SpellsAttackBonus);
                }

                saveFile.WriteLine("\nArea Damage for at-will abilities: " + creature.AreaDamageValues[0]);
                saveFile.WriteLine("Area Damage for limited abilities: " + creature.AreaDamageValues[1]);

                if (creature.Gear.Any())
                {
                    double totalBulk = 0;
                    double totalPrice = 0;
                    saveFile.WriteLine("\n_Gear_");
                    Dictionary<Item, int> uniqueItems = new Dictionary<Item, int>();
                    foreach (Item item in creature.Gear)
                    {
                        if (uniqueItems.ContainsKey(item))
                        {
                            uniqueItems[item]++;
                        }
                        else
                        {
                            uniqueItems.Add(item, 1);
                        }
                    }
                    foreach (var item in uniqueItems)
                    {
                        saveFile.Write(item.Key.name);
                        if (item.Value > 1)
                        {
                            saveFile.Write(" x" + item.Value);
                        }
                        saveFile.WriteLine();
                        saveFile.WriteLine("\tBulk per: " + item.Key.bulk +
                            "\n\tPrice per: " + item.Key.price);

                        totalBulk += item.Key.bulk * item.Value;
                        totalPrice += item.Key.price * item.Value;
                    }

                    saveFile.WriteLine("\nTotal Bulk: " + totalBulk +
                        "\nTotal Price: " + totalPrice);
                }

                saveFile.Close();

                Console.WriteLine("Creature saved");
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

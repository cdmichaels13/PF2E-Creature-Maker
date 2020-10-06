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
                        {Degree.mod, splitLine[2] },
                        {Degree.low, splitLine[3] }
                    };

                    Degree chosenDegree;
                    do
                    {
                        chosenDegree = creature.DegreeList.ElementAt(random.Next(creature.DegreeList.Count));
                    } while (!lineDegreeValues.ContainsKey(chosenDegree));
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

                    break;
                }
            }
        }
        public static void TraitsStep(Creature creature, Random random)
        {
            if (creature.Type == CreatureType.NPC)
            {
                for (int i = creature.traitPool.AllPossibleTraits.Count - 1; i >= 0; i--)
                {
                    if (!creature.traitPool.HumanoidIndexes.Contains(i))
                    {
                        creature.traitPool.AllPossibleTraits.RemoveAt(i);
                    }
                }
            }
            if (creature.Type == CreatureType.Monster)
            {
                for (int i = creature.traitPool.AllPossibleTraits.Count - 1; i >= 0; i--)
                {
                    if (creature.traitPool.HumanoidIndexes.Contains(i))
                    {
                        creature.traitPool.AllPossibleTraits.RemoveAt(i);
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
                        int traitIndex = random.Next(creature.traitPool.AllPossibleTraits.Count);
                        selectedTrait = creature.traitPool.AllPossibleTraits[traitIndex];
                        break;
                    }
                case "SELECT":
                    {
                        Console.WriteLine("Please enter a trait from the list below:");
                        string[] validTraits = creature.traitPool.AllPossibleTraits.ToArray();
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
                    creature.traitPool.SelectedTraits.Add(selectedTrait + ": " + traitRuleIfExists);
                }
                else
                {
                    creature.traitPool.SelectedTraits.Add(selectedTrait);
                }

                if (creature.traitPool.AllPossibleTraits.Contains(selectedTrait))
                {
                    creature.traitPool.AllPossibleTraits.Remove(selectedTrait);
                }

                foreach (string trait in creature.traitPool.AllPossibleTraits)
                {
                    if (traitRuleIfExists != "" && traitRuleIfExists.ToLower().Contains(trait.ToLower() + " trait"))
                    {
                        queuedTraits.Add(trait);
                    }
                }

                Console.WriteLine(creature.traitPool.SelectedTraits.Last());
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

        public static int LevelStep(Random random, Creature creature)
        {
            string userInput;
            int partyLevel = Program.GetMinMaxInt("Please enter the party's current level: ", MinMax.hasBoth, 1, 20);

            bool passable = true;

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
                    } while (creature.DegreeList[removingDegree] == Degree.ex);

                    creature.DegreeList.RemoveAt(removingDegree);
                    creature.DegreeList.Add(Degree.ex);
                }
            }

            return partyLevel;
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

            foreach (KeyValuePair<AbilityScoreEnum, int> ability in creature.AbilityScoreDictionary)
            {
                Console.Write(ability.Key + " = " + ability.Value + " ");
                if (ability.Key == creature.AbilityScoreDictionary.Last().Key)
                {
                    Console.WriteLine("");
                }
            }

            Console.WriteLine("Traits:");
            foreach (string trait in creature.traitPool.SelectedTraits)
            {
                Console.WriteLine(trait);
            }
        }
    }
}

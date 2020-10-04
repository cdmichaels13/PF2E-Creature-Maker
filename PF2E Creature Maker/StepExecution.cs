using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace PF2E_Creature_Maker
{
    public class ExecuteStep
    {
        public static void TraitsStep(Creature creature, Random random)
        {
            string[] creatureTraits = File.ReadAllLines(@"Data Files\Menu Page All Creature Traits.txt");
            string[] creatureTraitsWithoutHumanoidTag = new string[creatureTraits.Length];
            List<int> humanoidIndexes = new List<int>();
            for (int i = 0; i < creatureTraits.Length; i++)
            {
                creatureTraitsWithoutHumanoidTag[i] = creatureTraits[i];
                if (creatureTraitsWithoutHumanoidTag[i].Contains(','))
                {
                    humanoidIndexes.Add(i);
                    int commaIndex = creatureTraitsWithoutHumanoidTag[i].IndexOf(',');
                    creatureTraitsWithoutHumanoidTag[i] = creatureTraitsWithoutHumanoidTag[i].Substring(0, commaIndex);
                }
            }

            Console.WriteLine("Randomly select trait? Y/N");
            string userInput = Program.GetValidString("Y", "N");

            switch (userInput.ToUpper())
            {
                case "Y":
                    random.Next(creatureTraitsWithoutHumanoidTag.Length);
                    break;
                case "N":
                    break;
                default:
                    Console.WriteLine("No correct user input detected");
                    break;
            }

            foreach (string trait in creatureTraitsWithoutHumanoidTag)
            {
                Console.WriteLine(trait);
            }
        }
        public static void SizeStep(Creature creature, Random random)
        {
            string[] sizeOptions = File.ReadAllLines("Data Files\\Menu Page Size.txt");
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
                Console.WriteLine("Enter the creature's level or enter PARTY, NPC, or MONSTER to generate randomly based on the entry");
                do
                {
                    string levelSelectionStyle = Console.ReadLine();
                    passable = true;
                    switch (levelSelectionStyle.ToUpper())
                    {
                        case "PARTY":
                            creatureLevel = Program.PinkRandom(random, partyLevel - 7, partyLevel + 7);
                            break;
                        case "NPC":
                            creature.Type = CreatureType.NPC;
                            creatureLevel = Program.DisadvantageRandom(random, -1, 11);
                            break;
                        case "MONSTER":
                            creature.Type = CreatureType.Monster;
                            creatureLevel = Program.DisadvantageRandom(random, -1, 25);
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
    }
}

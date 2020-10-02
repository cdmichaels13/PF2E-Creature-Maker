using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PF2E_Creature_Maker
{
    public enum Degree
    {
        bad,
        low,
        mod,
        high,
        ex
    }

    public enum MinMax
    {
        hasMin,
        hasMax,
        hasBoth
    }

    public enum Step
    {
        Start,
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
        End
    }

    class Program
    {
        public static Creature _creature = new Creature();
        static void Main()
        {
            Random random = new Random();

            int partyLevel;
            bool endSteps = false;

            Step[] stepOrder = new Step[]
            {
                Step.Level,
                Step.Size,
                Step.Traits,
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
                Step.End
            };

            
            Dictionary<Step, Creature> creatureSaves = new Dictionary<Step, Creature>();

            for (int step = 0; endSteps == false; step++)
            {
                if (step >= 0 && step < stepOrder.Length)
                {
                    Console.WriteLine("Step: " + stepOrder[step]);
                }

                if (step > 0)
                {
                    step = ContinueOrBack(stepOrder, step, creatureSaves);
                    Console.WriteLine("Creature's Strength is Extreme: " + _creature.IsStrengthExtreme);
                }

                if (creatureSaves.ContainsKey(stepOrder[step]))
                {
                    creatureSaves.Remove(stepOrder[step]);
                }
                creatureSaves.Add(stepOrder[step], CopyCreature(_creature));
                Console.WriteLine("Creature's Strength is Extreme: " + _creature.IsStrengthExtreme);

                switch (stepOrder[step])
                {
                    case Step.Level:
                        {
                            partyLevel = LevelStep(random, _creature);
                            break;
                        }
                    case Step.Size:
                        {
                            SizeStep(_creature, random);
                            break;
                        }
                    case Step.End:
                    default:
                        {
                            endSteps = true;
                            break;
                        }
                }
            }

            Console.WriteLine("Creature Level: " + _creature.Level);
            Console.WriteLine(_creature.Size);
        }

        public static Creature CopyCreature(Creature creature)
        {
            Creature creatureCopy = new Creature
            {
                IsStrengthExtreme = creature.IsStrengthExtreme,
                SaveNumber = creature.SaveNumber,
                Level = creature.Level,
                Name = creature.Name,
                Size = creature.Size,
                DegreeList = CopyListValues(creature.DegreeList)
            };

            for (int i = 0; i < creature.AbilityScores.Length; i++)
            {
                creatureCopy.AbilityScores[i]._abilityBonus = creature.AbilityScores[i]._abilityBonus;
            }

            return creatureCopy;
        }

        public static void SizeStep(Creature creature, Random random)
        {
            //TODO change search for file name to find in Data Files
            string[] sizeOptions = System.IO.File.ReadAllLines("Menu Page Size.txt");
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
                userInput = GetValidString(validSizeOptions);

                if (userInput == "")
                {
                    SizeOption randomSize;
                    do
                    {
                        randomSize = sizeOptionsArray[WeightedRandom(random, 0, sizeOptionsArray.Length - 1, mediumSizeIndex, weight: 2)];
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
                                userInput = GetValidString("Y", "N");
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

        public static void AbilityScoresStep()
        {

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

        public static int ContinueOrBack(Step[] stepOrder, int step, Dictionary<Step, Creature> creatureSavesDictionary)
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

                        Console.WriteLine("Creature's Strength is Extreme: " + _creature.IsStrengthExtreme);
                        _creature = CopyCreature(creatureSavesDictionary[stepOrder[step]]);
                        Console.WriteLine("Creature's Strength is Extreme: " + _creature.IsStrengthExtreme);

                        Console.WriteLine("Returning to {0} Step", stepOrder[step]);
                    }
                }
            } while (userInput.ToUpper() == "BACK");

            return step;
        }

        public static List<Degree> CopyListValues(List<Degree> copiedList)
        {
            List<Degree> returningList = new List<Degree>();

            for (int i = 0; i < copiedList.Count(); i++)
            {
                returningList.Add(copiedList[i]);
            }

            return returningList;
        }

        public static int LevelStep(Random random, Creature creature)
        {
            string userInput;
            int partyLevel = GetMinMaxInt("Please enter the party's current level: ", MinMax.hasBoth, 1, 20);

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
                            creatureLevel = PinkRandom(random, partyLevel - 7, partyLevel + 7);
                            break;
                        case "NPC":
                            creatureLevel = DisadvantageRandom(random, -1, 11);
                            break;
                        case "MONSTER":
                            creatureLevel = DisadvantageRandom(random, -1, 25);
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
                    userInput = GetValidString("Y", "N").ToUpper();
                }
            } while (userInput == "N");

            Console.WriteLine("Creature Level: " + creatureLevel);
            creature.Level = creatureLevel;

            for (int i = 11; i <= creature.Level; i++)
            {
                int[] extremeLevels = new int[] { 11, 15, 20 };
                if(extremeLevels.Contains(i))
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

            if(minMaxes.Length >= 1 || minMaxes.Length <= 2)
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
    }
}

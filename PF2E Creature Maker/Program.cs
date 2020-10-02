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
        Gear
    }

    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            Creature creature = new Creature();
            List<Creature> creatureSaves = new List<Creature>() { creature };

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
                Step.Gear
            };

            for (int step = 0; endSteps == false; step++)
            {
                if (step >= 0 && step < stepOrder.Length)
                {
                    Console.WriteLine("Step: " + stepOrder[step]);
                }

                if (step > 0)
                {
                    step = ContinueOrBack(stepOrder, step, creature, creatureSaves);
                }

                switch (stepOrder[step])
                {
                    case Step.Level:
                        {
                            partyLevel = LevelStep(random, creature);
                            break;
                        }
                    case Step.Size:
                        {
                            SizeStep(creature, random);
                            break;
                        }
                    default:
                        endSteps = true;
                        break;
                }

                creatureSaves.Add(CopyCreature(creature));
            }

            Console.WriteLine("Creature Level: " + creature._level);
            Console.WriteLine(creature._size);
        }

        public static Creature CopyCreature(Creature creature)
        {
            Creature creatureCopy = new Creature();

            creatureCopy._isStrengthExtreme = creature._isStrengthExtreme;
            creatureCopy._saveNumber = creature._saveNumber;
            creatureCopy._level = creature._level;
            creatureCopy._name = creature._name;
            creatureCopy._size = creature._size;
            creatureCopy._lastSaved = creature._lastSaved;
            creatureCopy._degreeList = CopyListValues(creature._degreeList);

            for (int i = 0; i < creature._abilityScores.Length; i++)
            {
                creatureCopy._abilityScores[i]._abilityBonus = creature._abilityScores[i]._abilityBonus;
            }

            return creatureCopy;
        }

        public static void SizeStep(Creature creature, Random random)
        {
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
                sizeOptionsArray[i] = new SizeOption();
                sizeOptionsArray[i]._name = sizeOptionsSplit[i][0];
                sizeOptionsArray[i]._minLevel = int.Parse(sizeOptionsSplit[i][1]);
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
            bool tooLarge = false; //test commit

            do
            {
                tooLarge = false;
                userInput = GetValidString(validSizeOptions);

                if (userInput == "")
                {
                    SizeOption randomSize = new SizeOption();
                    do
                    {
                        randomSize = sizeOptionsArray[GravityRandom(random, 0, sizeOptionsArray.Length - 1, mediumSizeIndex, weight: 2)];
                    } while (creature._level < randomSize._minLevel);
                    creature._size = randomSize._name;
                }
                else
                {
                    for (int i = 0; i < sizeOptionsArray.Length; i++)
                    {
                        if (userInput.ToLower() == sizeOptionsArray[i]._name.ToLower())
                        {
                            if (creature._level < sizeOptionsArray[i]._minLevel)
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
                                creature._size = sizeOptionsArray[i]._name;
                            }
                        }
                    }
                }
            } while (tooLarge == true);

            Console.WriteLine("Creature Size: " + creature._size);

            for (int i = 0; i < sizeOptionsArray.Length; i++)
            {
                if (sizeOptionsArray[i]._name.ToLower() == creature._size.ToLower())
                {
                    if (creature._level <= sizeOptionsArray[i]._maxExStrForSizeAndLevel)
                    {
                        creature._isStrengthExtreme = true;
                    }
                    break;
                }
            }

            Console.WriteLine("Creature's Strength is Extreme: " + creature._isStrengthExtreme);
        }

        public static void AbilityScoresStep()
        {

        }

        public static int GravityRandom(Random random, int min, int max, int gravityValue, int weight)
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
                    if (randValue == gravityValue - difference || randValue == gravityValue + difference)
                    {
                        return randValue;
                    }
                }
                difference++;
            } while (true);
        }

        public static int ContinueOrBack(Step[] stepOrder, int step, Creature creature, List<Creature> creatureSaves)
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
                        creatureSaves.RemoveAt(creatureSaves.Count - 1);
                        creature = CopyCreature(creatureSaves[creatureSaves.Count - 1]);
                        Console.WriteLine("Restoring creature saved at " + creature._lastSaved);
                        Console.WriteLine("Creature save number: " + creature._saveNumber);
                        
                        step -= 1;

                        Console.WriteLine("Returning to {0} Step", stepOrder[step]);
                    }
                }
                else
                {
                    creature._lastSaved = stepOrder[step];
                    creature._saveNumber++;
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
            string userInput = "";
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
            creature._level = creatureLevel;

            for (int i = 11; i <= creature._level; i++)
            {
                int[] extremeLevels = new int[] { 11, 15, 20 };
                if(extremeLevels.Contains(i))
                {
                    int removingDegree;
                    do
                    {
                        removingDegree = random.Next(0, creature._degreeList.Count);
                    } while (creature._degreeList[removingDegree] == Degree.ex);

                    creature._degreeList.RemoveAt(removingDegree);
                    creature._degreeList.Add(Degree.ex);
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

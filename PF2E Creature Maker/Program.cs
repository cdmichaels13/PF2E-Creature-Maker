using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Pipes;

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
        End
    }
    
    public enum CreatureType
    {
        NPC,
        Monster,
        Any
    }

    class Program
    {
        public static Creature _creature = new Creature();
        public static Dictionary<Step, Creature> _creatureSaves = new Dictionary<Step, Creature>();
        static void Main()
        {
            Random random = new Random();

            int partyLevel;
            bool endSteps = false;

            Step[] stepOrder = new Step[]
            {
                Step.NPCorMonster,
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
                            Console.WriteLine("Is this creature an NPC, Monster, or either?");
                            string userInput = GetValidString("NPC", "Monster", "either");
                            switch(userInput.ToUpper())
                            {
                                case "NPC":
                                    _creature.Type = CreatureType.NPC;
                                    break;
                                case "Monster":
                                    _creature.Type = CreatureType.Monster;
                                    break;
                                case "Either":
                                    _creature.Type = CreatureType.Any;
                                    break;
                            }
                            break;
                        }
                    case Step.Level:
                        {
                            partyLevel = ExecuteStep.LevelStep(random, _creature);
                            break;
                        }
                    case Step.Size:
                        {
                            ExecuteStep.SizeStep(_creature, random);
                            break;
                        }
                    case Step.Traits:
                        {
                            string userInput = "";
                            do
                            {
                                ExecuteStep.TraitsStep(_creature, random);
                                Console.WriteLine("Would you like to add any more traits? Y/N");
                                userInput = GetValidString("Y", "N");
                            } while (userInput.ToUpper() == "Y");
                            break;
                        }
                    case Step.End:
                        {
                            ExecuteStep.EndStep(_creature);
                            endSteps = true;
                            break;
                        }
                    default:
                        {
                            //endSteps = true;
                            break;
                        }
                }
            }
        }

        #region Method Region
        public static Creature CopyCreature(Creature creature)
        {
            Creature creatureCopy = new Creature
            {
                Level = creature.Level,
                Name = creature.Name,
                Size = creature.Size,
                IsStrengthExtreme = creature.IsStrengthExtreme,
                Type = creature.Type,
                DegreeList = CopyListValues(creature.DegreeList)
            };

            for (int i = 0; i < creature.AbilityScores.Length; i++)
            {
                creatureCopy.AbilityScores[i]._abilityBonus = creature.AbilityScores[i]._abilityBonus;
            }

            creatureCopy.traitPool.AllPossibleTraits.Clear();
            creatureCopy.traitPool.SelectedTraits.Clear();
            creatureCopy.traitPool.HumanoidIndexes.Clear();

            foreach (string trait in creature.traitPool.AllPossibleTraits)
            {
                creatureCopy.traitPool.AllPossibleTraits.Add(trait);
            }
            foreach (string trait in creature.traitPool.SelectedTraits)
            {
                creatureCopy.traitPool.SelectedTraits.Add(trait);
            }
            foreach (int index in creature.traitPool.HumanoidIndexes)
            {
                creatureCopy.traitPool.HumanoidIndexes.Add(index);
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

        public static List<Degree> CopyListValues(List<Degree> copiedList)
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

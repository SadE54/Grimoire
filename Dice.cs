using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace Grimoire
{
    public enum RollMode
    {
        Normal,
        Advantage,
        Disadvantage
    }

    public class RollInstruction
    {
        public int DiceCount { get; set; }
        public int DiceSides { get; set; }
        public int Modifier { get; set; }
        public RollMode Mode { get; set; }
    }

    public static class DiceExecutor
    {
        private static readonly Random rng = Random.Shared;

        private static List<int> RollOnce(int count, int sides)
        {
            var rolls = new List<int>(count);
            byte[] buffer = new byte[4];

            for (int i = 0; i < count; i++)
            {
                RandomNumberGenerator.Fill(buffer);
                int value = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
                int roll = (value % sides) + 1;
                rolls.Add(roll);
            }
            return rolls;
        }

        public static string ExecuteRollCommand(RollInstruction instr)
        {
            var sb = new StringBuilder();

            var roll1 = RollOnce(instr.DiceCount, instr.DiceSides);
            var total1 = roll1.Sum();

            List<int> roll2 = new();
            int total2 = 0;

            string modeStr = instr.Mode switch
            {
                RollMode.Advantage => " (advantage)",
                RollMode.Disadvantage => " (disadvantage)",
                _ => ""
            };

            if (instr.Mode == RollMode.Advantage || instr.Mode == RollMode.Disadvantage)
            {
                roll2 = RollOnce(instr.DiceCount, instr.DiceSides);
                total2 = roll2.Sum();
            }

            int finalTotal;
            List<int> finalRoll = new();

            if (instr.Mode == RollMode.Advantage)
            {
                (finalRoll, finalTotal) = (total1 >= total2) ? (roll1, total1) : (roll2, total2);
            }
            else if (instr.Mode == RollMode.Disadvantage)
            {
                (finalRoll, finalTotal) = (total1 <= total2) ? (roll1, total1) : (roll2, total2);
            }
            else
            {
                finalRoll = roll1;
                finalTotal = total1;
            }

            int finalWithModifier = finalTotal + instr.Modifier;
            int max = finalRoll.Max();

            sb.AppendLine($"[gold3_1]Rolling {instr.DiceCount}d{instr.DiceSides}{(instr.Modifier >= 0 ? $" + {instr.Modifier}" : $"{instr.Modifier}")}{modeStr} :");

            //sb.AppendLine($" → Result: [[{string.Join(", ", finalRoll.Select(r => r == max ? $"[red]{r}[/]" : r.ToString())) }]]");
            sb.AppendLine($" → Result: [[{string.Join(", ", roll1.Select(r => r == max ? $"[red]{r}[/]" : r.ToString()))}]]");

            //if (instr.Mode != RollMode.Normal && roll2 != null)//
            //sb.AppendLine($" → Other: [[{string.Join(", ", (instr.Mode == RollMode.Advantage ? roll2 : roll1))}]]");
            sb.AppendLine($" → Other: [[{string.Join(", ", roll2.Select(r => r == max ? $"[red]{r}[/]" : r.ToString()))}]]");

            sb.AppendLine($" → Total: {finalTotal} {(instr.Modifier != 0 ? $"({(instr.Modifier >= 0 ? " + " : "")}{instr.Modifier})" : "")} = {finalWithModifier}[/]");
            sb.AppendLine();

            return sb.ToString();

        }
    }
}

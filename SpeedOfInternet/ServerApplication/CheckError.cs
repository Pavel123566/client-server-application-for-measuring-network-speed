using System;
using System.Text.RegularExpressions;

namespace ServerApplication
{
    internal class CheckError
    {
        public static int CheckNumber()
        {
            var input = Console.ReadLine();

            if (int.TryParse(input, out var output))
                return output;

         
            return CheckNumber();
        }

        public static int CheckNumber(int min, int max)
        {
            var input = CheckNumber();

            if (input >= min && input <= max)
                return input;

            Console.WriteLine("Некорректный ввод... Повторите попытку");
            return CheckNumber(min, max);
        }

        public static float CheckFloat()
        {
            string input = Console.ReadLine();

            if (float.TryParse(input, out var output))
                return output;

            Console.WriteLine("Некорректный ввод... Повторите попытку");
            return CheckNumber();
        }

        public static string CheckWord(int l)
        {
            //Переименовать в CheckWord
            string line;

            line = Console.ReadLine();
            bool hasDigit = Regex.IsMatch(line, @"[^a-zA-Zа-яА-Я\s]");
            if (line.Length > l || hasDigit)
            {
                Console.WriteLine("Некорректный ввод... Повторите снова");
                CheckWord(l);
            }
            return line;
        }
    }
}
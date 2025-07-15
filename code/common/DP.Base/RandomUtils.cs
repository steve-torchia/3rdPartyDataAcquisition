using System;
using System.Collections.Generic;
using System.Linq;

namespace DP.Base
{
    public static class RandomUtils
    {
        public static Random Random = new Random(Environment.TickCount);

        public static string RandomString(int minLength, int maxLength)
        {
            var length = Random.Next(minLength, maxLength);

            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static List<string> GetListOfRandomStrings(int listLength, int stringLength = 8)
        {
            var retVal = new List<string>();

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[stringLength];
            var random = new Random();

            for (int x = 0; x < listLength; x++)
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                retVal.Add(new string(stringChars));
            }

            return retVal;
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = Random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        public static float RandomNumberBetween(float minValue, float maxValue)
        {
            var next = (float)Random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        public static decimal RandomNumberBetween(decimal minValue, decimal maxValue)
        {
            var next = (decimal)Random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}

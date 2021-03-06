﻿using System;
using System.Linq;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class CodeGenerator
    {
        /// <summary>
        /// Get the next code with the code specified
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns></returns>
        public static string GetNextCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var reversedCode = code.ToUpper().Reverse().ToArray();
            var charArray = new char[code.Length];
            var increment = 1;

            for (var i = 0; i < code.Length; i++)
                charArray[i] = getNextSymbol(reversedCode[i], ref increment);

            // reverse it back
            Array.Reverse(charArray);

            return new string(charArray);
        }

        /// <summary>
        /// Compute the next alpha character with the given character
        /// </summary>
        /// <param name="character">The base character</param>
        /// <param name="increment">The increment tells how the base character to increment </param>
        /// <returns></returns>
        private static char getNextSymbol(char character, ref int increment)
        {
            // convert the char to ascii code and get the next ASCII code
            var resultCode = (int)character + increment;

            // check if result code is ':' symbol
            if (resultCode == 58)
                return '0'; // back it to '0'

            // check if resultCode is '['
            if (resultCode == 91)
                return 'A'; // back it to letter 'A'

            // if we reach here, reset the increment to 0
            increment = 0;
            return (char)resultCode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.Helpers
{
    public static class StringUtility
    {
        public static string Base64Encode ( string plainText )
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static byte[] Base64Decode ( string encodedString )
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return data;
        }

        public static string Base64DecodeString ( string encodedString )
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }
    }
}

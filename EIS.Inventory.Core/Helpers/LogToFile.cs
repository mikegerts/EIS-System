using System;
using System.IO;
using System.Text;

namespace EIS.Inventory.Core.Helpers
{
    public static class LogToFile
    {
        public static void CreateLog ( string serviceSubject, string logText, string filePath, string fileName )
        {
            var outputText = new StringBuilder();

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            outputText.Append("\n\n");
            outputText.AppendLine(string.Format("Log File: {0} ", serviceSubject));
            outputText.AppendLine(string.Format("Date Time: {0}", DateTime.Now));

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(filePath + @"\" + fileName + ".txt", true))
            {
                outputFile.WriteLine(outputText.ToString());
                outputFile.WriteLine(logText);
            }
        }

    }
}

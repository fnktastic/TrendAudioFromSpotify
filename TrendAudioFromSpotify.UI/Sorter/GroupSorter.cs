using System;
using System.Collections;
using System.Text.RegularExpressions;


namespace TrendAudioFromSpotify.UI.Sorter
{
    public class GroupSorter : IComparer
    {
        private static readonly Regex numTextSplitRegex = new Regex(@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

        public int Compare(object o1, object o2)
        {
            var pl1 = o1 as Model.Group;
            var pl2 = o2 as Model.Group;

            string x = pl1.Name;
            string y = pl2.Name;
            x = x ?? "";
            y = y ?? "";
            string[] xParts = numTextSplitRegex.Split(x);
            string[] yParts = numTextSplitRegex.Split(y);

            bool firstXIsNumber = xParts[0].Length > 0 && Char.IsDigit(xParts[0][0]);
            bool firstYIsNumber = yParts[0].Length > 0 && Char.IsDigit(yParts[0][0]);

            if (firstXIsNumber != firstYIsNumber)
            {
                return x.CompareTo(y);
            }

            for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
            {
                int result;
                if (firstXIsNumber == (i % 2 == 0))
                { // Compare numbers.
                    long a = Int64.Parse(xParts[i]);
                    long b = Int64.Parse(yParts[i]);
                    result = a.CompareTo(b);
                }
                else
                { // Compare texts.
                    result = xParts[i].CompareTo(yParts[i]);
                }
                if (result != 0)
                {
                    return result;
                }
            }
            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}

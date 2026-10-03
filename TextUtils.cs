using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModMenu
{
    public static class TextUtils
    {
        public static List<string> SplitLineIntoChunks(string str, string separator, int maxChunkLength)
        {
            List<string> strings = [];
            foreach (string s in str.Split(separator).ToList())
            {
                if (s.Length > maxChunkLength)
                {
                    int lastSpaceIndex = maxChunkLength - 1;
                    for (int i = 0; i < maxChunkLength; i++)
                    {
                        if (s[i] == ' ') lastSpaceIndex = i;
                    }

                    strings.Add(s.Substring(0, lastSpaceIndex));

                    List<string> split = SplitLineIntoChunks(s.Substring(lastSpaceIndex + 1), separator, maxChunkLength);
                    strings.AddRange(split);
                }
                else
                {
                    strings.Add(s);
                }
            }
            return strings;
        }



        public static string UnCamelCase(string text)
        {
            // Case 1: Inserts space between lowercase/digit and an uppercase letter
            // Case 2: Inserts space between an uppercase letter and the start of a new camelCase word
            return Regex.Replace(text, @"([a-z0-9])([A-Z])|([A-Z])([A-Z][a-z])", "$1$3 $2$4");
        }



        public static string BeautifyString(string str)
        {
            return $"<cspace=0.2em>{str.Replace(" ", "<space=0.72em>")}</cspace>";
        }



        public static string PadStringWithLines(string str)
        {
            return PadStringWithLines(str, MenuConstants.titleLabelLength);
        }



        public static string PadStringWithLines(string str, int length)
        {
            int spacesTot = MenuConstants.titleLabelSpacePadding * 2;
            string output = str;
            if (str.Length + MenuConstants.titleLabelSpacePadding < length)
            {
                int numDashesPerSide = (length - str.Length) / 2 - MenuConstants.titleLabelSpacePadding;
                output = new string('-', numDashesPerSide) + new string(' ', spacesTot) + str + new string(' ', spacesTot) + new string('-', numDashesPerSide);
            }
            return output;
        }



        // public static void SetTextMode(GameObject gameObject, FontStyles style)
        // {
        //     TextMeshProUGUI text = gameObject.transform.Find("ValueGroup/Value").GetComponent<TextMeshProUGUI>();
        //     if (text != null) text.fontStyle = style;
        // }
    }
}
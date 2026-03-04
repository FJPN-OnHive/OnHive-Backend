using System.Text;
using System.Text.RegularExpressions;

namespace OnHive.Core.Library.Extensions
{
    public static class StringExtensions
    {
        private static List<string> MaskChars = new List<string> { ".", "-", "/", "(", ")", " ", "+", ":" };

        public static string HashMd5(this string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }

        public static string HashSha1(this string input)
        {
            using (System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = sha.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }

        public static string RemoveMask(this string input)
        {
            return RemoveMask(input, MaskChars);
        }

        public static string RemoveMask(this string input, List<string> maskChars)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            foreach (var mask in maskChars)
            {
                input = input.Replace(mask, "");
            }
            return input;
        }

        public static double Similarity(this string str1, string str2)
        {
            var pairs1 = WordLetterPairs(str1.ToUpper());
            var pairs2 = WordLetterPairs(str2.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                for (int j = 0; j < pairs2.Count; j++)
                {
                    if (pairs1[i] == pairs2[j])
                    {
                        intersection++;
                        pairs2.RemoveAt(j);
                        break;
                    }
                }
            }

            return (2.0 * intersection * 100) / union;
        }

        public static string EscapeXml(this string value)
        {
            return System.Security.SecurityElement.Escape(value);
        }

        private static List<string> WordLetterPairs(string str)
        {
            List<string> AllPairs = new List<string>();

            string[] Words = Regex.Split(str, @"\s");

            for (int w = 0; w < Words.Length; w++)
            {
                if (!string.IsNullOrEmpty(Words[w]))
                {
                    String[] PairsInWord = LetterPairs(Words[w]);

                    for (int p = 0; p < PairsInWord.Length; p++)
                    {
                        AllPairs.Add(PairsInWord[p]);
                    }
                }
            }
            return AllPairs;
        }

        private static string[] LetterPairs(string str)
        {
            int numPairs = str.Length - 1;
            string[] pairs = new string[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                pairs[i] = str.Substring(i, 2);
            }
            return pairs;
        }
    }
}
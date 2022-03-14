namespace Validators
{
    public class ValidationUtilities
    {
        public static bool MinimumCharOccurrence(string str, char[] chars, int occureMin)
        {
            // TODO : Maybe come around to this at a later time.
            if (occureMin == 0)
                return true;

            int charOccurrences = 0;
            foreach (char c in chars)
            {
                if (str.Contains(c))
                {
                    charOccurrences++;
                    if (charOccurrences >= occureMin)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ContainsDigits(string str) => str.Any(char.IsDigit);

        public static bool ContainsLowercase(string str) => str.Any(char.IsLower);

        public static bool ContainsUppercase(string str) => str.Any(char.IsUpper);

        public static bool StringContainsOnlyValidChars(string str, char[] validChars)
        {
            if (str.Any(x => !validChars.Contains(x)))
                return false;
            return true;
        }
    }
}

namespace Rishvi.Modules.Core.Helpers
{
    public class RandomString
    {
        public static string Generate(int intLength)
        {
            string empty = string.Empty;
            string[] strArray = new string[62]
            {
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
        "S",
        "T",
        "U",
        "V",
        "W",
        "X",
        "Y",
        "Z",
        "a",
        "b",
        "c",
        "d",
        "e",
        "f",
        "g",
        "h",
        "i",
        "j",
        "k",
        "l",
        "m",
        "n",
        "o",
        "p",
        "q",
        "r",
        "s",
        "t",
        "u",
        "v",
        "w",
        "x",
        "y",
        "z"};
            Random random = new Random((int)DateTime.Now.Ticks);
            if (intLength < 4)
            {
                intLength = 4;
            }

            for (int index = 0; index < intLength; ++index)
            {
                empty += strArray[random.Next(1, strArray.Length)].ToString();
            }

            return empty;
        }

        public static string NumberGenerate(int intLength)
        {
            string empty = string.Empty;
            string[] strArray = new string[10]
            {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9"};
            Random random = new Random((int)DateTime.Now.Ticks);
            if (intLength < 4)
            {
                intLength = 4;
            }

            for (int index = 0; index < intLength; ++index)
            {
                empty += strArray[random.Next(1, strArray.Length)].ToString();
            }

            return empty;
        }
    }
}

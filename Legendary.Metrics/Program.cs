namespace Legendary.Metrics
{ 
    public static class Program
    {
        public static void Main(string[] args)
        { 
            var dirs = new string[1] { "/Users/matthewmartin/Projects/Legendary/Legendary.Core" };

            int csLines = 0;

            foreach (string dir in dirs)
            {
                var dirInfo = new DirectoryInfo(dir);

                var csFiles = dirInfo.GetFiles("*.cs");

                foreach (var file in csFiles)
                {
                    if (!string.IsNullOrWhiteSpace(file.DirectoryName))
                    {
                        csLines += TotalLines(file.DirectoryName);
                    }
                }
            }

            Console.WriteLine($"Read {csLines} total lines of C#, including comments.");
        }

        private static int TotalLines(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }
    }

}



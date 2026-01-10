using System;

namespace TildeTest
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Testing PathHelpers.ExpandPath function:");
            
            // Test cases
            string[] testPaths = {
                "~",
                "~/test.txt",  
                "~/.bashrc",
                "/absolute/path",
                "relative/path",
                "~/Documents/test file.txt"
            };
            
            foreach (var path in testPaths)
            {
                var expanded = PathHelpers.ExpandPath(path);
                Console.WriteLine($"'{path}' -> '{expanded}'");
            }
        }
    }
}
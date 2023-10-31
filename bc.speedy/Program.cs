using System;
namespace bc.speedy
{
    internal class Program
    {
        private static SchemaGenerator generator = null;
        static void Main(string[] args)
        {
            if (generator == null) // create on first pass.
            {
                generator = new SchemaGenerator();
            }
            Console.WriteLine("Enter some text expression to be evaluated? ");
            string input, output;
            do
            {
                Console.WriteLine();
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) {

                    try
                    {
                        output = generator.Parse(input);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine(output);
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error in query generation: {ex}");
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }
            } while (!string.IsNullOrWhiteSpace(input));
        }
    }
}
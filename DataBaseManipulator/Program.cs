using EMAS.Model;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Choose table:");
        Console.WriteLine("1 - Location.");
        Console.WriteLine("2 - Employee.");
        Console.WriteLine("3 - Equipment.");
        Console.WriteLine("or:");
        Console.WriteLine("C - Clear all Tables.");
        char command;
        command = Console.ReadKey().KeyChar;

        switch (command)
        {
            case '1':
                {
                    break;
                }
            case '2':
                {
                    break;
                }
            case '3':
                {
                    break;
                }
            case 'C':
                {
                    break;
                }
        }
            
    }
}
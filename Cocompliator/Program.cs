using Cocompliator;

class Program
{
    static void Main()
    {
        string filePath = "C:\\Users\\user\\source\\repos\\Cocompliator\\Cocompliator\\Program.txt";
        try
        {
            string cleanedContent = FileReader.Read(filePath);
            Console.WriteLine(cleanedContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
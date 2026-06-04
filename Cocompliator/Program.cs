using System;
using System.IO;

namespace Cocompliator
{
    public static class Programm
    {
        public static void Main()
        {
            try
            {
                // Универсальный путь к файлу теста. Ищет папку Tests в корне проекта.
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectDir = Directory.GetParent(baseDir).Parent.Parent.Parent.FullName;
                string filePath = Path.Combine(projectDir, "Tests", "sort_test.txt");
                
                var code = FileReader.Read(filePath);

                LexicalAnalyzer.IsLexicalCorrect(code);
                var terminals = LexicalAnalyzer.GetTerminals();

                var rpn = SyntaxAnalyzer.GenerateRPN(terminals);

                RPNInterpreter.ExecuteInstructions(rpn);
            }
            catch (CompilerException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ОШИБКА КОМПИЛЯЦИИ/ВЫПОЛНЕНИЯ]");
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine($"---> Строка: {ex.LineNumber}, Символ: {ex.CharPosition}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n[КРИТИЧЕСКАЯ СИСТЕМНАЯ ОШИБКА]: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
using System;
using System.IO;

namespace Cocompliator
{
    // Главный класс программы, отвечающий за запуск
    public static class Programm
    {
        public static void Main()
        {
            try
            {
                // Название нашего файла с формулой
                string testFileName = "C:\\Users\\user\\source\\repos\\Cocompliator\\Cocompliator\\Tests\\error_2.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
                
                // Чтение исходного кода из файла через ваш FileReader
                var code = FileReader.Read(filePath);

                // Лексический анализ кода
                LexicalAnalyzer.IsLexicalCorrect(code);
                var terminals = LexicalAnalyzer.GetTerminals();

                // Трансляция терминалов в ОПС
                var rpn = SyntaxAnalyzer.GenerateRPN(terminals);

                // Выполнение формулы
                RPNInterpreter.ExecuteInstructions(rpn);
            }
            catch (CompilerException ex)
            {
                Console.WriteLine($"\nОшибка компиляции: {ex.Message} на строке {ex.LineNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }
        }
    }
}
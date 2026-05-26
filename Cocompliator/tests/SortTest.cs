using System;
using System.IO;

namespace Cocompliator
{
   /// @brief Класс для загрузки и подготовки теста сортировки массива
   public static class SortTest
   {
      /// @brief Чтение исходного кода теста из внешнего файла
      /// @param filePath Путь к файлу теста sort_test.txt
      /// @return Очищенная строка с кодом программы для интерпретатора
      public static string LoadProgram( string filePath )
      {
         // Используем готовый класс FileReader ваших коллег для считывания файла
         return FileReader.Read( filePath );
      }
   }
}
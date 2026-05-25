using System;
using System.Collections.Generic;
using System.Text;

namespace Cocompliator
{
    /// @brief Класс для считывания текста из файла
    public static class FileReader
    {
        /// @brief Чтение файла
        /// @param filePath полный путь к файлу
        public static string Read( string filePath )
        {
            if ( !File.Exists( filePath ) )
            {
                throw new FileNotFoundException( $"File not found: {filePath}" );
            }

            string content = File.ReadAllText( filePath );
            return RemoveTabsAndCarriageReturns(content);

        }

        /// @brief Функция удаления табуляции и кареток
        /// @param input прочитанный текст из файла
        private static string RemoveTabsAndCarriageReturns( string input )
        {
            if ( string.IsNullOrEmpty( input ) )
                return input;

            return input.Replace( "\t", "" ).Replace( "\r", "" );
        }

    }
}

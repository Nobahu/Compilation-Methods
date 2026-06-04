using System;

namespace Cocompliator
{
    /// @brief Класс, отвечающий за вывод ошибок
    public class CompilerException : Exception
    {
        public int LineNumber { get; set; }
        public int CharPosition { get; set; }

        public CompilerException(string message, int lineNumber, int charPosition) : base(message)
        {
            LineNumber = lineNumber;
            CharPosition = charPosition;
        }

        // --- 1. ЛЕКСИЧЕСКИЕ ОШИБКИ ---
        public static CompilerException LexicalUnexpectedChar(char c, int line, int col) =>
            new CompilerException($"Лексическая ошибка: Недопустимый или неизвестный символ '{c}'.", line, col);

        public static CompilerException LexicalUnclosedString(int line, int col) =>
            new CompilerException("Лексическая ошибка: Незакрытая строковая константа (отсутствует закрывающая кавычка '\u0022').", line, col);

        // --- 2. СИНТАКСИЧЕСКИЕ ОШИБКИ ---
        public static CompilerException SyntaxTerminalMismatch(string expected, string actual, int line, int col) =>
            new CompilerException($"Синтаксическая ошибка: Ожидалось '{expected}', но встречено '{actual}'.", line, col);

        public static CompilerException SyntaxUnexpectedToken(string token, string expectedTokens, int line, int col) =>
            new CompilerException($"Синтаксическая ошибка: Неожиданный токен '{token}'. Ожидалось одно из: [ {expectedTokens} ].", line, col);

        public static CompilerException SyntaxUnexpectedEOF(int line, int col) =>
            new CompilerException("Синтаксическая ошибка: Неожиданный конец файла (возможно, пропущена '}' или ';').", line, col);

        public static CompilerException SyntaxTrailingGarbage(string token, int line, int col) =>
            new CompilerException($"Синтаксическая ошибка: Лишний символ '{token}' после завершения программы.", line, col);

        public static CompilerException SyntaxExpectedIdentifier(int line, int col) =>
            new CompilerException("Синтаксическая ошибка: Ожидалось имя переменной.", line, col);

        public static CompilerException SyntaxInvalidAssignmentTarget(int line, int col) =>
            new CompilerException("Синтаксическая ошибка: Левая часть выражения не может быть целью для присваивания.", line, col);

        // --- 3. ОШИБКИ ВРЕМЕНИ ВЫПОЛНЕНИЯ (РАНТАЙМ) ---
        public static CompilerException RuntimeStackUnderflow(int line, int col) =>
            new CompilerException("Критическая ошибка выполнения: Некорректное выражение (недостаточно операндов).", line, col);

        public static CompilerException RuntimeVariableNotInit(string varName, int line, int col) =>
            new CompilerException($"Ошибка выполнения: Использование неинициализированной переменной '{varName}'.", line, col);

        public static CompilerException RuntimeArrayNotDeclared(string arrName, int line, int col) =>
            new CompilerException($"Ошибка выполнения: Массив '{arrName}' не объявлен.", line, col);

        public static CompilerException RuntimeIndexOutOfBounds(string arrName, int index, int size, int line, int col) =>
            new CompilerException($"Ошибка доступа к памяти: Индекс {index} выходит за границы массива '{arrName}' (размер: {size}).", line, col);

        public static CompilerException RuntimeDivideByZero(int line, int col) =>
            new CompilerException("Математическая ошибка: Деление на ноль недопустимо.", line, col);

        public static CompilerException RuntimeInvalidArraySize(string arrName, int size, int line, int col) =>
            new CompilerException($"Ошибка выделения памяти: Размер массива '{arrName}' должен быть больше нуля (получено: {size}).", line, col);

        public static CompilerException RuntimeMathNegativeSqrt(double val, int line, int col) =>
            new CompilerException($"Математическая ошибка: Попытка вычислить корень из отрицательного числа ({val}).", line, col);

        public static CompilerException RuntimeFormatError(string input, int line, int col) =>
            new CompilerException($"Ошибка формата: Введено нечисловое значение '{input}'.", line, col);

        public static CompilerException RuntimeInvalidOperand(string op, int line, int col) =>
            new CompilerException($"Ошибка выполнения: Оператор '{op}' применим только к переменным.", line, col);

        public static CompilerException RuntimeTypeError(string expected, int line, int col) =>
            new CompilerException($"Ошибка типов: Ожидался тип {expected}.", line, col);

        public static CompilerException RuntimeNegativeMultiplier(int line, int col) => 
            new CompilerException("Отрицательный множитель для строки", line, col);

        public static CompilerException RuntimeInvalidStringMultiplication(int line, int col) => 
            new CompilerException("Операция '*' требует строку и число", line, col);

        public static CompilerException VariableAlreadyDeclared(string varName, int line, int col) => 
            new CompilerException($"Переменная '{varName}' уже объявлена", line, col);
    }
}
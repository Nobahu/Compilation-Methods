using System;
using System.Collections.Generic;
using System.Text;

namespace Cocompliator
{
   public enum TerminalType
    {
        /// Константа
        Number,
        /// Целое число
        Int,
        /// Строка текста
        Text,
        /// Булевое значение
        Boolean,
        /// Арифметические операции
        Plus,
        Minus,
        Multiply,
        Divide,
        /// Логические операторы
        Not,
        And,
        Or,
        /// Левая круглая скобка
        LeftParenthesis,
        /// Правая круглая скобка
        RightParenthesis,
        /// Левая квадратная скобка
        LeftBracket,
        /// Правая квадратная скобка
        RightBracket,
        /// Левая фигурная скобка
        LeftBrace,
        /// Правая фигурная скобка
        RightBrace,
        /// Присваивание
        Assignment,
        /// Имя переменной
        VariableName,
        /// Условный оператор если
        If,
        /// Условный оператор иначе
        Else,
        /// Цикл while
        While,
        /// Операторы сравнения
        Equal,
        NotEqual,
        Less,
        Greater,
        LessEqual,
        GreaterEqual,
        /// Ввод данных
        Read,
        /// Вывод данных
        Write,
        /// Оператор конца строки ;
        Semicolon,
        /// Функция квадратного корня
        Sqrt,
        /// Функция возведения в степень
        Pow,
        /// Функция синуса числа
        Sin,
        /// Функция косинуса числа
        Cos,
        /// Функция экспоненты
        Exp
    }

    public enum RPNType
    {
        // ОПЕРАЦИИ (F_)
        /// Функция вывода: Output(A)
        F_Write,
        /// Функция ввода: Input(A)
        F_Read,
        /// Присваивание: A = B (в RPN: B A F_Assignment)
        F_Assignment,
        /// Равенство: A == B
        F_Equal,
        /// Неравенство: A != B
        F_NotEqual,
        /// Меньше: A < B
        F_Less,
        /// Больше: A > B
        F_Greater,
        /// Меньше или равно: A <= B
        F_LessEqual,
        /// Больше или равно: A >= B
        F_GreaterEqual,
        /// Сложение/конкатенация: A + B
        F_Plus,
        /// Вычитание/унарный минус: A - B или -A
        F_Minus,
        /// Умножение: A * B
        F_Multiply,
        /// Деление: A / B
        F_Divide,
        /// Остаток от деления: A % B
        F_Modulus,
        /// Постфиксный инкремент ++
        F_PostIncrement,
        /// Постфиксный декремент --
        F_PostDecrement,
        /// Логические операторы
        F_Not,
        F_And,
        F_Or,
        /// Квадратный корень: sqrt(A)
        F_Sqrt,
        /// Возведение в степень: pow(A,B) (A^B)
        F_Pow,
        /// Вычисление синуса: sin(A)
        F_Sin,
        /// Вычисление косинуса: cos(A)
        F_Cos,
        /// Вычисление экспоненты': exp(A)
        F_Exp,
        /// Доступ к элементу массива: A[B] (в RPN: A B F_Index)
        F_Index,
        /// Объявление переменной int: int A
        F_Int,
        /// Объявление переменной string: string A
        F_String,
        /// Объявление переменной bool: bool A
        F_Bool,
        /// Объявление массива int: int[] A (размер B) (в RPN: B A F_IntArray)
        F_IntArray,
        /// Объявление массива string: string[] A (размер B)
        F_StringArray,
        /// Объявление массива bool: bool[] A (размер B)
        F_BoolArray,
        // АРГУМЕНТЫ/ОПЕРАНДЫ (A_)
        /// Числовой литерал
        A_Number,
        /// Строковый литерал
        A_TextLine,
        /// Булевый литерал
        A_Boolean,
        /// Имя переменной
        A_VariableName,
        // СЛУЖЕБНЫЕ ТОКЕНЫ RPN (T_)
        /// Ключевое слово if (для RPN)
        T_If,
        /// Ключевое слово else (для RPN)
        T_Else,
        /// Ключевое слово while (для RPN)
        T_While,
        /// Точка с запятой (служебный символ RPN)
        T_Semicolon,
        /// Открывающая круглая скобка (служебный символ RPN)
        T_LeftParenthesis,
        /// Закрывающая круглая скобка (служебный символ RPN)
        T_RightParenthesis,
        /// Открывающая квадратная скобка (служебный символ RPN)
        T_LeftBracket,
        /// Закрывающая квадратная скобка (служебный символ RPN)
        T_RightBracket,
        /// Открывающая фигурная скобка (служебный символ RPN)
        T_LeftBrace,
        /// Закрывающая фигурная скобка (служебный символ RPN)
        T_RightBrace,
        // УПРАВЛЕНИЕ ПОТОКОМ (F_)
        /// Условный переход: если на вершине стека FALSE, перейти к метке М_Mark
        F_ConditionalJumpToMark,
        /// Безусловный переход к метке М_Mark
        F_UnconditionalJumpToMark,
        // МЕТКИ (М_)
        /// Метка-указатель для переходов
        М_Mark,
    }

    public enum MarkType
    {
        /// Метка для обозначения начала цикла while.
        WhileBeginMark,
        /// Метка для обозначения конца тела цикла while (перед проверкой условия для следующей итерации).
        WhileEndMark,
        /// Метка для перехода в случае ложности условия оператора if (переход к блоку else или за пределы if).
        IfMark,
        /// Метка для безусловного перехода в конце блока if (чтобы пропустить блок else).
        ElseMark,
    }
}

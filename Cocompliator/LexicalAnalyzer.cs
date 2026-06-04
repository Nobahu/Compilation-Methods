using System;
using System.Collections.Generic;

namespace Cocompliator
{
    public static class LexicalAnalyzer
    {
        private static string Data;
        private static int _pointer = 0;
        private static int _charPointer = 1;
        private static int _linePointer = 1;
        private static List<Terminal> Terminals = new List<Terminal>();

        public static bool IsLexicalCorrect(string data)
        {
            Data = data + " "; /// Добавлен пробел для гарантированного чтения последнего токена
            _pointer = 0;
            _charPointer = 1;
            _linePointer = 1;
            Terminals.Clear();

            int state = 0;
            string buffer = "";
            int startLine = 1, startChar = 1;

            while (_pointer < Data.Length)
            {
                char c = Data[_pointer];
                int col = GetCharColumn(c);

                if (state == 0)
                {
                    startLine = _linePointer;
                    startChar = _charPointer;
                }

                /// Получение действия из таблицы переходов
                StateTransition action = TransitionTable.Matrix[state, col];

                if (action.IsError)
                {
                    throw CompilerException.LexicalUnexpectedChar(c, _linePointer, _charPointer);
                }

                if (action.NextState != -1)
                {
                    if (state == 0 && action.NextState == 0 && (col == 18 || col == 22))
                    {
                        AdvancePointer();
                    }
                    else
                    {
                        if ((state == 11 || state == 12) && action.NextState == 0) buffer = "";
                        else buffer += c;

                        state = action.NextState;
                        AdvancePointer();
                    }
                }
                else
                {
                    if (!action.IsZStar)
                    {
                        buffer += c;
                        AdvancePointer();
                    }

                    TerminalType finalToken = action.Token;
                    if (action.CheckKeyword)
                        finalToken = CheckIfKeyword(buffer);

                    SaveTerminal(finalToken, buffer, startLine, startChar);

                    state = 0;
                    buffer = "";
                }
            }

            /// Проверка на незакрытую строку в конце файла
            if (state == 12)
            {
                throw CompilerException.LexicalUnclosedString(_linePointer, _charPointer);
            }

            return true;
        }

        private static void AdvancePointer()
        {
            if (_pointer < Data.Length && Data[_pointer] == '\n')
            {
                _linePointer++;
                _charPointer = 0;
            }
            _pointer++;
            _charPointer++;
        }

        private static TerminalType CheckIfKeyword(string word) => word switch
        {
            "if" => TerminalType.If,
            "else" => TerminalType.Else,
            "while" => TerminalType.While,
            "read" => TerminalType.Read,
            "write" => TerminalType.Write,
            "sqrt" => TerminalType.Sqrt,
            "sin" => TerminalType.Sin,
            "cos" => TerminalType.Cos,
            "int" => TerminalType.Int,
            "exp" => TerminalType.Exp,
            "string" => TerminalType.String,
            _ => TerminalType.VariableName
        };

        private static void SaveTerminal(TerminalType type, string value, int line, int ch)
        {
            if (type == TerminalType.Number)
            {
                Terminals.Add(new Terminal.Number(type, line, ch, value));
            }
            else if (type == TerminalType.VariableName || type == TerminalType.Text)
            {
                /// Если это строковый литерал – удаляем окружающие кавычки
                if (type == TerminalType.Text && value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                {
                    value = value.Substring(1, value.Length - 2);
                }
                Terminals.Add(new Terminal.Identifier(type, line, ch, value));
            }
            else
            {
                Terminals.Add(new Terminal(type, line, ch));
            }
        }

        private static int GetCharColumn(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_') return 0;
            if (char.IsDigit(c)) return 1;
            if (c == '+') return 2;
            if (c == '-') return 3;
            if (c == '*') return 4;
            if (c == '/') return 5;
            if (c == '=') return 6;
            if (c == '<') return 7;
            if (c == '>') return 8;
            if (c == '!') return 9;
            if (c == '(') return 10;
            if (c == ')') return 11;
            if (c == '[') return 12;
            if (c == ']') return 13;
            if (c == '{') return 14;
            if (c == '}') return 15;
            if (c == ';') return 16;
            if (c == ',') return 17;
            if (c == ' ' || c == '\r' || c == '\t') return 18;
            if (c == '\n') return 22;
            if (c == '#') return 19; 
            if (c == '&') return 20; /// Не используется
            if (c == '"') return 21;
            return 23; 
        }

        public static List<Terminal> GetTerminals() => Terminals;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cocompliator
{
    public static class RPNTranslator
    {
        public static List<RPNSymbol> ConvertToRPN(List<Terminal> inputTerminals)
        {
            var output = new List<RPNSymbol>();
            var stack = new Stack<Terminal>();

            foreach (var token in inputTerminals)
            {
                // Если число или переменная - сразу в ОПС
                if (token.TerminalType == TerminalType.Number || token.TerminalType == TerminalType.VariableName)
                {
                    output.Add(TranslateOperand(token));
                }
                // Если это функция (read, write, sqrt) - в стек
                else if (IsFunction(token.TerminalType))
                {
                    stack.Push(token);
                }
                // Если открывающая скобка — в стек
                else if (token.TerminalType == TerminalType.LeftParenthesis)
                {
                    stack.Push(token);
                }
                // Если закрывающая скобка — выталкиваем всё до открывающей
                else if (token.TerminalType == TerminalType.RightParenthesis)
                {
                    while (stack.Count > 0 && stack.Peek().TerminalType != TerminalType.LeftParenthesis)
                        output.Add(TranslateOperator(stack.Pop()));

                    if (stack.Count > 0) stack.Pop(); // Удаляем '('

                    // Если перед скобкой была функция, выталкиваем и её
                    if (stack.Count > 0 && IsFunction(stack.Peek().TerminalType))
                        output.Add(TranslateOperator(stack.Pop()));
                }
                // Если оператор (+, -, *, /, =)
                else if (IsOperator(token.TerminalType))
                {
                    while (stack.Count > 0 && stack.Peek().TerminalType != TerminalType.LeftParenthesis &&
                          GetPriority(stack.Peek().TerminalType) >= GetPriority(token.TerminalType))
                    {
                        output.Add(TranslateOperator(stack.Pop()));
                    }
                    stack.Push(token);
                }
                // Конец строки (;) - выталкиваем всё оставшееся из стека
                else if (token.TerminalType == TerminalType.Semicolon)
                {
                    while (stack.Count > 0)
                    {
                        var op = stack.Pop();
                        if (op.TerminalType != TerminalType.LeftParenthesis && op.TerminalType != TerminalType.RightParenthesis)
                            output.Add(TranslateOperator(op));
                    }
                }
            }

            while (stack.Count > 0)
                output.Add(TranslateOperator(stack.Pop()));

            return output;
        }

        private static bool IsFunction(TerminalType type) =>
            type == TerminalType.Read || type == TerminalType.Write || 
            type == TerminalType.Sqrt || type == TerminalType.Pow || 
            type == TerminalType.Sin || type == TerminalType.Cos;

        private static bool IsOperator(TerminalType type) =>
            type == TerminalType.Plus || type == TerminalType.Minus ||
            type == TerminalType.Multiply || type == TerminalType.Divide ||
            type == TerminalType.Assignment;

        private static int GetPriority(TerminalType type) => type switch
        {
            TerminalType.Assignment => 1,
            TerminalType.Plus or TerminalType.Minus => 2,
            TerminalType.Multiply or TerminalType.Divide => 3,
            _ => 0
        };

        private static RPNSymbol TranslateOperand(Terminal token)
        {
            if (token.TerminalType == TerminalType.Number)
                return new RPNNumber(RPNType.A_Number) { Data = (token as Terminal.Number).Data };
            else
                return new RPNIdentifier(RPNType.A_VariableName) { Name = (token as Terminal.Identifier).Name };
        }

        private static RPNSymbol TranslateOperator(Terminal token) => token.TerminalType switch
        {
            TerminalType.Plus => new RPNSymbol(RPNType.F_Plus),
            TerminalType.Minus => new RPNSymbol(RPNType.F_Minus),
            TerminalType.Multiply => new RPNSymbol(RPNType.F_Multiply),
            TerminalType.Divide => new RPNSymbol(RPNType.F_Divide),
            TerminalType.Assignment => new RPNSymbol(RPNType.F_Assignment),
            TerminalType.Read => new RPNSymbol(RPNType.F_Read),
            TerminalType.Write => new RPNSymbol(RPNType.F_Write),
            TerminalType.Sqrt => new RPNSymbol(RPNType.F_Sqrt),
            TerminalType.Pow => new RPNSymbol(RPNType.F_Pow),
            _ => throw new Exception("Неизвестный оператор в ОПС")
        };
    }
}
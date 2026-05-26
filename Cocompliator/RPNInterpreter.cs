using System;
using System.Collections.Generic;

namespace Cocompliator
{
    public class RPNInterpreter
    {
        public static void ExecuteInstructions(List<RPNSymbol> rpn)
        {
            var variables = new Dictionary<string, double>();
            var stack = new Stack<RPNSymbol>();

            foreach (var symbol in rpn)
            {
                if (symbol.RPNType == RPNType.A_VariableName || symbol.RPNType == RPNType.A_Number)
                {
                    stack.Push(symbol);
                }
                //АРИФМЕТИКА
                else if (symbol.RPNType == RPNType.F_Plus || symbol.RPNType == RPNType.F_Minus ||
                        symbol.RPNType == RPNType.F_Multiply || symbol.RPNType == RPNType.F_Divide)
                {
                    double val2 = ResolveValue(stack.Pop(), variables);
                    double val1 = ResolveValue(stack.Pop(), variables);
                    double result = 0;

                    if (symbol.RPNType == RPNType.F_Plus) result = val1 + val2;
                    if (symbol.RPNType == RPNType.F_Minus) result = val1 - val2;
                    if (symbol.RPNType == RPNType.F_Multiply) result = val1 * val2;
                    if (symbol.RPNType == RPNType.F_Divide) result = val1 / val2;

                    stack.Push(new RPNNumber(RPNType.A_Number) { Data = (int)result, DoubleData = result });
                }
                //ПРИСВАИВАНИЕ (=)
                else if (symbol.RPNType == RPNType.F_Assignment)
                {
                    double val = ResolveValue(stack.Pop(), variables);
                    var target = stack.Pop() as RPNIdentifier;
                    variables[target.Name] = val;
                }
                // ВВОД (read)
                else if (symbol.RPNType == RPNType.F_Read)
                {
                    var target = stack.Pop() as RPNIdentifier;
                    Console.Write($"Введите значение для переменной {target.Name}: ");
                    variables[target.Name] = Convert.ToDouble(Console.ReadLine().Replace('.', ','));
                }
                //ВЫВОД (write)
                else if (symbol.RPNType == RPNType.F_Write)
                {
                    double val = ResolveValue(stack.Pop(), variables);
                    Console.WriteLine($">>> РЕЗУЛЬТАТ: {val}");
                }
                //ФУНКЦИЯ КОРНЯ (sqrt)
                else if (symbol.RPNType == RPNType.F_Sqrt)
                {
                    double val = ResolveValue(stack.Pop(), variables);
                    stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = Math.Sqrt(val) });
                }
            }
        }

        private static double ResolveValue(RPNSymbol sym, Dictionary<string, double> vars)
        {
            if (sym is RPNNumber num) 
                return num.DoubleData == 0 && num.Data != 0 ? num.Data : num.DoubleData;
            
            if (sym is RPNIdentifier id)
            {
                if (vars.ContainsKey(id.Name)) return vars[id.Name];
                else throw new Exception($"Переменная '{id.Name}' не инициализирована!");
            }
            throw new Exception("Неверный тип операнда!");
        }
    }

    public class RPNNumber : RPNSymbol
    {
        public int Data { get; set; }
        public double DoubleData { get; set; }
        public RPNNumber(RPNType type) : base(type) { }
    }

    public class RPNIdentifier : RPNSymbol
    {
        public string Name { get; set; }
        public RPNIdentifier(RPNType type) : base(type) { }
    }
}
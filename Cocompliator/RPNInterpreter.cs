using System;
using System.Collections.Generic;

namespace Cocompliator
{
    public class RPNInterpreter
    {
        public static void ExecuteInstructions(List<RPNSymbol> rpn)
        {
            try
            {
                if ( rpn == null )
                {
                    throw new Exception($"Error: Отсутствие кода программы");
                }

                /// Отладка
                //Console.WriteLine("=== RPN INSTRUCTIONS ===");
                //for (int i = 0; i < rpn.Count; i++)
                //{
                //    var s = rpn[i];
                //    string info = s.RPNType.ToString();
                //    if (s is RPNIdentifier id)
                //        info += $" ({id.Name})";
                //    else if (s is RPNNumber num)
                //        info += $" ({num.Data})";
                //    else if (s is RPNMark mark)
                //        info += $" ({mark.MarkType})";
                //    Console.WriteLine($"{i}: {info}");
                //}
                //Console.WriteLine("========================");

                var variables = new Dictionary<string, double>();
                var stringVariables = new Dictionary<string, string>();
                var arrays = new Dictionary<string, List<double>>();
                var stack = new Stack<RPNSymbol>();

                for (int iteration = 0; iteration < rpn.Count; iteration++)
                {

                    var symbol = rpn[iteration];

                    if ( symbol.RPNType == RPNType.A_VariableName || 
                        symbol.RPNType == RPNType.A_Number || 
                        symbol.RPNType == RPNType.A_TextLine ||
                        symbol.RPNType == RPNType.A_Boolean ||
                        symbol.RPNType == RPNType.М_Mark )
                    {
                        stack.Push(symbol);
                    }
                    else if (symbol.RPNType == RPNType.F_IntArray)
                    {
                        if (stack.Count < 2)
                            throw new Exception("F_IntArray Error: недостаточно аргументов в стеке.");

                        var arrayName = stack.Pop() as RPNIdentifier;   // первым достаём arr (вершина)
                        var sizeSymbol = stack.Pop();                    // потом n
                        int size = (int)ResolveValue(sizeSymbol, variables, arrays);

                        if (size <= 0)
                        {
                            throw new Exception($"F_IntArray Error: размер массива '{arrayName.Name}' должен быть положительным (получено: {size}).");
                        }

                        var newArray = new List<double>();
                        for (int i = 0; i < size; i++) newArray.Add(0);
                        arrays[arrayName.Name] = newArray;
                    }
                    else if (symbol.RPNType == RPNType.F_Index)
                    {
                        var index = (int)ResolveValue(stack.Pop(), variables, arrays);
                        var arrayName = stack.Pop() as RPNIdentifier;
                        stack.Push(new RPNArrayAccess { ArrayName = arrayName.Name, Index = index });
                    }
                    /// АРИФМЕТИКА
                    else if (symbol.RPNType == RPNType.F_Plus || symbol.RPNType == RPNType.F_Minus ||
                            symbol.RPNType == RPNType.F_Multiply || symbol.RPNType == RPNType.F_Divide)
                    {
                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);
                        double result = 0;

                        if (symbol.RPNType == RPNType.F_Plus)
                        {
                            result = val1 + val2;
                        }
                        if (symbol.RPNType == RPNType.F_Minus)
                        {
                            result = val1 - val2;
                        }
                        if (symbol.RPNType == RPNType.F_Multiply)
                        {
                            result = val1 * val2;
                        }
                        if (symbol.RPNType == RPNType.F_Divide)
                        {
                            if ( val2 != 0 )
                            {
                                result = val1 / val2;
                            }
                            else
                            {
                                throw new Exception($"Error: Обнаружено деление на 0");
                            }
                        }

                        stack.Push(new RPNNumber(RPNType.A_Number) { Data = (int)result, DoubleData = result });
                    }
                    else if (symbol.RPNType == RPNType.F_UMinus)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Недостаточно операндов для унарного минуса");

                        double val = ResolveValue(stack.Pop(), variables, arrays);
                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = -val });
                    }
                    /// Постфиксный инкремент (x++)
                    else if (symbol.RPNType == RPNType.F_PostIncrement)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Недостаточно операндов для '++'");

                        var target = stack.Pop() as RPNIdentifier;
                        if (target == null)
                            throw new Exception("Error: Операнд '++' должен быть переменной");

                        if (!variables.ContainsKey(target.Name))
                            throw new Exception($"Error: Переменная '{target.Name}' не объявлена");

                        double oldValue = variables[target.Name];
                        variables[target.Name] = oldValue + 1;

                        // Возвращаем старое значение
                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = oldValue });
                    }
                    /// Постфиксный декремент (x--)
                    else if (symbol.RPNType == RPNType.F_PostDecrement)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Недостаточно операндов для '--'");

                        var target = stack.Pop() as RPNIdentifier;
                        if (target == null)
                            throw new Exception("Error: Операнд '--' должен быть переменной");

                        if (!variables.ContainsKey(target.Name))
                            throw new Exception($"Error: Переменная '{target.Name}' не объявлена");

                        double oldValue = variables[target.Name];
                        variables[target.Name] = oldValue - 1;

                        // Возвращаем СТАРОЕ значение
                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = oldValue });
                    }
                    /// ПРИСВАИВАНИЕ (=)
                    else if (symbol.RPNType == RPNType.F_Assignment)
                    {
                        var valSymbol = stack.Pop();
                        var target = stack.Pop();

                        if (target is RPNIdentifier targetVar)
                        {
                            // 1. Присваивание строкового литерала
                            if (valSymbol is RPNTextLine textVal)
                            {
                                stringVariables[targetVar.Name] = textVal.Data;
                            }
                            // 2. Присваивание значения другой строковой переменной
                            else if (valSymbol is RPNIdentifier sourceVar && stringVariables.ContainsKey(sourceVar.Name))
                            {
                                stringVariables[targetVar.Name] = stringVariables[sourceVar.Name];
                            }
                            // 3. Стандартное присваивание чисел
                            else
                            {
                                double val = ResolveValue(valSymbol, variables, arrays);
                                if (!variables.ContainsKey(targetVar.Name))
                                {
                                    variables[targetVar.Name] = 0;
                                }
                                variables[targetVar.Name] = val;
                            }
                        }
                        else if (target is RPNArrayAccess arrayAccess)
                        {
                            double val = ResolveValue(valSymbol, variables, arrays);
                            if (!arrays.ContainsKey(arrayAccess.ArrayName))
                                throw new Exception($"Error: Массив '{arrayAccess.ArrayName}' не объявлен");

                            if (arrayAccess.Index < 0 || arrayAccess.Index >= arrays[arrayAccess.ArrayName].Count)
                                throw new Exception($"Error: Индекс {arrayAccess.Index} выходит за границы");

                            arrays[arrayAccess.ArrayName][arrayAccess.Index] = val;
                        }
                    }
                    /// ВВОД (read)
                    else if (symbol.RPNType == RPNType.F_Read)
                    {
                        var target = stack.Pop() as RPNIdentifier;
                        variables[target.Name] = Convert.ToDouble(Console.ReadLine().Replace('.', ','));
                    }
                    /// ВЫВОД (write)
                    else if (symbol.RPNType == RPNType.F_Write)
                    {
                        var valSymbol = stack.Pop();
                        
                        // Вывод строкового литерала
                        if (valSymbol is RPNTextLine textLine)
                        {
                            Console.WriteLine($">>> {textLine.Data.Trim('"')}");
                        }
                        // Вывод строковой переменной
                        else if (valSymbol is RPNIdentifier id && stringVariables.ContainsKey(id.Name))
                        {
                            Console.WriteLine($">>> {stringVariables[id.Name].Trim('"')}");
                        }
                        // Вывод чисел
                        else
                        {
                            double val = ResolveValue(valSymbol, variables, arrays);
                            Console.WriteLine($">>> {val}");
                        }
                    }
                    /// МАТЕМАТИЧЕСКИЕ ФУНКЦИИ
                    else if (symbol.RPNType == RPNType.F_Sqrt ||
                             symbol.RPNType == RPNType.F_Exp ||
                             symbol.RPNType == RPNType.F_Sin ||
                             symbol.RPNType == RPNType.F_Cos)
                    {
                        if (stack.Count < 1)
                            throw new Exception($"Error {symbol.RPNType}: отсутствует аргумент.");

                        double val = ResolveValue(stack.Pop(), variables, arrays);
                        double res = 0;

                        if (symbol.RPNType == RPNType.F_Sqrt)
                        {
                            if (val < 0) throw new Exception($"Error: аргумент функции sqrt меньше нуля ({val}).");
                            res = Math.Sqrt(val);
                        }
                        else if (symbol.RPNType == RPNType.F_Exp) res = Math.Exp(val);
                        else if (symbol.RPNType == RPNType.F_Sin) res = Math.Sin(val);
                        else if (symbol.RPNType == RPNType.F_Cos) res = Math.Cos(val);

                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = res });
                    }
                    /// ОПЕРАТОРЫ СРАВНЕНИЯ
                    /// Оператор >
                    else if (symbol.RPNType == RPNType.F_Greater)
                    {
                        if (stack.Count < 2)
                            throw new Exception("Error: Недостаточно операндов для операции '>'");

                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);

                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = val1 > val2 });
                    }
                    /// Оператор <
                    else if (symbol.RPNType == RPNType.F_Less)
                    {
                        if (stack.Count < 2)
                            throw new Exception("Error: Недостаточно операндов для операции '<'");

                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);

                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = val1 < val2 });
                    }
                    /// Оператор <=
                    else if ( symbol.RPNType == RPNType.F_LessEqual )
                    {
                        if ( stack.Count < 2 )
                            throw new Exception("Error: Недостаточно операндов для операции '<='");

                        double val2 = ResolveValue( stack.Pop(), variables, arrays);
                        double val1 = ResolveValue( stack.Pop(), variables, arrays);

                        stack.Push( new RPNBoolean( RPNType.A_Boolean ) { Data = val1 <= val2 } );
                    }
                    /// Оператор >=
                    else if ( symbol.RPNType == RPNType.F_GreaterEqual )
                    {
                        if ( stack.Count < 2 )
                            throw new Exception("Error: Недостаточно операндов для операции '>='");

                        double val2 = ResolveValue( stack.Pop(), variables, arrays);
                        double val1 = ResolveValue( stack.Pop(), variables, arrays);

                        stack.Push( new RPNBoolean( RPNType.A_Boolean ) { Data = val1 >= val2 } );
                    }
                    /// Оператор ==
                    else if ( symbol.RPNType == RPNType.F_Equal )
                    {
                        if ( stack.Count < 2 )
                            throw new Exception("Error: Недостаточно операндов для операции '=='");

                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);

                        bool result = Math.Abs( val1 - val2 ) < 1e-15;

                        stack.Push( new RPNBoolean( RPNType.A_Boolean ) { Data = result } );
                    }
                    /// Логические операторы
                    /// Оператор !
                    else if (symbol.RPNType == RPNType.F_Not)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Недостаточно операндов для операции '!'");

                        var operand = stack.Pop();
                        bool val = GetBoolValue(operand, variables, arrays);
                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = !val });
                    }

                    /// Условные и безусловные переходы
                    if (symbol.RPNType == RPNType.М_Mark)
                    {
                        continue;
                    }
                    /// Условный переход к метке (используется в if и while)
                    else if (symbol.RPNType == RPNType.F_ConditionalJumpToMark)
                    {
                        if (stack.Count < 2)
                            throw new Exception("Error: Недостаточно операндов для условного перехода");

                        // В стеке: сначала метка, потом условие (т.к. метка была положена раньше)
                        var mark = stack.Pop() as RPNMark;
                        var condition = stack.Pop();

                        if (mark == null || mark.Position == null)
                            throw new Exception("Error: Некорректная метка для перехода");

                        bool condValue = GetBoolValue(condition, variables, arrays);

                        // Если условие ЛОЖНО, переходим к метке
                        if (!condValue)
                        {
                            iteration = mark.Position.Value - 1;
                            continue;
                        }
                    }

                    /// Безусловный переход (используется в else и конце while)
                    else if (symbol.RPNType == RPNType.F_UnconditionalJumpToMark)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Отсутствует метка для перехода");

                        var mark = stack.Pop() as RPNMark;
                        if (mark == null || mark.Position == null)
                            throw new Exception("Error: Некорректная метка для перехода");

                        iteration = mark.Position.Value - 1;
                        continue;
                    }
                    else if (symbol.RPNType == RPNType.F_NotEqual)
                    {
                        if (stack.Count < 2)
                            throw new Exception("Error: Недостаточно операндов для операции '!='");

                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);

                        bool result = Math.Abs(val1 - val2) >= 1e-15;

                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = result });
                    }
                    else if (symbol.RPNType == RPNType.F_String)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Отсутствует идентификатор для объявления string");

                        var target = stack.Peek() as RPNIdentifier;
                        if (target == null)
                            throw new Exception("Error: Ожидался идентификатор при объявлении string");
                        stringVariables[target.Name] = "";
                    }
                    else if (symbol.RPNType == RPNType.F_Int)
                    {
                        if (stack.Count < 1)
                            throw new Exception("Error: Отсутствует идентификатор для объявления int");

                        var target = stack.Peek() as RPNIdentifier;
                        if (target == null)
                            throw new Exception("Error: Ожидался идентификатор при объявлении int");
                        
                        // Безопасно резервируем переменную в памяти со значением по умолчанию
                        if (!variables.ContainsKey(target.Name))
                        {
                            variables[target.Name] = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static double ResolveValue(RPNSymbol sym, Dictionary<string, double> vars, Dictionary<string, List<double>> arrays)
        {
            if (sym is RPNNumber num)
                return num.DoubleData == 0 && num.Data != 0 ? num.Data : num.DoubleData;

            if (sym is RPNIdentifier id)
            {
                if (vars.ContainsKey(id.Name)) return vars[id.Name];
                else throw new Exception($"Переменная '{id.Name}' не инициализирована!");
            }

            if (sym is RPNBoolean boolVal)
            {
                return boolVal.Data ? 1.0 : 0.0;
            }

            if (sym is RPNArrayAccess arrayAccess)
            {
                if (!arrays.ContainsKey(arrayAccess.ArrayName))
                    throw new Exception($"Массив '{arrayAccess.ArrayName}' не объявлен");
                if (arrayAccess.Index < 0 || arrayAccess.Index >= arrays[arrayAccess.ArrayName].Count)
                    throw new Exception($"Индекс {arrayAccess.Index} выходит за границы");
                return arrays[arrayAccess.ArrayName][arrayAccess.Index];
            }

            throw new Exception($"Неверный тип операнда: {sym?.GetType().Name}, RPNType: {sym?.RPNType}");
        }

        private static bool GetBoolValue(RPNSymbol sym, Dictionary<string, double> vars, Dictionary<string, List<double>> arrays)
        {
            if (sym is RPNBoolean boolVal)
                return boolVal.Data;

            if (sym is RPNIdentifier id)
            {
                if (!vars.ContainsKey(id.Name))
                    throw new Exception($"Переменная '{id.Name}' не инициализирована!");
                return Math.Abs(vars[id.Name]) > 1e-15;
            }

            if (sym is RPNNumber num)
            {
                double val = num.DoubleData == 0 && num.Data != 0 ? num.Data : num.DoubleData;
                return Math.Abs(val) > 1e-10;
            }

            if (sym is RPNArrayAccess arrayAccess)
            {
                if (!arrays.ContainsKey(arrayAccess.ArrayName))
                    throw new Exception($"Массив '{arrayAccess.ArrayName}' не объявлен");
                double val = arrays[arrayAccess.ArrayName][arrayAccess.Index];
                return Math.Abs(val) > 1e-15;
            }
            throw new Exception($"Невозможно преобразовать {sym?.RPNType} в булево значение");
        }
    }
}
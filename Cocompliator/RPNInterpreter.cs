using System;
using System.Collections.Generic;

namespace Cocompliator
{
    /// @brief Класс интерперататор
    /// @note Отвечает за исполнение программы, исходя из ОПС
    public class RPNInterpreter
    {
        /// @brief Исполнение инструкций
        public static void ExecuteInstructions(List<RPNSymbol> rpn)
        {
            /// Если ОПС пустая - выход
            if (rpn == null || rpn.Count == 0)
            {
                return;
            }

            /// Отладка
            Console.WriteLine("=== RPN INSTRUCTIONS ===");
            for (int i = 0; i < rpn.Count; i++)
            {
                var s = rpn[i];
                string info = s.RPNType.ToString();
                if (s is RPNIdentifier id)
                    info += $" ({id.Name})";
                else if (s is RPNNumber num)
                    info += $" ({num.Data})";
                else if (s is RPNMark mark)
                    info += $" ({mark.MarkType})";
                Console.WriteLine($"{i}: {info}");
            }
            Console.WriteLine("========================");

            var variables = new Dictionary<string, double>();
            var stringVariables = new Dictionary<string, string>();
            var arrays = new Dictionary<string, List<double>>();
            var stack = new Stack<RPNSymbol>();

            for (int iteration = 0; iteration < rpn.Count; iteration++)
            {
                var symbol = rpn[iteration];

                try
                {
                    /// Загрузка операндов в стек
                    if (symbol.RPNType == RPNType.A_VariableName || symbol.RPNType == RPNType.A_Number ||
                        symbol.RPNType == RPNType.A_TextLine || symbol.RPNType == RPNType.A_Boolean || symbol.RPNType == RPNType.М_Mark)
                    {
                        stack.Push(symbol);
                    }

                    /// Обработка массивов
                    else if (symbol.RPNType == RPNType.F_IntArray)
                    {
                        if (stack.Count < 2)
                        {
                            throw CompilerException.RuntimeStackUnderflow(symbol.LinePointer, symbol.CharPointer);
                        }
                        var arrayName = stack.Pop() as RPNIdentifier;
                        var sizeSymbol = stack.Pop();
                        int size = (int)ResolveValue(sizeSymbol, variables, arrays);

                        if (size <= 0)
                        {
                            throw CompilerException.RuntimeInvalidArraySize(arrayName.Name, size, symbol.LinePointer, symbol.CharPointer);
                        }

                        var newArray = new List<double>();
                        for (int i = 0; i < size; i++)
                        {
                            newArray.Add(0);
                        }
                        arrays[arrayName.Name] = newArray;
                    }
                    /// Обработка индексации
                    else if (symbol.RPNType == RPNType.F_Index)
                    {
                        var index = (int)ResolveValue(stack.Pop(), variables, arrays);
                        var arrayName = stack.Pop() as RPNIdentifier;
                        stack.Push(new RPNArrayAccess { ArrayName = arrayName.Name, Index = index, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }

                    /// Обработка арифметических операций + и *
                    else if (symbol.RPNType == RPNType.F_Plus || symbol.RPNType == RPNType.F_Multiply)
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();

                        /// Проверяем, есть ли среди операндов строки (литералы или строковые переменные)
                        bool leftIsString = (left is RPNTextLine) ||
                                            (left is RPNIdentifier leftId && stringVariables.ContainsKey(leftId.Name));
                        bool rightIsString = (right is RPNTextLine) ||
                                             (right is RPNIdentifier rightId && stringVariables.ContainsKey(rightId.Name));

                        if (symbol.RPNType == RPNType.F_Plus)
                        {
                            if (leftIsString || rightIsString)
                            {
                                /// Конкатенация строк
                                string leftStr = GetStringValue(left, variables, stringVariables, arrays);
                                string rightStr = GetStringValue(right, variables, stringVariables, arrays);
                                stack.Push(new RPNTextLine(RPNType.A_TextLine)
                                {
                                    Data = leftStr + rightStr,
                                    LinePointer = symbol.LinePointer,
                                    CharPointer = symbol.CharPointer
                                } );
                            }
                            else
                            {
                                /// Числовое сложение
                                double val2 = ResolveValue(right, variables, arrays);
                                double val1 = ResolveValue(left, variables, arrays);
                                double result = val1 + val2;
                                stack.Push(new RPNNumber(RPNType.A_Number)
                                {
                                    Data = (int)result,
                                    DoubleData = result,
                                    LinePointer = symbol.LinePointer,
                                    CharPointer = symbol.CharPointer
                                });
                            }
                        }
                        else if ( symbol.RPNType == RPNType.F_Multiply )
                        {
                            /// Умножение строки на число
                            if (leftIsString && !rightIsString)
                            {
                                string str = GetStringValue(left, variables, stringVariables, arrays);
                                double count = ResolveValue(right, variables, arrays);
                                int intCount = (int)count;
                                if (intCount < 0)
                                {
                                    throw CompilerException.RuntimeNegativeMultiplier(symbol.LinePointer, symbol.CharPointer);
                                }
                                string result = string.Concat(Enumerable.Repeat(str, intCount));
                                stack.Push(new RPNTextLine(RPNType.A_TextLine)
                                {
                                    Data = result,
                                    LinePointer = symbol.LinePointer,
                                    CharPointer = symbol.CharPointer
                                });
                            }
                            else if (rightIsString && !leftIsString)
                            {
                                /// Умножение строки на число
                                string str = GetStringValue(right, variables, stringVariables, arrays);
                                double count = ResolveValue(left, variables, arrays);
                                int intCount = (int)count;
                                if (intCount < 0)
                                {
                                    throw CompilerException.RuntimeNegativeMultiplier(symbol.LinePointer, symbol.CharPointer);
                                }
                                string result = string.Concat(Enumerable.Repeat(str, intCount));
                                stack.Push(new RPNTextLine(RPNType.A_TextLine)
                                {
                                    Data = result,
                                    LinePointer = symbol.LinePointer,
                                    CharPointer = symbol.CharPointer
                                });
                            }
                            else if (!leftIsString && !rightIsString)
                            {
                                /// Числовое умножение
                                double val2 = ResolveValue(right, variables, arrays);
                                double val1 = ResolveValue(left, variables, arrays);
                                double result = val1 * val2;
                                stack.Push(new RPNNumber(RPNType.A_Number)
                                {
                                    Data = (int)result,
                                    DoubleData = result,
                                    LinePointer = symbol.LinePointer,
                                    CharPointer = symbol.CharPointer
                                });
                            }
                            else
                            {
                                /// Оба операнда строки – ошибка
                                throw CompilerException.RuntimeInvalidStringMultiplication(symbol.LinePointer, symbol.CharPointer);
                            }
                        }
                    }
                    /// Обработка арифметических операций - и /
                    else if (symbol.RPNType == RPNType.F_Minus || symbol.RPNType == RPNType.F_Divide)
                    {
                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);
                        double result;
                        if (symbol.RPNType == RPNType.F_Minus)
                            result = val1 - val2;
                        else
                        {
                            /// Ошибка деления на 0
                            if (val2 == 0)
                            {
                                throw CompilerException.RuntimeDivideByZero(symbol.LinePointer, symbol.CharPointer);
                            }
                            result = val1 / val2;
                        }
                        stack.Push(new RPNNumber(RPNType.A_Number)
                        {
                            Data = (int)result,
                            DoubleData = result,
                            LinePointer = symbol.LinePointer,
                            CharPointer = symbol.CharPointer
                        });
                    }
                    /// Обработка унарного минуса
                    else if (symbol.RPNType == RPNType.F_UMinus)
                    {
                        double val = ResolveValue(stack.Pop(), variables, arrays);
                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = -val, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }
                    /// Обработка постфиксных инкремента или декремента
                    else if (symbol.RPNType == RPNType.F_PostIncrement || symbol.RPNType == RPNType.F_PostDecrement)
                    {
                        var target = stack.Pop() as RPNIdentifier;
                        if (target == null)
                        {
                            /// Если не идентификатор - ошибка
                            throw CompilerException.RuntimeInvalidOperand("++ / --", symbol.LinePointer, symbol.CharPointer);
                        }
                        if (!variables.ContainsKey(target.Name))
                        {
                            /// Если переменная не объявлена - ошибка
                            throw CompilerException.RuntimeVariableNotInit(target.Name, symbol.LinePointer, symbol.CharPointer);
                        }

                        double oldValue = variables[target.Name];
                        variables[target.Name] = symbol.RPNType == RPNType.F_PostIncrement ? oldValue + 1 : oldValue - 1;
                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = oldValue, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }
                    /// Обработка присваивания = 
                    else if (symbol.RPNType == RPNType.F_Assignment)
                    {
                        var valSymbol = stack.Pop();
                        var target = stack.Pop();

                        /// Присвоение к переменной (строка или число)
                        if (target is RPNIdentifier targetVar)
                        {
                            if (valSymbol is RPNTextLine textVal)
                            {
                                stringVariables[targetVar.Name] = textVal.Data;
                            }
                            else if (valSymbol is RPNIdentifier sourceVar && stringVariables.ContainsKey(sourceVar.Name))
                            {
                                stringVariables[targetVar.Name] = stringVariables[sourceVar.Name];
                            }
                            else
                            {
                                double val = ResolveValue(valSymbol, variables, arrays);
                                variables[targetVar.Name] = val;
                            }
                        }
                        /// Присвоение к переменной (элемент массива)
                        else if (target is RPNArrayAccess arrayAccess)
                        {
                            double val = ResolveValue(valSymbol, variables, arrays);
                            if (!arrays.ContainsKey(arrayAccess.ArrayName))
                            {
                                throw CompilerException.RuntimeArrayNotDeclared(arrayAccess.ArrayName, symbol.LinePointer, symbol.CharPointer);
                            }
                            if (arrayAccess.Index < 0 || arrayAccess.Index >= arrays[arrayAccess.ArrayName].Count)
                            {
                                throw CompilerException.RuntimeIndexOutOfBounds(arrayAccess.ArrayName, arrayAccess.Index, arrays[arrayAccess.ArrayName].Count, symbol.LinePointer, symbol.CharPointer);
                            }
                            arrays[arrayAccess.ArrayName][arrayAccess.Index] = val;
                        }
                        else
                        {
                            /// Ошибка: цель не переменная и не элемент массива
                            throw CompilerException.SyntaxInvalidAssignmentTarget(symbol.LinePointer, symbol.CharPointer);
                        }
                    }
                    /// Обработка операции чтения
                    else if (symbol.RPNType == RPNType.F_Read)
                    {
                        /// Уточнить
                        var target = stack.Pop() as RPNIdentifier;
                        string input = Console.ReadLine();
                        if (!double.TryParse(input.Replace('.', ','), out double parsed))
                        {
                            throw CompilerException.RuntimeFormatError(input, symbol.LinePointer, symbol.CharPointer);
                        }
                        variables[target.Name] = parsed;
                    }
                    /// Обработка операции вывода
                    else if (symbol.RPNType == RPNType.F_Write)
                    {
                        var valSymbol = stack.Pop();
                        if (valSymbol is RPNTextLine textLine)
                        {
                            Console.WriteLine($">>> {textLine.Data.Trim('"')}");
                        }
                        else if (valSymbol is RPNIdentifier id && stringVariables.ContainsKey(id.Name))
                        {
                            Console.WriteLine($">>> {stringVariables[id.Name].Trim('"')}");
                        }
                        else
                        {
                            double val = ResolveValue(valSymbol, variables, arrays);
                            Console.WriteLine($">>> {val}");
                        }
                    }
                    /// Обработка математических функций
                    else if (symbol.RPNType == RPNType.F_Sqrt || symbol.RPNType == RPNType.F_Exp || symbol.RPNType == RPNType.F_Sin || symbol.RPNType == RPNType.F_Cos)
                    {
                        double val = ResolveValue(stack.Pop(), variables, arrays);
                        double res = 0;
                        if (symbol.RPNType == RPNType.F_Sqrt)
                        {
                            if (val < 0)
                            {
                                throw CompilerException.RuntimeMathNegativeSqrt(val, symbol.LinePointer, symbol.CharPointer);
                            }
                            res = Math.Sqrt(val);
                        }
                        else if (symbol.RPNType == RPNType.F_Exp) res = Math.Exp(val);
                        else if (symbol.RPNType == RPNType.F_Sin) res = Math.Sin(val);
                        else if (symbol.RPNType == RPNType.F_Cos) res = Math.Cos(val);

                        stack.Push(new RPNNumber(RPNType.A_Number) { DoubleData = res, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }
                    /// Обработка операций сравнения
                    else if (symbol.RPNType == RPNType.F_Greater || symbol.RPNType == RPNType.F_Less || 
                        symbol.RPNType == RPNType.F_LessEqual || symbol.RPNType == RPNType.F_GreaterEqual || 
                        symbol.RPNType == RPNType.F_Equal || symbol.RPNType == RPNType.F_NotEqual)
                    {
                        double val2 = ResolveValue(stack.Pop(), variables, arrays);
                        double val1 = ResolveValue(stack.Pop(), variables, arrays);
                        bool result = false;

                        if (symbol.RPNType == RPNType.F_Greater)
                        {
                            result = val1 > val2;
                        }
                        else if (symbol.RPNType == RPNType.F_Less)
                        {
                            result = val1 < val2;
                        }
                        else if (symbol.RPNType == RPNType.F_LessEqual)
                        {
                            result = val1 <= val2;
                        }
                        else if (symbol.RPNType == RPNType.F_GreaterEqual)
                        {
                            result = val1 >= val2;
                        }
                        else if (symbol.RPNType == RPNType.F_Equal)
                        {
                            result = Math.Abs(val1 - val2) < 1e-15;
                        }
                        else if (symbol.RPNType == RPNType.F_NotEqual)
                        {
                            result = Math.Abs(val1 - val2) >= 1e-15;
                        }

                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = result, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }
                    /// Обработка логического НЕ
                    else if (symbol.RPNType == RPNType.F_Not)
                    {
                        bool val = GetBoolValue(stack.Pop(), variables, arrays);
                        stack.Push(new RPNBoolean(RPNType.A_Boolean) { Data = !val, LinePointer = symbol.LinePointer, CharPointer = symbol.CharPointer });
                    }
                    /// Обработка условного перехода на метку
                    else if (symbol.RPNType == RPNType.F_ConditionalJumpToMark)
                    {
                        var mark = stack.Pop() as RPNMark;
                        bool condValue = GetBoolValue(stack.Pop(), variables, arrays);
                        if (!condValue)
                        {
                            iteration = mark.Position.Value - 1;
                        }
                    }
                    /// Обработка безусловного перехода на метку
                    else if (symbol.RPNType == RPNType.F_UnconditionalJumpToMark)
                    {
                        var mark = stack.Pop() as RPNMark;
                        iteration = mark.Position.Value - 1;
                    }
                    /// Обработка явного задания типа данных
                    else if (symbol.RPNType == RPNType.F_String || symbol.RPNType == RPNType.F_Int)
                    {
                        var target = stack.Peek() as RPNIdentifier;
                        if (target == null)
                            throw CompilerException.SyntaxExpectedIdentifier(symbol.LinePointer, symbol.CharPointer);

                        if (symbol.RPNType == RPNType.F_String)
                        {
                            stringVariables[target.Name] = "";
                        }
                        else /// F_Int
                        {
                            variables[target.Name] = 0;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    throw CompilerException.RuntimeStackUnderflow(symbol.LinePointer, symbol.CharPointer);
                }
            }
        }

        /// @brief Извлечение числового значение
        private static double ResolveValue(RPNSymbol sym, Dictionary<string, double> vars, Dictionary<string, List<double>> arrays)
        {
            /// Числовой литерал
            if (sym is RPNNumber num)
            {
                return num.DoubleData == 0 && num.Data != 0 ? num.Data : num.DoubleData;
            }
            /// Идентификатор переменной
            if (sym is RPNIdentifier id)
            {
                if (vars.ContainsKey(id.Name)) return vars[id.Name];
                throw CompilerException.RuntimeVariableNotInit(id.Name, sym.LinePointer, sym.CharPointer);
            }
            /// Логическое значение
            if (sym is RPNBoolean boolVal)
            {
                return boolVal.Data ? 1.0 : 0.0;
            }
            /// Элемент массива
            if (sym is RPNArrayAccess arrayAccess)
            {
                if (!arrays.ContainsKey(arrayAccess.ArrayName))
                {
                    throw CompilerException.RuntimeArrayNotDeclared(arrayAccess.ArrayName, sym.LinePointer, sym.CharPointer);
                }
                if (arrayAccess.Index < 0 || arrayAccess.Index >= arrays[arrayAccess.ArrayName].Count) 
                {
                    throw CompilerException.RuntimeIndexOutOfBounds(arrayAccess.ArrayName, arrayAccess.Index, arrays[arrayAccess.ArrayName].Count, sym.LinePointer, sym.CharPointer);
                }
                return arrays[arrayAccess.ArrayName][arrayAccess.Index];
            }
            throw CompilerException.RuntimeTypeError("Ожидалось число", sym.LinePointer, sym.CharPointer);
        }

        /// @brief Извлечение булевого значения
        private static bool GetBoolValue(RPNSymbol sym, Dictionary<string, double> vars, Dictionary<string, List<double>> arrays)
        {
            if (sym is RPNBoolean boolVal) return boolVal.Data;
            if (sym is RPNIdentifier id)
            {
                if (!vars.ContainsKey(id.Name))
                {
                    throw CompilerException.RuntimeVariableNotInit(id.Name, sym.LinePointer, sym.CharPointer);
                }
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
                {
                    throw CompilerException.RuntimeArrayNotDeclared(arrayAccess.ArrayName, sym.LinePointer, sym.CharPointer);
                }
                if (arrayAccess.Index < 0 || arrayAccess.Index >= arrays[arrayAccess.ArrayName].Count)
                {
                    throw CompilerException.RuntimeIndexOutOfBounds(arrayAccess.ArrayName, 
                        arrayAccess.Index, arrays[arrayAccess.ArrayName].Count, sym.LinePointer, sym.CharPointer);
                }
                return Math.Abs(arrays[arrayAccess.ArrayName][arrayAccess.Index]) > 1e-15;
            }
            throw CompilerException.RuntimeTypeError("Ожидалось логическое условие", sym.LinePointer, sym.CharPointer);
        }

        /// @brief Извлечение строкового значения
        private static string GetStringValue(RPNSymbol sym, Dictionary<string, double> vars,
               Dictionary<string, string> stringVars, Dictionary<string, List<double>> arrays)
        {
            if (sym is RPNTextLine text)
                return text.Data;
            if (sym is RPNIdentifier id)
            {
                if (stringVars.ContainsKey(id.Name))
                {
                    return stringVars[id.Name];
                }
                if (vars.ContainsKey(id.Name))
                {
                    return vars[id.Name].ToString();
                }
                throw CompilerException.RuntimeVariableNotInit(id.Name, sym.LinePointer, sym.CharPointer);
            }
            if (sym is RPNNumber num)
            {
                return (num.DoubleData == 0 && num.Data != 0 ? num.Data : num.DoubleData).ToString();
            }
            if (sym is RPNBoolean boolVal)
            {
                return boolVal.Data.ToString();
            }
            throw CompilerException.RuntimeTypeError("Ожидалась строка", sym.LinePointer, sym.CharPointer);
        }
    }
}
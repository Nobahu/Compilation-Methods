using System;
using System.Collections.Generic;
using System.IO;

namespace Cocompliator
{
   /// @brief Типы нетерминальных символов для управляющей таблицы
   public enum NonTerminal
   {
      Program, // Стартовый нетерминал
      StatementList, // Список инструкций
      Statement, // Отдельная инструкция
      StatementSuffix, //Суффикс инструкции
      Expression, // Выражение
      ExpressionPrime, // Правое разветвление выражения
      Term, // Слагаемое
      TermPrime, // Правое разветвление слагаемого
      Factor, //Элементарный множитель
      ArrayAccess, // Доступ к элементу массива по индексу
      Condition, //Логическое условие для if, while
      ConditionPrime, // Правая часть условия
      IfStatement, // Конструкция is-else
      ElsePart, // Необязательная ветка else
      WhileStatement, // Цикл while
      ReadStatement, // Оператор ввода
      WriteStatement // Оператор вывода
   }

   /// @brief Типы семантических действий для генерации ОПС
   public enum SemanticAction
   {
      // Запись переменной переменной/константы в ОПС
      GenPushVar,
      GenPushConst,
      // Запись арифметических операций и присваивания
      GenPlus,
      GenMinus,
      GenMultiply,
      GenDivide,
      GenAssign,
      // Запись операций сравнения
      GenEqual,
      GenLess,
      GenGreater,
      GenLessEqual,
      GenGreaterEqual,
      // Запись функций ввода и вывода
      GenRead,
      GenWrite,
      // Запись обращения к индексу или объявления массива
      GenIndex,
      GenArrayDecl,
      // Генерация меток начала, проверки условия и конца цикла while
      StartWhile,
      WhileCondEnd,
      EndWhile,
      // Генерация мето переходов для if и else
      IfCondEnd,
      IfElseStart,
      IfElseEnd
   }

   /// @brief Тип символа на стек-памяти автомата
   public enum StackSymbolType
   {
      Terminal,
      NonTerminal,
      Action
   }

   /// @brief Элемент стека магазинного автомата
   public class StackSymbol
   {
      public StackSymbolType Type { get; set; }
      public TerminalType Terminal { get; set; }
      public NonTerminal NonTerminal { get; set; }
      public SemanticAction Action { get; set; }

      public StackSymbol( TerminalType terminal )
      {
         Type = StackSymbolType.Terminal;
         Terminal = terminal;
      }

      public StackSymbol( NonTerminal nonTerminal )
      {
         Type = StackSymbolType.NonTerminal;
         NonTerminal = nonTerminal;
      }

      public StackSymbol( SemanticAction action )
      {
         Type = StackSymbolType.Action;
         Action = action;
      }
   }

   /// @brief Табличный синтаксический анализатор (генератор ОПС)
   public static class SyntaxAnalyzer
   {
      private static readonly Dictionary<(NonTerminal, TerminalType), StackSymbol[]> ParseTable = 
         new Dictionary<(NonTerminal, TerminalType), StackSymbol[]>();

      static SyntaxAnalyzer()
      {
         InitializeParseTable();
      }

      private static void InitializeParseTable()
      {
         // Program -> StatementList
         AddRule( NonTerminal.Program, TerminalType.VariableName, new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.Program, TerminalType.If, new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.Program, TerminalType.While, new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.Program, TerminalType.Read, new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.Program, TerminalType.Write, new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.Program, TerminalType.LeftBrace, new StackSymbol( NonTerminal.StatementList ) );

         // StatementList -> Statement StatementList | epsilon
         AddRule( NonTerminal.StatementList, TerminalType.VariableName, new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.StatementList, TerminalType.If, new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.StatementList, TerminalType.While, new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.StatementList, TerminalType.Read, new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.StatementList, TerminalType.Write, new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.StatementList ) );
         AddRule( NonTerminal.StatementList, TerminalType.LeftBrace, new StackSymbol( TerminalType.LeftBrace ), new StackSymbol( NonTerminal.StatementList ), new StackSymbol( TerminalType.RightBrace ) );
         AddRule( NonTerminal.StatementList, TerminalType.RightBrace, Array.Empty<StackSymbol>() );

         // Statement -> VariableName [GenPushVar] StatementSuffix | If | While | Read | Write
         AddRule( NonTerminal.Statement, TerminalType.VariableName, new StackSymbol( TerminalType.VariableName ), new StackSymbol( SemanticAction.GenPushVar ), new StackSymbol( NonTerminal.StatementSuffix ) );
         AddRule( NonTerminal.Statement, TerminalType.If, new StackSymbol( NonTerminal.IfStatement ) );
         AddRule( NonTerminal.Statement, TerminalType.While, new StackSymbol( NonTerminal.WhileStatement ) );
         AddRule( NonTerminal.Statement, TerminalType.Read, new StackSymbol( NonTerminal.ReadStatement ), new StackSymbol( TerminalType.Semicolon ) );
         AddRule( NonTerminal.Statement, TerminalType.Write, new StackSymbol( NonTerminal.WriteStatement ), new StackSymbol( TerminalType.Semicolon ) );

         // StatementSuffix -> = Expression [GenAssign] ; | [ Expression ] ArrayAccess
         AddRule( NonTerminal.StatementSuffix, TerminalType.Assignment, new StackSymbol( TerminalType.Assignment ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenAssign ), new StackSymbol( TerminalType.Semicolon ) );
         AddRule( NonTerminal.StatementSuffix, TerminalType.LeftBracket, new StackSymbol( TerminalType.LeftBracket ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( TerminalType.RightBracket ), new StackSymbol( NonTerminal.ArrayAccess ) );

         // ArrayAccess -> = Expression [GenIndex] [GenAssign] ; | VariableName [GenPushVar] [GenArrayDecl] ;
         AddRule( NonTerminal.ArrayAccess, TerminalType.Assignment, new StackSymbol( TerminalType.Assignment ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenIndex ), new StackSymbol( SemanticAction.GenAssign ), new StackSymbol( TerminalType.Semicolon ) );
         AddRule( NonTerminal.ArrayAccess, TerminalType.VariableName, new StackSymbol( TerminalType.VariableName ), new StackSymbol( SemanticAction.GenPushVar ), new StackSymbol( SemanticAction.GenArrayDecl ), new StackSymbol( TerminalType.Semicolon ) );

         // Expression -> Term ExpressionPrime
         AddRule( NonTerminal.Expression, TerminalType.VariableName, new StackSymbol( NonTerminal.Term ), new StackSymbol( NonTerminal.ExpressionPrime ) );
         AddRule( NonTerminal.Expression, TerminalType.Number, new StackSymbol( NonTerminal.Term ), new StackSymbol( NonTerminal.ExpressionPrime ) );
         AddRule( NonTerminal.Expression, TerminalType.LeftParenthesis, new StackSymbol( NonTerminal.Term ), new StackSymbol( NonTerminal.ExpressionPrime ) );

         // ExpressionPrime -> + Term [GenPlus] ExpressionPrime | - Term [GenMinus] ExpressionPrime | epsilon
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Plus, new StackSymbol( TerminalType.Plus ), new StackSymbol( NonTerminal.Term ), new StackSymbol( SemanticAction.GenPlus ), new StackSymbol( NonTerminal.ExpressionPrime ) );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Minus, new StackSymbol( TerminalType.Minus ), new StackSymbol( NonTerminal.Term ), new StackSymbol( SemanticAction.GenMinus ), new StackSymbol( NonTerminal.ExpressionPrime ) );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Semicolon, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.RightParenthesis, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.RightBracket, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Equal, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Less, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.Greater, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.LessEqual, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ExpressionPrime, TerminalType.GreaterEqual, Array.Empty<StackSymbol>() );

         // Term -> Factor TermPrime
         AddRule( NonTerminal.Term, TerminalType.VariableName, new StackSymbol( NonTerminal.Factor ), new StackSymbol( NonTerminal.TermPrime ) );
         AddRule( NonTerminal.Term, TerminalType.Number, new StackSymbol( NonTerminal.Factor ), new StackSymbol( NonTerminal.TermPrime ) );
         AddRule( NonTerminal.Term, TerminalType.LeftParenthesis, new StackSymbol( NonTerminal.Factor ), new StackSymbol( NonTerminal.TermPrime ) );

         // TermPrime -> * Factor [GenMultiply] TermPrime | / Factor [GenDivide] TermPrime | epsilon
         AddRule( NonTerminal.TermPrime, TerminalType.Multiply, new StackSymbol( TerminalType.Multiply ), new StackSymbol( NonTerminal.Factor ), new StackSymbol( SemanticAction.GenMultiply ), new StackSymbol( NonTerminal.TermPrime ) );
         AddRule( NonTerminal.TermPrime, TerminalType.Divide, new StackSymbol( TerminalType.Divide ), new StackSymbol( NonTerminal.Factor ), new StackSymbol( SemanticAction.GenDivide ), new StackSymbol( NonTerminal.TermPrime ) );
         AddRule( NonTerminal.TermPrime, TerminalType.Plus, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.Minus, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.Semicolon, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.RightParenthesis, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.RightBracket, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.Equal, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.Less, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.Greater, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.LessEqual, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.TermPrime, TerminalType.GreaterEqual, Array.Empty<StackSymbol>() );

         // Factor -> VariableName [GenPushVar] ArrayAccess | Number [GenPushConst] | ( Expression )
         AddRule( NonTerminal.Factor, TerminalType.VariableName, new StackSymbol( TerminalType.VariableName ), new StackSymbol( SemanticAction.GenPushVar ), new StackSymbol( NonTerminal.ArrayAccess ) );
         AddRule( NonTerminal.Factor, TerminalType.Number, new StackSymbol( TerminalType.Number ), new StackSymbol( SemanticAction.GenPushConst ) );
         AddRule( NonTerminal.Factor, TerminalType.LeftParenthesis, new StackSymbol( TerminalType.LeftParenthesis ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( TerminalType.RightParenthesis ) );

         // ArrayAccess -> [ Expression ] [GenIndex] | epsilon
         AddRule( NonTerminal.ArrayAccess, TerminalType.LeftBracket, new StackSymbol( TerminalType.LeftBracket ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( TerminalType.RightBracket ), new StackSymbol( SemanticAction.GenIndex ) );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Plus, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Minus, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Multiply, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Divide, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Semicolon, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.RightParenthesis, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.RightBracket, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Equal, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Less, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.Greater, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.LessEqual, Array.Empty<StackSymbol>() );
         AddRule( NonTerminal.ArrayAccess, TerminalType.GreaterEqual, Array.Empty<StackSymbol>() );

         // Condition -> Expression ConditionPrime (Обеспечивает сначала разбор левого операнда, затем правого)
         AddRule( NonTerminal.Condition, TerminalType.VariableName, new StackSymbol( NonTerminal.Expression ), new StackSymbol( NonTerminal.ConditionPrime ) );
         AddRule( NonTerminal.Condition, TerminalType.Number, new StackSymbol( NonTerminal.Expression ), new StackSymbol( NonTerminal.ConditionPrime ) );
         AddRule( NonTerminal.Condition, TerminalType.LeftParenthesis, new StackSymbol( NonTerminal.Expression ), new StackSymbol( NonTerminal.ConditionPrime ) );

         // ConditionPrime -> CompOperator Expression [SemanticAction] (Генерирует оператор ПОСЛЕ правого операнда)
         AddRule( NonTerminal.ConditionPrime, TerminalType.Equal, new StackSymbol( TerminalType.Equal ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenEqual ) );
         AddRule( NonTerminal.ConditionPrime, TerminalType.Less, new StackSymbol( TerminalType.Less ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenLess ) );
         AddRule( NonTerminal.ConditionPrime, TerminalType.Greater, new StackSymbol( TerminalType.Greater ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenGreater ) );
         AddRule( NonTerminal.ConditionPrime, TerminalType.LessEqual, new StackSymbol( TerminalType.LessEqual ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenLessEqual ) );
         AddRule( NonTerminal.ConditionPrime, TerminalType.GreaterEqual, new StackSymbol( TerminalType.GreaterEqual ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( SemanticAction.GenGreaterEqual ) );

         // Read/Write Statements
         AddRule( NonTerminal.ReadStatement, TerminalType.Read, new StackSymbol( TerminalType.Read ), new StackSymbol( TerminalType.LeftParenthesis ), new StackSymbol( TerminalType.VariableName ), new StackSymbol( SemanticAction.GenPushVar ), new StackSymbol( TerminalType.RightParenthesis ), new StackSymbol( SemanticAction.GenRead ) );
         AddRule( NonTerminal.WriteStatement, TerminalType.Write, new StackSymbol( TerminalType.Write ), new StackSymbol( TerminalType.LeftParenthesis ), new StackSymbol( NonTerminal.Expression ), new StackSymbol( TerminalType.RightParenthesis ), new StackSymbol( SemanticAction.GenWrite ) );

             // While
            // Один оператор без скобок
            AddRule(NonTerminal.WhileStatement, TerminalType.While,
                new StackSymbol(TerminalType.While),
                new StackSymbol(SemanticAction.StartWhile),
                new StackSymbol(TerminalType.LeftParenthesis),
                new StackSymbol(NonTerminal.Condition),
                new StackSymbol(TerminalType.RightParenthesis),
                new StackSymbol(SemanticAction.WhileCondEnd),
                new StackSymbol(NonTerminal.Statement),
                new StackSymbol(SemanticAction.EndWhile));

            // Блок в скобках
            AddRule(NonTerminal.WhileStatement, TerminalType.While,
                new StackSymbol(TerminalType.While),
                new StackSymbol(SemanticAction.StartWhile),
                new StackSymbol(TerminalType.LeftParenthesis),
                new StackSymbol(NonTerminal.Condition),
                new StackSymbol(TerminalType.RightParenthesis),
                new StackSymbol(SemanticAction.WhileCondEnd),
                new StackSymbol(TerminalType.LeftBrace),
                new StackSymbol(NonTerminal.StatementList),
                new StackSymbol(TerminalType.RightBrace),
                new StackSymbol(SemanticAction.EndWhile));

         // If Statement
         AddRule( NonTerminal.IfStatement, TerminalType.If, new StackSymbol( TerminalType.If ), new StackSymbol( TerminalType.LeftParenthesis ), new StackSymbol( NonTerminal.Condition ), new StackSymbol( TerminalType.RightParenthesis ), new StackSymbol( SemanticAction.IfCondEnd ), new StackSymbol( NonTerminal.Statement ), new StackSymbol( NonTerminal.ElsePart ) );
         AddRule( NonTerminal.ElsePart, TerminalType.Else, new StackSymbol( TerminalType.Else ), new StackSymbol( SemanticAction.IfElseStart ), new StackSymbol( NonTerminal.Statement ), new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.VariableName, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.If, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.While, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.Read, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.Write, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.RightBrace, new StackSymbol( SemanticAction.IfElseEnd ) );
         AddRule( NonTerminal.ElsePart, TerminalType.Semicolon, new StackSymbol( SemanticAction.IfElseEnd ) );
      }

      private static void AddRule( NonTerminal nt, TerminalType t, params StackSymbol[] symbols )
      {
         ParseTable[( nt, t )] = symbols;
      }

      /// @brief Генерация ОПС на основе списка терминалов от лексера коллег
      /// @param terminals Терминалы от лексического анализатора
      /// @return ОПС в виде списка RPNSymbol
      public static List<RPNSymbol> GenerateRPN( List<Terminal> terminals )
      {
         List<RPNSymbol> rpn = new List<RPNSymbol>();
         Stack<StackSymbol> parseStack = new Stack<StackSymbol>();
         Stack<RPNMark> labelStack = new Stack<RPNMark>();

         int labelCounter = 0;
         int currentTokenIndex = 0;
         Terminal lastTerminal = null;

         parseStack.Push( new StackSymbol( NonTerminal.Program ) );

         while ( parseStack.Count > 0 )
         {
            StackSymbol top = parseStack.Pop();
            Terminal currentToken = currentTokenIndex < terminals.Count ? terminals[currentTokenIndex] : null;

            if ( top.Type == StackSymbolType.Terminal )
            {
               if ( currentToken == null || currentToken.TerminalType != top.Terminal )
               {
                  throw new CompilerException( 
                     $"Синтаксическая ошибка: Ожидался '{top.Terminal}', но встречен '{currentToken?.TerminalType.ToString() ?? "конец файла"}'", 
                     currentToken?.LinePointer ?? -1, 
                     currentToken?.CharPointer ?? -1 
                  );
               }
               lastTerminal = currentToken;
               currentTokenIndex++;
            }
            else if ( top.Type == StackSymbolType.NonTerminal )
            {
               if ( currentToken == null )
               {
                  throw new CompilerException( $"Синтаксическая ошибка: Неожиданный конец файла", lastTerminal?.LinePointer ?? 1, lastTerminal?.CharPointer ?? 1 );
               }

               var key = ( top.NonTerminal, currentToken.TerminalType );
               if ( !ParseTable.TryGetValue( key, out var production ) )
               {
                  throw new CompilerException( 
                     $"Синтаксическая ошибка: Неожиданный символ '{currentToken.TerminalType}' в контексте {top.NonTerminal}", 
                     currentToken.LinePointer, 
                     currentToken.CharPointer 
                  );
               }

               for ( int i = production.Length - 1; i >= 0; i-- )
               {
                  parseStack.Push( production[i] );
               }
            }
            else if ( top.Type == StackSymbolType.Action )
            {
               ExecuteSemanticAction( top.Action, rpn, labelStack, ref labelCounter, lastTerminal );
            }
         }

         return rpn;
      }

      private static void ExecuteSemanticAction( SemanticAction action, List<RPNSymbol> rpn, Stack<RPNMark> labels, ref int labelCounter, Terminal last )
      {
         int line = last?.LinePointer ?? 1;
         int col = last?.CharPointer ?? 1;

         switch ( action )
         {
            case SemanticAction.GenPushVar:
               if ( last is Terminal.Identifier id )
               {
                  if ( id.Name != "int" )
                  {
                     rpn.Add( new RPNIdentifier( RPNType.A_VariableName ) { Name = id.Name, LinePointer = line, CharPointer = col } );
                  }
               }
               break;

            case SemanticAction.GenPushConst:
               if ( last is Terminal.Number num )
               {
                  rpn.Add( new RPNNumber( RPNType.A_Number ) { Data = num.Data, LinePointer = line, CharPointer = col } );
               }
               break;

            case SemanticAction.GenPlus:
               rpn.Add( new RPNSymbol( RPNType.F_Plus ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenMinus:
               rpn.Add( new RPNSymbol( RPNType.F_Minus ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenMultiply:
               rpn.Add( new RPNSymbol( RPNType.F_Multiply ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenDivide:
               rpn.Add( new RPNSymbol( RPNType.F_Divide ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenAssign:
               rpn.Add( new RPNSymbol( RPNType.F_Assignment ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenEqual:
               rpn.Add( new RPNSymbol( RPNType.F_Equal ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenLess:
               rpn.Add( new RPNSymbol( RPNType.F_Less ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenGreater:
               rpn.Add( new RPNSymbol( RPNType.F_Greater ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenLessEqual:
               rpn.Add( new RPNSymbol( RPNType.F_LessEqual ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenGreaterEqual:
               rpn.Add( new RPNSymbol( RPNType.F_GreaterEqual ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenRead:
               rpn.Add( new RPNSymbol( RPNType.F_Read ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenWrite:
               rpn.Add( new RPNSymbol( RPNType.F_Write ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenIndex:
               rpn.Add( new RPNSymbol( RPNType.F_Index ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenArrayDecl:
               rpn.Add( new RPNSymbol( RPNType.F_IntArray ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.StartWhile:
               var startLabel = new RPNMark( RPNType.М_Mark, MarkType.WhileBeginMark ) { Position = rpn.Count, LinePointer = line, CharPointer = col };
               rpn.Add( startLabel );
               labels.Push( startLabel );
               break;

            case SemanticAction.WhileCondEnd:
               var endLabel = new RPNMark( RPNType.М_Mark, MarkType.WhileEndMark ) { LinePointer = line, CharPointer = col };
               rpn.Add( endLabel );
               rpn.Add( new RPNSymbol( RPNType.F_ConditionalJumpToMark ) { LinePointer = line, CharPointer = col } );
               labels.Push( endLabel );
               break;

            case SemanticAction.EndWhile:
               var exitLabel = labels.Pop();
               var backLabel = labels.Pop();
               rpn.Add( backLabel );
               rpn.Add( new RPNSymbol( RPNType.F_UnconditionalJumpToMark ) { LinePointer = line, CharPointer = col } );
               exitLabel.Position = rpn.Count;
               rpn.Add( exitLabel );
               break;

            case SemanticAction.IfCondEnd:
               var falseLabel = new RPNMark( RPNType.М_Mark, MarkType.IfMark ) { LinePointer = line, CharPointer = col };
               rpn.Add( falseLabel );
               rpn.Add( new RPNSymbol( RPNType.F_ConditionalJumpToMark ) { LinePointer = line, CharPointer = col } );
               labels.Push( falseLabel );
               break;

            case SemanticAction.IfElseStart:
               var skipElseLabel = new RPNMark( RPNType.М_Mark, MarkType.ElseMark ) { LinePointer = line, CharPointer = col };
               rpn.Add( skipElseLabel );
               rpn.Add( new RPNSymbol( RPNType.F_UnconditionalJumpToMark ) { LinePointer = line, CharPointer = col } );
               var prevFalseLabel = labels.Pop();
               prevFalseLabel.Position = rpn.Count;
               rpn.Add( prevFalseLabel );
               labels.Push( skipElseLabel );
               break;

            case SemanticAction.IfElseEnd:
               var finalLabel = labels.Pop();
               finalLabel.Position = rpn.Count;
               rpn.Add( finalLabel );
               break;
         }
      }
   }
}
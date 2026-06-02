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
      WriteStatement, // Оператор вывода
      VarInit, // Инициализация переменной
      IntStatement // Целочисленный тип данных
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
      GenNotEqual,
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
      GenIntDecl,
      // Запись встроеннных математических функций
      GenSqrt,
      GenSin,
      GenCos,
      GenPostIncrement,
      GenPostDecrement,
      GenExp,
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
         AddRule( NonTerminal.Program, TerminalType.VariableName,
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.Program, TerminalType.Int,
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.Program, TerminalType.If,
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.Program, TerminalType.While,
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.Program, TerminalType.Read,
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.Program, TerminalType.Write,
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.Program, TerminalType.LeftBrace, 
          new StackSymbol( NonTerminal.StatementList )
         );

         // StatementList -> Statement StatementList | epsilon

         AddRule( NonTerminal.StatementList, TerminalType.VariableName,
          new StackSymbol( NonTerminal.Statement ),
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.StatementList, TerminalType.Int, new StackSymbol( NonTerminal.Statement ), 
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.StatementList, TerminalType.If,
          new StackSymbol( NonTerminal.Statement ), 
          new StackSymbol( NonTerminal.StatementList )
         );

         AddRule( NonTerminal.StatementList, TerminalType.While, 
          new StackSymbol( NonTerminal.Statement ), 
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.StatementList, TerminalType.Read, 
          new StackSymbol( NonTerminal.Statement ), 
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.StatementList, TerminalType.Write, 
          new StackSymbol( NonTerminal.Statement ), 
          new StackSymbol( NonTerminal.StatementList ) 
         );

         AddRule( NonTerminal.StatementList, TerminalType.LeftBrace, 
          new StackSymbol( TerminalType.LeftBrace ), 
          new StackSymbol( NonTerminal.StatementList ), 
          new StackSymbol( TerminalType.RightBrace ) 
         );

         AddRule( NonTerminal.StatementList, TerminalType.RightBrace, 
          Array.Empty<StackSymbol>()
         );

         // Statement -> VariableName [GenPushVar] StatementSuffix | If | While | Read | Write

         AddRule( NonTerminal.Statement, TerminalType.VariableName, 
          new StackSymbol( TerminalType.VariableName ),
          new StackSymbol( SemanticAction.GenPushVar ), 
          new StackSymbol( NonTerminal.StatementSuffix ) 
         );

         AddRule( NonTerminal.Statement, TerminalType.Int,
          new StackSymbol( TerminalType.Int ), 
          new StackSymbol( NonTerminal.IntStatement ) 
         );

         AddRule( NonTerminal.Statement, TerminalType.If,
          new StackSymbol( NonTerminal.IfStatement ) 
         );

         AddRule( NonTerminal.Statement, TerminalType.While, 
          new StackSymbol( NonTerminal.WhileStatement )
         );

         AddRule( NonTerminal.Statement, TerminalType.Read, 
          new StackSymbol( NonTerminal.ReadStatement ), 
          new StackSymbol( TerminalType.Semicolon ) 
          );

         AddRule( NonTerminal.Statement, TerminalType.Write, 
          new StackSymbol( NonTerminal.WriteStatement ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.IntStatement, TerminalType.VariableName, 
            new StackSymbol( TerminalType.VariableName ), 
            new StackSymbol( SemanticAction.GenPushVar ), 
            new StackSymbol( SemanticAction.GenIntDecl ), 
            new StackSymbol( NonTerminal.VarInit ) 
         );

         AddRule( NonTerminal.IntStatement, TerminalType.LeftBracket, 
            new StackSymbol( TerminalType.LeftBracket ), 
            new StackSymbol( NonTerminal.Expression ), 
            new StackSymbol( TerminalType.RightBracket ), 
            new StackSymbol( TerminalType.VariableName ), 
            new StackSymbol( SemanticAction.GenPushVar ), 
            new StackSymbol( SemanticAction.GenArrayDecl ), 
            new StackSymbol( TerminalType.Semicolon ) 
         );

         // StatementSuffix -> = Expression [GenAssign] ; | [ Expression ] ArrayAccess

         AddRule( NonTerminal.StatementSuffix, TerminalType.Assignment, 
          new StackSymbol( TerminalType.Assignment ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenAssign ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.StatementSuffix, TerminalType.LeftBracket,
          new StackSymbol( TerminalType.LeftBracket ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightBracket ), 
          new StackSymbol( NonTerminal.ArrayAccess ) 
         );
         
         AddRule( NonTerminal.StatementSuffix, TerminalType.VariableName, 
          new StackSymbol( TerminalType.VariableName ), 
          new StackSymbol( SemanticAction.GenPushVar ), 
          new StackSymbol( SemanticAction.GenIntDecl ), 
          new StackSymbol( NonTerminal.VarInit ) 
         );

         AddRule( NonTerminal.StatementSuffix, TerminalType.Plus, 
          new StackSymbol( TerminalType.Plus ), 
          new StackSymbol( TerminalType.Plus ), 
          new StackSymbol( SemanticAction.GenPostIncrement ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.StatementSuffix, TerminalType.Minus, 
          new StackSymbol( TerminalType.Minus ), 
          new StackSymbol( TerminalType.Minus ), 
          new StackSymbol( SemanticAction.GenPostDecrement ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.VarInit, TerminalType.Semicolon, 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.VarInit, TerminalType.Assignment, 
          new StackSymbol( TerminalType.Assignment ), 
          new StackSymbol( SemanticAction.GenPushVar ), // Дублируем имя переменной на стек ОПС для операции присваивания
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenAssign ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         // ArrayAccess -> = Expression [GenIndex] [GenAssign] ; | VariableName [GenPushVar] [GenArrayDecl] ;

         AddRule( NonTerminal.ArrayAccess, TerminalType.Assignment, 
          new StackSymbol( TerminalType.Assignment ), 
          new StackSymbol( SemanticAction.GenIndex ),      // Команда индексации выполняется сразу
          new StackSymbol( NonTerminal.Expression ),    // Затем вычисляется правая часть
          new StackSymbol( SemanticAction.GenAssign ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.VariableName, 
          new StackSymbol( TerminalType.VariableName ), 
          new StackSymbol( SemanticAction.GenPushVar ), 
          new StackSymbol( SemanticAction.GenArrayDecl ), 
          new StackSymbol( TerminalType.Semicolon ) 
         );

         // Expression -> Term ExpressionPrime

         AddRule( NonTerminal.Expression, TerminalType.VariableName, 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime )
         );

         AddRule( NonTerminal.Expression, TerminalType.Number, 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.Expression, TerminalType.LeftParenthesis,
          new StackSymbol( NonTerminal.Term ),
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.Expression, TerminalType.Sqrt,
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.Expression, TerminalType.Sin,
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.Expression, TerminalType.Cos, 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.Expression, TerminalType.Exp, 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         // ExpressionPrime -> + Term [GenPlus] ExpressionPrime | - Term [GenMinus] ExpressionPrime | epsilon

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Plus, 
          new StackSymbol( TerminalType.Plus ), 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( SemanticAction.GenPlus ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Minus, 
          new StackSymbol( TerminalType.Minus ), 
          new StackSymbol( NonTerminal.Term ), 
          new StackSymbol( SemanticAction.GenMinus ), 
          new StackSymbol( NonTerminal.ExpressionPrime ) 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Semicolon, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.RightParenthesis, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.RightBracket, 
          Array.Empty<StackSymbol>()
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Equal, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Less,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.Greater,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.LessEqual, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ExpressionPrime, TerminalType.GreaterEqual, 
          Array.Empty<StackSymbol>() 
         );

         // Term -> Factor TermPrime

         AddRule( NonTerminal.Term, TerminalType.VariableName,
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime )
         );

         AddRule( NonTerminal.Term, TerminalType.Number, 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.Term, TerminalType.LeftParenthesis,
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.Term, TerminalType.Sqrt,
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.Term, TerminalType.Sin, 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.Term, TerminalType.Cos, 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.Term, TerminalType.Exp, 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         // TermPrime -> * Factor [GenMultiply] TermPrime | / Factor [GenDivide] TermPrime | epsilon

         AddRule( NonTerminal.TermPrime, TerminalType.Multiply, 
          new StackSymbol( TerminalType.Multiply ), 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( SemanticAction.GenMultiply ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );
         
         AddRule( NonTerminal.TermPrime, TerminalType.Divide, 
          new StackSymbol( TerminalType.Divide ), 
          new StackSymbol( NonTerminal.Factor ), 
          new StackSymbol( SemanticAction.GenDivide ), 
          new StackSymbol( NonTerminal.TermPrime ) 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Plus,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Minus, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Semicolon, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.RightParenthesis, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.RightBracket,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Equal,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Less,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.Greater, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.LessEqual, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.TermPrime, TerminalType.GreaterEqual, 
          Array.Empty<StackSymbol>() 
         );

         // Factor -> VariableName [GenPushVar] ArrayAccess | Number [GenPushConst] | ( Expression )

         AddRule( NonTerminal.Factor, TerminalType.VariableName, 
          new StackSymbol( TerminalType.VariableName ), 
          new StackSymbol( SemanticAction.GenPushVar ), 
          new StackSymbol( NonTerminal.ArrayAccess ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.Number, 
          new StackSymbol( TerminalType.Number ), 
          new StackSymbol( SemanticAction.GenPushConst ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.LeftParenthesis, 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightParenthesis ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.Sqrt, 
          new StackSymbol( TerminalType.Sqrt ), 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightParenthesis ), 
          new StackSymbol( SemanticAction.GenSqrt ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.Sin, 
          new StackSymbol( TerminalType.Sin ), 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightParenthesis ), 
          new StackSymbol( SemanticAction.GenSin ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.Cos, 
          new StackSymbol( TerminalType.Cos ), 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightParenthesis ), 
          new StackSymbol( SemanticAction.GenCos ) 
         );

         AddRule( NonTerminal.Factor, TerminalType.Exp, 
            new StackSymbol( TerminalType.Exp ), 
            new StackSymbol( TerminalType.LeftParenthesis ), 
            new StackSymbol( NonTerminal.Expression ), 
            new StackSymbol( TerminalType.RightParenthesis ), 
            new StackSymbol( SemanticAction.GenExp ) 
         );

         // ArrayAccess -> [ Expression ] [GenIndex] | epsilon

         AddRule( NonTerminal.ArrayAccess, TerminalType.LeftBracket, 
          new StackSymbol( TerminalType.LeftBracket ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightBracket ), 
          new StackSymbol( SemanticAction.GenIndex ) 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Plus, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Minus,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Multiply,
          Array.Empty<StackSymbol>()
         );
         
         AddRule( NonTerminal.ArrayAccess, TerminalType.Divide, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Semicolon,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.RightParenthesis, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.RightBracket,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Equal, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Less,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.Greater, 
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.LessEqual,
          Array.Empty<StackSymbol>() 
         );

         AddRule( NonTerminal.ArrayAccess, TerminalType.GreaterEqual, 
          Array.Empty<StackSymbol>() 
         );

         // Condition -> Expression ConditionPrime (Обеспечивает сначала разбор левого операнда, затем правого)

         AddRule( NonTerminal.Condition, TerminalType.VariableName, 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( NonTerminal.ConditionPrime ) 
         );

         AddRule( NonTerminal.Condition, TerminalType.Number, 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( NonTerminal.ConditionPrime ) 
          );
         AddRule( NonTerminal.Condition, TerminalType.LeftParenthesis, 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( NonTerminal.ConditionPrime ) 
         );

         // ConditionPrime -> CompOperator Expression [SemanticAction] (Генерирует оператор ПОСЛЕ правого операнда)

         AddRule( NonTerminal.ConditionPrime, TerminalType.Equal, 
          new StackSymbol( TerminalType.Equal ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenEqual ) 
         );

         AddRule( NonTerminal.ConditionPrime, TerminalType.Less, 
          new StackSymbol( TerminalType.Less ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenLess ) 
         );

         AddRule( NonTerminal.ConditionPrime, TerminalType.Greater, 
          new StackSymbol( TerminalType.Greater ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenGreater ) 
         );

         AddRule( NonTerminal.ConditionPrime, TerminalType.LessEqual, 
          new StackSymbol( TerminalType.LessEqual ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenLessEqual ) 
         );

         AddRule( NonTerminal.ConditionPrime, TerminalType.GreaterEqual, 
          new StackSymbol( TerminalType.GreaterEqual ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( SemanticAction.GenGreaterEqual ) 
         );

         AddRule( NonTerminal.ConditionPrime, TerminalType.NotEqual, 
            new StackSymbol( TerminalType.NotEqual ), 
            new StackSymbol( NonTerminal.Expression ), 
            new StackSymbol( SemanticAction.GenNotEqual ) 
         );

         // Read/Write Statements

         AddRule( NonTerminal.ReadStatement, TerminalType.Read, 
          new StackSymbol( TerminalType.Read ), 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( TerminalType.VariableName ), 
          new StackSymbol( SemanticAction.GenPushVar ), 
          new StackSymbol( TerminalType.RightParenthesis ), 
          new StackSymbol( SemanticAction.GenRead ) 
         );

         AddRule( NonTerminal.WriteStatement, TerminalType.Write, 
          new StackSymbol( TerminalType.Write ), 
          new StackSymbol( TerminalType.LeftParenthesis ), 
          new StackSymbol( NonTerminal.Expression ), 
          new StackSymbol( TerminalType.RightParenthesis ), 
          new StackSymbol( SemanticAction.GenWrite ) 
         );

         // While

         AddRule(NonTerminal.WhileStatement, TerminalType.While,
            new StackSymbol(TerminalType.While),
            new StackSymbol(SemanticAction.StartWhile),
            new StackSymbol(TerminalType.LeftParenthesis),
            new StackSymbol(NonTerminal.Condition),
            new StackSymbol(TerminalType.RightParenthesis),
            new StackSymbol(SemanticAction.WhileCondEnd),
            new StackSymbol(NonTerminal.BlockOrStatement), // Используем строгий блок {}
            new StackSymbol(SemanticAction.EndWhile)
         );

         // If Statement

         AddRule( NonTerminal.IfStatement, TerminalType.If,
            new StackSymbol( TerminalType.If ), 
            new StackSymbol( TerminalType.LeftParenthesis ), 
            new StackSymbol( NonTerminal.Condition ), 
            new StackSymbol( TerminalType.RightParenthesis ), 
            new StackSymbol( SemanticAction.IfCondEnd ), 
            new StackSymbol( NonTerminal.BlockOrStatement ), // Используем строгий блок {} вместо Statement
            new StackSymbol( NonTerminal.ElsePart ) 
         );
         
         AddRule( NonTerminal.ElsePart, TerminalType.Else, 
            new StackSymbol( TerminalType.Else ), 
            new StackSymbol( SemanticAction.IfElseStart ), 
            new StackSymbol( NonTerminal.BlockOrStatement ), // Используем строгий блок {} вместо Statement
            new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.VariableName, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.If, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.While, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.Read, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.Write, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.RightBrace, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.ElsePart, TerminalType.Semicolon, 
          new StackSymbol( SemanticAction.IfElseEnd ) 
         );

         AddRule( NonTerminal.BlockOrStatement, TerminalType.LeftBrace, 
            new StackSymbol( TerminalType.LeftBrace ), 
            new StackSymbol( NonTerminal.StatementList ), 
            new StackSymbol( TerminalType.RightBrace ) 
         );
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
                    // Если мы дошли до конца файла (EOF), то некоторые нетерминалы 
                    // могут быть безопасно раскрыты в пустоту (epsilon)
                    if ( top.NonTerminal == NonTerminal.StatementList || 
                        top.NonTerminal == NonTerminal.ElsePart || 
                        top.NonTerminal == NonTerminal.ExpressionPrime ||
                        top.NonTerminal == NonTerminal.TermPrime ||
                        top.NonTerminal == NonTerminal.ArrayAccess )
                    {
                        continue; // Просто убираем нетерминал со стека и продолжаем разбор
                    }
                    
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
         if ( currentTokenIndex < terminals.Count )
         {
            Terminal trailingToken = terminals[currentTokenIndex];
            throw new CompilerException( 
               $"Синтаксическая ошибка: Лишний символ '{trailingToken.TerminalType}' после конца программы. Возможно, пропущена открывающая скобка '{{' в начале.", 
               trailingToken.LinePointer, 
               trailingToken.CharPointer 
            );
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
                     rpn.Add( new RPNIdentifier( RPNType.A_VariableName ) { Name = id.Name, LinePointer = line, CharPointer = col } );
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

            case SemanticAction.GenNotEqual:
               rpn.Add( new RPNSymbol( RPNType.F_NotEqual ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenNotEqual:
               rpn.Add( new RPNSymbol( RPNType.F_Equal ) { LinePointer = line, CharPointer = col } );
               rpn.Add( new RPNSymbol( RPNType.F_Not ) { LinePointer = line, CharPointer = col } );
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

            case SemanticAction.GenIntDecl:
                rpn.Add( new RPNSymbol( RPNType.F_Int ) { LinePointer = line, CharPointer = col } );
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

            case SemanticAction.GenSqrt:
               rpn.Add( new RPNSymbol( RPNType.F_Sqrt ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenSin:
               rpn.Add( new RPNSymbol( RPNType.F_Sin ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenCos:
               rpn.Add( new RPNSymbol( RPNType.F_Cos ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenExp:
               rpn.Add( new RPNSymbol( RPNType.F_Exp ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenPostIncrement:
               rpn.Add( new RPNSymbol( RPNType.F_PostIncrement ) { LinePointer = line, CharPointer = col } );
               break;

            case SemanticAction.GenPostDecrement:
               rpn.Add( new RPNSymbol( RPNType.F_PostDecrement ) { LinePointer = line, CharPointer = col } );
               break;
         }
      }
   }
}
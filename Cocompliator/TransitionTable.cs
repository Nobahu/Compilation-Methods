namespace Cocompliator
{
    public struct StateTransition
    {
        public int NextState;
        public TerminalType Token;
        public bool IsZStar;
        public bool IsError;
        public bool CheckKeyword;

        public static StateTransition GoTo(int state) => new StateTransition { NextState = state, Token = TerminalType.Number };
        public static StateTransition Z(TerminalType token) => new StateTransition { NextState = -1, Token = token, IsZStar = false };
        public static StateTransition ZStar(TerminalType token, bool checkKw = false) => new StateTransition { NextState = -1, Token = token, IsZStar = true, CheckKeyword = checkKw };
        public static StateTransition Error() => new StateTransition { IsError = true };
        public static StateTransition Skip() => new StateTransition { NextState = 0 };
    }

    public static class TransitionTable
    {
        public static readonly StateTransition[,] Matrix = new StateTransition[12, 24];

        static TransitionTable()
        {
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 24; j++)
                    Matrix[i, j] = StateTransition.Error();

            // s0
            Matrix[0, 0] = StateTransition.GoTo(1);
            Matrix[0, 1] = StateTransition.GoTo(2);
            Matrix[0, 2] = StateTransition.GoTo(3);
            Matrix[0, 3] = StateTransition.GoTo(4);
            Matrix[0, 4] = StateTransition.GoTo(9);
            Matrix[0, 5] = StateTransition.GoTo(10);
            Matrix[0, 6] = StateTransition.GoTo(5);
            Matrix[0, 7] = StateTransition.GoTo(6);
            Matrix[0, 8] = StateTransition.GoTo(7);
            Matrix[0, 9] = StateTransition.GoTo(8);
            Matrix[0, 10] = StateTransition.Z(TerminalType.LeftParenthesis);
            Matrix[0, 11] = StateTransition.Z(TerminalType.RightParenthesis);
            Matrix[0, 12] = StateTransition.Z(TerminalType.LeftBracket);
            Matrix[0, 13] = StateTransition.Z(TerminalType.RightBracket);
            Matrix[0, 14] = StateTransition.Z(TerminalType.LeftBrace);
            Matrix[0, 15] = StateTransition.Z(TerminalType.RightBrace);
            Matrix[0, 16] = StateTransition.Z(TerminalType.Semicolon);
            Matrix[0, 17] = StateTransition.Skip();
            Matrix[0, 18] = StateTransition.Skip();
            Matrix[0, 19] = StateTransition.GoTo(11);
            Matrix[0, 20] = StateTransition.Skip();
            Matrix[0, 21] = StateTransition.Skip();
            Matrix[0, 22] = StateTransition.Skip();

            // s1
            Matrix[1, 0] = StateTransition.GoTo(1);
            Matrix[1, 1] = StateTransition.GoTo(1);
            for (int j = 2; j < 24; j++) Matrix[1, j] = StateTransition.ZStar(TerminalType.VariableName, true);

            // s2
            Matrix[2, 0] = StateTransition.Error();
            Matrix[2, 1] = StateTransition.GoTo(2);
            for (int j = 2; j < 24; j++) Matrix[2, j] = StateTransition.ZStar(TerminalType.Number);

            // s3
            for (int j = 0; j < 24; j++) Matrix[3, j] = StateTransition.ZStar(TerminalType.Plus);

            // s4
            for (int j = 0; j < 24; j++) Matrix[4, j] = StateTransition.ZStar(TerminalType.Minus);

            // s5
            Matrix[5, 6] = StateTransition.Z(TerminalType.Equal);
            for (int j = 0; j < 24; j++) if (j != 6) Matrix[5, j] = StateTransition.ZStar(TerminalType.Assignment);

            // s6
            Matrix[6, 6] = StateTransition.Z(TerminalType.LessEqual);
            for (int j = 0; j < 24; j++) if (j != 6) Matrix[6, j] = StateTransition.ZStar(TerminalType.Less);

            // s7
            Matrix[7, 6] = StateTransition.Z(TerminalType.GreaterEqual);
            for (int j = 0; j < 24; j++) if (j != 6) Matrix[7, j] = StateTransition.ZStar(TerminalType.Greater);

            // s8
            Matrix[8, 6] = StateTransition.Z(TerminalType.NotEqual); 
            for (int j = 0; j < 24; j++) 
            {
                if (j != 6) Matrix[8, j] = StateTransition.ZStar(TerminalType.Not); 
            }

            // s9
            for (int j = 0; j < 24; j++) Matrix[9, j] = StateTransition.ZStar(TerminalType.Multiply);

            // s10
            for (int j = 0; j < 24; j++) Matrix[10, j] = StateTransition.ZStar(TerminalType.Divide);
            
            //s11
            for (int j = 0; j < 24; j++)
            {
                if (j == 22) // Колонка 22 соответствует символу новой строки '\n'
                {
                    Matrix[11, j] = StateTransition.Skip(); // Завершаем комментарий, возвращаемся в s0
                }
                else
                {
                    Matrix[11, j] = StateTransition.GoTo(11); // Все остальные символы просто поглощаем
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace CaroGame.Models
{
    public class AI_Mode_Normal
    {
        Statistics stats;
        public AI_Mode_Normal(Statistics s)
        {
            stats = s;
        }
        Stopwatch sw = new Stopwatch();//Bấm thời gian để biết khi nào AI tính toán xong
        int WinScore = 500000; //500.000 điểm nếu đạt được win
        int AI_depth = 3;
       

        //==== Hàm sinh nước đi, lọc và sắp xếp các nước đi ====
        List<Move> GenerateMove(Board board, int AI = 0)
        {
            //------- Sinh nước đi --------
            HashSet<Move> NewMoves = new HashSet<Move>();
            //Chỉ sinh các nước đi xung quanh các nước đã đi trong bán kính 1 nước
            for (int i = 0; i < board.size; i++)
            {
                for (int j = 0; j < board.size; j++)
                {
                    if (!board.IsAvailable(i, j))
                    {
                        for (int dr = -1; dr <= 1; dr++)
                        {
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                Move NMove = new Move();
                                NMove.row = i + dr;
                                NMove.col = j + dc;
                                if (board.IsAvailable(NMove.row, NMove.col))
                                {
                                    NewMoves.Add(NMove);
                                }
                            }
                        }
                    }
                }
            }
            //Nếu vẫn chưa có nước nào trên bàn cờ thì thực hiện sinh nước đầu tiên tại ô chính giữa bàn cờ
            if (NewMoves.Count == 0)
            {
                Move NMove = new Move();
                NMove.row = board.size / 2;
                NMove.col = board.size / 2;
                NewMoves.Add(NMove);
            }

            List<Move> movesList = NewMoves.ToList();
            return movesList;
        }
        

        //==== Tính tổng điểm của người chơi ====
        int EvaluatePlayer(Board board, int player)
        {
            int score = 0;
            for (int i = 0; i < board.size; i++)
                for (int j = 0; j < board.size; j++)
                    if (board.cells[i, j] == player)
                    {
                        score += 1;
                    }
            return score;
        }

        //==== So sánh lợi thế= ====
        int EvaluateBoard(Board board, int AI)
        {
            int aiScore = EvaluatePlayer(board, AI);
            int humanScore = EvaluatePlayer(board, 3 - AI);
            return aiScore - humanScore;
        }

        //==== Hàm xử lý Minimax, Alpha-beta với lọc nước đi ====
        int Alpha_Beta(Board board, int depth, int alpha, int beta, bool IsMaximizing, int AI)
        {
            if (depth == 0)
                return EvaluateBoard(board, AI);

            if (IsMaximizing)
            {
                int maxEval = int.MinValue;
                int currentPlayer = AI;

                foreach (var move in GenerateMove(board, currentPlayer))
                {
                    board.cells[move.row, move.col] = AI;
                    if (board.CheckWin(move.row, move.col))
                    {
                        board.cells[move.row, move.col] = 0;
                        return WinScore;
                    }
                    int eval = Alpha_Beta(board, depth - 1, alpha, beta, false, AI);
                    board.cells[move.row, move.col] = 0;
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                int human = 3 - AI;

                foreach (var move in GenerateMove(board, human))
                {
                    board.cells[move.row, move.col] = human;
                    if (board.CheckWin(move.row, move.col))
                    {
                        board.cells[move.row, move.col] = 0;
                        return -WinScore;
                    }
                    int eval = Alpha_Beta(board, depth - 1, alpha, beta, true, AI);
                    board.cells[move.row, move.col] = 0;
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        //==== Hàm tìm nước đi tốt nhất ====
        public Move FindBestMove(Board board, int HumanPlayer)
        {
            int AI = 3 - HumanPlayer;
            sw.Restart();
            //Kiểm tra AI có thể thắng hong
            foreach (var move in GenerateMove(board, AI))
            {
                board.cells[move.row, move.col] = AI;
                if (board.CheckWin(move.row, move.col))
                {
                    board.cells[move.row, move.col] = 0;
                    return move;
                }
                board.cells[move.row, move.col] = 0;
            }

            //Tìm nước đi tốt nhất bằng Minimax
            int BestScore = int.MinValue;
            Move BestMove = null;

            //Lấy danh sách nước đi 
            List<Move> Moves = GenerateMove(board, AI);

            foreach (var move in Moves)
            {
                board.cells[move.row, move.col] = AI;
                int score = Alpha_Beta(board, AI_depth - 1, int.MinValue, int.MaxValue, false, AI);
                board.cells[move.row, move.col] = 0;

                if (score > BestScore)
                {
                    BestScore = score;
                    BestMove = move;
                }
            }

            sw.Stop();
            stats.TotalTime += sw.ElapsedMilliseconds;
            stats.Moves++;
            return BestMove;
        }
    }
}
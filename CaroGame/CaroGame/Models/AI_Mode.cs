using System;
using System.Collections.Generic;
using System.Linq;

namespace CaroGame.Models
{
    public class AI_Mode
    {
        // ====== điểm mở và bị chặn phân biệt ======
        int WinScore = 500000;

        int OpenFourScore = 200000;
        int BlockedFourScore = 100000;

        int OpenThreeScore = 30000;
        int BlockedThreeScore = 10000;

        int OpenTwoScore = 3000;
        int BlockedTwoScore = 1000;

        // ======kiểm tra ô có trống và hợp lệ không ======
        bool IsEmpty(Board board, int r, int c)
        {
            return r >= 0 && r < board.size &&
                   c >= 0 && c < board.size &&
                   board.cells[r, c] == 0;
        }

        bool IsInside(Board board, int r, int c)
        {
            return r >= 0 && r < board.size &&
                   c >= 0 && c < board.size;
        }

        // ====== GENERATE MOVE (KHÔNG CẮT – DÙNG CHO CHẶN) ======
        //==== Hàm sinh nước đi hợp lệ (chỉ lấy các nước trống và gần các quân đã đánh - trong vòng 2 ô) ====
        List<Move> GenerateMove(Board board)
        {
            HashSet<Move> moves = new HashSet<Move>();

            for (int i = 0; i < board.size; i++)
                for (int j = 0; j < board.size; j++)
                    if (!board.IsAvailable(i, j))
                        for (int dr = -1; dr <= 1; dr++)
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                int r = i + dr;
                                int c = j + dc;
                                if (board.IsAvailable(r, c))
                                    moves.Add(new Move { row = r, col = c });
                            }

            if (moves.Count == 0)
            {
                moves.Add(new Move
                {
                    row = board.size / 2,
                    col = board.size / 2
                });
            }

            return moves.ToList();
        }

        // ======giới hạn số nước đi 10-12 (dùng cho minimax) ======
        List<Move> GenerateMoveLimited(Board board, int player)
        {
            return GenerateMove(board)
                .Select(m =>
                {
                    board.cells[m.row, m.col] = player;
                    int score = PatternRecognition(board, m.row, m.col);
                    board.cells[m.row, m.col] = 0;
                    return new { Move = m, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .Take(12) // giới hạn số nước sinh 
                .Select(x => x.Move)
                .ToList();
        }

        //==== Hàm nhận diện mẫu trên một ô và chấm điểm ====
        int PatternRecognition(Board board, int r, int c)
        {
            int score = 0;
            int[,] dirs = board.direction;

            for (int i = 0; i < 4; i++)
            {
                int dr = dirs[i, 0];
                int dc = dirs[i, 1];

                int c1 = board.CountDirect(r, c, dr, dc);
                int c2 = board.CountDirect(r, c, -dr, -dc);
                int count = 1 + c1 + c2;

                bool open1 = IsEmpty(board, r + (c1 + 1) * dr, c + (c1 + 1) * dc);
                bool open2 = IsEmpty(board, r - (c2 + 1) * dr, c - (c2 + 1) * dc);

                if (count >= 5)
                    score += WinScore;
                else if (count == 4)
                    score += (open1 && open2) ? OpenFourScore :
                             (open1 || open2) ? BlockedFourScore : 0;
                else if (count == 3)
                    score += (open1 && open2) ? OpenThreeScore :
                             (open1 || open2) ? BlockedThreeScore : 0;
                else if (count == 2)
                    score += (open1 && open2) ? OpenTwoScore :
                             (open1 || open2) ? BlockedTwoScore : 0;
            }

            return score;
        }

        // ======  Tính tổng điểm của người chơi ======
        int EvaluatePlayer(Board board, int player)
        {
            int score = 0;
            for (int i = 0; i < board.size; i++)
                for (int j = 0; j < board.size; j++)
                    if (board.cells[i, j] == player)
                        score += PatternRecognition(board, i, j);
            return score;
        }

        //==== So sánh lợi thế giữa human và AI ====
        int EvaluateBoard(Board board, int AI)
        {
            return EvaluatePlayer(board, AI)
                 - 3 * EvaluatePlayer(board, 3 - AI);
        }

        // ======hàm xử lý minimax, alpha_beta======
        int Alpha_Beta(Board board, int depth, int alpha, int beta, bool isMax, int AI)
        {
            if (depth == 0)
            {
                int eval = EvaluateBoard(board, AI);
                if (eval < -OpenFourScore)
                    return eval - 100000;
                return eval;
            }

            int currentPlayer = isMax ? AI : (3 - AI);
            var moves = GenerateMoveLimited(board, currentPlayer);

            if (isMax)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    board.cells[move.row, move.col] = currentPlayer;

                    if (board.CheckWin(move.row, move.col))
                    {
                        board.cells[move.row, move.col] = 0;
                        return WinScore;
                    }

                    int eval = Alpha_Beta(board, depth - 1, alpha, beta, false, AI);
                    board.cells[move.row, move.col] = 0;

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    board.cells[move.row, move.col] = currentPlayer;

                    if (board.CheckWin(move.row, move.col))
                    {
                        board.cells[move.row, move.col] = 0;
                        return -WinScore;
                    }

                    int eval = Alpha_Beta(board, depth - 1, alpha, beta, true, AI);
                    board.cells[move.row, move.col] = 0;

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        // ======nước chặn (CỰC QUAN TRỌNG) ======
        Move FindBlockingMove(Board board, int human)
        {
            int baseThreat = EvaluatePlayer(board, human);
            Move bestBlock = null;
            int maxThreat = 0;

            foreach (var move in GenerateMove(board))
            {
                board.cells[move.row, move.col] = human;
                int threat = EvaluatePlayer(board, human);
                board.cells[move.row, move.col] = 0;

                if (threat >= baseThreat + OpenFourScore)
                    return move;

                if (threat > maxThreat)
                {
                    maxThreat = threat;
                    bestBlock = move;
                }
            }

            return bestBlock;
        }

        // ====== tìm nước đi tối ưu nhất ======
        public Move FindBestMove(Board board, int humanPlayer)
        {
            int ai = 3 - humanPlayer;

            // 1️ AI thắng ngay
            foreach (var move in GenerateMove(board))
            {
                board.cells[move.row, move.col] = ai;
                if (board.CheckWin(move.row, move.col))
                {
                    board.cells[move.row, move.col] = 0;
                    return move;
                }
                board.cells[move.row, move.col] = 0;
            }

            // 2️ CHẶN BẮT BUỘC
            Move block = FindBlockingMove(board, humanPlayer);
            if (block != null)
                return block;

            // 3️ MINIMAX
            int moveCount = board.cells.Cast<int>().Count(x => x != 0);
            int depth = moveCount < 6 ? 2 : 3;

            int bestScore = int.MinValue;
            Move bestMove = null;

            foreach (var move in GenerateMoveLimited(board, ai))
            {
                board.cells[move.row, move.col] = ai;
                int score = Alpha_Beta(board, depth, int.MinValue, int.MaxValue, false, ai);
                board.cells[move.row, move.col] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }
    }
}

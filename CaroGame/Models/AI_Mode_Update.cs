using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace CaroGame.Models
{
    public class AI_Mode_Update
    {   
        Statistics stats;
        public AI_Mode_Update(Statistics s)
        {
            stats = s;
        }
        Stopwatch sw=new Stopwatch();//Bấm thời gian để biết khi nào AI tính toán xong

        //Điểm của các trường hợp
        int WinScore = 1000000; //1.000.000 điểm nếu đạt được win
        int FourScore = 100000; //100.000 điểm cho 4 điểm liên tiếp 
        int FourOpenScore = 150000; //150.000 điểm cho 4 điểm mở 
        int ThreeOpenScore = 100000; //100.000 điểm cho 3 điểm mở 2 đầu
        int ThreeScore = 10000; //10.000 điểm cho 3 điểm liên tiếp
        int TwoOpenScore = 5000; //5.000 điểm cho 2 điểm mở 2 đầu
        int TwoScore = 1000; //1.000 điểm cho 2 điểm liên tiếp
        int AI_depth = 4; //Độ xâu bằng 4

        //==== Đánh giá nước đi bằng nhận diện mẫu ====
        int EvaluateMove(Board board, int r, int c, int player)
        {
            //Đánh giá nước đi này có tốt hong
            board.cells[r, c] = player;
            int score = Pattern_Recognition(board, r, c);
            board.cells[r, c] = 0;
            return score;
        }

        //==== Hàm sinh nước đi, lọc và sắp xếp các nước đi ====
        List<Move> GenerateMove(Board board, int AI = 0, bool filterMoves = true)
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

            //------- Lọc và sắp xếp nước đi theo thang điểm ---------
            if (filterMoves && AI != 0 && movesList.Count > 5)
            {
                List<(Move move, int score)> scoredMoves = new List<(Move, int)>();
                int player = 3 - AI;

                foreach (var move in movesList)
                {
                    int attackScore = EvaluateMove(board, move.row, move.col, AI);
                    int defenseScore = EvaluateMove(board, move.row, move.col, player);

                    //Ưu tiên phòng thủ nếu đối thủ có điểm cao
                    int totalScore;
                    if (defenseScore > ThreeScore)
                        totalScore = attackScore + (int)(defenseScore * 1.25);
                    else
                        totalScore = attackScore + defenseScore;
                    scoredMoves.Add((move, totalScore));
                }

                //Sắp xếp giảm dần theo điểm
                scoredMoves.Sort((a, b) => b.score.CompareTo(a.score));

                //Chỉ lấy 10 nước đi tốt nhất
                int maxMoves = Math.Min(5, scoredMoves.Count);
                movesList = scoredMoves.Take(maxMoves).Select(x => x.move).ToList();
            }

            return movesList;
        }

        //==== Kiểm tra một hướng có bị chặn không ====
        bool IsBlocked(Board board, int CheckR,int CheckC, int player)
        {
            if (CheckR < 0 || CheckR >= board.size || CheckC < 0 || CheckC >= board.size)
                return true;
            if (board.cells[CheckR, CheckC] == (3 - player))
                return true;

            return false;
        }

        //==== Hàm nhận diện mẫu phân biệt mở/chặn ====
        int Pattern_Recognition(Board board, int r, int c)
        {
            int score = 0;
            int player = board.cells[r, c];
            int[,] directions = board.direction;

            for (int i = 0; i < 4; i++)
            {
                int count = 1;
                int dr = directions[i, 0];
                int dc = directions[i, 1];
                //Đếm theo cả 2 hướng
                int Forward = board.CountDirect(r, c, dr, dc);
                int Backward = board.CountDirect(r, c, -dr, -dc);
                count += Forward + Backward;

                //Kiểm tra xem 2 đầu có bị chặn không
                bool blockedForward = IsBlocked(board, (r + (Forward + 1) * dr) , (c + (Forward + 1) * dc), player);
                bool blockedBackward = IsBlocked(board, (r - (Backward + 1) * dr), (c - (Backward + 1) * dc), player);

                int CountOpen = 0;
                if (!blockedForward) CountOpen++;
                if (!blockedBackward) CountOpen++;

                //Chấm điểm dựa trên số quân và số đầu mở
                if (count == 5)
                {
                    score += WinScore;
                }
                else if (count == 4)
                {
                    if (CountOpen == 2)
                        score += FourOpenScore; 
                    else if (CountOpen == 1)
                        score += FourScore;     
                    else
                        score += FourScore / 10; 
                }
                else if (count == 3)
                {
                    if (CountOpen == 2)
                        score += ThreeOpenScore; 
                    else if (CountOpen == 1)
                        score += ThreeScore;   
                    else
                        score += ThreeScore / 10;
                }
                else if (count == 2)
                {
                    if (CountOpen == 2)
                        score += TwoOpenScore;   
                    else if (CountOpen == 1)
                        score += TwoScore;      
                    else
                        score += TwoScore / 10; 
                }
            }
            return score;
        }

        //==== Tính tổng điểm của người chơi ====
        int EvaluatePlayer(Board board, int player)
        {
            int score = 0;
            for (int i = 0; i < board.size; i++)
                for (int j = 0; j < board.size; j++)
                    if (board.cells[i, j] == player)
                    {
                        score += Pattern_Recognition(board, i, j);
                    }
            return score;
        }

        //==== So sánh lợi thế - Ưu tiên phòng thủ ====
        int EvaluateBoard(Board board, int AI)
        {
            int aiScore = EvaluatePlayer(board, AI);
            int humanScore = EvaluatePlayer(board, 3 - AI);
            //Nếu đối thủ có điểm cao thì ưu tiên phòng thủ
            if (humanScore > ThreeOpenScore)
                humanScore = (int)(humanScore * 1.25);

            return aiScore - humanScore;
        }

        //==== Hàm xử lý Minimax, Alpha-beta ====
        int Alpha_Beta(Board board, int depth, int alpha, int beta, bool IsMaximizing, int AI)
        {
            if (depth == 0)
                return EvaluateBoard(board, AI);

            if (IsMaximizing)
            {
                int maxEval = int.MinValue;
                int currentPlayer = AI;
                bool Filter = depth > 1;
                foreach (var move in GenerateMove(board, currentPlayer, Filter))
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
                bool FilterDepth = depth > 1;

                foreach (var move in GenerateMove(board, human, FilterDepth))
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

        //==== Phát hiện mối đe dọa ====
        Move FindImmediateThreat(Board board, int HumanPlayer)
        {
            //Kiểm tra xem người chơi có nước nào sắp thắng k
            foreach (var move in GenerateMove(board, HumanPlayer, false))
            {
                board.cells[move.row, move.col] = HumanPlayer;
                if (board.CheckWin(move.row, move.col))
                {
                    board.cells[move.row, move.col] = 0;
                    return move;
                }
                board.cells[move.row, move.col] = 0;
            }
            return null;
        }

        //==== Hàm tìm nước đi tốt nhất ====
        public Move FindBestMove(Board board, int HumanPlayer)
        {
            int AI = 3 - HumanPlayer;
            sw.Restart();   
            //Kiểm tra AI có thể thắng hay k
            foreach (var move in GenerateMove(board, AI, false))
            {
                board.cells[move.row, move.col] = AI;
                if (board.CheckWin(move.row, move.col))
                {
                    board.cells[move.row, move.col] = 0;
                    return move; 
                }
                board.cells[move.row, move.col] = 0;
            }

            //Kiểm tra xem đối thủ có nước thắng ngay k
            Move threatMove = FindImmediateThreat(board, HumanPlayer);
            if (threatMove != null)
                return threatMove; 

            //Tìm nước đi tốt nhất bằng Minimax
            int BestScore = int.MinValue;
            Move BestMove = null;

            //Lấy danh sách nước đi 
            List<Move> filteredMoves = GenerateMove(board, AI, true);

            foreach (var move in filteredMoves)
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
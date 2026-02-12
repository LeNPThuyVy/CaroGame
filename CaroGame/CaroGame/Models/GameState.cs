using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaroGame.Models
{
    public class GameState
    {
        public Board board =new Board();
        public int CurrentPlayer { get; set; }
        public bool IsOver { get; set; } = false;
        public int Winner { get; set; }
        public int HumanPlayer { get; set; } = 1;//Cho phép người chơi chọn chơi với vai trò X hoặc O trong AI mode
        public bool IsAIMode {  get; set; }//Chế độ AI Mode
        public int Depth { get; set; } = 2; //Độ sâu cho chế độ AI mode
    }
}
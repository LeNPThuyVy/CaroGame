using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CaroGame.Models;
using Microsoft.Ajax.Utilities;

namespace CaroGame.Controllers
{
    public class GameController : Controller
    {
        // GET: Game
        //Lưu thông tin bàn cờ trong session
        public GameState state
        {
            get
            {
                if (Session["State"] == null)
                {
                    Session["State"] = new GameState()
                    {
                        board = new Board(),
                        CurrentPlayer = 1,//Người chơi X đi trước
                        IsOver = false,
                        Winner = 0
                    };

                }
                return (GameState)Session["State"];
            }
            set
            {
                Session["State"] = value;
            }
        }


        public ActionResult Index()
        {
            return View(state);
        }

		public ActionResult SelectMode(bool isAI)
		{
            GameState st = state;
            st.IsAIMode = isAI;
            st.IsOver = false;
            st.Winner = 0;
            st.board= new Board();
            st.CurrentPlayer = 1;
            state = st;
            return RedirectToAction("Index");
		}

		//AI mode
		public ActionResult AfterClick_AI_Mode(int r,int c)
		{
            GameState st = state;

			//Kiểm tra game có còn chơi tiếp được không
			if (st.IsOver)
				return RedirectToAction("Index");

            //-- Lượt của người chơi --
            if (!st.board.IsAvailable(r, c))
                return RedirectToAction("Index");

            st.board.cells[r, c] = st.HumanPlayer;
            st.CurrentPlayer = st.HumanPlayer;

            if(st.board.CheckWin(r, c))
            {
                st.IsOver = true;
                st.Winner = st.HumanPlayer;
                TempData["Winner"] = st.Winner;
                state = st;
                return View("Index",st);
            }

            //Lượt của AI
            AI_Mode AI = new AI_Mode();
            Move AI_Move = AI.FindBestMove(st.board, st.HumanPlayer);

            if (AI_Move!=null)
            {
                st.board.cells[AI_Move.row, AI_Move.col] = 3 - st.HumanPlayer;
                st.CurrentPlayer= 3- st.HumanPlayer;

                if (st.board.CheckWin(AI_Move.row,AI_Move.col))
                {
                    st.IsOver = true;
                    st.Winner = 3-st.HumanPlayer;
                    TempData["Winner"] = st.Winner;
                }    
            }
            state = st;
            return View("Index",st);

        }

        // 2 Player mode
		public ActionResult AfterClick(int r,int c)
        {
            GameState st = state;
            //Kiểm tra game có còn chơi tiếp được không
            if(st.IsOver)
                return RedirectToAction("Index");
            //Kiểm tra đã có quân trong ô chưa
            if (!st.board.IsAvailable(r,c))
                return RedirectToAction("Index");//Nếu có thì không cho phép đánh
            //Hợp lệ thì cho phép đánh vào ô
            st.board.cells[r, c] = st.CurrentPlayer;

            //Kiểm tra có ai đã win trên board không
            if(st.board.CheckWin(r,c))
            {
                st.IsOver = true;
                st.Winner = st.CurrentPlayer;
                TempData["Winner"] = st.Winner;
            }
            else//Nếu chưa thì chuyển lượt
            {
                if (st.CurrentPlayer == 1)
                    st.CurrentPlayer = 2;
                else
                    st.CurrentPlayer = 1;
            }    
            state = st;
            return View("Index", st);
        }
    }
}
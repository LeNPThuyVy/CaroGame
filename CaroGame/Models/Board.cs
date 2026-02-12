using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaroGame.Models
{
    public class Board
    {
        public int size = 15;//Kích thước bàn cờ
        public int[,] cells { get; set; }//bàn cờ
        public Board()
        {
            cells = new int[size,size];
        }

        //Hàm kiểm tra ô có hợp lệ để đánh vào hay không
        public bool IsAvailable(int r, int c)
        {
            if ((r>=0 && r<size) && (c>=0 && c<size))
                if(cells[r, c] == 0)
                    return true;
            return false;
        }

        //Hàm gán nước đi của người chơi vào ô tương ứng
        public void PlayerMovement(int r, int c,int player)
        {
            cells[r, c] = player;
        }

        public int[,] direction = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };// Thứ tự lần lượt: ngang, dọc, chéo chính, chéo phụ

        //Hàm đếm số nút liền kề theo hướng chỉ định
        public int CountDirect(int r, int c, int dr, int dc)
        {
            int count=0, player=cells[r, c];
            r += dr;
            c += dc;
            while ((r>=0 && r<size) && (c>=0&& c<size)&& (cells[r,c]==player))
            {
                count++;
                r += dr;
                c += dc;
            }    
            return count;
        }
        
        //Hàm kiểm tra liệu đã có ai thắng hay chưa (chỉ kiểm tra nước cuối thay vì kiểm tra toàn bộ trạng thái bàn cờ)
        public bool CheckWin(int r, int c)
        {
            //Lấy thông tin player hiện tại
            int player=cells[r, c];
            //Duyệt từng hướng để kiểm tra
            for (int i =0;i<4;i++)
            {
                int count =1;
                //Đếm xem hướng phía trước có bao nhiêu nút liên tiếp giống nút vừa đi
                count +=CountDirect(r, c, direction[i, 0], direction[i,1]);
                //Đếm xem hướng phía sau có bao nhiêu nút liên tiếp giống nút vừa đi
                count += CountDirect(r, c, -direction[i, 0], -direction[i, 1]);
                //Kiểm tra điều kiện win (5 ô liên tiếp)
                if (count>=5)
                    return true;
            }
            return false;
        }

        

    }
}
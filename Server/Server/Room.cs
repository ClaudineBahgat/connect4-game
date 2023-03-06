using System.Collections.Generic;

namespace Server
{
    public class Room
    {
        public string player1Name { get; set; }
        public string player2Name { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public string player1Color { get; set; }
        string player2Color;
        public int roomIndex { get; set; }
        public List<User> players = new List<User>();
        public List<User> watchers = new List<User>();
        public Room()
        {
            roomIndex = 0;
        }
        public Room(int roomNo,int row,int col)
        {
            this.roomIndex = roomNo;
            this.row = row;
            this.col = col;
        }
        public Room(string player1Name,int row, int col, string player1Color,int roomIndex,User player1)
        {
            this.player1Name = player1Name;
            this.row = row; 
            this.col = col;
            player1.Color = player1Color;
            this.player1Color = player1Color;
            this.roomIndex = roomIndex;
            players.Add(player1);
        }
        public void setPlayer2Color()
        {
            if(player1Color=="red")
            {
                player2Color = "yellow";
                players[1].Color = "yellow";
            }
            else
            {
                player2Color = "red";
                players[1].Color = "red";
            }
        }
    }
}

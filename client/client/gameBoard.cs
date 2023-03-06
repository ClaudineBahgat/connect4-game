using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace client
{
    public partial class gameBoard : Form
    {
        Brush brush;
        Color color;
        int roomNo;
        private Rectangle[] boardColumns;
        /************ Board Measurements *************/
        int boardWidth;
        int boardHeight;
        int XStart;
        int YStart;
        /************ Cells Measurments  ****************/
        int XSpace;
        int YSpace;
        //
        int[,] board;
        int rowN;
        int colN;
        //******************
        int MyNumber;
        int otherPlayerNo;
        int turn;
        Thread thread;
        int columnIndex;
        int rowIndex;
        string myColor;
        string otherPlayerColor;
        bool flag = true;
        int playerscount;
        string playertype;
        Pen myPen;  
        public gameBoard(string playerType)
        {
            InitializeComponent();
            rowN = CreateRoom.Row;
            colN = CreateRoom.Col;
            this.boardColumns = new Rectangle[colN];
            board = new int[rowN, colN];
            myPen = new Pen(Color.Black, 3);
            label1.Text = $"Player: {start.UserName}";
            this.turn = 1;
            thread = new Thread(new ThreadStart(ClientListener));
            thread.Start();
            roomNo = int.Parse(Roomgame.RoomNo);
            playertype = playerType;
            if(playerType=="watcher")
            {
                button_WOC1.Enabled = true;
                button_WOC1.Visible = true;
                label1.Text = $"Watcher: {start.UserName}";
            }
        }
        private void ClientListener()
        {
            while (true)
            {
                if (Connection.getStream() != null)
                {
                    string message = Connection.getReader().ReadLine();
                    string[] x = message.Split('|');
                    if (x[0] == "roomNumber")//roomNumber|roomNo|red|playerNo|playersCount|row|col
                    {
                        roomNo = int.Parse(x[1]);
                        MyNumber = int.Parse(x[3]);
                        myColor = x[2];
                        playerscount = int.Parse(x[4]);
                        rowN = int.Parse(x[5]);
                        colN = int.Parse(x[6]);
                        this.boardColumns = new Rectangle[colN];
                        board = new int[rowN, colN];
                        Invalidate();
                    }
                    if (x[0] == "Lose") 
                    {
                        string caption = $"player{MyNumber} : {start.UserName}";
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result;
                        result = MessageBox.Show("sorry You lose! Do you want play again?", caption, buttons);
                        if (result == DialogResult.Yes)  
                        {
                            board = new int[rowN, colN];
                            turn = 1;
                            Invalidate();
                        }
                        else
                        {
                            Connection.getWriter().WriteLine($"cancel|{MyNumber}|{Roomgame.RoomNo}");
                            this.Close();
                        }
                    }
                    if (x[0] == "ChangePoint") //"pointChanged|row|col|color|turn"
                    {         
                        if (x[3] == "red")
                        {
                            color = Color.Red;
                        }
                        else
                        {
                            color = Color.Yellow;
                        }
                        if (!this.Text.Contains("Watcher"))
                            turn = int.Parse(x[4]);
                        ReDrawEllipse(int.Parse(x[1]), int.Parse(x[2]), color);
                    }
                    if (x[0]== "GameEnded")
                    {
                        MessageBox.Show("The other player Ended the Game");
                        this.Close();
                    }
                    if (x[0]== "startGame")
                    {
                        playerscount = 2;
                    }
                    Connection.getStream().Flush();
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            DisplayBoard();
        }
        public void DisplayBoard()
        {
            Graphics g = this.CreateGraphics();
            if(rowN==6&&colN==7)
            {
                boardHeight = 300;
                boardWidth = 350;
            }
            else if(rowN == 7 && colN == 8)
            {
                boardHeight = 365;
                boardWidth = 400;
            }
            else if (rowN == 7 && colN == 9)
            {
                boardHeight = 365;
                boardWidth = 450;
            }
            XStart = 24;
            YStart = 70;
            XSpace = 50;
            YSpace = 50;

            g.FillRectangle(Brushes.DodgerBlue, XStart, YStart, boardWidth, boardHeight);
            for (int i = 0; i < rowN; i++)
            {
                for (int j = 0; j < colN; j++)
                {
                    if (i == 0)
                    {
                        this.boardColumns[j] = new Rectangle(32 + XSpace * j, YStart, 32, boardHeight);
                    }
                    g.FillEllipse(Brushes.White, 32 + XSpace * j, 80 + (YSpace * i), 32, 32);
                    g.DrawEllipse(myPen, 32 + XSpace * j, 80 + (YSpace * i), 32, 32);
                }
            }
        }
        private void button_WOC1_Click(object sender, EventArgs e)
        {
            Connection.stopWatch(roomNo.ToString());
            this.Close();
        }
        private void label1_Click(object sender, EventArgs e)
        {
        }
        void ReDrawEllipse(int row, int col, Color playercolor)
        {
            rowIndex = this.emptyRow(col);
            if (rowIndex != -1)
            {
                if (MyNumber == 1)
                    this.board[row, col] = 2;
                else
                    this.board[row, col] = 1;
                Graphics g = this.CreateGraphics();
                brush = new SolidBrush(playercolor);
                g.FillEllipse(brush, 32 + 50 * col, 80 + (50 * row), 32, 32);
            }
        }
        void drawEllipse(int row, int col, Color playercolor)
        {
            Graphics g = this.CreateGraphics();
            brush = new SolidBrush(playercolor);
            g.FillEllipse(brush, 32 + 50 * col, 80 + (50 * row), 32, 32);
        }
        private void gameBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if(playertype!= "watcher")
            {
                if (playerscount == 2)
                {
                    columnIndex = this.columnNumber(e.Location);
                    if (columnIndex != -1)
                    {
                        rowIndex = this.emptyRow(columnIndex);
                        if (rowIndex != -1)
                        {
                            this.board[rowIndex, columnIndex] = this.turn;
                            if (turn == MyNumber)
                            {
                                if (myColor == "red")
                                {
                                    color = Color.Red;
                                    otherPlayerColor = "Yellow";
                                }
                                else
                                {
                                    color = Color.Yellow;
                                    otherPlayerColor = "Red";
                                }
                                if (this.turn == 1)
                                {
                                    this.turn = 2;
                                }
                                else
                                {
                                    this.turn = 1;
                                }
                                drawEllipse(rowIndex, columnIndex, color);
                                Connection.getWriter().WriteLine($"pointChanged|{Roomgame.RoomNo}|{turn}|{rowIndex}|{columnIndex}");
                            }
                            else
                            {
                                MessageBox.Show("Not your Turn");
                            }
                            int winner = this.Winner(MyNumber);
                            if (winner != -1)
                            {
                                string caption = $"player{MyNumber} : {start.UserName}";
                                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                                DialogResult result;
                                Connection.getWriter().WriteLine($"sendReault|{MyNumber}|{Roomgame.RoomNo}");
                                result = MessageBox.Show("Congratulations You won! Do you want play again?", caption, buttons);
                                if (result == DialogResult.Yes)  //playAgain|winnerNo|roomNO
                                {
                                    board = new int[rowN,colN];
                                    turn = 1;
                                    Invalidate();
                                }
                                else
                                {
                                    Connection.getWriter().WriteLine($"cancel|{MyNumber}|{Roomgame.RoomNo}");
                                    this.Close();
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please wait for Player2 to join");
                }
            }
            else
            {
                MessageBox.Show("Your permission is watching only");
            }
        }
        private int columnNumber(Point mouse)
        {
            for (int i = 0; i < boardColumns.Length; i++)
            {
                if ((mouse.X >= boardColumns[i].X) && (mouse.Y >= boardColumns[i].Y))
                {
                    if ((mouse.X <= this.boardColumns[i].X + this.boardColumns[i].Width) &&
                     (mouse.Y <= this.boardColumns[i].Y + this.boardColumns[i].Height))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        private int emptyRow(int col)
        {
            for (int i = rowN-1; i >= 0; i--)
            {
                if (this.board[i, col] == 0)
                {
                    return i; }
            }
            return -1;
        }
        /******************************************************************************/
        private bool EqualNums(int toCheck, params int[] numbers)
        {
            foreach (int num in numbers)
            {
                if (toCheck != num)
                    return false;
            }
            return true;
        }
        private int Winner(int playerToCheck)
        {
            //vertical win check
            for (int row = 0; row < this.board.GetLength(0) - 3; row++)
            {
                for (int col = 0; col < this.board.GetLength(1); col++)
                {
                    if (this.EqualNums(playerToCheck, this.board[row, col], this.board[row + 1, col],
                        this.board[row + 2, col], this.board[row + 3, col]))
                        return playerToCheck;
                }
            }
            //horizontal win check
            for (int row = 0; row < this.board.GetLength(0); row++)
            {
                for (int col = 0; col < this.board.GetLength(1) - 3; col++)
                {
                    if (this.EqualNums(playerToCheck, this.board[row, col], this.board[row, col + 1],
                        this.board[row, col + 2], this.board[row, col + 3]))
                        return playerToCheck;
                }
            }
            //top-left diagonal check
            for (int row = 0; row < this.board.GetLength(0) - 3; row++)
            {
                for (int col = 0; col < this.board.GetLength(1) - 3; col++)
                {
                    if (this.EqualNums(playerToCheck, this.board[row, col], this.board[row + 1, col + 1],
                        this.board[row + 2, col + 2], this.board[row + 3, col + 3]))
                        return playerToCheck;
                }
            }
            //Back-right diagonal check
            for (int row = 0; row < this.board.GetLength(0) - 3; row++)
            {
                for (int col = 3; col < this.board.GetLength(1); col++)
                {
                    if (this.EqualNums(playerToCheck, this.board[row, col], this.board[row + 1, col - 1],
                        this.board[row + 2, col - 2], this.board[row + 3, col - 3]))
                    {

                        return playerToCheck;
                    }
                }
            }

            return -1;
        }
    }
}

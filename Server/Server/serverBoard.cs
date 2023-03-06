using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Server
{
    public partial class serverBoard : Form
    {
        TcpListener listener;
        List<User> users = new List<User>();
        List<Room> availableRooms = new List<Room>();
        bool flag = true;
        int playerId;
        int roomsCount = 2;
        public serverBoard()
        {
            InitializeComponent();
        }
        void  StartConnection()
        {
            playerId= 0;
            IPAddress localaddr = new IPAddress(new byte[] { 127, 0, 0, 1 });
            listener = new TcpListener(localaddr, 1025);
            listener.Start();
            createDefaultRooms();
            listView1.Items.Add(new ListViewItem("The server started..."));
            Thread myThread = new Thread(()=>{
            while (flag)
            {
                Socket Connection = listener.AcceptSocket();
                User user = new User(Connection);
                if (Connection != null) 
                {
                    users.Add(user);
                }
                HandleExceptionOnControls("new client entered game...","listView1");
                user.newClientMessage += this.Item_newClientMessage; //subscribe to newClientMessage event
                user.publishEvent();
            }
            });
            myThread.Start();
        }
        private void Item_newClientMessage(User sender, StreamWriter Writer, StreamReader Reader, string[] streamData, Socket Connection)
        {
            if (streamData[0] == "signIn")
            {
                signIn(streamData, Writer);
            }
            else if (streamData[0] == "createRoom")
            {
                createRoomRequest(streamData);
            }
            else if (streamData[0] == "join")
            {
                joinGameRequest(streamData);
            }
            else if (streamData[0] == "watch")
            {
                watchGameRequest(streamData);
            }
            else if (streamData[0] == "stopWatch")
            {
                stopWatching(streamData);
            }
            else if (streamData[0] == "UsersData")
            {
                requestPlayersData(streamData, Writer);
            }
            else if (streamData[0] == "pointChanged")
            {
                sendLocation(streamData, sender);
            }
            else if (streamData[0] == "sendReault")
            {
                sendResultToPlayers(streamData);
            }
            else if (streamData[0] == "cancel")
            {
                CancelGame(streamData);
            }
            else if (streamData[0] == "Close")
            {
                users.Remove(sender);
                Reader.Close();
                Writer.Close();
                Connection.Shutdown(SocketShutdown.Both);
                Connection.Close();
            }
        }
        private void createDefaultRooms()
        {
            Room room = new Room(1, 6, 7);
            availableRooms.Add(room);
            HandleExceptionOnControls($"Room available count = {0}", "listView1");
        }
        void writeInFile(int roomNo, int winner)
        {
            StreamWriter writer = File.CreateText(@"C:\Users\hp\Desktop\GameData.txt");
            writer.WriteLine($"Room Number: {roomNo}");
            writer.WriteLine($"Player 1 name: {availableRooms[roomNo - 1].player1Name}");
            writer.WriteLine($"Player 2 name: {availableRooms[roomNo - 1].player2Name}");
            writer.WriteLine($"Player {winner} is The WINNER of this Game");
            writer.WriteLine($"Date of Game: {DateTime.Now.ToString("F")}");
            writer.Close();
        }
        void HandleExceptionOnControls(string message, string control)
        {
            if (control == "listView1")
            {
                if (listView1.InvokeRequired)
                {
                    listView1.Invoke(new MethodInvoker(() =>
                    {
                        listView1.Items.Add(new ListViewItem(message));
                    }));
                }
            }
            else
            {
                if (label2.InvokeRequired)
                {
                    label2.Invoke(new MethodInvoker(() =>
                    {
                        label2.Text = message;
                    }));
                }
            }
        }
        public void signIn(string[] streamData, StreamWriter Writer)
        {
            users[playerId].Id = playerId;
            users[playerId++].UserName = streamData[1];
            HandleExceptionOnControls($"CLIENT Name = {streamData[1]} & Id = {playerId}", "listView1");
            HandleExceptionOnControls($"Number of players: {users.Count}", "label2");
            displayAvailableRooms(Writer);
        }
        void displayAvailableRooms(StreamWriter Writer)
        {
            try
            {
                string allRooms = "";
                if (availableRooms.Count > 0)
                {
                    foreach (var item in availableRooms)
                    {
                        allRooms += $"{item.roomIndex}|{item.row}|{item.col}|{item.players.Count}&";
                    }
                    Writer.WriteLine(allRooms);
                    HandleExceptionOnControls("Rooms data sent done", "listView1");
                }
            }
            catch (NullReferenceException e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void createRoomRequest(string[] streamData)
        {
            Room room = new Room(streamData[1], int.Parse(streamData[2]), int.Parse(streamData[3]), streamData[4], roomsCount++, users[users.Count - 1]);
            availableRooms.Add(room);
            for (int i = 1; i < streamData.Length; i++)
            {
                HandleExceptionOnControls($"Room Data = {streamData[i]}", "listView1");
            }            
        }
        private void joinGameRequest(string[] streamData)
        {
            int roomIndex = int.Parse(streamData[1]) - 1;
            try
            {
                if (availableRooms[roomIndex].players.Count == 0)
                {
                    availableRooms[roomIndex].player1Name = streamData[2];
                    availableRooms[roomIndex].player1Color = streamData[3];
                    availableRooms[roomIndex].players.Add(users[users.Count - 1]);
                    availableRooms[roomIndex].players[0].Color = streamData[3];
                    HandleExceptionOnControls($"{streamData[2]} Joined game in room {roomIndex + 1} as player1", "listView1");
                }
                else if (availableRooms[roomIndex].players.Count == 1)
                {
                    availableRooms[roomIndex].player2Name = streamData[2];
                    availableRooms[roomIndex].players.Add(users[users.Count - 1]);
                    availableRooms[roomIndex].setPlayer2Color();
                    foreach (var item in availableRooms[roomIndex].players)
                    {
                        StreamWriter writer = new StreamWriter(item.nstream);
                        writer.WriteLine($"startGame|2");
                        writer.Flush();
                    }
                    HandleExceptionOnControls($"{streamData[2]} Joined game in room {roomIndex + 1} as player2", "listView1");
                }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void watchGameRequest(string[] streamData)
        {
            int roomIndex = int.Parse(streamData[1]) - 1;
            availableRooms[roomIndex].watchers.Add(users[users.Count - 1]);
            HandleExceptionOnControls($"{streamData[2]} strarted watch game in room {roomIndex + 1}", "listView1");
        }
        private void stopWatching(string[] streamData)
        {
            //stopWatch|roomNo|userName
            int roomNo = int.Parse(streamData[1]);
            for (int i = 0; i < availableRooms[roomNo-1].watchers.Count; i++)
            {
              if(availableRooms[roomNo - 1].watchers[i].UserName == streamData[2])
                {
                    availableRooms[roomNo - 1].watchers.RemoveAt(i);    
                }
            }
            playerId--;
            HandleExceptionOnControls($"{streamData[2]} Stopped Watching game in room {roomNo}", "listView1");
        }
        private void requestPlayersData(string[] streamData, StreamWriter writer)
        {
            try
            {
                for (int i = 0; i < availableRooms.Count; i++)
                {
                    for (int j = 0; j < availableRooms[i].players.Count; j++)
                    {
                        if (availableRooms[i].players[j].UserName == streamData[1])
                        {
                            //roomNumber|roomNo|red|playerNo|playersCount|row|col
                            writer.WriteLine($"roomNumber|{availableRooms[i].roomIndex}|{availableRooms[i].players[j].Color}|{j + 1}|{availableRooms[i].players.Count}|{availableRooms[i].row}|{availableRooms[i].col}");
                            break;
                        }
                    }
                    for (int j = 0; j < availableRooms[i].watchers.Count; j++)
                    {
                        if (availableRooms[i].watchers[j].UserName == streamData[1])
                        {
                            //roomNumber|roomNo|red|playerNo|playersCount|row|col
                            writer.WriteLine($"roomNumber|{availableRooms[i].roomIndex}|red|{j + 1}|{availableRooms[i].watchers.Count}|{availableRooms[i].row}|{availableRooms[i].col}");
                            MessageBox.Show($"roomNumber|{availableRooms[i].roomIndex}|red|{j + 1}|{availableRooms[i].watchers.Count}|{availableRooms[i].row}|{availableRooms[i].col}");
                            break;
                        }
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void sendLocation(string[] streamData, User player)
        {
            try
            {
                //"pointChanged|roomnNo|playerNo|Row|Col"
                int playerNo = int.Parse(streamData[2]);
                int roomNo = int.Parse(streamData[1]) - 1;

                foreach (var item in availableRooms[roomNo].watchers)
                {
                    StreamWriter writer = new StreamWriter(item.nstream);
                    writer.WriteLine($"ChangePoint|{streamData[3]}|{streamData[4]}|{player.Color}|0");
                    writer.Flush();
                }
                if (playerNo == 1)
                {
                    StreamWriter writer = new StreamWriter(availableRooms[roomNo].players[0].nstream);
                    writer.WriteLine($"ChangePoint|{streamData[3]}|{streamData[4]}|{player.Color}|1");
                    writer.Flush();
                }
                else
                {
                    StreamWriter writer = new StreamWriter(availableRooms[roomNo].players[1].nstream);
                    writer.WriteLine($"ChangePoint|{streamData[3]}|{streamData[4]}|{player.Color}|2");
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void sendResultToPlayers(string[] streamData)
        {
            //sendReault|WinnerNo|roomNO
            int roomNo = int.Parse(streamData[2]);
            writeInFile(roomNo, int.Parse(streamData[1]));
            if (int.Parse(streamData[1]) == 1)
            {
                StreamWriter writer = new StreamWriter(availableRooms[roomNo - 1].players[1].nstream);
                writer.WriteLine($"Lose|{roomNo}"); //Lose|roomNo
                writer.Flush();
            }
            else
            {
                StreamWriter writer = new StreamWriter(availableRooms[roomNo - 1].players[0].nstream);
                writer.WriteLine($"Lose|{roomNo}");
                writer.Flush();
            }
            HandleExceptionOnControls($"Winner in room {roomNo} is player{streamData[1]}", "listView1");
        }
        private void CancelGame(string[] streamData)
        {
            //cancel|player Who Clicked|roomNO
            int roomNo = int.Parse(streamData[2]);
            try
            {
                if (int.Parse(streamData[1]) == 1)
                {
                    StreamWriter writer = new StreamWriter(availableRooms[roomNo - 1].players[1].nstream);
                    writer.WriteLine($"GameEnded|{roomNo}|{int.Parse(streamData[1])}");
                    writer.Flush();
                    HandleExceptionOnControls($"Both players left room {roomNo}", "listView1");
                    availableRooms[roomNo - 1].players.RemoveAt(0);
                    availableRooms[roomNo - 1].players.RemoveAt(0);
                }
                else
                {
                    StreamWriter writer = new StreamWriter(availableRooms[roomNo - 1].players[0].nstream);
                    writer.WriteLine($"GameEnded|{roomNo}|{int.Parse(streamData[1])}");
                    writer.Flush();
                    HandleExceptionOnControls($"Both players left room {roomNo}", "listView1");
                    availableRooms[roomNo - 1].players.RemoveAt(0);
                    availableRooms[roomNo - 1].players.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            StartConnection();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            flag = false;
        }
        private void serverBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Close();
            flag = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace client
{
    public class Connection
    {
        TcpClient client;
        static NetworkStream nStream;
        static StreamReader Reader;
        static StreamWriter Writer;
        public Connection()
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 1025);
            nStream = client.GetStream();
            Writer = new StreamWriter(nStream);
            Writer.AutoFlush = true;
            Reader = new StreamReader(nStream);
        }
        public void sendUserName(string UserName)
        {
            Writer.WriteLine($"signIn|{UserName}");
        }
        public static void sendRoomData(string Player1color)
        {
            Writer.WriteLine($"createRoom|{start.UserName}|{CreateRoom.Row}|{CreateRoom.Col}|{Player1color}");
        }
        public static StreamReader getReader()
        {
            return Reader;
        }
        public static StreamWriter getWriter()
        {
            return Writer;
        }
        public static NetworkStream getStream()
        {
            return nStream;
        }
        public static void join(string RoomNo,string player1color="red")
        {
            Writer.WriteLine($"join|{RoomNo}|{start.UserName}|{player1color}");
        }
        public static void watch(string RoomNo)
        {
            Writer.WriteLine($"watch|{RoomNo}|{start.UserName}");
        }
        public static void stopWatch(string RoomNo)
        {
            //stopWatch|roomNo|userName
            Writer.WriteLine($"stopWatch|{RoomNo}|{start.UserName}");
        }
        public static void ClosingForm()
        {
            Writer.WriteLine($"Close|send");
            Reader.Close();
            Writer.Close();
            nStream.Close();
        }
    }
}

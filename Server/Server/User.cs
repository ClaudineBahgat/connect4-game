using System.IO;
using System.Net.Sockets;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Server
{
    public delegate void NewClientMessageHandeler(User sender,StreamWriter Writer,StreamReader Reader,string[] streamData, Socket userConnection);
    public class User
    {
        public event NewClientMessageHandeler newClientMessage;
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Color { get; set; }
        StreamReader Reader;
        StreamWriter Writer;
        Socket userConnection;
        public NetworkStream nstream;
        string[] streamData;
        public User(Socket socket)
        {
            streamData = new string[10];
            userConnection = socket;
            nstream = new NetworkStream(userConnection); ;
            Writer = new StreamWriter(nstream);
            Reader = new StreamReader(nstream);
            Writer.AutoFlush = true;
        }
        async protected virtual void ReadMessages()
        {
            while (true)
            {
                if(nstream!=null)
                {
                    string value =await Reader.ReadLineAsync();
                    streamData = value.Split('|');
                    newClientMessage(this, Writer, Reader, streamData, userConnection); //publish event
                    nstream.Flush();
                }
            }
        }
        public void publishEvent()
        {
             ReadMessages();
        }
    }
}

using System;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace client
{
    public partial class chooseColor : Form
    {
        public static string Player1color { set; get; }
        string sendAs;
        public chooseColor(string type)
        {
            InitializeComponent();
            sendAs= type;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => Application.Run(new gameBoard("player")));
            thr.Start();
            if (sendAs== "createRoom")
            {
               Connection.sendRoomData(Player1color);
               Connection.getWriter().WriteLine($"UsersData|{start.UserName}");
            }
            else
            {
               Connection.join(Roomgame.RoomNo,Player1color);
               Connection.getWriter().WriteLine($"UsersData|{start.UserName}");
            }

            this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Player1color = "red";
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Player1color = "yellow";
        }
    }
}

using System;
using System.Threading;
using System.Windows.Forms;
namespace client
{
    public partial class Roomgame : Form
    {
        public static string RoomNo { get; set; }
        string[] roomsData;
        bool flag =true;
        public Roomgame()
        {
            InitializeComponent();
            RoomNo = "1";
            /*Thread thread = new Thread(() => {
                while(flag)
                {
                    if()
                }
            });*/
        }
        private void button3_Click(object sender, EventArgs e) //CreateRoom
        {
            RoomNo = roomsData.Length.ToString();
            Thread thr = new Thread(() => Application.Run(new CreateRoom()));
            thr.Start();
        }
        private void Roomgame_Load(object sender, EventArgs e)
        {
            this.Text = $"Welcome, {start.UserName}. we wish you Enjoy the Game!";
        }
        async private void button1_Click(object sender, EventArgs e) //show available rooms
        {
            roomsData = new string[10];
            string[] room = new string[5];
            string data = await Connection.getReader().ReadLineAsync();
            roomsData = data.Split('&');
            try
            {
                for(int i=0;i<roomsData.Length-1;i++) 
                {
                    room = roomsData[i].Split('|');
                    if (listView1.InvokeRequired)
                    {
                        listView1.Invoke(new MethodInvoker(() =>
                        {
                            listView1.Items.Add(new ListViewItem($"RoomNumber: {room[0]} & numbers of players : {room[3]}"));
                        }));
                    }
                    else
                    {
                        listView1.Items.Add(new ListViewItem($"RoomNumber: {room[0]} & numbers of players : {room[3]}"));
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message);
            }
            button4.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }
        private void button4_Click(object sender, EventArgs e)//watch
        {
            RoomNo = textBox1.Text;
            if (int.Parse(listView1.Items[int.Parse(RoomNo) - 1].Text.Split(':')[2]) < 2)
            {
                MessageBox.Show("Room is Not full,you can join it or choose another room with two players");
            }
            else
            {
                Thread thrWatch = new Thread(() => Application.Run(new gameBoard("watcher")));
                thrWatch.Start();
                Connection.watch(RoomNo);
                Connection.getWriter().WriteLine($"UsersData|{start.UserName}");
            }
        }
        private void button2_Click(object sender, EventArgs e) //join
        {
            RoomNo = textBox1.Text;
            //RoomNo: 1 & numbers of players : 2
            if (int.Parse(listView1.Items[int.Parse(RoomNo) - 1].Text.Split(':')[2]) >= 2)
            {
                MessageBox.Show("Room is full,you can watch it only");
            }
            else if (int.Parse(listView1.Items[int.Parse(RoomNo) - 1].Text.Split(':')[2]) == 0)
            {
                Thread thrToChooseColor = new Thread(() => Application.Run(new chooseColor("join")));
                thrToChooseColor.Start();
            }
            else if(int.Parse(listView1.Items[int.Parse(RoomNo) - 1].Text.Split(':')[2]) == 1)
            {
                Connection.join(RoomNo);
                Thread thr = new Thread(() => Application.Run(new gameBoard("player")));
                thr.Start();
                Connection.getWriter().WriteLine($"UsersData|{start.UserName}");
            }
            else
            {
                MessageBox.Show("Invalid input");
            }
            Invalidate();
        }
        private void Roomgame_FormClosing(object sender, FormClosingEventArgs e)
        {
           Connection.ClosingForm();
        }
        private void Roomgame_MouseClick(object sender, MouseEventArgs e)
        {

        }
    }
}

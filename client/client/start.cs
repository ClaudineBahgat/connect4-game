using System;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace client
{
    public partial class start : Form
    {
        public static string UserName { set; get;}
        public start()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text!="")
            {
                UserName = textBox1.Text;
                try
                {
                    Connection obj = new Connection();
                    obj.sendUserName(UserName);
                    Thread thr = new Thread(() => Application.Run(new Roomgame()));
                    thr.Start();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("UserName is Required!");
            }   
        } 
    }
}

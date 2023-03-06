using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class CreateRoom : Form
    {
        public static int Row { get; set; }
        public static int Col { get; set; }
        public CreateRoom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => Application.Run(new chooseColor("createRoom")));
            thr.Start();
            this.Close();
            Row = 6;
            Col = 7;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => Application.Run(new chooseColor("createRoom")));
            thr.Start();
            this.Close();
            Row = 7;
            Col = 8;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => Application.Run(new chooseColor("createRoom")));
            thr.Start();
            this.Close();
            Row = 7;
            Col = 9;
        }
    }
}

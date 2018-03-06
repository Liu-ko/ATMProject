using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ATM
{
    public partial class  Form1 : Form
    {
        private Account[] ac = new Account[3];

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Create a new thred to execute the form in
            Thread atmNew = new Thread(new ThreadStart(createShow));
            atmNew.Start();
        }

        private void createShow()
        {
            // The created ATM will run preventing datarace
            ATM_Form a = new ATM_Form(ac, false);
            a.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Create a new thred to execute the form in. 
            Thread atmNew = new Thread(new ThreadStart(createShow2));
            atmNew.Start();
        }

        private void createShow2()
        {
            // The created ATM will run with datarace
            ATM_Form a = new ATM_Form(ac, true);
            a.ShowDialog();
        }

        // if the main frame is closed, close all other forms and exit the application
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}

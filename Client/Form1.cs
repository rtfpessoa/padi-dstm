using System;
using System.Windows.Forms;
using PADI_DSTM;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, System.EventArgs e)
        {

        }

        private void button7_Click(object sender, System.EventArgs e)
        {
            PadiDstm.Fail(textBox3.Text);
        }

        private void button8_Click(object sender, System.EventArgs e)
        {
            PadiDstm.Freeze(textBox3.Text);
        }

        private void button9_Click(object sender, System.EventArgs e)
        {
            PadiDstm.Recover(textBox3.Text);
        }

        private void button6_Click(object sender, System.EventArgs e)
        {
            PadiDstm.Status();
        }

        private void button3_Click_1(object sender, System.EventArgs e)
        {
            PadiDstm.TxBegin();
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            PadiDstm.TxCommit();
        }

        private void button5_Click(object sender, System.EventArgs e)
        {
            PadiDstm.TxAbort();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            var padInt = PadiDstm.AccessPadInt(Int32.Parse(textBox1.Text));

            if (padInt != null)
            {
                label3.Text = padInt.Read().ToString();
            }
            else
            {
                label3.Text = "#";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var uid = Int32.Parse(textBox1.Text);
            var padInt = PadiDstm.AccessPadInt(uid) ?? PadiDstm.CreatePadInt(uid);

            var value = Int32.Parse(textBox2.Text);
            padInt.Write(value);
        }
    }
}
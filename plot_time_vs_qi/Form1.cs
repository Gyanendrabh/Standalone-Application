using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plot_time_vs_qi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            double Ti = 0;
            double Tp = 30;
            double step = 0.01;
            int dof = 2;
            double[] qin = { 0, 0 };
            double[] qf = { Math.PI / 3, Math.PI / 2 };

            double[] time = GenerateTimeArray(Ti, Tp, step);
            double[,] qi = new double[time.Length, dof];

            for (int j = 0; j < time.Length; j++)
            {
                double tim = time[j];
                for (int i = 0; i < dof; i++)
                {
                    qi[j, i] = qin[i] + ((qf[i] - qin[i]) / Tp) * (tim - (Tp / (2 * Math.PI)) * Math.Sin((2 * Math.PI / Tp) * tim));
                }
            }

            PlotGraph(time, qi);
        }
        private double[] GenerateTimeArray(double Ti, double Tp, double step)
        {
            int length = (int)((Tp - Ti) / step) + 1;
            double[] time = new double[length];
            for (int i = 0; i < length; i++)
            {
                time[i] = Ti + i * step;
            }
            return time;
        }
        private void PlotGraph(double[] time, double[,] qi)
        {
            // Assuming you have a PictureBox control named pictureBox1 on your form
            Graphics gg = pictureBox1.CreateGraphics();
            gg.Clear(Color.White);

            Pen[] pens = { Pens.Red, Pens.Blue }; // For plotting multiple lines with different colors
            int dof = qi.GetLength(1);

            // Assuming the range of qi is known, adjust the scale as needed
            float scaleFactor = 100; // You may need to adjust this value based on the range of qi values

            for (int i = 0; i < dof; i++)
            {
                for (int j = 1; j < time.Length; j++)
                {
                    int x1 = (int)(time[j - 1] * 10); // Adjust the scale as needed
                    int y1 = (int)(qi[j - 1, i] * scaleFactor);
                    int x2 = (int)(time[j] * 10); // Adjust the scale as needed
                    int y2 = (int)(qi[j, i] * scaleFactor);
                    gg.DrawLine(pens[i], x1, y1, x2, y2);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

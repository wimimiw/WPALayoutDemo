using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFALayoutDemo
{
    public partial class Form1 : Form
    {  
        private Size beforeClientSize = Size.Empty;

        struct bakInfo
        {
            public Size s;
            public Point p;
            public Font f;
        }

        public Form1()
        {
            InitializeComponent();

            const int x_cnt = 10;
            const int y_cnt = 15;
            Button[] btn = new Button[x_cnt * y_cnt];
            
            float btnW = tabPage3.ClientSize.Width*1f / x_cnt;
            float btnH = tabPage3.ClientSize.Height*1f / y_cnt;

            for (int i = 0; i < btn.Length; i++)
			{
                btn[i] = new Button();
                btn[i].Name = "button" + i;               
                btn[i].Font = new Font(btn[i].Font.FontFamily,9);
                btn[i].BackColor = Color.Teal;
                btn[i].ForeColor = Color.White;
                btn[i].Width = (int)btnW;
                btn[i].Height = (int)btnH;
                btn[i].Location = new Point((int)(i % 10 * btnW), (int)(i / 10 * btnH));
                btn[i].Text = i.ToString();
                //btn[i].Text = btn[i].Width + "," + btn[i].Height + "," + btn[i].Location.X + "," + btn[i].Location.Y;
			}

            //this.Text = this.tabPage3.ClientSize.Width + "," + this.tabPage3.ClientSize.Height;

            tabPage3.Controls.AddRange(btn);

            this.StartPosition = FormStartPosition.CenterScreen;

            foreach (Control item in this.Controls)
            {
                AutoScaleInit(item);
            }                        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bakInfo bi = new bakInfo();
            bi.p = this.Location;
            bi.s = this.ClientSize;
            beforeClientSize = this.ClientSize;
            this.Tag = bi; //this.Size是整个Form的大小，这里要特别注意，否则比例会不对         
            this.Resize += Form1_Resize;

            this.dataGridView1.DataSource = this.bindingSource1;
            this.bindingSource1.DataSource = GetTable();
        }

        void Form1_Resize(object sender, EventArgs e)
        {
            //if (this.ClientSize.Width < beforeClientSize.Width || this.ClientSize.Height < beforeClientSize.Height) return;

            foreach (Control item in this.Controls)
            {
                AutoScaleHandle(item);
            }

            //this.Text = this.Width + "," + this.Height;
            //this.Text = this.tabPage3.ClientSize.Width + "," + this.tabPage3.ClientSize.Height;
            this.Update();
            //throw new NotImplementedException();
        }

        void AutoScaleInit(Control control)
        {
            bakInfo bi = new bakInfo();
            bi.p = control.Location;
            bi.s = control.Size;
            bi.f = control.Font;
            control.Tag = bi;
            //if (control.GetType() == typeof(DataGridView))
            //if (control.Name.Equals("dataGridView1"))
            //    control.Anchor = AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right | AnchorStyles.Bottom;

            if (control.HasChildren)
            { 
                foreach (Control ctl in control.Controls)
                {
                    AutoScaleInit(ctl);
                }
            }            
        }

        void AutoScaleHandle(Control control)
        {            
                bakInfo beginResizeSize = (bakInfo)control.Parent.Tag;
                Size endResizeSize = control.Parent.ClientSize;               
                float ScaleLen = (float)endResizeSize.Width / beginResizeSize.s.Width;
                float ScaleLocal = (float)endResizeSize.Height / beginResizeSize.s.Height;
                control.Width = (int)Math.Round(((bakInfo)(control.Tag)).s.Width * ScaleLen);
                control.Height = (int)Math.Round(((bakInfo)(control.Tag)).s.Height * ScaleLocal);
                control.Left = (int)Math.Round(((bakInfo)(control.Tag)).p.X * ScaleLen);
                control.Top = (int)Math.Round(((bakInfo)(control.Tag)).p.Y * ScaleLocal);
                control.Font = new Font(control.Font.FontFamily, ((bakInfo)(control.Tag)).f.Size * Math.Min(ScaleLocal, ScaleLen));
                        
            //if (control.GetType() == typeof(Button))
            //    control.Text = control.Width + "," + control.Height + "," + control.Left + "," + control.Top;

            if (control.HasChildren)
            {
                foreach (Control ctl in control.Controls)
                {
                    AutoScaleHandle(ctl);
                }                
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                //最大化时所需的操作 
                //OnResizeBegin(null);
                Form1_Resize(null,null);
                //MessageBox.Show("max");
            }
            
            base.OnResize(e);
        }

        DataTable GetTable()
        {
            // Here we create a DataTable with four columns.
            DataTable table = dt;
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Dosage2", typeof(int));
            table.Columns.Add("Drug2", typeof(string));
            table.Columns.Add("Patient2", typeof(string));
            table.Columns.Add("Date2", typeof(DateTime));

            // Here we add five DataRows.
            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            table.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            table.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            table.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            table.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);
            return table;
        }

        DataSet ds = new DataSet();
        DataTable dt = new DataTable();

        private void button1_Click(object sender, EventArgs e)
        {
            dt.Rows.Add(25, "Mashuai", "David", DateTime.Now);
            //this.dataGridView1.Height += this.dataGridView1.Rows[0].Height;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][0] = i;
                dt.Rows[i][3] = DateTime.Now;
                if (i % 2 == 0)
                {
                    dataGridView1.Rows[i].Cells[3].Style.ForeColor = Color.Red;
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.BlueViolet;
                }
            }
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1_Resize(null, null);
        }        
    }
}

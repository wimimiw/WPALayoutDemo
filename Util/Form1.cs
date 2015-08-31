using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WindowsFormsApplication1
{
    [Serializable]
    public partial class Form1 : Form
    {
        // Put the next line into the Declarations section.
        private System.Data.DataSet dataSet = new DataSet();
        int idx = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void MakeParentTable()
        {
            // Create a new DataTable.
            System.Data.DataTable table = new DataTable("ParentTable");
            // Declare variables for DataColumn and DataRow objects.
            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "id";
            column.ReadOnly = true;
            column.Unique = true;
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "ParentItem";
            column.AutoIncrement = false;
            column.Caption = "ParentItem";
            column.ReadOnly = false;
            column.Unique = false;
            // Add the column to the table.
            table.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "TT";
            column.AutoIncrement = false;
            column.Caption = "ABC";
            column.ReadOnly = false;
            column.Unique = false;
            // Add the column to the table.
            table.Columns.Add(column);

            // Make the ID column the primary key column.
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = table.Columns["id"];
            table.PrimaryKey = PrimaryKeyColumns;

            // Instantiate the DataSet variable.
            dataSet = new DataSet();
            // Add the new DataTable to the DataSet.
            dataSet.Tables.Add(table);

            // Create three new DataRow objects and add 
            // them to the DataTable
            for (int i = 0; i <= 2; i++)
            {
                row = table.NewRow();
                row["id"] = i;
                row["ParentItem"] = "ParentItem " + i;
                table.Rows.Add(row);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {           
            MakeParentTable();
            Serialize(dataSet,"form");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object obj = new object();
            DeSerialize(ref obj, "form");

            dataSet = obj as DataSet;           

            this.dataGridView1.DataSource = dataSet.Tables[0];
        }

        static void Serialize(object obj,string name)
        {
            FileStream fs = new FileStream(Application.StartupPath + @"\" + name + ".xml", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);
            fs.Close();
        }

        static void DeSerialize(ref object obj,string name)
        {
            FileStream fs = new FileStream(Application.StartupPath + @"\" + name + ".xml", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs);
            fs.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataSet.Tables[0].Rows[0][2] = idx++;
            this.dataGridView1.Rows[0].Cells[2].Style.ForeColor = Color.White;
            this.dataGridView1.Rows[0].Cells[2].Style.BackColor = Color.Black;
            //this.dataGridView1.Rows.Clear();
            //dataSet.Tables[0].Columns["ParentItem"][0] = idx++;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Control ctl = this.dataGridView1;

            Control ctl = Control.FromHandle(this.dataGridView1.Handle);            

            {             
                Bitmap bt = new Bitmap(ctl.Width, ctl.Height);
                ctl.DrawToBitmap(bt, new System.Drawing.Rectangle(0, 0, bt.Width, bt.Height));
                bt.Save("abc.gif",System.Drawing.Imaging.ImageFormat.Gif);

                Document document = new Document(PageSize.A4, 10,10, 10,10);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(Application.StartupPath + @"\abcd.pdf", FileMode.Create));
                writer.ViewerPreferences = (PdfWriter.CenterWindow | PdfWriter.FitWindow | PdfWriter.PageModeUseNone);
                document.Open();

                //使用宋体字体
                BaseFont baseFont = BaseFont.CreateFont("C:\\WINDOWS\\FONTS\\simsun.ttc,0",
                                                        BaseFont.IDENTITY_H,
                                                        BaseFont.NOT_EMBEDDED);

                PdfContentByte cb = writer.DirectContent;

                Chapter chapter1 = new Chapter(new Paragraph("This is Chapter 1"), 1);
                Section section1 = chapter1.AddSection(20f, "Section 1.1", 2);
                Section section2 = chapter1.AddSection(20f, "Section 1.2", 2);
                Section subsection1 = section2.AddSection(20f, "Subsection 1.2.1", 3);
                Section subsection2 = section2.AddSection(20f, "Subsection 1.2.2", 3);
                Section subsubsection = subsection2.AddSection(20f, "Sub Subsection 1.2.2.1", 4);
                Chapter chapter2 = new Chapter(new Paragraph("This is Chapter 2"), 1);
                Section section3 = chapter2.AddSection("Section 2.1", 2);
                Section subsection3 = section3.AddSection("Subsection 2.1.1", 3);
                Section section4 = chapter2.AddSection("Section 2.2", 2);
                chapter1.BookmarkTitle = "Changed Title";
                chapter1.BookmarkOpen = true;
                chapter2.BookmarkOpen = false;
                document.Add(chapter1);
                document.Add(chapter2);

                ZapfDingbatsList zlist = new ZapfDingbatsList(49, 15);
                zlist.Add("One");
                zlist.Add("Two");
                zlist.Add("Three");
                zlist.Add("Four");
                zlist.Add("Five");
                document.Add(zlist);

                RomanList romanlist = new RomanList(true, 20);
                romanlist.IndentationLeft = 30f;
                romanlist.Add("One");
                romanlist.Add("Two");
                romanlist.Add("Three");
                romanlist.Add("Four");
                romanlist.Add("Five");
                document.Add(romanlist);

                PdfPTable table1 = new PdfPTable(3);                
                PdfPCell cell1 = new PdfPCell(new Phrase("Header spanning 3 columns"));                
                cell1.Colspan = 3;                
                cell1.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table1.AddCell(cell1);
                table1.AddCell(cell1);
                table1.AddCell("Col 1 Row 2");
                table1.AddCell("Col 2 Row 2");
                table1.AddCell("Col 3 Row 2");

                table1.SetWidths(new int[]{50,100,100});

                table1.WidthPercentage = 100f;                
                document.Add(table1);

                //using it = iTextSharp.text;

                //PdfPTable table2 = new PdfPTable(3);
                //table2.AddCell("Cell 1");
                //PdfPCell cell2 = new PdfPCell(new it.Phrase("Cell 2", new Font(Font.HELVETICA, 8f, Font.NORMAL, Color.YELLOW)));
                //cell2.BackgroundColor = new Color(0, 150, 0);
                //cell2.BorderColor = new Color(255, 242, 0);
                //cell2.Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER;
                //cell2.BorderWidthBottom = 3f;
                //cell2.BorderWidthTop = 3f;
                //cell2.PaddingBottom = 10f;
                //cell2.PaddingLeft = 20f;
                //cell2.PaddingTop = 4f;
                //table2.AddCell(cell2);
                //table2.AddCell("Cell 3");
                //document.Add(table2);


                System.Drawing.Image img = bt;
                MemoryStream mem = new MemoryStream();
                img.Save(mem, System.Drawing.Imaging.ImageFormat.Gif);
                byte[] bytes = mem.ToArray();

                iTextSharp.text.Image img2 = iTextSharp.text.Image.GetInstance(bytes);
                iTextSharp.text.Image img3 = iTextSharp.text.Image.GetInstance(bytes);
                img2.ScalePercent(100f);
                img3.ScalePercent(100f);
                img2.SetAbsolutePosition(50f, 400f);
                img3.SetAbsolutePosition(50f, 400f - img2.Height);                
                //cb.AddImage(img2);
                //cb.AddImage(img3);

                cb.BeginText();  

                float Xleading = 27.5f;
                float Xdelta = 10f;
                float Yleading = 27.5f;
                float Ydelta = 20f;

                cb.SetLineWidth(4f);
                cb.MoveTo(Xleading, (842 - Yleading - Ydelta));
                cb.LineTo((595f - Xleading), (842f - Yleading - Ydelta));
                cb.Stroke();

                cb.EndText();

                document.NewPage();

                //绘制近下方细直线上的文字
                cb.BeginText();

                BaseFont fbaseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font font = new iTextSharp.text.Font(fbaseFont);

                cb.SetFontAndSize(fbaseFont,15);
                cb.SetColorFill(BaseColor.LIGHT_GRAY);

                PdfPCell cell = new PdfPCell();
                PdfPTable table = new PdfPTable(5);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;

                table.AddCell(cell);
                document.Add(table);


                document.AddAuthor("ms");
                document.AddCreationDate();
                document.AddTitle("TEST");
                //document.Add(new Paragraph("", font));
                //document.Add(new Paragraph("    你好， PDF !", font));
                //document.Add(new Paragraph("    你好， PDF !", font));
                //document.Add(new Paragraph("    你好， PDF !", font));


                cb.ShowTextAligned(Element.ALIGN_LEFT, "一二三   :", 50, 800f, 0);
                cb.ShowTextAligned(Element.ALIGN_LEFT, "一二三   :", 50, 770f, 0);

                cb.EndText();

                cb.AddImage(img2); 

                document.Close();
            }        
    
            Control.ControlCollection ctls = this.Controls;

            foreach (Control item in ctls)
            {
                item.Enabled = false;
                //item.Width += 100;
            }
        }
    }
}

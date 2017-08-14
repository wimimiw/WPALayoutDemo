/// <summary>
/// 版本: 1.0.0.1
/// <summary>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PIM_ATE
{
    public class CurveChart : UserControl
    {
        Curve2D cuv2D = new Curve2D();
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!base.Visible)
            {
                return;
            }
            CopyImage(e.Graphics, e.ClipRectangle, e.ClipRectangle);
        }

        public CurveChart()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResumeLayout(false);

            cuv2D.XSliceBegin = 930;
            cuv2D.XSliceEnd = 960;
            cuv2D.XSliceValue = 3;

            cuv2D.YSliceBegin = -150;
            cuv2D.YSliceEnd = -50;
            cuv2D.YSliceValue = 20;
        }

        internal void CopyImage(Graphics g, Rectangle des, Rectangle res)
        {
            cuv2D.Width = base.Width;
            cuv2D.Height = base.Height;
            g.DrawImage(cuv2D.CreateImage(), des, res, GraphicsUnit.Pixel);
        }

        public void SetPlotTitle(string Title, string xTitle, string yTitle)
        {
            cuv2D.XAxisText = xTitle;
            cuv2D.YAxisText = yTitle;
            cuv2D.Title = Title;
            this.Invalidate();
        }

        /// <summary>
        /// 设置X轴起始结束
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="s"></param>
        public void SetPlotX(float begin, float end, float s = 0)
        {
            cuv2D.XSliceBegin = begin;
            cuv2D.XSliceEnd = end;
            if (s <= 0)
            {
                cuv2D.XSliceValue = (end - begin) / 10f;
            }
            else
            {
                cuv2D.XSliceValue = s;
            }
            this.Invalidate();
        }

        /// <summary>
        /// 设置Y轴起始结束
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="s"></param>
        public void SetPlotY(float begin, float end, float s = 0)
        {
            cuv2D.YSliceBegin = begin;
            cuv2D.YSliceEnd = end;
            if (s <= 0)
            {
                cuv2D.YSliceValue = (end - begin) / 5f;
            }
            else
            {
                cuv2D.YSliceValue = s;
            }
            
            this.Invalidate();
        }

        /// <summary>
        /// 清除所有点
        /// </summary>
        public void Clear()
        {
            cuv2D.Channel0.Clear();
            cuv2D.Channel1.Clear();
            this.Invalidate();
        }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="p"></param>
        /// <param name="channel"></param>
        public void Add(PointF p, int channel)
        {
            if (channel == 0)
                cuv2D.Channel0.Add(p);
            else if (channel == 1)
                cuv2D.Channel1.Add(p);
            this.Invalidate();
        }

        //public Curve2D Curve2D
        //{
        //    get { return cuv2D; }
        //}

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }

    public class Curve2D
    {
        //Graphics 类提供将对象绘制到显示设备的方法
        private Graphics m_objGraphics;
        //位图对象
        private Bitmap m_objBitmap;
        private List<PointF> fltsChannel0 = new List<PointF>();
        private List<PointF> fltsChannel1 = new List<PointF>();

        //图像宽度
        private float fltWidth = 480;
        //图像高度
        private float fltHeight = 248;

        //X轴刻度宽度
        private float fltXSlice = 50;
        //X轴刻度的数值宽度
        private float fltXSliceValue = 1;
        //X轴刻度开始值
        private float fltXSliceBegin = 0; 
        private float fltXSliceEnd = 10;

        //Y轴刻度宽度
        private float fltYSlice = 50;
        //Y轴刻度的数值宽度
        private float fltYSliceValue = 20;
        //Y轴刻度开始值
        private float fltYSliceBegin = 0; 
        private float fltYSliceEnd = 100; 

        private float fltTension = 0.5f;
        //标题
        private string strTitle = "扫频模式";
        //X轴说明文字
        private string strXAxisText = "MHz";
        //Y轴说明文字
        private string strYAxisText = "dBm";
        //背景色
        private Color clrBgColor = Color.Snow;
        //文字颜色
        private Color clrTextColor = Color.Black;
        //整体边框颜色
        private Color clrBorderColor = Color.Black;
        //轴线颜色
        private Color clrAxisColor = Color.Black;
        //轴说明文字颜色
        private Color clrAxisTextColor = Color.Black;
        //刻度文字颜色
        private Color clrSliceTextColor = Color.Black;
        //刻度颜色
        private Color clrSliceColor = Color.Black;
        //标记点颜色
        private Color markColor = Color.SteelBlue;
        //曲线颜色
        private Color[] clrsCurveColors = new Color[] { Color.Red, Color.Blue };
        //图像左右距离边缘距离
        private float fltXSpace = 100f;
        //图像上下距离边缘距离
        private float fltYSpace = 100f;
        //字体大小号数
        private int intFontSize = 9;
        //X轴文字旋转角度
        private float fltXRotateAngle = 30f;
        //Y轴文字旋转角度
        private float fltYRotateAngle = 0f;
        //曲线线条大小
        private int intCurveSize = 2;
        //intFontSpace 是字体大小和距离调整出来的一个比较适合的数字
        private int intFontSpace = 0; 

        #region 公共属性

        /// <summary>
        /// 图像的宽度
        /// </summary>
        public float Width
        {
            set
            {
                if (value < 100)
                {
                    fltWidth = 100;
                }
                else
                {
                    fltWidth = value;
                }
            }
            get
            {
                if (fltWidth <= 100)
                {
                    return 100;
                }
                else
                {
                    return fltWidth;
                }
            }
        }

        /// <summary>
        /// 图像的高度
        /// </summary>
        public float Height
        {
            set
            {
                if (value < 100)
                {
                    fltHeight = 100;
                }
                else
                {
                    fltHeight = value;
                }
            }
            get
            {
                if (fltHeight <= 100)
                {
                    return 100;
                }
                else
                {
                    return fltHeight;
                }
            }
        }

        /// <summary>
        /// X轴刻度宽度
        /// </summary>
        public float XSlice
        {
            set { fltXSlice = value; }
            get { return fltXSlice; }
        }

        /// <summary>
        /// X轴刻度的数值宽度
        /// </summary>
        public float XSliceValue
        {
            set { fltXSliceValue = value; }
            get { return fltXSliceValue; }
        }

        /// <summary>
        /// X轴刻度开始值
        /// </summary>
        public float XSliceBegin
        {
            set { fltXSliceBegin = value; }
            get { return fltXSliceBegin; }
        }

        /// <summary>
        /// Y轴刻度结束值
        /// </summary>
        public float XSliceEnd
        {
            set { fltXSliceEnd = value; }
            get { return fltXSliceEnd; }
        }

        /// <summary>
        /// Y轴刻度宽度
        /// </summary>
        public float YSlice
        {
            set { fltYSlice = value; }
            get { return fltYSlice; }
        }

        /// <summary>
        /// Y轴刻度的数值宽度
        /// </summary>
        public float YSliceValue
        {
            set { fltYSliceValue = value; }
            get { return fltYSliceValue; }
        }

        /// <summary>
        /// Y轴刻度开始值
        /// </summary>
        public float YSliceBegin
        {
            set { fltYSliceBegin = value; }
            get { return fltYSliceBegin; }
        }

        /// <summary>
        /// Y轴刻度结束值
        /// </summary>
        public float YSliceEnd
        {
            set { fltYSliceEnd = value; }
            get { return fltYSliceEnd; }
        }

        /// <summary>
        /// 张力系数
        /// </summary>
        public float Tension
        {
            set
            {
                if (value < 0.0f && value > 1.0f)
                {
                    fltTension = 0.5f;
                }
                else
                {
                    fltTension = value;
                }
            }
            get
            {
                return fltTension;
            }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            set { strTitle = value; }
            get { return strTitle; }
        }

        /// <summary>
        /// 值，线1的Y轴数据
        /// </summary>
        public List<PointF> Channel0
        {
            set { fltsChannel0 = value; }
            get { return fltsChannel0; }
        }

        /// <summary>
        /// 值，线2的Y轴数据
        /// </summary>
        public List<PointF> Channel1
        {
            set { fltsChannel1 = value; }
            get { return fltsChannel1; }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public Color BgColor
        {
            set { clrBgColor = value; }
            get { return clrBgColor; }
        }

        /// <summary>
        /// 文字颜色
        /// </summary>
        public Color TextColor
        {
            set { clrTextColor = value; }
            get { return clrTextColor; }
        }

        /// <summary>
        /// 整体边框颜色
        /// </summary>
        public Color BorderColor
        {
            set { clrBorderColor = value; }
            get { return clrBorderColor; }
        }

        /// <summary>
        /// 轴线颜色
        /// </summary>
        public Color AxisColor
        {
            set { clrAxisColor = value; }
            get { return clrAxisColor; }
        }

        /// <summary>
        /// X轴说明文字
        /// </summary>
        public string XAxisText
        {
            set { strXAxisText = value; }
            get { return strXAxisText; }
        }

        /// <summary>
        /// Y轴说明文字
        /// </summary>
        public string YAxisText
        {
            set { strYAxisText = value; }
            get { return strYAxisText; }
        }

        /// <summary>
        /// 轴说明文字颜色
        /// </summary>
        public Color AxisTextColor
        {
            set { clrAxisTextColor = value; }
            get { return clrAxisTextColor; }
        }

        /// <summary>
        /// 刻度文字颜色
        /// </summary>
        public Color SliceTextColor
        {
            set { clrSliceTextColor = value; }
            get { return clrSliceTextColor; }
        }

        /// <summary>
        /// 刻度颜色
        /// </summary>
        public Color SliceColor
        {
            set { clrSliceColor = value; }
            get { return clrSliceColor; }
        }

        public Color MarkColor
        {
            set { markColor = value;  }
            get { return markColor; }
        }

        /// <summary>
        /// 曲线颜色
        /// </summary>
        public Color[] CurveColors
        {
            set { clrsCurveColors = value; }
            get { return clrsCurveColors; }
        }

        /// <summary>
        /// X轴文字旋转角度
        /// </summary>
        public float XRotateAngle
        {
            get { return fltXRotateAngle; }
            set { fltXRotateAngle = value; }
        }

        /// <summary>
        /// Y轴文字旋转角度
        /// </summary>
        public float YRotateAngle
        {
            get { return fltYRotateAngle; }
            set { fltYRotateAngle = value; }
        }

        /// <summary>
        /// 图像左右距离边缘距离
        /// </summary>
        public float XSpace
        {
            get { return fltXSpace; }
            set { fltXSpace = value; }
        }

        /// <summary>
        /// 图像上下距离边缘距离
        /// </summary>
        public float YSpace
        {
            get { return fltYSpace; }
            set { fltYSpace = value; }
        }

        /// <summary>
        /// 字体大小号数
        /// </summary>
        public int FontSize
        {
            get { return intFontSize; }
            set { intFontSize = value; }
        }

        /// <summary>
        /// 曲线线条大小
        /// </summary>
        public int CurveSize
        {
            get { return intCurveSize; }
            set { intCurveSize = value; }
        }

        #endregion

        public Bitmap CreateImage()
        {// 生成图像并返回bmp图像对象
            InitializeGraph();
            if (fltsChannel0 != null)
                DrawContent(ref m_objGraphics, fltsChannel0.ToArray(), Color.Red);
            if (fltsChannel1 != null)
                DrawContent(ref m_objGraphics, fltsChannel1.ToArray(), Color.Blue);

            PointF maxPoint = new PointF();
            float max = float.MinValue;
            foreach (var p in fltsChannel0)
            {
                if (p.Y > max)
                {
                    max = p.Y;
                    maxPoint.X = p.X;
                    maxPoint.Y = p.Y;
                }
            }
            foreach (var p in fltsChannel1)
            {
                if (p.Y > max)
                {
                    max = p.Y;
                    maxPoint.X = p.X;
                    maxPoint.Y = p.Y;
                }
            }
            DrawMax(ref m_objGraphics, maxPoint, markColor);

            return m_objBitmap;
        }

        private void InitializeGraph()
        {//初始化

            //计算字体距离
            intFontSpace = FontSize + 5;
            //计算图像边距
            float fltSpace = Math.Min(Width / 6, Height / 6);
            XSpace = fltSpace;
            YSpace = fltSpace;

            //根据给定的高度和宽度创建一个位图图像
            m_objBitmap = new Bitmap((int)Width, (int)Height);

            //从指定的 objBitmap 对象创建 objGraphics 对象 (即在objBitmap对象中画图)
            m_objGraphics = Graphics.FromImage(m_objBitmap);

            //根据给定颜色(LightGray)填充图像的矩形区域 (背景)
            m_objGraphics.DrawRectangle(new Pen(BorderColor, 1), 0, 0, Width - 1, Height - 1); //画边框
            m_objGraphics.FillRectangle(new SolidBrush(BgColor), 1, 1, Width - 2, Height - 2); //填充边框

            //画X轴,注意图像的原始X轴和Y轴计算是以左上角为原点，向右和向下计算的
            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = Width - XSpace + XSlice / 2;
            float fltY2 = fltY1;
            m_objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), fltX1, fltY1, fltX2, fltY2);

            //画Y轴
            fltX1 = XSpace;
            fltY1 = Height - YSpace;
            fltX2 = XSpace;
            fltY2 = YSpace - YSlice / 2;
            m_objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor), 1), fltX1, fltY1, fltX2, fltY2);

            //初始化轴线说明文字
            SetAxisText(ref m_objGraphics);

            //初始化X轴上的刻度和文字
            SetXAxis(ref m_objGraphics);

            //初始化Y轴上的刻度和文字
            SetYAxis(ref m_objGraphics);

            //初始化标题
            CreateTitle(ref m_objGraphics);
        }

        private void SetAxisText(ref Graphics objGraphics)
        {//初始化轴线说明文字
            float fltX = Width - XSpace + XSlice / 2 - (XAxisText.Length - 1) * intFontSpace;
            float fltY = Height - YSpace - intFontSpace;
            objGraphics.DrawString(XAxisText, new Font("宋体", FontSize), new SolidBrush(AxisTextColor), fltX, fltY);

            fltX = XSpace + 5;
            fltY = YSpace - YSlice / 2 - intFontSpace;
            for (int i = 0; i < YAxisText.Length; i++)
            {
                objGraphics.DrawString(YAxisText[i].ToString(), new Font("宋体", FontSize), new SolidBrush(AxisTextColor), fltX, fltY);
                //fltY += intFontSpace; //字体上下距离
                fltY += intFontSize;
            }
        }

        private void SetXAxis(ref Graphics objGraphics)
        {// 初始化X轴上的刻度和文字

            //计算X轴刻度宽度
            //XSlice = (Width - 2 * XSpace) / (Keys.Length - 1);
            int intXSliceCount = (int)((XSliceEnd - XSliceBegin) / XSliceValue);
            if (XSliceEnd % XSliceValue != 0)
            {
                intXSliceCount++;
            }
            XSlice = (Width - 2 * XSpace) / intXSliceCount;

            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = XSpace;
            float fltY2 = Height - YSpace;
            int iCount = 0;
            int iSliceCount = 1;
            float Scale = 0;
            float iWidth = ((Width - 2 * XSpace) / XSlice) * 50; //将要画刻度的长度分段，并乘以50，以10为单位画刻度线。
            float fltSliceHeight = XSlice / 20; //刻度线的高度

            objGraphics.TranslateTransform(fltX1, fltY1); //平移图像(原点)
            objGraphics.RotateTransform(XRotateAngle, MatrixOrder.Prepend); //旋转图像
            objGraphics.DrawString(XSliceBegin/*Keys[0]*/.ToString(), new Font("宋体", FontSize), new SolidBrush(SliceTextColor), 0, 10);
            objGraphics.ResetTransform(); //重置图像

            for (int i = 0; i <= iWidth; i += 10) //以10为单位
            {
                Scale = i * XSlice / 50;//即(i / 10) * (XSlice / 5)，将每个刻度分五部分画，但因为i以10为单位，得除以10

                if (iCount == 5)
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor)), fltX1 + Scale, fltY1 + fltSliceHeight * 1.5f, fltX2 + Scale, fltY2 - fltSliceHeight * 1.5f);
                    //画网格虚线
                    Pen penDashed = new Pen(new SolidBrush(AxisColor));
                    penDashed.DashStyle = DashStyle.Dash;
                    objGraphics.DrawLine(penDashed, fltX1 + Scale, fltY1, fltX2 + Scale, YSpace - YSlice / 2);
                    //这里显示X轴刻度
                    if (iSliceCount <= intXSliceCount/*Keys.Length - 1*/)
                    {
                        string strXSlice = (XSliceBegin + iSliceCount * XSliceValue).ToString();
                        objGraphics.TranslateTransform(fltX1 + Scale, fltY1);
                        objGraphics.RotateTransform(XRotateAngle, MatrixOrder.Prepend);
                        objGraphics.DrawString(strXSlice/*Keys[iSliceCount].ToString()*/, new Font("宋体", FontSize), new SolidBrush(SliceTextColor), 0, 10);
                        objGraphics.ResetTransform();
                    }
                    else
                    {
                        //超过范围，不画任何刻度文字
                    }
                    iCount = 0;
                    iSliceCount++;
                    if (fltX1 + Scale > Width - XSpace)
                    {
                        break;
                    }
                }
                else
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(SliceColor)), 
                                         fltX1 + Scale, fltY1 + fltSliceHeight, 
                                         fltX2 + Scale, fltY2 - fltSliceHeight);
                }
                iCount++;
            }
        }

        private void SetYAxis(ref Graphics objGraphics)
        {// 初始化Y轴上的刻度和文字
            int intYSliceCount = (int)((YSliceEnd - YSliceBegin) / YSliceValue);
            if (YSliceEnd % YSliceValue != 0)
            {
                intYSliceCount++;
            }
            YSlice = (Height - 2 * YSpace) / intYSliceCount;

            float fltX1 = XSpace;
            float fltY1 = Height - YSpace;
            float fltX2 = XSpace;
            float fltY2 = Height - YSpace;
            int iCount = 0;
            float Scale = 0;
            int iSliceCount = 1;
            float iHeight = ((Height - 2 * YSpace) / YSlice) * 50; //将要画刻度的长度分段，并乘以50，以10为单位画刻度线。
            float fltSliceWidth = YSlice / 10; //刻度线的宽度
            string strSliceText = string.Empty;

            //objGraphics.TranslateTransform(XSpace - intFontSpace * YSliceBegin.ToString().Length, Height - YSpace); //平移图像(原点)
            objGraphics.TranslateTransform(XSpace - intFontSpace * 2.5f, Height - YSpace - intFontSpace / 2); //平移图像(原点)

            objGraphics.RotateTransform(YRotateAngle, MatrixOrder.Prepend); //旋转图像
            objGraphics.DrawString(YSliceBegin.ToString(), new Font("宋体", FontSize), new SolidBrush(SliceTextColor), 0, 0);
            objGraphics.ResetTransform(); //重置图像

            for (int i = 0; i < iHeight; i += 10)
            {
                Scale = i * YSlice / 50; //即(i / 10) * (YSlice / 5)，将每个刻度分五部分画，但因为i以10为单位，得除以10

                if (iCount == 5)
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(AxisColor)), 
                                         fltX1 - fltSliceWidth * 1.5f, fltY1 - Scale, 
                                         fltX2 + fltSliceWidth * 1.5f, fltY2 - Scale);
                    //画网格虚线
                    Pen penDashed = new Pen(new SolidBrush(AxisColor));
                    penDashed.DashStyle = DashStyle.Dash;
                    objGraphics.DrawLine(penDashed, XSpace, fltY1 - Scale, Width - XSpace + XSlice / 2, fltY2 - Scale);
                    //这里显示Y轴刻度
                    strSliceText = Convert.ToString(YSliceValue * iSliceCount + YSliceBegin);

                    //objGraphics.TranslateTransform(XSpace - intFontSize * strSliceText.Length, fltY1 - Scale); //平移图像(原点)
                    objGraphics.TranslateTransform(XSpace - intFontSpace * 2.5f, fltY1 - Scale - intFontSpace / 2); //平移图像(原点)

                    objGraphics.RotateTransform(YRotateAngle, MatrixOrder.Prepend); //旋转图像
                    objGraphics.DrawString(strSliceText, new Font("宋体", FontSize), new SolidBrush(SliceTextColor), 0, 0);
                    objGraphics.ResetTransform(); //重置图像

                    iCount = 0;
                    iSliceCount++;
                }
                else
                {
                    objGraphics.DrawLine(new Pen(new SolidBrush(SliceColor)), 
                                         fltX1 - fltSliceWidth, fltY1 - Scale, 
                                         fltX2 + fltSliceWidth, fltY2 - Scale);
                }
                iCount++;
            }
        }

        private void DrawContent(ref Graphics objGraphics, PointF[] fltCurrentValues, Color clrCurrentColor)
        {// 画曲线
            if (fltCurrentValues == null || fltCurrentValues.Length == 0) return;
            int r = 4;
            PointF[] CurvePointF = new PointF[fltCurrentValues.Length];
            for (int i = 0; i < fltCurrentValues.Length; i++)
            {
                float values = Height - YSpace - (fltCurrentValues[i].Y - fltYSliceBegin) / fltYSliceValue * YSlice;
                float keys = XSpace + (fltCurrentValues[i].X - fltXSliceBegin) / fltXSliceValue * XSlice;
                CurvePointF[i] = new PointF(keys, values);
                objGraphics.FillEllipse(new SolidBrush(clrCurrentColor), keys - r, values - r, r * 2, r * 2);
            }

            if (CurvePointF.Length > 1)
            {
                objGraphics.DrawLines(new Pen(new SolidBrush(clrCurrentColor), 2), CurvePointF);
                //objGraphics.DrawCurve(new Pen(clrCurrentColor, CurveSize), CurvePointF, Tension);
            }
        }

        /// <summary>
        /// 绘制倒三角
        /// </summary>
        /// <param name="objGraphics"></param>
        /// <param name="paddingColor">填充颜色</param>
        /// <param name="centerPoint">形状中心位置</param>
        private void DrawInverseTrangle(Graphics objGraphics, Color paddingColor, PointF centerPoint)
        {
            //放大百分比
            float percent = 1.2f;
            //XY延伸比值 [1.0 = 正倒三角 | > 1.0 = 扁的倒三角 | < 1.0 瘦的倒三角]
            float xy_ratio = 0.7f;
            //XY基础延伸长度
            float xy_base_extend = 7;
            //倒三角中心点向上或下延伸距离
            float y_extend = (1 / xy_ratio) * xy_base_extend * percent;
            //倒三角中心点向左或右延伸距离
            float x_extend = xy_ratio * xy_base_extend * percent;
            //倒三角底部点与对应点的边距
            float y_bottom_distance = -6;
            //倒三角Y轴整体偏移量
            float y_offset = -y_extend + y_bottom_distance;
            PointF bottom = new PointF(centerPoint.X, centerPoint.Y + y_extend + y_offset);
            PointF left = new PointF(centerPoint.X - x_extend, centerPoint.Y + y_offset);
            PointF right = new PointF(centerPoint.X + x_extend, centerPoint.Y + y_offset);
            GraphicsPath path = new GraphicsPath(new PointF[] { left, bottom, right },
                                new byte[] { (byte)(PathPointType.Start),
                                             (byte)(PathPointType.Line),
                                             (byte)(PathPointType.Line | PathPointType.CloseSubpath)});//终结点放在右点上才能形成闭环路径

            //objGraphics.DrawPath(new Pen(paddingColor), path); //只有外框
            objGraphics.FillPath(new SolidBrush(paddingColor), path); //实心
        }


        /// <summary>
        /// 绘制菱形
        /// </summary>
        /// <param name="objGraphics"></param>
        /// <param name="paddingColor">填充颜色</param>
        /// <param name="centerPoint">形状中心位置</param>
        private void DrawRhombus(Graphics objGraphics, Color paddingColor, PointF centerPoint)
        {
            //放大百分比
            float percent = 1.2f;
            //XY延伸比值 [1.0 = 正菱形 | > 1.0 = 扁的菱形 | < 1.0 瘦的菱形]
            float xy_ratio = 0.7f;
            //XY基础延伸长度
            float xy_base_extend = 7;
            //菱形中心点向上或下延伸距离
            float y_extend = (1 / xy_ratio) * xy_base_extend * percent;
            //菱形中心点向左或右延伸距离
            float x_extend = xy_ratio * xy_base_extend * percent;
            //菱形底部点与对应点的边距
            float y_bottom_distance = -6;
            //菱形Y轴整体偏移量
            float y_offset = -y_extend + y_bottom_distance;
            PointF top = new PointF(centerPoint.X, centerPoint.Y - y_extend + y_offset);
            PointF bottom = new PointF(centerPoint.X, centerPoint.Y + y_extend + y_offset);
            PointF left = new PointF(centerPoint.X - x_extend, centerPoint.Y+ y_offset);
            PointF right = new PointF(centerPoint.X + x_extend, centerPoint.Y + y_offset);
            GraphicsPath path = new GraphicsPath(new PointF[] { top, left, bottom, right }, 
                                new byte[] { (byte)(PathPointType.Start),
                                             (byte)(PathPointType.Line),
                                             (byte)(PathPointType.Line),
                                             (byte)(PathPointType.Line | PathPointType.CloseSubpath)});//终结点放在右点上才能形成闭环路径

            //objGraphics.DrawPath(new Pen(paddingColor), path); //只有外框
            objGraphics.FillPath(new SolidBrush(paddingColor), path); //实心
        }

        private void DrawMax(ref Graphics objGraphics, PointF fltMaxValues, Color clrCurrentColor)
        {
            float values = Height - YSpace - (fltMaxValues.Y - fltYSliceBegin) / fltYSliceValue * YSlice;
            //
            float keys = XSpace + (fltMaxValues.X - fltXSliceBegin) / fltXSliceValue * XSlice;

            float fltY1 = Height - YSpace;
            float fltY2 = YSpace - YSlice / 2;

            DrawRhombus(objGraphics, clrCurrentColor, new PointF(keys, values));
            //DrawInverseTrangle(objGraphics, clrCurrentColor, new PointF(keys, values));
            //objGraphics.DrawLine(new Pen(new SolidBrush(clrCurrentColor), 4), keys, fltY1, keys, fltY2);

            string markText = string.Format("Peak: [{0:f2},{1:f1}]", fltMaxValues.X, fltMaxValues.Y);
            Font markFont = new Font("宋体", 10, FontStyle.Bold);
            var markSize = objGraphics.MeasureString(markText, markFont);
            PointF markLocation = new PointF(keys + 10, values - 30);

            //填充MARK文本背景色
            objGraphics.FillRectangle(new SolidBrush(clrBgColor), new RectangleF(markLocation, markSize));

            objGraphics.DrawString(markText,
                                   markFont,
                                   new SolidBrush(clrCurrentColor), markLocation
                                   );
        }

        private void CreateTitle(ref Graphics objGraphics)
        {// 初始化标题
            objGraphics.DrawString(Title, 
                new Font("宋体", FontSize), 
                new SolidBrush(TextColor), 
                new Point((int)(Width - XSpace) - intFontSize * Title.Length, (int)(YSpace - YSlice / 2 - intFontSpace)));
        }
    }
}

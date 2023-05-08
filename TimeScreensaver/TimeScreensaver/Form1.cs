using Model;
using System.Drawing.Text;
using Util;

namespace TimeScreensaver
{
    public partial class TimeScreensaver : Form
    {
        // �Ƿ���ͣ
        private bool IsPause = false;

        // ��ֹ���������ʱ�������������쳣
        private readonly bool Flag = false;

        #region ��¼��ʼ���ݣ���������ʱ�����Զ���Ӧ��С
        private readonly float InitWidth;
        private readonly float InitHeight;
        private readonly float InitFontSize;
        #endregion

        #region ����Ϸ��Լ�����
        // ��¼����Ƿ�Ϊ����״̬
        private bool IsLeftMouseDown = false;
        // ��¼�����ק���ڱ�Ե�ķ���
        private MouseDirection MouseDirection = MouseDirection.None;
        // ��갴�µ����꣬���ڼ����ϷŴ���ʱ��λ��
        private Point LeftMouseDownPoint;
        #endregion

        public TimeScreensaver()
        {
            // ��ȡ appsettings.json �����ļ�
            AppHelper.GetSettings();

            InitializeComponent();

            #region ��¼��ʼ���ݣ���������ʱ�����Զ���Ӧ��С
            InitWidth = Width;
            InitHeight = Height;
            InitFontSize = Font.Size;
            #endregion

            // ��ֹ��������ʱ SetFontSize ������쳣
            Flag = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsPause)
                Refresh();
        }

        private void TimeScreensaver_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // ESC���˳�
                case Keys.Escape:
                    Dispose();
                    break;
                // �ո���ͣ
                case Keys.Space:
                    if (IsPause)
                        IsPause = false;
                    else
                        IsPause = true;
                    break;
                // �س����˵���
                case Keys.Enter:
                    if (FormBorderStyle == FormBorderStyle.Sizable)
                        FormBorderStyle = FormBorderStyle.None;
                    else
                        FormBorderStyle = FormBorderStyle.Sizable;
                    break;
                // F11��ȫ��
                case Keys.F11:
                    if (WindowState != FormWindowState.Maximized)
                    {
                        FormBorderStyle = FormBorderStyle.None;
                        // ������Ϊ Normal ������Ϊ Maximized Ϊ�˷�ֹ�����Ѿ����ʱ��
                        // �л�Ϊ FormBorderStyle.None �����������������δȫ�������
                        WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        // ������Ϊ Normal ������Ϊ Maximized Ϊ�˷�ֹ�����Ѿ����ʱ��
                        // �л�Ϊ FormBorderStyle.None �����������������δȫ�������
                        WindowState = FormWindowState.Normal;
                    }
                    break;
                // Tab���л�����
                case Keys.Tab:
                    if (++GlobalVariable.Settings.ThemeColorIndex >= GlobalVariable.Settings.ThemeColors.Count)
                    {
                        GlobalVariable.Settings.ThemeColorIndex = 1;
                    }
                    break;
            }
        }

        private void TimeScreensaver_Paint(object sender, PaintEventArgs e)
        {
            #region ���л���ʱ�䵽������
            string stringTime = DateTime.Now.ToString("HH:mm:ss");

            // �����и�ϵ��
            double fontRate = 1.2;
            // ���� X ������
            int fontX = 0;
            // ���� Y �����꣨�����и�ϵ����Ϊ�˵������屣�־��У�
            int fontY = (Height - (int)(Font.GetHeight(e.Graphics) * fontRate)) / 2;

            Rectangle Rectangle = new(fontX, fontY, Width, Height);

            StringFormat stringFormat = new()
            {
                Alignment = StringAlignment.Center
            };

            // ʹ��������ɫ���ñ�����ɫ
            BackColor = ColorTranslator.FromHtml(
                GlobalVariable.Settings.ThemeColors[GlobalVariable.Settings.ThemeColorIndex - 1].BackColor
            );
            // ʹ��������ɫ����������ɫ
            Brush brush = new SolidBrush(ColorTranslator.FromHtml(
                GlobalVariable.Settings.ThemeColors[GlobalVariable.Settings.ThemeColorIndex - 1].FontColor
            ));

            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            e.Graphics.DrawString(stringTime, Font, brush, Rectangle, stringFormat);
            #endregion
        }

        private void TimeScreensaver_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // ������
                case MouseButtons.Left:
                    IsLeftMouseDown = true;
                    LeftMouseDownPoint = e.Location;
                    break;
                // ����Ҽ���Todo
                case MouseButtons.Right:
                    break;
                // ����м���Todo
                case MouseButtons.Middle:
                    break;
            }
        }

        private void TimeScreensaver_MouseUp(object sender, MouseEventArgs e)
        {
            IsLeftMouseDown = false;
            MouseDirection = MouseDirection.None;
        }

        private void TimeScreensaver_MouseMove(object sender, MouseEventArgs e)
        {
            #region ��ק���Ŵ����Լ��ƶ�����
            if (IsLeftMouseDown)
            {
                // ��ק���Ŵ���
                if (MouseDirection != MouseDirection.None)
                {
                    //�趨�÷���󣬵������淽�����ı䴰���С  
                    ResizeForm();
                    return;
                }
                // ��ק�ƶ�����
                else
                {
                    Left += e.X - LeftMouseDownPoint.X;
                    Top += e.Y - LeftMouseDownPoint.Y;
                }
            }
            #endregion

            #region ����ڴ��ڱ�Ե��ʾΪ����ͼ�꣬����¼��귽��������ק���Ŵ���
            if (e.Location.X <= 5 && e.Location.Y <= 5)
            {
                Cursor = Cursors.SizeNWSE;
                MouseDirection = MouseDirection.TopLeft;
            }
            else if (e.Location.X >= Width - 5 && e.Location.Y <= 5)
            {
                Cursor = Cursors.SizeNESW;
                MouseDirection = MouseDirection.TopRight;
            }
            else if (e.Location.X <= 5 && e.Location.Y >= Height - 5)
            {
                Cursor = Cursors.SizeNESW;
                MouseDirection = MouseDirection.BottomLeft;
            }
            else if (e.Location.X >= Width - 5 && e.Location.Y >= Height - 5)
            {
                Cursor = Cursors.SizeNWSE;
                MouseDirection = MouseDirection.BottomRight;
            }
            else if (e.Location.Y <= 5)
            {
                Cursor = Cursors.SizeNS;
                MouseDirection = MouseDirection.Top;
            }
            else if (e.Location.X <= 5)
            {
                Cursor = Cursors.SizeWE;
                MouseDirection = MouseDirection.Left;
            }
            else if (e.Location.Y >= Height - 5)
            {
                Cursor = Cursors.SizeNS;
                MouseDirection = MouseDirection.Bottom;
            }
            else if (e.Location.X >= Width - 5)
            {
                Cursor = Cursors.SizeWE;
                MouseDirection = MouseDirection.Right;
            }
            else
            {
                Cursor = Cursors.Arrow;
                MouseDirection = MouseDirection.None;
            }
            #endregion
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            #region ���ڴ�С�仯ʱ�����ݴ��ڿ�����ű������������С��ʹ�����С����Ӧ
            // ��ֹ��������ʱ SetFont Size ������쳣
            if (!Flag) return;

            float widthRate = Width / InitWidth;
            float heightRate = Height / InitHeight;

            SetFontSize(widthRate, heightRate, this);
            #endregion

            base.OnSizeChanged(e);
        }

        /// <summary>
        /// �������ڴ�С
        /// </summary>
        private void ResizeForm()
        {
            int heightRate;
            int widthRate;
            // ������ק����������ڴ�С�������ƴ��ڴ�С���ɱȳ�ʼ����С
            switch (MouseDirection)
            {
                // ����
                case MouseDirection.TopLeft:
                    Cursor = Cursors.SizeNWSE;
                    heightRate = Top - MousePosition.Y;
                    widthRate = Left - MousePosition.X;
                    if (Height + heightRate > InitHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    if (Width + widthRate > InitWidth)
                    {
                        Width += widthRate;
                        Left -= widthRate;
                    }
                    break;
                // ����
                case MouseDirection.TopRight:
                    Cursor = Cursors.SizeNESW;
                    heightRate = Top - MousePosition.Y;
                    widthRate = MousePosition.X - Left;
                    if (Height + heightRate > InitHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    if (widthRate > InitWidth)
                    {
                        Width = widthRate;
                    }
                    break;
                // ����
                case MouseDirection.BottomLeft:
                    Cursor = Cursors.SizeNESW;
                    heightRate = MousePosition.Y - Top;
                    widthRate = Left - MousePosition.X;
                    if (heightRate > InitHeight)
                    {
                        Height = heightRate;
                    }
                    if (Width + widthRate > InitWidth)
                    {
                        Width += widthRate;
                        Left -= widthRate;
                    }
                    break;
                // ����
                case MouseDirection.BottomRight:
                    Cursor = Cursors.SizeNWSE;
                    heightRate = MousePosition.Y - Top;
                    widthRate = MousePosition.X - Left;
                    if (heightRate > InitHeight)
                    {
                        Height = heightRate;
                    }
                    if (widthRate > InitWidth)
                    {
                        Width = widthRate;
                    }
                    break;
                // ��
                case MouseDirection.Top:
                    Cursor = Cursors.SizeNS;
                    heightRate = Top - MousePosition.Y;
                    if (Height + heightRate > InitHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    break;
                // ��
                case MouseDirection.Left:
                    Cursor = Cursors.SizeWE;
                    widthRate = Left - MousePosition.X;
                    if (Width + widthRate > InitWidth)
                    {
                        Width += widthRate;
                        Left -= widthRate;
                    }
                    break;
                // ��
                case MouseDirection.Bottom:
                    Cursor = Cursors.SizeNS;
                    heightRate = MousePosition.Y - Top;
                    if (heightRate > InitHeight)
                    {
                        Height = heightRate;
                    }
                    Height = MousePosition.Y - Top;
                    break;
                // ��
                case MouseDirection.Right:
                    Cursor = Cursors.SizeWE;
                    widthRate = MousePosition.X - Left;
                    if (widthRate > InitWidth)
                    {
                        Width = widthRate;
                    }
                    break;
            }
        }

        /// <summary>
        /// �������ű������������С
        /// </summary>
        /// <param name="widthRate">������ű���</param>
        /// <param name="heightRate">�߶����ű���</param>
        /// <param name="baseControl">���ؼ�</param>
        private void SetFontSize(float widthRate, float heightRate, Control baseControl)
        {
            Single fontSizeNew;

            if (widthRate < heightRate)
                fontSizeNew = Convert.ToSingle(InitFontSize) * widthRate;
            else
                fontSizeNew = Convert.ToSingle(InitFontSize) * heightRate;

            Font = new Font(Font.Name, fontSizeNew, Font.Style, Font.Unit);
        }
    }

    /// <summary>
    /// �����ק����
    /// </summary>
    public enum MouseDirection
    {
        // �޷���
        None,
        // ��
        Top,
        // ��
        Bottom,
        // ��
        Left,
        // ��
        Right,
        // ����
        TopLeft,
        // ����
        TopRight,
        // ����
        BottomLeft,
        // ����
        BottomRight
    }
}
using Model;
using System.Drawing.Text;
using Util;

namespace TimeScreensaver
{
    public partial class TimeScreensaver : Form
    {
        #region һЩ Flag
        // �Ƿ�����
        private bool IsLock = false;
        // �Ƿ���ͣ
        private bool IsPause = false;
        // ��ֹ���������ʱ�������������쳣
        private readonly bool InitFlag = false;
        // �Ƿ���괩͸
        private bool IsMousePenetration = false;
        #endregion

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

        // ��ʾ��ʱ���ַ���
        private string? stringTime;

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
            InitFlag = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsPause)
                Refresh();
        }

        private void TimeScreensaver_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                // �л���һ������
                case Keys.Shift | Keys.Tab:
                    if (!IsLocked())
                        if (--GlobalVariable.Settings.ThemeColorIndex < 1)
                            GlobalVariable.Settings.ThemeColorIndex = GlobalVariable.Settings.ThemeColors.Count;
                    break;
                // �л���һ������
                case Keys.Tab:
                    if (!IsLocked())
                        if (++GlobalVariable.Settings.ThemeColorIndex > GlobalVariable.Settings.ThemeColors.Count)
                            GlobalVariable.Settings.ThemeColorIndex = 1;
                    break;
                // ��ͣ
                case Keys.Space:
                    if (IsPause)
                        IsPause = false;
                    else
                        IsPause = true;
                    break;
                // �˳�
                case Keys.Escape:
                    if (!IsLocked())
                    {
                        // ��Ϊȫ��״̬������ ESC ��Ϊ�˳�ȫ��������Ϊ�˳�����
                        if (WindowState == FormWindowState.Maximized)
                            WindowState = FormWindowState.Normal;
                        else
                        {
                            DialogResult = MessageBox.Show("ȷ��Ҫ�˳���", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (DialogResult == DialogResult.Yes)
                            {
                                Dispose();
                                Close();
                            }
                        }
                    }
                    break;
            }
        }

        private void TimeScreensaver_Paint(object sender, PaintEventArgs e)
        {
            if (!IsPause)
            {
                #region ���л���ʱ�䵽������
                stringTime = DateTime.Now.ToString("HH:mm:ss");

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
            if (!IsLock)
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
                    if (IsLeftMouseDown)
                        Cursor = Cursors.SizeAll;
                    else
                        Cursor = Cursors.Arrow;
                    MouseDirection = MouseDirection.None;
                }
                #endregion
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            if(toolStripMenuItem.Name != "previousThemeMenuItem" && toolStripMenuItem.Name != "nextThemeMenuItem")
                toolStripMenuItem.Checked = !toolStripMenuItem.Checked;

            ShortcutKeys(toolStripMenuItem.ShortcutKeys);
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShortcutKeys(Keys.Control | Keys.M);
                minimizeMenuItem.Checked = !minimizeMenuItem.Checked;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            #region ���ڴ�С�仯ʱ�����ݴ��ڿ�����ű������������С��ʹ�����С����Ӧ
            // ��ֹ��������ʱ SetFont Size ������쳣
            if (!InitFlag) return;

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
                    if (Height + heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    if (Width + widthRate > GlobalVariable.Settings.MinimumWidth)
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
                    if (Height + heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    if (widthRate > GlobalVariable.Settings.MinimumWidth)
                    {
                        Width = widthRate;
                    }
                    break;
                // ����
                case MouseDirection.BottomLeft:
                    Cursor = Cursors.SizeNESW;
                    heightRate = MousePosition.Y - Top;
                    widthRate = Left - MousePosition.X;
                    if (heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height = heightRate;
                    }
                    if (Width + widthRate > GlobalVariable.Settings.MinimumWidth)
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
                    if (heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height = heightRate;
                    }
                    if (widthRate > GlobalVariable.Settings.MinimumWidth)
                    {
                        Width = widthRate;
                    }
                    break;
                // ��
                case MouseDirection.Top:
                    Cursor = Cursors.SizeNS;
                    heightRate = Top - MousePosition.Y;
                    if (Height + heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    break;
                // ��
                case MouseDirection.Left:
                    Cursor = Cursors.SizeWE;
                    widthRate = Left - MousePosition.X;
                    if (Width + widthRate > GlobalVariable.Settings.MinimumWidth)
                    {
                        Width += widthRate;
                        Left -= widthRate;
                    }
                    break;
                // ��
                case MouseDirection.Bottom:
                    Cursor = Cursors.SizeNS;
                    heightRate = MousePosition.Y - Top;
                    if (heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height = heightRate;
                    }
                    break;
                // ��
                case MouseDirection.Right:
                    Cursor = Cursors.SizeWE;
                    widthRate = MousePosition.X - Left;
                    if (widthRate > GlobalVariable.Settings.MinimumWidth)
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

        /// <summary>
        /// ��ݼ�����
        /// </summary>
        /// <param name="shortcutKey">���µĿ�ݼ�</param>
        private void ShortcutKeys(Keys shortcutKey)
        {
            switch (shortcutKey)
            {
                // �л���һ������
                case Keys.Control | Keys.Shift | Keys.Tab:
                    if (!IsLocked())
                        if (--GlobalVariable.Settings.ThemeColorIndex < 1)
                            GlobalVariable.Settings.ThemeColorIndex = GlobalVariable.Settings.ThemeColors.Count;
                    break;
                // �л���һ������
                case Keys.Control | Keys.Tab:
                    if (!IsLocked())
                        if (++GlobalVariable.Settings.ThemeColorIndex > GlobalVariable.Settings.ThemeColors.Count)
                            GlobalVariable.Settings.ThemeColorIndex = 1;
                    break;
                // ����ʱ��
                case Keys.Control | Keys.C:
                    Clipboard.SetData(DataFormats.Text, stringTime);
                    notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "��ǰ��ʾʱ���Ѹ��Ƶ�������", ToolTipIcon.Info);
                    break;
                // ��ͣ
                case Keys.Control | Keys.Space:
                    if (IsPause)
                        IsPause = false;
                    else
                        IsPause = true;
                    break;
                // ȫ��
                case Keys.F11:
                    if (!IsLocked())
                    {
                        if (WindowState != FormWindowState.Maximized)
                            WindowState = FormWindowState.Maximized;
                        else
                            WindowState = FormWindowState.Normal;
                    }
                    break;
                // ��������
                case Keys.Control | Keys.L:
                    if (!IsLock)
                        // ����ʱ�������ָ�Ĭ��ͼ��
                        Cursor = Cursors.Arrow;
                    IsLock = !IsLock;
                    break;
                // �ö�����
                case Keys.Control | Keys.T:
                    if (!IsLocked())
                        TopMost = !TopMost;
                    break;
                // ��괩͸
                case Keys.Control | Keys.K:
                    if (IsMousePenetration)
                        FormBorderStyle = FormBorderStyle;
                    else
                    {
                        _ = WinHelper.SetWindowLong(Handle, -20, 0x20 | 0x80000);
                        notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "������괩͸���޷�ʹ�ÿ�ݼ������Ҽ������е�ͼ�����", ToolTipIcon.Info);
                    }
                    IsMousePenetration = !IsMousePenetration;
                    break;
                // �յ�����
                case Keys.Control | Keys.M:
                    if (!IsLocked())
                    {
                        if (WindowState == FormWindowState.Minimized)
                            WindowState = FormWindowState.Normal;
                        else
                            WindowState = FormWindowState.Minimized;
                    }
                    break;
                // �ر�
                case Keys.Alt | Keys.F4:
                    if (!IsLocked())
                    {
                        DialogResult = MessageBox.Show("ȷ��Ҫ�˳���", "��ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (DialogResult == DialogResult.Yes)
                        {
                            Dispose();
                            Close();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// �ж��Ƿ����������������״̬��������ʾ֪ͨ
        /// </summary>
        /// <returns>�Ƿ�����</returns>
        private bool IsLocked()
        {
            if (IsLock)
            {
                notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "���������ڣ����Ƚ�����Ctrl + L��", ToolTipIcon.Info);
                return true;
            }
            else
                return false;
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
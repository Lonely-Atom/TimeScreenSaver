using Model;
using System.Drawing.Text;
using Util;

namespace TimeScreensaver
{
    public partial class TimeScreensaver : Form
    {
        #region 一些 Flag
        // 是否锁定
        private bool IsLock = false;
        // 是否暂停
        private bool IsPause = false;
        // 是否鼠标穿透
        private bool IsMousePenetration = false;
        #endregion

        #region 记录初始数据，用于缩放时字体自动适应大小
        // 防止程序刚运行时设置字体会出现异常
        private readonly bool InitFlag = false;
        // 窗体初始宽度
        private readonly float InitWidth;
        // 窗体初始高度
        private readonly float InitHeight;
        // 窗体初始字体大小
        private readonly float InitFontSize;
        #endregion

        #region 鼠标拖放以及缩放
        // 记录鼠标是否为按下状态
        private bool IsLeftMouseDown = false;
        // 记录鼠标拖拽窗口边缘的方向
        private MouseDirection MouseDirection = MouseDirection.None;
        // 鼠标按下的坐标，用于计算拖放窗口时的位置
        private Point LeftMouseDownPoint;
        #endregion

        #region 时间字符串
        // 显示的时间字符串
        private string? stringTime;
        #endregion

        public TimeScreensaver()
        {
            // 读取 appsettings.json 配置文件
            AppHelper.GetSettings();

            InitializeComponent();

            #region 记录初始数据，用于缩放时字体自动适应大小
            InitWidth = Width;
            InitHeight = Height;
            InitFontSize = Font.Size;
            // 防止程序运行时 SetFontSize 会出现异常
            InitFlag = true;
            #endregion
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
                // 切换上一个主题
                case Keys.Shift | Keys.Tab:
                    if (!IsLocked())
                        if (--GlobalVariable.Settings.ThemeColorIndex < 1)
                            GlobalVariable.Settings.ThemeColorIndex = GlobalVariable.Settings.ThemeColors.Count;
                    break;
                // 切换下一个主题
                case Keys.Tab:
                    if (!IsLocked())
                        if (++GlobalVariable.Settings.ThemeColorIndex > GlobalVariable.Settings.ThemeColors.Count)
                            GlobalVariable.Settings.ThemeColorIndex = 1;
                    break;
                // 暂停
                case Keys.Space:
                    if (IsPause)
                        IsPause = false;
                    else
                        IsPause = true;
                    pauseMenuItem.Checked = !pauseMenuItem.Checked;
                    break;
                // 退出
                case Keys.Escape:
                    if (!IsLocked())
                    {
                        // 若为全屏状态，按下 ESC 键为退出全屏，否则为退出程序
                        if (WindowState == FormWindowState.Maximized)
                        {
                            WindowState = FormWindowState.Normal;
                            fullScreenMenuItem.Checked = !fullScreenMenuItem.Checked;
                        }
                        else
                        {
                            DialogResult = MessageBox.Show("确定要退出吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                #region 居中绘制时间到窗口上
                // 获取当前时间，并格式化为字符串
                stringTime = DateTime.Now.ToString("HH:mm:ss");
            }

            // 字体行高系数
            double fontRate = 1.2;
            // 字体 X 轴坐标
            int fontX = 0;
            // 字体 Y 轴坐标（乘以行高系数是为了调整字体保持居中）
            int fontY = (Height - (int)(Font.GetHeight(e.Graphics) * fontRate)) / 2;

            Rectangle rectangle = new(fontX, fontY, Width, Height);

            StringFormat stringFormat = new()
            {
                Alignment = StringAlignment.Center
            };

            // 使用主题配色设置背景颜色
            BackColor = ColorTranslator.FromHtml(
                GlobalVariable.Settings.ThemeColors[GlobalVariable.Settings.ThemeColorIndex - 1].BackColor
            );

            // 使用主题配色设置字体颜色
            Brush brush = new SolidBrush(ColorTranslator.FromHtml(
                GlobalVariable.Settings.ThemeColors[GlobalVariable.Settings.ThemeColorIndex - 1].FontColor
            ));

            // 字体抗锯齿
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            // 绘制
            e.Graphics.DrawString(stringTime, Font, brush, rectangle, stringFormat);
            #endregion
        }

        private void TimeScreensaver_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // 鼠标左键
                case MouseButtons.Left:
                    IsLeftMouseDown = true;
                    // 记录鼠标左键按下的坐标
                    LeftMouseDownPoint = e.Location;
                    break;
                // 鼠标右键：Todo
                case MouseButtons.Right:
                    break;
                // 鼠标中键：Todo
                case MouseButtons.Middle:
                    break;
            }
        }

        private void TimeScreensaver_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                // 鼠标左键
                case MouseButtons.Left:
                    IsLeftMouseDown = false;
                    // 将鼠标拖拽窗口边缘的方向置空
                    MouseDirection = MouseDirection.None;
                    break;
                // 鼠标右键：Todo
                case MouseButtons.Right:
                    break;
                // 鼠标中键：Todo
                case MouseButtons.Middle:
                    break;
            }
        }

        private void TimeScreensaver_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsLock)
            {
                #region 拖拽缩放窗口以及移动窗口
                if (IsLeftMouseDown)
                {
                    // 拖拽缩放窗口
                    if (MouseDirection != MouseDirection.None)
                    {
                        //设定好方向后，调用下面方法，改变窗体大小  
                        ResizeForm();
                        return;
                    }
                    // 拖拽移动窗口
                    else
                    {
                        Cursor = Cursors.SizeAll;
                        Left += e.X - LeftMouseDownPoint.X;
                        Top += e.Y - LeftMouseDownPoint.Y;
                    }
                }
                else
                {
                    #region 鼠标在窗口边缘显示为缩放图标，并记录鼠标方向，用于拖拽缩放窗口
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
                #endregion
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            // 点击的菜单项
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            if (toolStripMenuItem.Name != "previousThemeMenuItem" && 
                toolStripMenuItem.Name != "nextThemeMenuItem" &&
                toolStripMenuItem.Name != "copyTimeMenuItem")
                toolStripMenuItem.Checked = !toolStripMenuItem.Checked;

            // 调用对应的菜单项功能
            ShortcutKeys(toolStripMenuItem.ShortcutKeys);
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // 鼠标左键点击小托盘收到托盘
            if (e.Button == MouseButtons.Left)
            {
                ShortcutKeys(Keys.Control | Keys.M);
                minimizeMenuItem.Checked = !minimizeMenuItem.Checked;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            #region 窗口大小变化时，根据窗口宽高缩放比例设置字体大小，使字体大小自适应
            // 防止程序运行时 SetFont Size 会出现异常
            if (!InitFlag) return;

            float widthRate = Width / InitWidth;
            float heightRate = Height / InitHeight;

            SetFontSize(widthRate, heightRate);
            #endregion

            base.OnSizeChanged(e);
        }

        /// <summary>
        /// 调整窗口大小
        /// </summary>
        private void ResizeForm()
        {
            int heightRate;
            int widthRate;
            // 按照拖拽方向调整窗口大小，且限制窗口大小不可比初始窗口小
            switch (MouseDirection)
            {
                // 左上
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
                // 右上
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
                // 左下
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
                // 右下
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
                // 上
                case MouseDirection.Top:
                    Cursor = Cursors.SizeNS;
                    heightRate = Top - MousePosition.Y;
                    if (Height + heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height += heightRate;
                        Top -= heightRate;
                    }
                    break;
                // 左
                case MouseDirection.Left:
                    Cursor = Cursors.SizeWE;
                    widthRate = Left - MousePosition.X;
                    if (Width + widthRate > GlobalVariable.Settings.MinimumWidth)
                    {
                        Width += widthRate;
                        Left -= widthRate;
                    }
                    break;
                // 下
                case MouseDirection.Bottom:
                    Cursor = Cursors.SizeNS;
                    heightRate = MousePosition.Y - Top;
                    if (heightRate > GlobalVariable.Settings.MinimumHeight)
                    {
                        Height = heightRate;
                    }
                    break;
                // 右
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
        /// 根据缩放比例设置字体大小
        /// </summary>
        /// <param name="widthRate">宽度缩放比例</param>
        /// <param name="heightRate">高度缩放比例</param>
        private void SetFontSize(float widthRate, float heightRate)
        {
            Single fontSizeNew;

            if (widthRate < heightRate)
                fontSizeNew = Convert.ToSingle(InitFontSize) * widthRate;
            else
                fontSizeNew = Convert.ToSingle(InitFontSize) * heightRate;

            Font = new Font(Font.Name, fontSizeNew, Font.Style, Font.Unit);
        }

        /// <summary>
        /// 快捷键操作
        /// </summary>
        /// <param name="shortcutKey">按下的快捷键</param>
        private void ShortcutKeys(Keys shortcutKey)
        {
            switch (shortcutKey)
            {
                // 切换上一个主题
                case Keys.Control | Keys.Shift | Keys.Tab:
                    if (!IsLocked())
                        if (--GlobalVariable.Settings.ThemeColorIndex < 1)
                            GlobalVariable.Settings.ThemeColorIndex = GlobalVariable.Settings.ThemeColors.Count;
                    break;
                // 切换下一个主题
                case Keys.Control | Keys.Tab:
                    if (!IsLocked())
                        if (++GlobalVariable.Settings.ThemeColorIndex > GlobalVariable.Settings.ThemeColors.Count)
                            GlobalVariable.Settings.ThemeColorIndex = 1;
                    break;
                // 复制时间
                case Keys.Control | Keys.C:
                    // 将当前显示的时间文本添加到剪贴板
                    Clipboard.SetData(DataFormats.Text, stringTime);
                    notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "当前显示时间已复制到剪贴板", ToolTipIcon.Info);
                    break;
                // 暂停
                case Keys.Control | Keys.Space:
                    if (IsPause)
                        IsPause = false;
                    else
                        IsPause = true;
                    break;
                // 全屏
                case Keys.F11:
                    if (!IsLocked())
                    {
                        if (WindowState != FormWindowState.Maximized)
                            WindowState = FormWindowState.Maximized;
                        else
                            WindowState = FormWindowState.Normal;
                    }
                    break;
                // 置顶窗口
                case Keys.Control | Keys.T:
                    if (!IsLocked())
                        TopMost = !TopMost;
                    break;
                // 背景透明
                case Keys.Control | Keys.J:
                    if (TransparencyKey != BackColor)
                        TransparencyKey = BackColor;
                    else
                        TransparencyKey = default;
                    break;
                // 鼠标穿透
                case Keys.Control | Keys.K:
                    IsMousePenetration = !IsMousePenetration;
                    if (IsMousePenetration)
                        notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "开启鼠标穿透后快捷键无法捕获，需右键托盘中的图标操作", ToolTipIcon.Info);
                    // 调用 User32.dll 中的方法实现鼠标穿透
                    WinHelper.SetMousePenetrate(Handle, IsMousePenetration);
                    break;
                // 锁定窗口
                case Keys.Control | Keys.L:
                    if (!IsLock)
                        // 锁定时，鼠标光标恢复默认图标
                        Cursor = Cursors.Arrow;
                    IsLock = !IsLock;
                    break;
                // 收到托盘
                case Keys.Control | Keys.M:
                    if (!IsLocked())
                    {
                        Visible = !Visible;
                    }
                    break;
                // 关闭
                case Keys.Alt | Keys.F4:
                    if (!IsLocked())
                    {
                        DialogResult = MessageBox.Show("确定要退出吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
        /// 判断是否锁定，如果是锁定状态，则发送提示通知
        /// </summary>
        /// <returns>是否锁定</returns>
        private bool IsLocked()
        {
            if (IsLock)
            {
                notifyIcon.ShowBalloonTip(0, "TimeScreensaver", "已锁定窗口，请先解锁（Ctrl + L）", ToolTipIcon.Info);
                return true;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// 鼠标拖拽方向
    /// </summary>
    public enum MouseDirection
    {
        // 无方向
        None,
        // 上
        Top,
        // 下
        Bottom,
        // 左
        Left,
        // 右
        Right,
        // 左上
        TopLeft,
        // 右上
        TopRight,
        // 左下
        BottomLeft,
        // 右下
        BottomRight
    }
}
using System.Windows.Forms;

namespace KeyboardLayoutSwitcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private CheckBox chkEnableSwitching;
        private Button btnExit;
        private Label lblStatus;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem menuItemOpen;
        private ToolStripMenuItem menuItemExit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                keyboardHook.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            chkEnableSwitching = new CheckBox();
            btnExit = new Button();
            lblStatus = new Label();
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip();
            menuItemOpen = new ToolStripMenuItem();
            menuItemExit = new ToolStripMenuItem();

            // chkEnableSwitching
            chkEnableSwitching.AutoSize = true;
            chkEnableSwitching.Location = new System.Drawing.Point(12, 12);
            chkEnableSwitching.Text = "�������� ����������� ����������� ���������";
            chkEnableSwitching.Checked = true;

            // btnExit
            btnExit.Location = new System.Drawing.Point(12, 60);
            btnExit.Size = new System.Drawing.Size(260, 30);
            btnExit.Text = "�����";

            // lblStatus
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(12, 40);
            lblStatus.Text = "����������� ����������� ��������";

            // notifyIcon
            notifyIcon.Text = "Keyboard Layout Switcher";

            // contextMenuStrip
            contextMenuStrip.Items.AddRange(new ToolStripItem[] {
                menuItemOpen,
                menuItemExit
            });

            // menuItemOpen
            menuItemOpen.Text = "³������";
            menuItemOpen.Click += menuItemOpen_Click;

            // menuItemExit
            menuItemExit.Text = "�����";
            menuItemExit.Click += menuItemExit_Click;

            // MainForm
            this.ClientSize = new System.Drawing.Size(284, 101);
            this.Controls.Add(chkEnableSwitching);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnExit);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Keyboard Layout Switcher";
            this.FormClosing += MainForm_FormClosing;
        }
    }
}

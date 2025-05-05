
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

class BluebookJailer : Form
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CreateDesktop(string desktopName, IntPtr device, IntPtr devmode, int flags, uint access, IntPtr securityAttributes);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SwitchDesktop(IntPtr hDesktop);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool CloseDesktop(IntPtr hDesktop);

    [DllImport("user32.dll")]
    static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("kernel32.dll")]
    static extern bool CreateProcess(string lpApplicationName, string lpCommandLine,
        IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles,
        uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
        [In] ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [StructLayout(LayoutKind.Sequential)]
    struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public ushort wShowWindow;
        public ushort cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    const uint GENERIC_ALL = 0x10000000;
    const uint CREATE_NEW_CONSOLE = 0x00000010;

    static IntPtr bluebookDesktop;
    static IntPtr mainDesktop;
    static NotifyIcon trayIcon;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        mainDesktop = OpenInputDesktop(0, false, GENERIC_ALL);
        bluebookDesktop = CreateDesktop("BluebookPrison", IntPtr.Zero, IntPtr.Zero, 0, GENERIC_ALL, IntPtr.Zero);

        var si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.lpDesktop = "BluebookPrison";

        PROCESS_INFORMATION pi;

        bool result = CreateProcess(null, "bluebook.exe", IntPtr.Zero, IntPtr.Zero, false,
            CREATE_NEW_CONSOLE, IntPtr.Zero, null, ref si, out pi);

        if (!result)
        {
            MessageBox.Show("Failed to launch Bluebook.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Shield,
            Visible = true,
            Text = "Bluebook Jailer",
            ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Switch to Bluebook", (s, e) => SwitchDesktop(bluebookDesktop)),
                new MenuItem("Switch to Normal", (s, e) => SwitchDesktop(mainDesktop)),
                new MenuItem("Exit", (s, e) => ExitApplication())
            })
        };

        Application.Run();
    }

    static void ExitApplication()
    {
        trayIcon.Visible = false;
        Application.Exit();
    }
}

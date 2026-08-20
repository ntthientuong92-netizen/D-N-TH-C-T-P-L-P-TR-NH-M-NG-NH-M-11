using System;
using System.Windows.Forms;

namespace ChatClient
{
    static class ClientProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainChatForm());
        }
    }
}
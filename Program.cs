using System;
using System.Windows.Forms;

namespace SnakeGame
{
    /// <summary>
    /// University Project: Classic Snake Game
    /// Built with C# and .NET Framework Windows Forms.
    /// Entry Point class for the application execution.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Configures visual styles and launches FormMain.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
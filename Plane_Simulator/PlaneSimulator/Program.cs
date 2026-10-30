using System;
using System.Windows.Forms;

namespace PlaneSimulator;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PlaneForm());
    }
}

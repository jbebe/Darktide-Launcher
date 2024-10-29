using System.CodeDom.Compiler;
using System.Diagnostics;
using System;
using System.Windows;

namespace Launcher;

public partial class App : Application
{
    [STAThread]
    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public static void Main()
    {
        App app = new App();
        app.InitializeComponent();
        app.Run();
    }
}

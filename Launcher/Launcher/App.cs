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
#if DEBUG
        // We need to hook on the instance launched by Steam to get a valid app context.
        // This message box waits for VS to attach the debugger then we can start debugging.
        MessageBox.Show("Enter Launcher in debug mode");
#endif
        App app = new App();
        app.InitializeComponent();
        app.Run();
    }
}

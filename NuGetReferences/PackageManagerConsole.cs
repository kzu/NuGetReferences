namespace ClariusLabs.NuGetReferences
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Clide;
    using Clide.VisualStudio;
    using System.Dynamic;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IToolWindow))]
    [Export(typeof(IPackageManagerConsole))]
    internal class PackageManagerConsole : IPackageManagerConsole
    {
        private VsToolWindow toolWindow;
        private IServiceProvider serviceProvider;
        private bool firstAccess = true;

        [ImportingConstructor]
        public PackageManagerConsole([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.toolWindow = new VsToolWindow(serviceProvider, new Guid("0AD07096-BBA9-4900-A651-0598D26F6D24"));
        }

        public bool IsBusy
        {
            get
            {
                // \o/: there should be a very easy way to ask whether the console is running a command
                var isBusy = (bool)GetPowerConsole().ConsoleStatus.IsBusy;
                return isBusy;
            }
        }

        public bool IsInitialized
        {
            get
            {
                if (firstAccess)
                {
                    Show();
                    firstAccess = false;
                }

                // \o/: there should be a very easy way to ask whether the console can run commands
                var isInitialized = (bool)GetPowerConsole().WpfConsole.Dispatcher.IsStartCompleted;
                return isInitialized;
            }
        }

        public void Close()
        {
            toolWindow.Close();
        }

        public bool IsVisible
        {
            get { return toolWindow.IsVisible; }
        }

        public void Show()
        {
            toolWindow.Show();
        }

        public void Execute(string command)
        {
            Show();
            var powerConsole = GetPowerConsole();
            // \o/: there should be a very easy way to bring up the console and execute a command :)
            // This also works, but doesn't write the command to the output window, which is nice.
            //var console = powerConsole.AsDynamicReflection().WpfConsole;
            //console.Host.Execute(console, command, default(object[]));
            //powerConsole.Execute(command, null);

            // NOTE: PowerConsoleToolWindow.cs (609) will write the command 
            // on the window, but never actually issue the command. So we need 
            // to clear it after execution, or it will linger in the internal 
            // buffer and cause weird behavior.
            //for (int i = 0; i < command.Length + 2; i++)
            //{
            //    powerConsole.WpfConsole.WriteBackspace();
            //}

            powerConsole.WpfConsole.Dispatcher.ClearConsole();
            powerConsole.WpfConsole.WriteLine(command);
            System.Windows.Forms.SendKeys.SendWait(Environment.NewLine);
        }

        private dynamic GetPowerConsole()
        {
            object windowPane;
            ErrorHandler.ThrowOnFailure(toolWindow.Frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out windowPane));
            return windowPane.AsDynamicReflection();
        }
    }
}
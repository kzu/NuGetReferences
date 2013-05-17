namespace ClariusLabs.NuGetReferences.Commands
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using Clide;
    using Clide.Commands;
    using Clide.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.GraphModel.Schemas;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using System.Dynamic;
    using ClariusLabs.NuGetReferences.Properties;

    [Command(Guids.PackageGuid, Guids.CommandSetGuid, CommandIds.Update)]
    public class Update : ICommandExtension
    {
        private static readonly ITracer tracer = Tracer.Get<Update>();
        private Lazy<IShellPackage> package;
        private IPackageManagerConsole console;

        [ImportingConstructor]
        public Update(Lazy<IShellPackage> package, IPackageManagerConsole console)
        {
            this.package = package;
            this.console = console;
        }

        public void Execute(IMenuCommand command)
        {
            tracer.Info("Update");

            if (package.Value.SelectedNode != null)
            {
                var project = package.Value.DevEnv.SolutionExplorer().SelectedNodes.OfType<IItemNode>().First().OwningProject;

                // TODO:  doesn't work
                // package.Value.SelectedNode.Node.SetValue<string>(DgmlNodeProperties.Icon, GraphIcons.PackageUpdate);
                var nuget = package.Value.SelectedNode.Node.GetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty);
                var psCommand = "Update-Package " + nuget.Id + " -ProjectName " + project.DisplayName;
                tracer.Info("Updating package " + nuget.Id);

                console.Show();
                console.Execute(psCommand);
            }
        }

        public void QueryStatus(IMenuCommand command)
        {
            command.Enabled = console.IsInitialized && !console.IsBusy;
            if (!command.Enabled)
                command.Text = Strings.Update.Text + " " + Strings.InitializingConsole;
            if (console.IsBusy)
                command.Text = Strings.Update.Text + " " + Strings.ExecutingCommand;
        }

        public string Text { get; set; }
    }
}
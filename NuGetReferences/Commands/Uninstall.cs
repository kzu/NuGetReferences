#region Apache Licensed
/*
 Copyright 2013 Clarius Consulting, Daniel Cazzulino

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
#endregion

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
    using NuGet.VisualStudio;
    using ClariusLabs.NuGetReferences.Properties;
    using Clide.Diagnostics;

    [Command(Guids.PackageGuid, Guids.CommandSetGuid, CommandIds.Uninstall)]
    public class Uninstall : ICommandExtension
    {
        private static readonly ITracer tracer = Tracer.Get<Uninstall>();
        private Lazy<IShellPackage> package;
        private IPackageManagerConsole console;

        [ImportingConstructor]
        public Uninstall(Lazy<IShellPackage> package, IPackageManagerConsole console)
        {
            this.package = package;
            this.console = console;
        }

        public void Execute(IMenuCommand command)
        {
            tracer.Info("Uninstall");

            if (package.Value.SelectedNode != null)
            {
                var project = package.Value.DevEnv.SolutionExplorer().SelectedNodes.OfType<IItemNode>().First().OwningProject;

                var nuget = package.Value.SelectedNode.Node.GetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty);
                var psCommand = "Uninstall-Package " + nuget.Id + " -ProjectName " + project.DisplayName;
                tracer.Info("Uninstalling package " + nuget.Id);

                console.Show();
                console.Execute(psCommand);
            }
        }

        public void QueryStatus(IMenuCommand command)
        {
            command.Enabled = console.IsInitialized && !console.IsBusy;
            if (!console.IsInitialized)
                command.Text = Strings.Uninstall.Text + " " + Strings.InitializingConsole;
            if (console.IsBusy)
                command.Text = Strings.Uninstall.Text + " " + Strings.ExecutingCommand;
        }

        public string Text { get; set; }
    }
}
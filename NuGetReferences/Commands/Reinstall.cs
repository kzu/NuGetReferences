#region Apache Licensed
/*
 Copyright 2013 Daniel Cazzulino

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

namespace Kzu.NuGetReferences.Commands
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Kzu.NuGetReferences.Properties;
    using Clide;
    using Clide.Commands;
    using Clide.Diagnostics;
    using Clide.Solution;
    using NuGet.VisualStudio;

    [Command(Guids.CommandSetGuid, CommandIds.Reinstall)]
    public class Reinstall : ICommandExtension
    {
        private static readonly ITracer tracer = Tracer.Get<Reinstall>();
        private Lazy<IShellPackage> package;
        private IPackageManagerConsole console;

        [ImportingConstructor]
        public Reinstall(Lazy<IShellPackage> package, IPackageManagerConsole console)
        {
            this.package = package;
            this.console = console;
        }

        public void Execute(IMenuCommand command)
        {
            tracer.Info("Reinstall");

            if (package.Value.SelectedNode != null)
            {
                var project = package.Value.DevEnv.SolutionExplorer().SelectedNodes.OfType<IItemNode>().First().OwningProject;

                // TODO:  doesn't work
                // package.Value.SelectedNode.Node.SetValue<string>(DgmlNodeProperties.Icon, GraphIcons.PackageUpdate);
                var nuget = package.Value.SelectedNode.Node.GetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty);
                var psCommand = "Update-Package " + nuget.Id + " -ProjectName " + project.DisplayName + " -ReInstall";
                tracer.Info("Reinstalling package " + nuget.Id);

                console.Show();
                console.Execute(psCommand);
            }
        }

        public void QueryStatus(IMenuCommand command)
        {
            command.Enabled = console.IsInitialized && !console.IsBusy;
            if (!console.IsInitialized)
                command.Text = Strings.Reinstall.Text + " " + Strings.InitializingConsole;
            if (console.IsBusy)
                command.Text = Strings.Reinstall.Text + " " + Strings.ExecutingCommand;
        }

        public string Text { get; set; }
    }
}
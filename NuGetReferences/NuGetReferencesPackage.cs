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

namespace ClariusLabs.NuGetReferences
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using ClariusLabs.NuGetReferences.Properties;
    using Clide;
    using Clide.Diagnostics;
    using Microsoft.VisualStudio.ExtensionManager;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource(1000, 5)]
    [Guid(Guids.PackageGuid)]
    [ProvideBindingPath]
    public class NuGetReferencesPackage : Package, IShellPackage
    {
        private ITracer tracer;
        private IDisposable host;

        protected override void Initialize()
        {
            base.Initialize();

            this.host = Host.Initialize(this, "NuGet References", this.GetType().Namespace);
            this.DevEnv = Clide.DevEnv.Get(this);

#if DEBUG
            Tracer.Manager.SetTracingLevel(this.GetType().Namespace, SourceLevels.Verbose);
#else
            Tracer.Manager.SetTracingLevel(this.GetType().Namespace, SourceLevels.Information);
#endif

            this.tracer = Tracer.Get<NuGetReferencesPackage>();
            Trial.Initialize(tracer);

            try
            {
                var nugetPackage = this.GetLoadedPackage(new Guid(Guids.PackageGuid));
                var version = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(asm => asm.GetName())
                    .Where(name => name.Name.StartsWith("NuGet.Tools"))
                    .Select(name => name.Version)
                    .FirstOrDefault();

                var buildVersion = new Version(Constants.NuGetBuildVersion);
                if (version != null && version < buildVersion)
                {
                    DevEnv.MessageBoxService.Show(
                        Strings.IncompatibleNuGet(version, Constants.ProductName, Constants.NuGetBuildVersion),
                        title: Constants.ProductName,
                        icon: System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
                DevEnv.MessageBoxService.Show(Strings.FailedToLoadNuGetPackage,
                    title: Constants.ProductName,
                    icon: System.Windows.MessageBoxImage.Error);
            }

            Trial.EnsureDevStore(Constants.VsixIdentifier, "Clarius.DevStore11", "DevStore11.vsix");
        }

        public IDevEnv DevEnv { get; private set; }
        public ISelectedGraphNode SelectedNode { get; set; }
    }
}
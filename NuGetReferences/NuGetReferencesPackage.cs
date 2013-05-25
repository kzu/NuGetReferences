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
        private static readonly ITracer tracer = Tracer.Get<NuGetReferencesPackage>();
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

            VerifyDevStore();
        }

        public IDevEnv DevEnv { get; private set; }
        public ISelectedGraphNode SelectedNode { get; set; }

        [Conditional("TRIAL")]
        private void VerifyDevStore()
        {
            try
            {
                var extensionManager = ServiceLocator.GlobalProvider.TryGetService<SVsExtensionManager, IVsExtensionManager>();
                if (extensionManager == null)
                    return;

                var myExtension = default(IInstalledExtension);
                if (!extensionManager.TryGetInstalledExtension(Constants.VsixIdentifier, out myExtension));
                if (myExtension == null)
                    return;

                var vsixPath = Path.Combine(myExtension.InstallPath, "DevStore11.vsix");
                if (!File.Exists(vsixPath))
                {
                    tracer.Error(Strings.Package.DevStoreVsixNotFound(vsixPath, Constants.ProductName));
                    return;
                }

                var installableDevStore = extensionManager.CreateInstallableExtension(vsixPath);
                if (installableDevStore == null)
                    return;

                var existingDevStore = default(IInstalledExtension);
                if (!extensionManager.TryGetInstalledExtension("Clarius.DevStore11", out existingDevStore))
                    tracer.Info(Strings.Package.DevStoreNotInstalled);
                // TODO: auto-enable if user has disabled ;)
                // else if (!extensionManager.Enable(
                    
                if (existingDevStore != null && existingDevStore.Header.Version < installableDevStore.Header.Version)
                    tracer.Info(Strings.Package.DevStoreOldVersion(existingDevStore.Header.Version, installableDevStore.Header.Version));

                var shouldInstall = existingDevStore == null ||
                    existingDevStore.Header.Version < installableDevStore.Header.Version;

                if (shouldInstall)
                {
                    tracer.Info(Strings.Package.DevStoreInstalling(installableDevStore.Header.Version, vsixPath));
                    if (extensionManager.Install(installableDevStore, false) != RestartReason.None)
                        tracer.Info(Strings.Package.DevStoreRestartNeeded);
                }
            }
            catch (Exception e)
            {
                tracer.Error(e, "Failed to update DevStore extension");
            }
        }
    }
}
namespace ClariusLabs.NuGetReferences
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Clide;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource(1000, 5)]
    [Guid(Guids.PackageGuid)]
    [ProvideBindingPath]
    public class ShellPackage : Package, IShellPackage
    {
        protected override void Initialize()
        {
            base.Initialize();

#if DEBUG
            Tracer.Manager.SetTracingLevel("ClariusLabs.NuGetReferences", SourceLevels.All);
#else
			Tracer.Manager.SetTracingLevel("ClariusLabs.NuGetReferences", SourceLevels.Warning);
#endif

            Host.Initialize(this, "NuGet References");
            this.DevEnv = Clide.DevEnv.Get(this);
        }

        public IDevEnv DevEnv { get; private set; }
        public ISelectedGraphNode SelectedNode { get; set; }
    }
}
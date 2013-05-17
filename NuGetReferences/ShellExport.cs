namespace ClariusLabs.NuGetReferences
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Microsoft.VisualStudio.Shell;

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellExport
    {
        [Export]
        public IShellPackage Shell { get { return (IShellPackage)ServiceProvider.GlobalProvider.GetLoadedPackage(new Guid(Guids.PackageGuid)); } }
    }
}
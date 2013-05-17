//namespace ClariusLabs.NuGetReferences
//{
    using System;
    using System.Linq;
    using ClariusLabs.NuGetReferences.Properties;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.Design;

    // TODO: how to get the type to display nicely in the properties window?
    [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.DisplayName)]
    [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.DisplayName)]
    public class NuGetPackage
    {
        [ResourceCategory(typeof(Resources), ResourceNames.PackageInfo.Category)]
        [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.Id.DisplayName)]
        [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.Id.Description)]
        public string Id { get; internal set; }

        [ResourceCategory(typeof(Resources), ResourceNames.PackageInfo.Category)]
        [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.Version.DisplayName)]
        [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.Version.Description)]
        public string Version { get; internal set; }

        [ResourceCategory(typeof(Resources), ResourceNames.PackageInfo.Category)]
        [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.Title.DisplayName)]
        [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.Title.Description)]
        public string Title { get; internal set; }

        [ResourceCategory(typeof(Resources), ResourceNames.PackageInfo.Category)]
        [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.Authors.DisplayName)]
        [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.Authors.Description)]
        public string Authors { get; internal set; }

        [ResourceCategory(typeof(Resources), ResourceNames.PackageInfo.Category)]
        [ResourceDisplayName(typeof(Resources), ResourceNames.PackageInfo.InstallPath.DisplayName)]
        [ResourceDescription(typeof(Resources), ResourceNames.PackageInfo.InstallPath.Description)]
        public string InstallPath { get; internal set; }

        public override string ToString()
        {
            return Strings.PackageInfo.DisplayName;
        }
    }
//}
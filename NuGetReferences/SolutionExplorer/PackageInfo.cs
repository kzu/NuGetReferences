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

//namespace Kzu.NuGetReferences
//{
using Kzu.NuGetReferences.Properties;
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
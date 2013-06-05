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
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.VisualStudio.Shell.Interop;

    public class GraphIcons
    {
        public const string Packages = Id.PrefixDot + "Packages";
        public const string Package = Id.PrefixDot + "Package";
        public const string PackagesConfig = Id.PrefixDot + "PackagesConfig";
        public const string PackageUpdate = Id.PrefixDot + "PackageUpdate";

        private IServiceProvider serviceProvider;

        public GraphIcons(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Initialize();
        }

        public void Initialize()
        {
            RegisterIcon(Packages, "nuget.png");
            RegisterIcon(Package, "package.ico");
            RegisterIcon(PackagesConfig, "config.ico");
            RegisterIcon(PackageUpdate, "update.ico");
        }

        private void RegisterIcon(string imageName, string resourceName)
        {
            var imageService = this.serviceProvider.GetService(typeof(SVsImageService)) as IVsImageService;
            imageService.Add(imageName, new LazyImage(() => LoadWpfImage(resourceName)));
        }

        private ImageSource LoadWpfImage(string resourceName)
        {
            string prefix = "pack://application:,,,/" + this.GetType().Assembly.GetName().Name + ";component/Resources/";
            string fullResourceName = prefix + resourceName;

            return new BitmapImage(new Uri(fullResourceName));
        }
    }
}

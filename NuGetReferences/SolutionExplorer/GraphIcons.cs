namespace ClariusLabs.NuGetReferences
{
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Internal.VisualStudio.PlatformUI;

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

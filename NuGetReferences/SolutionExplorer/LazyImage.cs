namespace ClariusLabs.NuGetReferences
{
    using Microsoft.Internal.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media;

    internal class LazyImage : IVsUIObject
    {
        private readonly Lazy<IVsUIObject> instance;

        public LazyImage(Func<ImageSource> imageCreator)
        {
            instance = new Lazy<IVsUIObject>(() => WpfPropertyValue.CreateIconObject(imageCreator()));
        }

        public int Equals(IVsUIObject pOtherObject, out bool pfAreEqual)
        {
            return instance.Value.Equals(pOtherObject, out pfAreEqual);
        }

        public int get_Data(out object pVar)
        {
            return instance.Value.get_Data(out pVar);
        }

        public int get_Format(out uint pdwDataFormat)
        {
            return instance.Value.get_Format(out pdwDataFormat);
        }

        public int get_Type(out string pTypeName)
        {
            return instance.Value.get_Type(out pTypeName);
        }
    }
}

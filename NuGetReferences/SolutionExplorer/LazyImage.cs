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
    using Microsoft.Internal.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell.Interop;

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

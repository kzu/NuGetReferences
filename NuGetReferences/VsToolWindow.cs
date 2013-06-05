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

namespace Clide.VisualStudio
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class VsToolWindow
    {
        private IVsUIShell uiShell;
        private Guid toolWindowId;
        private Lazy<IVsWindowFrame> frame;

        public VsToolWindow(IServiceProvider serviceProvider, Guid toolWindowId)
        {
            this.uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            this.toolWindowId = toolWindowId;
            this.frame = new Lazy<IVsWindowFrame>(() => ThreadHelper.Generic.Invoke(() =>
            {
                IVsWindowFrame frame;
                ErrorHandler.ThrowOnFailure(this.uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref toolWindowId, out frame));
                return frame;
            }));
        }

        public bool IsVisible
        {
            get
            {
                return Frame != null && Frame.IsVisible() == 0;
            }
        }

        public void Show()
        {
            if (Frame != null)
                ErrorHandler.ThrowOnFailure(Frame.Show());
        }

        public void Close()
        {
            if (Frame != null)
            {
                ErrorHandler.ThrowOnFailure(Frame.Hide());
            }
        }

        public IVsWindowFrame Frame { get { return frame.Value; } }
    }
}

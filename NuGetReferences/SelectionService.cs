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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Collections;
    using System.ComponentModel.Composition;

    internal class SelectionService
    {
        ITrackSelection trackSelection;
        SelectionContainer selectionContainer;

        public SelectionService(IServiceProvider serviceProvider)
        {
            this.trackSelection = serviceProvider.GetService<ITrackSelection>();
            this.selectionContainer = new SelectionContainer();
        }

        internal void Select(object element)
        {
            var objects = new ArrayList();
            objects.Add(element);

            this.selectionContainer.SelectableObjects = objects;
            this.selectionContainer.SelectedObjects = objects;

            this.trackSelection.OnSelectChange(this.selectionContainer);
        }
    }
}

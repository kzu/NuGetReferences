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

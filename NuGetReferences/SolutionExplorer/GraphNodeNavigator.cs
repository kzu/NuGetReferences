namespace ClariusLabs.NuGetReferences
{
    using EnvDTE;
    using Microsoft.VisualStudio.GraphModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class GraphNodeNavigator : IGraphNavigateToItem
    {
        public void NavigateTo(GraphObject graphObject)
        {
            // TODO: implement navigating to the packages.config maybe?
        }

        public int GetRank(GraphObject graphObject)
        {
            return GraphNavigateToItemRanks.CanNavigateToItem;
        }
    }
}

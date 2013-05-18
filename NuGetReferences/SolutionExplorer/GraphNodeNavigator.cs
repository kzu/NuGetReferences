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

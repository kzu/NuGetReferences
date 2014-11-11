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

namespace Kzu.NuGetReferences
{
    using Microsoft.VisualStudio.GraphModel;
    using Microsoft.VisualStudio.GraphModel.Schemas;

    internal static class GraphNodeExtensions
    {
        internal static bool IsRootNode(this GraphNode node)
        {
            return node.IsPropertiesNode() || node.IsConfigNode();
        }

        internal static bool IsPropertiesNode(this GraphNode node)
        {
            return node.HasCategory("ProjectFolder") && node.Label == "Properties";
        }

        internal static bool IsConfigNode(this GraphNode node)
        {
            return node.HasCategory(CodeNodeCategories.ProjectItem) && node.Label == "packages.config";
        }

        internal static bool IsPackageNode(this GraphNode node)
        {
            return node.HasCategory(ReferencesGraphSchema.PackageCategory);
        }
    }
}

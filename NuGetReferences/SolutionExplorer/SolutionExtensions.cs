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
    using System.IO;
    using Clide.Solution;
    using EnvDTE;
    using Microsoft.VisualStudio.GraphModel;
    using Microsoft.VisualStudio.GraphModel.Schemas;

    internal static class SolutionExtensions
    {
        public static GraphNodeId GetId(this IProjectNode node)
        {
            var project = node.As<Project>();
            if (project.Kind == EnvDTE.Constants.vsProjectKindUnmodeled)
                return null;

            var fileName = GetProjectFileUri(project);
            if (fileName == null)
                return null;

            return GraphNodeId.GetNested(CodeGraphNodeIdName.Assembly, fileName);
        }

        public static GraphNodeId GetId(this IItemNode node)
        {
            return GraphNodeId.GetNested(
                GraphNodeId.GetPartial(CodeGraphNodeIdName.Assembly, GetProjectFileUri(node.OwningProject.As<Project>())),
                GraphNodeId.GetPartial(CodeGraphNodeIdName.File, new Uri(node.PhysicalPath, UriKind.RelativeOrAbsolute)));
        }

        private static Uri GetProjectFileUri(Project project)
        {
            // Get the Project's unique name
            string projectName = project.FullName;

            if (string.IsNullOrEmpty(projectName))
            {
                var solutionFile = project.DTE.Solution.FullName;
                var folder = string.Empty;

                var folderProject = project;
                while (folderProject != null)
                {
                    folder = Path.Combine(folderProject.Name, folder);

                    if (folderProject.ParentProjectItem == null)
                        folderProject = null;
                    else
                        folderProject = folderProject.ParentProjectItem.ContainingProject;
                }

                if (string.IsNullOrEmpty(solutionFile))
                    projectName = folder; // Miscellaneous project.
                else
                    projectName = Path.Combine(Path.GetDirectoryName(solutionFile), folder);
            }

            return new Uri(projectName, UriKind.RelativeOrAbsolute);
        }
    }
}

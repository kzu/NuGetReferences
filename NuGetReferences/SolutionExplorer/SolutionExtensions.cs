namespace ClariusLabs.NuGetReferences
{
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.GraphModel;
    using Microsoft.VisualStudio.GraphModel.Schemas;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.IO;
    using Clide.Solution;

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

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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using Clide;
    using Clide.Solution;
    using Microsoft.Internal.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.GraphModel;
    using Microsoft.VisualStudio.GraphModel.Schemas;
    using Microsoft.VisualStudio.Shell.Interop;
    using NuGet.VisualStudio;
    using EnvDTE;
    using System.IO;
    using NuGet;
    using Clide.Diagnostics;
    using System.Xml.Linq;

    [GraphProvider(Name = Id.PrefixDot + "GraphProvider")]
    public class ReferencesGraphProvider : IGraphProvider, IVsSelectionEvents
    {
        delegate void SearchHandler(string term, IGraphContext context);

        private static readonly ITracer tracer = Tracer.Get<ReferencesGraphProvider>();
        private GraphIcons icons;

        private uint selectionCookie;

        private IShellPackage package;
        private IVsPackageInstallerServices packageInstaller;
        private IVsPackageInstallerEvents installerEvents;
        private SelectionService selectionService;
        private ConcurrentQueue<IItemNode> searchItems;

        private List<IGraphContext> trackingContext = new List<IGraphContext>();

        [ImportingConstructor]
        public ReferencesGraphProvider(IShellPackage package,
            IVsPackageInstallerServices packageInstaller,
            IVsPackageInstallerEvents installerEvents)
        {
            this.package = package;
            this.packageInstaller = packageInstaller;
            this.installerEvents = installerEvents;

            installerEvents.PackageReferenceAdded += e => System.Threading.Tasks.Task.Run((Action)RefreshNodes);
            installerEvents.PackageReferenceRemoved += e => System.Threading.Tasks.Task.Run((Action)RefreshNodes);

            this.icons = new GraphIcons(package);
            icons.Initialize();

            var monitorSelection = package.GetService<IVsMonitorSelection>();
            if (monitorSelection != null)
            {
                monitorSelection.AdviseSelectionEvents(this, out selectionCookie);
            }
        }

        public void BeginGetGraphData(IGraphContext context)
        {
            this.BeginGetGraphData(context, true);
        }

        private void BeginGetGraphData(IGraphContext context, bool queueUserWorkItem)
        {
            if (context.Direction == GraphContextDirection.Self &&
                 context.RequestedProperties.Contains(DgmlNodeProperties.ContainsChildren))
            {
                var configNode = context.InputNodes.FirstOrDefault(node => node.IsConfigNode());
                if (configNode != null)
                {
                    using (var scope = new GraphTransactionScope())
                    {
                        configNode.SetValue(DgmlNodeProperties.ContainsChildren, true);
                        scope.Complete();
                    }
                }
            }
            else if (context.Direction == GraphContextDirection.Contains)
            {
                var configNode = context.InputNodes.FirstOrDefault(node => node.IsConfigNode());
                if (configNode != null)
                {
                    if (queueUserWorkItem)
                        ThreadPool.QueueUserWorkItem(TryAddPackageNodes, context);
                    else
                        TryAddPackageNodes(context);

                    // Prevent OnCompleted since we're either doing it 
                    // async or synchronously and completing in AddPackageNodes.
                    return;
                }
            }
            else if (context.Direction == GraphContextDirection.Custom)
            {
                // TODO: search isn't working :(
                StartSearch(context);
                return;
            }

            context.OnCompleted();
        }

        private void RefreshNodes()
        {
            foreach (var context in this.trackingContext.ToList())
            {
                this.BeginGetGraphData(context, false);
            }
        }

        private void TryAddPackageNodes(object state)
        {
            var context = (IGraphContext)state;
            try
            {
                AddPackageNodes(context, context.InputNodes.First());
                TrackChanges(context);
            }
            catch (Exception e)
            {
                tracer.Error(e);
            }
            finally
            {
                context.OnCompleted();
            }
        }

        private void AddPackageNodes(IGraphContext context, GraphNode parentNode)
        {
            var filePath = parentNode.Properties
                .Where(pair => pair.Key.Id == "FilePath")
                .Select(pair => pair.Value)
                .OfType<string>()
                .FirstOrDefault();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            AddPackageNodes(context, parentNode, GetInstalledPackages(filePath));
        }

        private static void AddPackageNodes(IGraphContext context, GraphNode parentNode, IEnumerable<IVsPackageMetadata> installedPackages)
        {
            var allPackages = installedPackages.ToList();
            var installedIds = new HashSet<Tuple<string, string>>(allPackages.Select(x => Tuple.Create(x.Id, x.VersionString)));

            // Remove nodes that aren't installed anymore.
            var toRemove = from node in context.OutputNodes
                           where node.IsPackageNode()
                           let installed = node.GetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty)
                           where !installedIds.Contains(Tuple.Create(installed.Id, installed.VersionString))
                           select node;

            if (toRemove.Any())
            {
                using (var scope = new GraphTransactionScope())
                {
                    toRemove.ToList().ForEach(node => node.Remove());
                    scope.Complete();
                }
            }

            var progress = 0;
            var count = allPackages.Count;
            foreach (var nugetPackage in allPackages)
            {
                var node = GetOrCreatePackageNode(context, parentNode, nugetPackage);
                context.ReportProgress(++progress, count, null);

                if (context.CancelToken.IsCancellationRequested)
                    break;

                context.CancelToken.ThrowIfCancellationRequested();
                System.Threading.Thread.Sleep(50);
            }
        }

        private GraphNode GetOrCreateConfigNode(IGraphContext context, IItemNode packagesConfig)
        {
            using (var scope = new GraphTransactionScope())
            {
                var fileNode = context.Graph.Nodes.GetOrCreate(
                    packagesConfig.GetId(),
                    Path.GetFileName(packagesConfig.PhysicalPath),
                    CodeNodeCategories.ProjectItem);
                
                fileNode.SetValue("Guid", Guid.NewGuid());
                fileNode.SetValue("FilePath", packagesConfig.PhysicalPath);
                fileNode.SetValue<string>(DgmlNodeProperties.Icon, GraphIcons.PackagesConfig);

                context.OutputNodes.Add(fileNode);
                scope.Complete();

                return fileNode;
            }
        }

        private static GraphNode GetOrCreatePackageNode(IGraphContext context, GraphNode parent, IVsPackageMetadata package)
        {
            using (var scope = new GraphTransactionScope())
            {
                var parentId = parent.GetValue<GraphNodeId>("Id");
                var nodeId = GraphNodeId.GetNested(
                    parentId,
                    GraphNodeId.GetPartial(CodeGraphNodeIdName.Member, package.Id),
                    GraphNodeId.GetPartial(CodeGraphNodeIdName.Parameter, package.VersionString));

                var node = context.Graph.Nodes.Get(nodeId);
                if (node == null)
                {
                    node = context.Graph.Nodes.GetOrCreate(nodeId, package.Id, ReferencesGraphSchema.PackageCategory);

                    node.SetValue<string>(DgmlNodeProperties.Icon, GraphIcons.Package);
                    node.SetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty, package);

                    // Establish the relationship with the parent node.
                    context.Graph.Links.GetOrCreate(parent, node, "Packages", GraphCommonSchema.Contains);

                    context.OutputNodes.Add(node);
                    scope.Complete();
                }

                return node;
            }
        }

        private IEnumerable<IVsPackageMetadata> GetInstalledPackages(string packagesConfig)
        {
            var projectPackages = new HashSet<Tuple<string, string>>(
                from package in XDocument.Load(packagesConfig).Root.Elements()
                select Tuple.Create(package.Attribute("id").Value, package.Attribute("version").Value));

            var allPackages = packageInstaller.GetInstalledPackages().Where(x => projectPackages.Contains(Tuple.Create(x.Id, x.VersionString)));

            return allPackages;
        }

        private void TrackChanges(IGraphContext context)
        {
            if (!this.trackingContext.Contains(context))
            {
                context.Canceled += OnContextCanceled;
                this.trackingContext.Add(context);
            }
        }

        private void OnContextCanceled(object sender, EventArgs e)
        {
            var context = sender as IGraphContext;
            if (this.trackingContext.Contains(context))
            {
                this.trackingContext.Remove(context);
            }
        }

        private void StartSearch(IGraphContext context)
        {
            var searchParameter = context.GetValue<ISolutionSearchParameters>(typeof(ISolutionSearchParameters).GUID.ToString());
            if (searchParameter != null)
            {
                var term = searchParameter.SearchQuery.SearchString;
                var items = package.DevEnv.SolutionExplorer().Solution
                    .Traverse()
                    .OfType<IItemNode>()
                    .Where(item => item.DisplayName == "packages.config");

                searchItems = new ConcurrentQueue<IItemNode>();

                foreach (var item in items)
                {
                    searchItems.Enqueue(item);
                }

                TrackChanges(context);
                Application.Current.Dispatcher.BeginInvoke(new SearchHandler(SearchNextItem), term, context);
            }
        }

        private void SearchNextItem(string term, IGraphContext context)
        {
            if (context.CancelToken.IsCancellationRequested || string.IsNullOrEmpty(term))
            {
                // stop right here, no more calls and empty the queue.
                this.searchItems = null;
                return;
            }

            IItemNode item;
            if (this.searchItems != null && this.searchItems.TryDequeue(out item))
            {
                var normalizedTerm = term.ToLowerInvariant();
                var installedPackages = GetInstalledPackages(item.PhysicalPath)
                    .Where(x => x.Id.ToLowerInvariant().Contains(normalizedTerm));

                var configNode = this.GetOrCreateConfigNode(context, item);
                AddPackageNodes(context, configNode, installedPackages);

                Application.Current.Dispatcher.BeginInvoke(new SearchHandler(SearchNextItem), term, context);
            }
            else
            {
                context.OnCompleted();
            }
        }

        public IEnumerable<GraphCommand> GetCommands(IEnumerable<GraphNode> nodes)
        {
            return new GraphCommand[] 
            {          
                //new GraphCommand(ShowInstalled),
                new GraphCommand(GraphCommandDefinition.Contains, null, null, true)
            };
        }

        public T GetExtension<T>(GraphObject graphObject, T previous) where T : class
        {
            if (typeof(T) == typeof(IGraphFormattedLabel))
            {
                // TODO: format labels dynamically?
            }
            else if (typeof(T) == typeof(IGraphNavigateToItem))
            {
                return new GraphNodeNavigator() as T;
            }

            return null;
        }

        public Graph Schema { get { return null; } }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            if (pSCNew != null)
            {
                try
                {
                    uint c;
                    pSCNew.CountObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_SELECTED, out c);

                    if (c == 1)
                    {
                        object[] objects = new object[c];
                        pSCNew.GetObjects((uint)Microsoft.VisualStudio.Shell.Interop.Constants.GETOBJS_SELECTED, c, objects);

                        var selectedGraphNode = objects.FirstOrDefault() as ISelectedGraphNode;
                        if (selectedGraphNode != null)
                        {
                            package.SelectedNode = selectedGraphNode;
                            var selectedPackage = selectedGraphNode.Node.GetValue<IVsPackageMetadata>(ReferencesGraphSchema.PackageProperty);
                            if (selectedPackage != null)
                            {
                                var selection = new NuGetPackage
                                {
                                    Id = selectedPackage.Id,
                                    Title = selectedPackage.Title, 
                                    Version = selectedPackage.VersionString,
                                    Authors = string.Join(", ", selectedPackage.Authors),
                                    InstallPath = selectedPackage.InstallPath,
                                };

                                if (this.selectionService == null)
                                {
                                    // Get the service provider form Microsoft.VisualStudio.PlatformUI.HierarchyPivotNavigator
                                    IServiceProvider serviceProvider = pSCNew.AsDynamicReflection()._navigator.ServiceProvider;
                                    this.selectionService = new SelectionService(serviceProvider);
                                }

                                this.selectionService.Select(selection);

                                System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    System.Threading.Thread.Sleep(10);

                                    object selected = selection;
                                    var browsable = ((object)selectedPackage) as IBrowsablePattern;
                                    if (browsable != null)
                                        selected = browsable.GetBrowseObject();

                                    ThreadHelper.Generic.Invoke(() => this.selectionService.Select(selected));
                                });
                            }

                        }
                    }
                }
                catch { }
            }

            return VSConstants.S_OK;
        }
    }
}
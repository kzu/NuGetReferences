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

namespace ClariusLabs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using ClariusLabs.NuGetReferences.Properties;
    using Clide;
    using Clide.Diagnostics;
    using Microsoft.VisualStudio.ExtensionManager;

    /// <summary>
    /// Allows to modify the status of menus and functionality depending on whether 
    /// the user has already opened the devstore to check for the full version or 
    /// not.
    /// </summary>
    internal static class Trial
    {
        private static ITracer tracer = Tracer.Get(typeof(Trial));

        /// <summary>
        /// The number of times we want the menus to be available before disabling for a devstore check.
        /// </summary>
        private const int UsageLimit = 4;

        // At least on the graph node context menu, we get called multiple times on each rigth-click :S
        private const int QueryStatusMultiplier = 2 /* We get called twice for each menu */ * 3 /* And each time we get called 3 times */;
        private const int UsageLimitTrigger = UsageLimit * QueryStatusMultiplier;

        private static int usageCount;

        /// <summary>
        /// Gets or sets a value indicating whether the user must 
        /// check the full paid version on the DevStore before continuing 
        /// to use the extension.
        /// </summary>
        public static bool CheckPending { get; private set; }

        public static void Initialize(ITracer tracer)
        {
            Trial.tracer = tracer;
        }

        /// <summary>
        /// Increments the usage counter for the extension.
        /// </summary>
        [Conditional("TRIAL")]
        public static void IncrementUsage(
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!string.IsNullOrEmpty(memberName))
                tracer.Verbose("{0}({1}): {2} - Increment Usage", sourceFilePath, sourceLineNumber, memberName);

            usageCount++;
            if (usageCount > UsageLimitTrigger)
            {
                CheckPending = true;
            }
        }

        /// <summary>
        /// Resets the usage counter after the user has checked out the full version 
        /// on the devstore.
        /// </summary>
        public static void ResetUsage()
        {
            CheckPending = false;
            usageCount = 0;
        }

        /// <summary>
        /// Verifies that the devstore extension is installed, enabled and 
        /// up to date with regards to an extension-deployed one.
        /// </summary>
        [Conditional("TRIAL")]
        public static void EnsureDevStore(string trialExtensionId, string devStoreExtensionId, string devStoreVsixFileName)
        {
            try
            {
                var extensionManager = ServiceLocator.GlobalProvider.TryGetService<SVsExtensionManager, IVsExtensionManager>();
                if (extensionManager == null)
                    return;

                var trialExtension = default(IInstalledExtension);
                if (!extensionManager.TryGetInstalledExtension(trialExtensionId, out trialExtension))
                    return;

                var vsixPath = Path.Combine(trialExtension.InstallPath, devStoreVsixFileName);
                if (!File.Exists(vsixPath))
                {
                    tracer.Error(Strings.Package.DevStoreVsixNotFound(vsixPath, trialExtension.Header.Name));
                    return;
                }

                var installableDevStore = extensionManager.CreateInstallableExtension(vsixPath);
                if (installableDevStore == null)
                    return;

                var existingDevStore = default(IInstalledExtension);
                if (!extensionManager.TryGetInstalledExtension(devStoreExtensionId, out existingDevStore))
                    tracer.Info(Strings.Package.DevStoreNotInstalled);
                else if (!extensionManager.GetEnabledExtensions().Any(e => e.Header.Identifier == existingDevStore.Header.Identifier))
                    // TODO: auto-enable if user has disabled the devstore ;)
                    extensionManager.Enable(existingDevStore);

                if (existingDevStore != null && existingDevStore.Header.Version < installableDevStore.Header.Version)
                    tracer.Info(Strings.Package.DevStoreOldVersion(existingDevStore.Header.Version, installableDevStore.Header.Version));

                var shouldInstall = existingDevStore == null ||
                    existingDevStore.Header.Version < installableDevStore.Header.Version;

                if (shouldInstall)
                {
                    tracer.Info(Strings.Package.DevStoreInstalling(installableDevStore.Header.Version, vsixPath));
                    if (extensionManager.Install(installableDevStore, false) != RestartReason.None)
                        tracer.Info(Strings.Package.DevStoreRestartNeeded);
                }
            }
            catch (Exception e)
            {
                tracer.Error(e, "Failed to update DevStore extension");
            }
        }
    }
}
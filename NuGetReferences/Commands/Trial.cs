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

namespace ClariusLabs.NuGetReferences.Commands
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
using Clide.Diagnostics;

    /// <summary>
    /// Allows to modify the status of menus and functionality depending on whether 
    /// the user has already opened the devstore to check for the full version or 
    /// not.
    /// </summary>
    internal static class Trial
    {
        /// <summary>
        /// The number of times we want the menus to be available before disabling for a devstore check.
        /// </summary>
        private const int UsageLimit = 3;

        private static ITracer tracer = Tracer.Get(typeof(Trial));
       
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

        public static void Reset()
        {
            CheckPending = false;
            usageCount = 0;
        }
    }
}
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
    using System.ComponentModel.Composition;
    using System.Linq;
    using Clide.Commands;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;

    [Command(Guids.PackageGuid, Guids.CommandSetGuid, CommandIds.CheckFullVersion)]
    public class CheckFullVersion : ICommandExtension
    {
        private IServiceProvider provider;

        [ImportingConstructor]
        public CheckFullVersion([Import(typeof(SVsServiceProvider))] IServiceProvider provider)
        {
            this.provider = provider;
        }

        public void Execute(IMenuCommand command)
        {
            try
            {
                System.Threading.Tasks.Task.Run(() => 
                    provider.GetService<DTE>().ExecuteCommand("Tools.OpenInDevStore", NuGetReferences.Constants.VsixIdentifier));
            }
            catch (Exception)
            {
            }
            finally
            {
                Trial.ResetUsage();
            }
        }

        public void QueryStatus(IMenuCommand command)
        {
            command.Visible = command.Enabled = Trial.CheckPending;
        }

        public string Text { get; set; }
    }
}
namespace ClariusLabs.NuGetReferences
{
    using System;
    using Clide;

    public interface IPackageManagerConsole : IToolWindow
    {
        void Execute(string command);
        bool IsBusy { get; }
        bool IsInitialized { get; }
    }
}
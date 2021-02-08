using System;
using System.Runtime.InteropServices;

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.
    [Guid( "cb5737d7-85df-4165-b7cc-12c35d0436ef" )]
    public interface ICompilerServiceProvider
    {
        Version Version { get; }

        T? GetCompilerService<T>()
            where T : class, ICompilerService;

        event Action<ICompilerServiceProvider> Unloaded;
    }
}
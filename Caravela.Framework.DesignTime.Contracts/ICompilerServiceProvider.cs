using System;

namespace Caravela.Framework.DesignTime.Contracts
{
    public interface ICompilerServiceProvider
    {
        Version Version { get; }
        
        T? GetCompilerService<T>() where T : class, ICompilerService;

        event Action<ICompilerServiceProvider> Unloaded;

    }
}
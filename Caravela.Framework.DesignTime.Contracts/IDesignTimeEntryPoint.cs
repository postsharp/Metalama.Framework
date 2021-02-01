using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.DesignTime.Contracts
{
    public interface IDesignTimeEntryPoint
    {
        Version Version { get; }
        
        T? GetCompilerService<T>() where T : class;

        event Action<IDesignTimeEntryPoint> Unloaded;

    }

    public interface IProjectDesignTimeEntryPoint
    {
        bool TryGetTextSpanClassifier( SemanticModel model, SyntaxNode root, out ITextSpanClassifier classifier );
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.DesignTime.Contracts
{
    public interface IDesignTimeEntryPoint
    {
        bool HandlesProject( Project project );
        
        T? GetService<T>() where T : class;
        event Action<IDesignTimeEntryPoint> Disposed;

    }

    public interface IProjectDesignTimeEntryPoint
    {
        bool TryProvideClassifiedSpans( SemanticModel model, SyntaxNode root, out ITextSpanClassifier classifier );
    }
}
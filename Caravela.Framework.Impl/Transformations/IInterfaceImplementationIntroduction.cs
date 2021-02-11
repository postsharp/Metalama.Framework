using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IInterfaceImplementationIntroduction : ISyntaxTreeIntroduction, IObservableTransformation 
    {
        
        IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations();

    }
}
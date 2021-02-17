using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IInterfaceImplementationIntroduction : ISyntaxTreeTransformation, IObservableTransformation
    {

        IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations();
    }
}
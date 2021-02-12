using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IInterfaceImplementationIntroduction : ISyntaxTreeIntroduction, IObservableTransformation
    {

        IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations();
    }
}
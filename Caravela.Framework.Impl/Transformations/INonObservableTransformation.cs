using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represent a transformation that is not observable by the aspects running after the aspect
    /// that provided the transformation.,
    /// </summary>
    internal interface INonObservableTransformation
    {
        
    }
}

// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IIntroducedInterface : ISyntaxTreeTransformation, IObservableTransformation
    {
        IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations();
    }
}
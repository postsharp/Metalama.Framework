// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    internal interface IInterfaceImplementationIntroduction : ISyntaxTreeTransformation, IObservableTransformation
    {

        IEnumerable<BaseTypeSyntax> GetIntroducedInterfaceImplementations();
    }
}
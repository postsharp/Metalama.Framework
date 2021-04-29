// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeParameterInfo : ParameterInfo, IReflectionMockCodeElement
    {
        public IParameterSymbol ParameterSymbol { get; }

        public ICodeElement DeclaringMember { get; }

        public CompileTimeParameterInfo( IParameterSymbol parameterSymbol, ICodeElement declaringMember )
        {
            this.ParameterSymbol = parameterSymbol;
            this.DeclaringMember = declaringMember;
        }

        ISymbol IReflectionMockCodeElement.Symbol => this.ParameterSymbol;
    }
}
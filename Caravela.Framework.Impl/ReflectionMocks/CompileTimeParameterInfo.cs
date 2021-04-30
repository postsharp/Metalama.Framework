// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeParameterInfo : ParameterInfo, ICompileTimeReflectionObject
    {
        public IParameterSymbol ParameterSymbol { get; }

        public ICodeElement DeclaringMember { get; }

        private CompileTimeParameterInfo( IParameterSymbol parameterSymbol, ICodeElement declaringMember )
        {
            this.ParameterSymbol = parameterSymbol.AssertNotNull();
            this.DeclaringMember = declaringMember.AssertNotNull();
        }

        public static ParameterInfo Create( IParameterSymbol parameterSymbol, ICodeElement declaringMember )
            => new CompileTimeParameterInfo( parameterSymbol, declaringMember );

        ISymbol ICompileTimeReflectionObject.Symbol => this.ParameterSymbol;
    }
}
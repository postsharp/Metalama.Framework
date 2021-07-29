// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Caravela.Framework.Code.SpecialType;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class RoslynType<T> : ITypeInternal
        where T : ITypeSymbol
    {
        protected CompilationModel Compilation { get; }

        public T Symbol { get; }

        protected RoslynType( T symbol, CompilationModel compilation )
        {
            this.Compilation = compilation;
            this.Symbol = symbol;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        public abstract TypeKind TypeKind { get; }

        public SpecialType SpecialType => this.Symbol.SpecialType.ToOurSpecialType();

        public Type ToType() => CompileTimeType.Create( this );

        ICompilation ICompilationElement.Compilation => this.Compilation;

        ITypeSymbol? ISdkType.TypeSymbol => this.Symbol;

        public bool Equals( IType other ) => this.Symbol.Equals( ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this.Symbol.ToString();
    }
}
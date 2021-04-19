// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class MethodBase : Member, IMethodBase
    {
        public override ISymbol Symbol => this.MethodSymbol;

        internal IMethodSymbol MethodSymbol { get; }

        public MethodBase( IMethodSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this.MethodSymbol = symbol;
        }

        [Memo]
        public IMethodList LocalFunctions
            => new MethodList(
                this,
                this.MethodSymbol.DeclaringSyntaxReferences
                    .Select( r => r.GetSyntax() )
                    /* don't descend into nested local functions */
                    .SelectMany( n => n.DescendantNodes( c => c == n || c is not LocalFunctionStatementSyntax ) )
                    .OfType<LocalFunctionStatementSyntax>()
                    .Select( f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel( f.SyntaxTree ).GetDeclaredSymbol( f )! )
                    .Select( s => new MemberLink<IMethod>( s ) ) );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.MethodSymbol.Parameters.Select( p => CodeElementLink.FromSymbol<IParameter>( p ) ) );

        MethodKind IMethodBase.MethodKind
            => this.MethodSymbol.MethodKind switch
            {
                Microsoft.CodeAnalysis.MethodKind.Ordinary => MethodKind.Default,
                Microsoft.CodeAnalysis.MethodKind.Constructor => MethodKind.Constructor,
                Microsoft.CodeAnalysis.MethodKind.StaticConstructor => MethodKind.StaticConstructor,
                Microsoft.CodeAnalysis.MethodKind.Destructor => MethodKind.Finalizer,
                Microsoft.CodeAnalysis.MethodKind.PropertyGet => MethodKind.PropertyGet,
                Microsoft.CodeAnalysis.MethodKind.PropertySet => MethodKind.PropertySet,
                Microsoft.CodeAnalysis.MethodKind.EventAdd => MethodKind.EventAdd,
                Microsoft.CodeAnalysis.MethodKind.EventRemove => MethodKind.EventRemove,
                Microsoft.CodeAnalysis.MethodKind.EventRaise => MethodKind.EventRaise,
                Microsoft.CodeAnalysis.MethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
                Microsoft.CodeAnalysis.MethodKind.Conversion => MethodKind.ConversionOperator,
                Microsoft.CodeAnalysis.MethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
                Microsoft.CodeAnalysis.MethodKind.LocalFunction => MethodKind.LocalFunction,
                Microsoft.CodeAnalysis.MethodKind.AnonymousFunction or
                    Microsoft.CodeAnalysis.MethodKind.BuiltinOperator or
                    Microsoft.CodeAnalysis.MethodKind.DelegateInvoke or
                    Microsoft.CodeAnalysis.MethodKind.ReducedExtension or
                    Microsoft.CodeAnalysis.MethodKind.DeclareMethod or
                    Microsoft.CodeAnalysis.MethodKind.FunctionPointerSignature => throw new NotSupportedException(),
                _ => throw new InvalidOperationException()
            };

        public override string ToString() => this.MethodSymbol.ToString();
    }
}
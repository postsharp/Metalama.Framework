// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;
using MethodKind = Caravela.Framework.Code.MethodKind;
using SymbolMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class MethodBase : Member, IMethodBase
    {
        public override ISymbol Symbol => this.MethodSymbol;

        internal IMethodSymbol MethodSymbol { get; }

        [Memo]
        public override IDeclaration? ContainingDeclaration
            => this.Symbol switch
            {
                IMethodSymbol method when
                    method.MethodKind == SymbolMethodKind.PropertyGet
                    || method.MethodKind == SymbolMethodKind.PropertySet
                    || method.MethodKind == SymbolMethodKind.EventAdd
                    || method.MethodKind == SymbolMethodKind.EventRemove
                    || method.MethodKind == SymbolMethodKind.EventRaise
                    => this.Compilation.Factory.GetDeclaration( method.AssociatedSymbol.AssertNotNull() ),
                _ => base.ContainingDeclaration
            };

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
                    .Select( s => new MemberRef<IMethod>( s ) ) );

        [Memo]
        public IParameterList Parameters
            => new ParameterList(
                this,
                this.MethodSymbol.Parameters.Select( p => DeclarationRef.FromSymbol<IParameter>( p ) ) );

        MethodKind IMethodBase.MethodKind
            => this.MethodSymbol.MethodKind switch
            {
                SymbolMethodKind.Ordinary => MethodKind.Default,
                SymbolMethodKind.Constructor => MethodKind.Constructor,
                SymbolMethodKind.StaticConstructor => MethodKind.StaticConstructor,
                SymbolMethodKind.Destructor => MethodKind.Finalizer,
                SymbolMethodKind.PropertyGet => MethodKind.PropertyGet,
                SymbolMethodKind.PropertySet => MethodKind.PropertySet,
                SymbolMethodKind.EventAdd => MethodKind.EventAdd,
                SymbolMethodKind.EventRemove => MethodKind.EventRemove,
                SymbolMethodKind.EventRaise => MethodKind.EventRaise,
                SymbolMethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
                SymbolMethodKind.Conversion => MethodKind.ConversionOperator,
                SymbolMethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
                SymbolMethodKind.LocalFunction => MethodKind.LocalFunction,
                SymbolMethodKind.AnonymousFunction or
                    SymbolMethodKind.BuiltinOperator or
                    SymbolMethodKind.DelegateInvoke or
                    SymbolMethodKind.ReducedExtension or
                    SymbolMethodKind.DeclareMethod or
                    SymbolMethodKind.FunctionPointerSignature => throw new NotSupportedException(),
                _ => throw new InvalidOperationException()
            };

        public abstract System.Reflection.MethodBase ToMethodBase();

        public override MemberInfo ToMemberInfo() => this.ToMethodBase();

        public override string ToString() => this.MethodSymbol.ToString();
    }
}
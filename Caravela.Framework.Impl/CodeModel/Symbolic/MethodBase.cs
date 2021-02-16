using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal abstract class MethodBase : Member, IMethodBase
    {
        protected internal override ISymbol Symbol => this.MethodSymbol;

        internal IMethodSymbol MethodSymbol { get; }

        public MethodBase( IMethodSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this.MethodSymbol = symbol;
        }

        [Memo]
        public IReadOnlyList<IMethod> LocalFunctions =>
            this.MethodSymbol.DeclaringSyntaxReferences
                .Select( r => r.GetSyntax() )
                /* don't descend into nested local functions */
                .SelectMany( n => n.DescendantNodes( descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax ) )
                .OfType<LocalFunctionStatementSyntax>()
                .Select( f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel( f.SyntaxTree ).GetDeclaredSymbol( f )! )
                .Select( s => this.Compilation.Factory.GetMethod( s ) )
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IParameter> Parameters => this.MethodSymbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableArray<IParameter>();


        MethodKind IMethodBase.MethodKind => this.MethodSymbol.MethodKind switch
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
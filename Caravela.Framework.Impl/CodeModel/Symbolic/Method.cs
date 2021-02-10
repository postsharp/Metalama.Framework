using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Method : Member, IMethod
    {
        protected internal override ISymbol Symbol => this.MethodSymbol;


        internal IMethodSymbol MethodSymbol { get; }

        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base(compilation)
        {
            this.MethodSymbol = symbol;
        }

        [Memo]
        public IParameter? ReturnParameter => ((IMethod) this).MethodKind is MethodKind.Constructor ? null : new MethodReturnParameter( this );

        [Memo]
        public IType ReturnType => this.Compilation.GetIType( this.MethodSymbol.ReturnType );

        [Memo]
        public IReadOnlyList<IMethod> LocalFunctions =>
            this.MethodSymbol.DeclaringSyntaxReferences
                .Select( r => r.GetSyntax() )
                /* don't descend into nested local functions */
                .SelectMany( n => n.DescendantNodes( descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax ) )
                .OfType<LocalFunctionStatementSyntax>()
                .Select( f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel( f.SyntaxTree ).GetDeclaredSymbol( f )! )
                .Select( s => this.Compilation.GetMethod( s ) )
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IParameter> Parameters => this.MethodSymbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableArray<IParameter>();

        [Memo]
        public IReadOnlyList<IGenericParameter> GenericParameters =>
            this.MethodSymbol.TypeParameters.Select( tp => this.Compilation.GetGenericParameter( tp ) ).ToImmutableArray();

        MethodKind IMethod.MethodKind => this.MethodSymbol.MethodKind switch
        {
            RoslynMethodKind.Ordinary => MethodKind.Default,
            RoslynMethodKind.Constructor => MethodKind.Constructor,
            RoslynMethodKind.StaticConstructor => MethodKind.StaticConstructor,
            RoslynMethodKind.Destructor => MethodKind.Finalizer,
            RoslynMethodKind.PropertyGet => MethodKind.PropertyGet,
            RoslynMethodKind.PropertySet => MethodKind.PropertySet,
            RoslynMethodKind.EventAdd => MethodKind.EventAdd,
            RoslynMethodKind.EventRemove => MethodKind.EventRemove,
            RoslynMethodKind.EventRaise => MethodKind.EventRaise,
            RoslynMethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
            RoslynMethodKind.Conversion => MethodKind.ConversionOperator,
            RoslynMethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
            RoslynMethodKind.LocalFunction => MethodKind.LocalFunction,
            RoslynMethodKind.AnonymousFunction or
            RoslynMethodKind.BuiltinOperator or
            RoslynMethodKind.DelegateInvoke or
            RoslynMethodKind.ReducedExtension or
            RoslynMethodKind.DeclareMethod or
            RoslynMethodKind.FunctionPointerSignature => throw new NotSupportedException(),
            _ => throw new InvalidOperationException()
        };


      

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        [Memo]
        public INamedType? DeclaringType => this.MethodSymbol.ContainingType == null ? null : this.Compilation.GetNamedType( this.MethodSymbol.ContainingType );

        public override string ToString() => this.MethodSymbol.ToString();

        internal sealed class MethodReturnParameter : ReturnParameter
        {
            public Method Method { get; }

            public MethodReturnParameter( Method method )
            {
                this.Method = method;
            }

            protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.Method.MethodSymbol.RefKind;

            public override IType Type => this.Method.ReturnType;

            public bool Equals( ICodeElement other ) => other is MethodReturnParameter methodReturnParameter &&
                                                                 SymbolEqualityComparer.Default.Equals( this.Method.Symbol,
                                                                     methodReturnParameter.Method.Symbol );

            public override ICodeElement? ContainingElement => this.Method;

            public override IReadOnlyList<IAttribute> Attributes 
                => this.Method.MethodSymbol.GetReturnTypeAttributes()
                .Select( a => new Attribute( a, this.Method.Compilation, this ) )
                .ToImmutableArray();

        }
    }
}

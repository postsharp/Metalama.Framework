using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class SourceMethod : Method, ISourceCodeElement
    {
        private readonly IMethodSymbol _symbol;

        public SourceCompilationModel Compilation { get; }

        public SourceMethod( SourceCompilationModel compilation, IMethodSymbol symbol )
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }
        public ISymbol Symbol => this._symbol;

        public override bool IsStatic => this._symbol.IsStatic;

        public override bool IsVirtual => this._symbol.IsVirtual;

        public override string Name => this._symbol.Name;

        [Memo]
        public override Parameter? ReturnParameter => this.MethodKind == Code.MethodKind.Constructor ? null : new SourceMethodReturnParameter( this );

        [Memo]
        public override ITypeInternal ReturnType => this.Compilation.SymbolMap.GetIType( this._symbol.ReturnType );

        [Memo]
        public override IReadOnlyList<GenericParameter> GenericParameters =>
            this._symbol.TypeParameters.Select( tp => this.Compilation.SymbolMap.GetGenericParameter( tp ) ).ToImmutableList();

        [Memo]
        public override IReadOnlyList<Parameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableList<Parameter>();

        [Memo]
        public override CodeElement? ContainingElement => this._symbol.ContainingSymbol switch
        {
            INamedTypeSymbol type => this.Compilation.SymbolMap.GetNamedType( type ),
            IMethodSymbol method => this.Compilation.SymbolMap.GetMethod( method ),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override NamedType? DeclaringType => this._symbol.ContainingType == null ? null : this.Compilation.SymbolMap.GetNamedType( this._symbol.ContainingType );

        public override Code.MethodKind MethodKind => this._symbol.MethodKind switch
        {
            RoslynMethodKind.Ordinary => Code.MethodKind.Default,
            RoslynMethodKind.Constructor => Code.MethodKind.Constructor,
            RoslynMethodKind.StaticConstructor => Code.MethodKind.StaticConstructor,
            RoslynMethodKind.Destructor => Code.MethodKind.Finalizer,
            RoslynMethodKind.PropertyGet => Code.MethodKind.PropertyGet,
            RoslynMethodKind.PropertySet => Code.MethodKind.PropertySet,
            RoslynMethodKind.EventAdd => Code.MethodKind.EventAdd,
            RoslynMethodKind.EventRemove => Code.MethodKind.EventRemove,
            RoslynMethodKind.EventRaise => Code.MethodKind.EventRaise,
            RoslynMethodKind.ExplicitInterfaceImplementation => Code.MethodKind.ExplicitInterfaceImplementation,
            RoslynMethodKind.Conversion => Code.MethodKind.ConversionOperator,
            RoslynMethodKind.UserDefinedOperator => Code.MethodKind.UserDefinedOperator,
            RoslynMethodKind.LocalFunction => Code.MethodKind.LocalFunction,
            RoslynMethodKind.AnonymousFunction or
            RoslynMethodKind.BuiltinOperator or
            RoslynMethodKind.DelegateInvoke or
            RoslynMethodKind.ReducedExtension or
            RoslynMethodKind.DeclareMethod or
            RoslynMethodKind.FunctionPointerSignature => throw new NotSupportedException(),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override IReadOnlyList<Method> LocalFunctions =>
            this._symbol.DeclaringSyntaxReferences
                .Select( r => r.GetSyntax() )
                /* don't descend into nested local functions */
                .SelectMany( n => n.DescendantNodes( descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax ) )
                .OfType<LocalFunctionStatementSyntax>()
                .Select( f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel( f.SyntaxTree ).GetDeclaredSymbol( f )! )
                .Select( s => this.Compilation.SymbolMap.GetMethod( s ) )
                .ToImmutableList();

        [Memo]
        public override IReadOnlyList<Attribute> Attributes => this._symbol.GetAttributes().Select( a => new SourceAttribute( a, this.Compilation.SymbolMap ) ).ToImmutableList();

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public override string ToString() => this._symbol.ToString();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public override bool Equals( ICodeElement other )
        {
            throw new NotImplementedException();
        }

        internal sealed class SourceMethodReturnParameter : ReturnParameter
        {
            public SourceMethod Method { get; }

            public SourceMethodReturnParameter( SourceMethod method )
            {
                this.Method = method;
            }

            protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.Method._symbol.RefKind;

            public override IType Type => this.Method.ReturnType;

            public override CodeElement? ContainingElement => this.Method;

            [Memo]
            public override IImmutableList<IAttribute> Attributes =>
                this.Method._symbol.GetReturnTypeAttributes().Select( a => new Attribute( a, this.Method.SymbolMap ) ).ToImmutableReactive();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class Method : Member, IMethod
    {
        protected internal override ISymbol Symbol => this.MethodSymbol;

        internal IMethodSymbol MethodSymbol { get; }

        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this.MethodSymbol = symbol;
        }

        [Memo] public IParameter? ReturnParameter => ((IMethod) this).MethodKind is MethodKind.Constructor ? null : new MethodReturnParameter( this );

        [Memo] public IType ReturnType => this.Compilation.GetIType( this.MethodSymbol.ReturnType );

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

            public override bool Equals( ICodeElement other ) => other is MethodReturnParameter methodReturnParameter &&
                                                                 SymbolEqualityComparer.Default.Equals( this.Method.Symbol,
                                                                     methodReturnParameter.Method.Symbol );

            public override ICodeElement? ContainingElement => this.Method;

            public override IReadOnlyList<IAttribute> Attributes
                => this.Method.MethodSymbol.GetReturnTypeAttributes()
                    .Select( a => new Attribute( a, this.Method.Compilation, this ) )
                    .ToImmutableArray();
        }

        [Memo]
        public IReadOnlyList<IType> GenericArguments =>
            this.MethodSymbol.TypeArguments.Select( this.Compilation.GetIType ).ToImmutableList();

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || this.DeclaringType?.IsOpenGeneric == true;

        public object Invoke( object? instance, params object[] args ) => new MethodInvocation( this ).Invoke( instance, args );

        public bool HasBase => true;

        public IMethodInvocation Base => new MethodInvocation( this ).Base;

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            var symbolWithGenericArguments = this.MethodSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        private readonly struct MethodInvocation : IMethodInvocation
        {
            private readonly Method _method;

            public MethodInvocation( Method method )
            {
                this._method = method;
            }

            public bool HasBase => true;

            public IMethodInvocation Base => throw new InvalidOperationException();

            public object Invoke( object? instance, params object[] args )
            {
                if ( this._method.IsOpenGeneric )
                {
                    throw new CaravelaException( GeneralDiagnosticDescriptors.CantAccessOpenGenericMember, this._method );
                }

                var name = this._method.GenericArguments.Any()
                    ? (SimpleNameSyntax) this._method.Compilation.SyntaxGenerator.GenericName( this._method.Name,
                        this._method.GenericArguments.Select( a => a.GetSymbol() ) )
                    : IdentifierName( this._method.Name );
                var arguments = this._method.GetArguments( this._method.Parameters, args );

                if ( ((IMethod) this._method).MethodKind == MethodKind.LocalFunction )
                {
                    if ( this._method.ContainingElement != TemplateContext.target.Method )
                    {
                        throw new CaravelaException( GeneralDiagnosticDescriptors.CantInvokeLocalFunctionFromAnotherMethod, this._method,
                            TemplateContext.target.Method, this._method.ContainingElement );
                    }

                    var instanceExpression = (RuntimeExpression) instance!;

                    if ( !instanceExpression.IsNull )
                    {
                        throw new CaravelaException( GeneralDiagnosticDescriptors.CantProvideInstanceForLocalFunction, this._method );
                    }

                    return new DynamicMetaMember(
                        InvocationExpression( name ).AddArgumentListArguments( arguments ),
                        this._method.ReturnType );
                }

                var receiver = this._method.GetReceiverSyntax( instance! );

                return new DynamicMetaMember(
                    InvocationExpression( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name ) )
                        .AddArgumentListArguments( arguments ),
                    this._method.ReturnType );
            }
        }

        public override bool IsReadOnly => this.MethodSymbol.IsReadOnly;

        public override bool IsAsync => this.MethodSymbol.IsAsync;
    }
}

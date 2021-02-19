using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Method : MethodBase, IMethodInternal, IMemberLink<IMethod>
    {

        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
        }

        [Memo]
        public IParameter ReturnParameter => new MethodReturnParameter( this );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodSymbol.ReturnType );

        [Memo]
        public IGenericParameterList GenericParameters =>
            new GenericParameterList(
                this.MethodSymbol.TypeParameters.Select( tp => CodeElementLink.FromSymbol<IGenericParameter>(tp) ),
                this.Compilation );

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        internal sealed class MethodReturnParameter : ReturnParameter
        {
            public Method DeclaringMethod { get; }

            public override IMember DeclaringMember => this.DeclaringMethod;

            public MethodReturnParameter( Method declaringMethod )
            {
                this.DeclaringMethod = declaringMethod;
            }

            protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.DeclaringMethod.MethodSymbol.RefKind;

            public override IType ParameterType => this.DeclaringMethod.ReturnType;

            public override bool Equals( ICodeElement other ) => other is MethodReturnParameter methodReturnParameter &&
                                                                 SymbolEqualityComparer.Default.Equals( this.DeclaringMethod.Symbol, methodReturnParameter.DeclaringMethod.Symbol );

            [Memo]
            public override IAttributeList Attributes
                => new AttributeList(
                    this.DeclaringMethod.MethodSymbol.GetReturnTypeAttributes()
                        .Select( a => new AttributeLink( a, new CodeElementLink<ICodeElement>( this ) ) ),
                    (CompilationModel) this.Compilation );

        }

        [Memo]
        public IReadOnlyList<IType> GenericArguments =>
            this.MethodSymbol.TypeArguments.Select( this.Compilation.Factory.GetIType ).ToImmutableList();

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || this.DeclaringType?.IsOpenGeneric == true;

        public object Invoke( object? instance, params object[] args ) => new MethodInvocation( this ).Invoke( instance, args );

        public bool HasBase => true;

        public IMethodInvocation Base => new MethodInvocation( this ).Base;

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            var symbolWithGenericArguments = this.MethodSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        IReadOnlyList<ISymbol> IMethodInternal.LookupSymbols()
        {
            if ( this.Symbol.DeclaringSyntaxReferences.Length == 0 )
            {
                throw new InvalidOperationException();
            }

            var syntaxReference = this.Symbol.DeclaringSyntaxReferences[0];
            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );
            var methodBodyNode = ((MethodDeclarationSyntax) syntaxReference.GetSyntax()).Body;
            var lookupPosition = methodBodyNode != null ? methodBodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition );
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
                    throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember, this._method );
                }

                var name = this._method.GenericArguments.Any()
                    ? (SimpleNameSyntax) this._method.Compilation.SyntaxGenerator.GenericName( 
                        this._method.Name,
                        this._method.GenericArguments.Select( a => a.GetSymbol() ) )
                    : IdentifierName( this._method.Name );
                var arguments = this._method.GetArguments( this._method.Parameters, RuntimeExpression.FromDynamic( args ) );

                if ( ((IMethod) this._method).MethodKind == MethodKind.LocalFunction )
                {
                    var instanceExpression = RuntimeExpression.FromDynamic( instance );

                    if ( instanceExpression != null )
                    {
                        throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction, this._method );
                    }

                    return new DynamicMember(
                        InvocationExpression( name ).AddArgumentListArguments( arguments ),
                        this._method.ReturnType,
                        false );
                }

                var receiver = this._method.GetReceiverSyntax( RuntimeExpression.FromDynamic( instance! ) );

                return new DynamicMember(
                    InvocationExpression( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name ) )
                        .AddArgumentListArguments( arguments ),
                    this._method.ReturnType,
                    false );
            }
        }

        public override bool IsReadOnly => this.MethodSymbol.IsReadOnly;

        public override bool IsAsync => this.MethodSymbol.IsAsync;
        IMethod ICodeElementLink<IMethod>.GetForCompilation( CompilationModel compilation ) => this.GetForCompilation<IMethod>( compilation );
    }
}

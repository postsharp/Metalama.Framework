using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Method : CodeElement, IMethod
    {
        private readonly IMethodSymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;

        internal override SourceCompilation Compilation { get; }

        public Method(IMethodSymbol symbol, SourceCompilation compilation)
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        [Memo]
        public IParameter? ReturnParameter => ((IMethod)this).Kind is MethodKind.Constructor ? null : new MethodReturnParameter(this);

        [Memo]
        public IType ReturnType => this.SymbolMap.GetIType( this._symbol.ReturnType);

        [Memo]
        public IImmutableList<IMethod> LocalFunctions =>
            this._symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                // don't descend into nested local functions
                .SelectMany(n => n.DescendantNodes(descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax))
                .OfType<LocalFunctionStatementSyntax>()
                .Select(f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel(f.SyntaxTree).GetDeclaredSymbol(f)!)
                .Select(s => this.SymbolMap.GetMethod(s))
                .ToImmutableList();

        [Memo]
        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableList<IParameter>();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters =>
            this._symbol.TypeParameters.Select( this.SymbolMap.GetGenericParameter ).ToImmutableList();

        [Memo]
        public IImmutableList<IType> GenericArguments =>
            this._symbol.TypeArguments.Select( this.SymbolMap.GetIType ).ToImmutableList();

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || this.DeclaringType?.IsOpenGeneric == true;

        MethodKind IMethod.Kind => this._symbol.MethodKind switch
        {
            RoslynMethodKind.Ordinary => MethodKind.Ordinary,
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

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        [Memo]
        public override ICodeElement ContainingElement => this._symbol.ContainingSymbol switch
        {
            INamedTypeSymbol type => this.SymbolMap.GetNamedType(type),
            IMethodSymbol method => this.SymbolMap.GetMethod(method),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Method;

        public bool IsVirtual => this._symbol.IsVirtual;

        [Memo]
        public INamedType? DeclaringType => this._symbol.ContainingType == null ? null : this.SymbolMap.GetNamedType( this._symbol.ContainingType );

        public object Invoke( object? instance, params object[] args ) => new MethodInvocation( this ).Invoke( instance, args );

        public bool HasBase => true;

        public IMethodInvocation Base => new MethodInvocation( this ).Base;

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            var symbolWithGenericArguments = this._symbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        public override string ToString() => this._symbol.ToString();

        internal sealed class MethodReturnParameter : ReturnParameter
        {
            public Method Method { get; }

            public MethodReturnParameter( Method method ) => this.Method = method;

            protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.Method._symbol.RefKind;

            public override IType Type => this.Method.ReturnType;

            public override ICodeElement? ContainingElement => this.Method;

            [Memo]
            public override IReactiveCollection<IAttribute> Attributes =>
                this.Method._symbol.GetReturnTypeAttributes().Select( a => new Attribute( a, this.Method.SymbolMap ) ).ToImmutableReactive();
        }

        private readonly struct MethodInvocation : IMethodInvocation
        {
            private readonly Method _method;

            public MethodInvocation( Method method ) => this._method = method;

            public bool HasBase => true;

            public IMethodInvocation Base => throw new NotImplementedException();

            public object Invoke( object? instance, params object[] args )
            {
                if ( this._method.IsOpenGeneric )
                    throw new CaravelaException(GeneralDiagnosticDescriptors.CantAccessOpenGenericMember, this._method);

                var name = this._method.GenericArguments.Any()
                    ? (SimpleNameSyntax) this._method.Compilation.SyntaxGenerator.GenericName( this._method.Name, this._method.GenericArguments.Select( a => a.GetSymbol() ) )
                    : IdentifierName( this._method.Name );
                var arguments = this._method.GetArguments( this._method.Parameters, args );

                if ( ((IMethod) this._method).Kind == MethodKind.LocalFunction )
                {
                    if ( this._method.ContainingElement != TemplateContext.target.Method )
                        throw new CaravelaException( GeneralDiagnosticDescriptors.CantInvokeLocalFunctionFromAnotherMethod, this._method, TemplateContext.target.Method, this._method.ContainingElement );

                    var instanceExpression = (RuntimeExpression) instance!;

                    if ( !instanceExpression.IsNull )
                        throw new CaravelaException( GeneralDiagnosticDescriptors.CantProvideInstanceForLocalFunction, this._method );


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
    }
}

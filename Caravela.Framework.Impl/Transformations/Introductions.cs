using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Linq;
using Caravela.Framework.Advices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CodeAnalysis = Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Transformations
{

    abstract class IntroducedElement : Transformation, ICodeElement, IToSyntax
    {
        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public abstract CodeElementKind ElementKind { get; }

        public IntroducedElement( IAdvice advice ) : base( advice )
        {
        }

        public abstract CSharpSyntaxNode GetSyntaxNode();
        public abstract IEnumerable<CSharpSyntaxNode> GetSyntaxNodes();

        public abstract MemberDeclarationSyntax GetDeclaration();

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null );
    }

    abstract class IntroducedMember : IntroducedElement, IMember
    {
        public INamedType TargetDeclaration { get; }

        public abstract string Name { get; }
        public abstract bool IsStatic { get; }
        public abstract bool IsVirtual { get; }
        public abstract INamedType? DeclaringType { get; }

        public IntroducedMember(IAdvice advice, INamedType targetDeclaration) : base(advice)
        {
            this.TargetDeclaration = targetDeclaration;
        }
    }

    sealed class IntroducedMethod : IntroducedMember, IMethod
    {
        private readonly IntroductionScope? _scope;
        private readonly string? _name;
        private readonly bool? _isStatic;
        private readonly bool? _isVirtual;
        private readonly Visibility? _visibility;

        public override ICodeElement? ContainingElement { get; }

        public IMethod TemplateMethod { get; }
        
        [Memo]
        public IParameter? ReturnParameter => this.TemplateMethod!.ReturnParameter;

        public IType ReturnType => this.TemplateMethod!.ReturnType;

        [Memo]
        public IImmutableList<IMethod> LocalFunctions => this.TemplateMethod!.LocalFunctions;

        [Memo]
        public IImmutableList<IParameter> Parameters => this.TemplateMethod!.Parameters!.Select(x => (IParameter)new IntroducedParameter(this.Advice, this, x)).ToImmutableList();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters => this.TemplateMethod!.GenericParameters!.Select( x => (IGenericParameter) new IntroducedGenericParameter( this.Advice, this, x ) ).ToImmutableList();

        public Code.MethodKind MethodKind => this.TemplateMethod!.MethodKind;

        public override string Name => this._name ?? this.TemplateMethod!.Name;

        public override bool IsStatic => this._isStatic ?? this.TemplateMethod!.IsStatic;

        public override bool IsVirtual => this._isVirtual ?? this.TemplateMethod!.IsVirtual;

        public override INamedType? DeclaringType => this.TemplateMethod!.DeclaringType;

        public override IReactiveCollection<IAttribute> Attributes => this.TemplateMethod!.Attributes;

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public IntroducedMethod( IAdvice advice, INamedType targetType, IMethod templateMethod, IntroductionScope? scope, string? name, bool? isStatic, bool? isVirtual, Visibility? visibility )
            : base(advice, targetType)
        {
            this.ContainingElement = targetType;
            this.TemplateMethod = templateMethod;
            this._scope = scope;
            this._name = name;
            this._isStatic = isStatic;
            this._isVirtual = isVirtual;
            this._visibility = visibility;
        }

        //TODO: Memo for methods?
        private MemberDeclarationSyntax declaration;

        public override MemberDeclarationSyntax GetDeclaration()
        {
            if ( this.declaration != null )
                return this.declaration;

            var templateSyntax = (MethodDeclarationSyntax) this.TemplateMethod!.GetSyntaxNode()!;

            this.declaration = MethodDeclaration(
                List<AttributeListSyntax>(), // TODO: Copy some attributes?
                templateSyntax.Modifiers, 
                templateSyntax.ReturnType, 
                templateSyntax.ExplicitInterfaceSpecifier!, 
                templateSyntax.Identifier, 
                templateSyntax.TypeParameterList!,
                templateSyntax.ParameterList, 
                templateSyntax.ConstraintClauses, 
                Block( ThrowStatement( ObjectCreationExpression( ParseTypeName("System.NotImplementedException")))), 
                null, 
                templateSyntax.SemicolonToken
                );

            return this.declaration;
        }

        public override CSharpSyntaxNode GetSyntaxNode() => this.GetDeclaration();
        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => new[] { (CSharpSyntaxNode) this.GetDeclaration() };

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null )
        {
            throw new NotImplementedException();
        }
    }

    sealed class IntroducedParameter : IntroducedElement, IParameter
    {
        private readonly IParameter _template;

        public Code.RefKind RefKind => this._template.RefKind;

        public bool IsByRef => this._template.IsByRef;

        public bool IsRef => this._template.IsRef;

        public bool IsOut => this._template.IsOut;

        public IType Type => this._template.Type;

        public string? Name => this._template.Name;

        public int Index => this._template.Index;

        public bool HasDefaultValue => this._template.HasDefaultValue;

        public object? DefaultValue => this._template.DefaultValue;

        public override ICodeElement? ContainingElement { get; }

        public override IReactiveCollection<IAttribute> Attributes => this._template.Attributes;

        public override CodeElementKind ElementKind => this._template.ElementKind;

        public IntroducedParameter( IAdvice advice, IMethod containingMethod, IParameter template ) : base(advice)
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

        public override MemberDeclarationSyntax GetDeclaration()
        {
            throw new NotSupportedException();
        }

        public override CSharpSyntaxNode GetSyntaxNode() => this._template.GetSyntaxNode();
        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => this._template.GetSyntaxNodes();

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null )
        {
            throw new NotImplementedException();
        }
    }

    sealed class IntroducedGenericParameter : IntroducedElement, IGenericParameter
    {
        private readonly IGenericParameter _template;

        public string Name => this._template.Name;

        public int Index => this._template.Index;

        public IImmutableList<IType> TypeConstraints => this._template.TypeConstraints;

        public bool IsCovariant => this._template.IsCovariant;

        public bool IsContravariant => this._template.IsContravariant;

        public bool HasDefaultConstructorConstraint => this._template.HasDefaultConstructorConstraint;

        public bool HasReferenceTypeConstraint => this._template.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this._template.HasNonNullableValueTypeConstraint;

        Code.TypeKind IType.TypeKind => Code.TypeKind.GenericParameter;

        public override ICodeElement? ContainingElement { get; }

        public override IReactiveCollection<IAttribute> Attributes => this._template.Attributes;

        public override CodeElementKind ElementKind => CodeElementKind.GenericParameter;

        public IntroducedGenericParameter( IAdvice advice, IMethod containingMethod, IGenericParameter template ) : base( advice )
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

        public override MemberDeclarationSyntax GetDeclaration()
        {
            throw new NotSupportedException();
        }

        public override CSharpSyntaxNode GetSyntaxNode() => this._template.GetSyntaxNode()!;
        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => this._template.GetSyntaxNodes()!;

        public bool Is( IType other )
        {
            return this._template.Is( other );
        }

        public bool Is( Type other )
        {
            return this._template.Is( other );
        }

        public IArrayType MakeArrayType( int rank = 1 )
        {
            return this._template.MakeArrayType( rank );
        }

        public IPointerType MakePointerType()
        {
            return this._template.MakePointerType();
        }

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null )
        {
            throw new NotImplementedException();
        }
    }
}

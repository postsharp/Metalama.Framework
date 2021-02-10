using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Sdk;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Attribute = Caravela.Framework.Impl.CodeModel.Attribute;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class IntroducedElement : Transformation, ICodeElement, IToSyntax, IMemberInternal
    {
        public abstract CodeElement? ContainingElement { get; }

        public abstract IImmutableList<Attribute> Attributes { get; }

        public abstract CodeElementKind ElementKind { get; }

        public IntroducedElement( IAdvice advice ) : base( advice )
        {
        }

        public abstract CSharpSyntaxNode GetSyntaxNode();

        public abstract IEnumerable<CSharpSyntaxNode> GetSyntaxNodes();

        public abstract MemberDeclarationSyntax GetDeclaration();

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );
    }

    internal abstract class IntroducedMember : IntroducedElement, IMember
    {
        public INamedType TargetDeclaration { get; }

        public abstract string Name { get; }

        public abstract bool IsStatic { get; }

        public abstract bool IsVirtual { get; }

        public abstract INamedType? DeclaringType { get; }

        public IntroducedMember( IAdvice advice, INamedType targetDeclaration ) : base( advice )
        {
            this.TargetDeclaration = targetDeclaration;
        }
    }

    internal sealed class IntroducedMethod : IntroducedMember, IMethod
    {
        private readonly IntroductionScope? _scope;
        private readonly string? _name;
        private readonly bool? _isStatic;
        private readonly bool? _isVirtual;
        private readonly Visibility? _visibility;

        public override CodeElement? ContainingElement { get; }

        public IMethod TemplateMethod { get; }

        [Memo]
        public IParameter? ReturnParameter => this.TemplateMethod!.ReturnParameter;

        public IType ReturnType => this.TemplateMethod!.ReturnType;

        [Memo]
        public IImmutableList<IMethod> LocalFunctions => this.TemplateMethod!.LocalFunctions;

        [Memo]
        public IImmutableList<IParameter> Parameters => this.TemplateMethod!.Parameters!.Select( x => (IParameter) new IntroducedParameter( this.Advice, this, x ) ).ToImmutableArray();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters => this.TemplateMethod!.GenericParameters!.Select( x => (IGenericParameter) new IntroducedGenericParameter( this.Advice, this, x ) ).ToImmutableArray();

        public Code.MethodKind MethodKind => this.TemplateMethod!.MethodKind;

        public override string Name => this._name ?? this.TemplateMethod!.Name;

        public override bool IsStatic => this._isStatic ?? this.TemplateMethod!.IsStatic;

        public override bool IsVirtual => this._isVirtual ?? this.TemplateMethod!.IsVirtual;

        public override INamedType? DeclaringType => this.TemplateMethod!.DeclaringType;

        public override IImmutableList<Attribute> Attributes => this.TemplateMethod!.Attributes;

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public IntroducedMethod( IAdvice advice, INamedType targetType, IMethod templateMethod, IntroductionScope? scope, string? name, bool? isStatic, bool? isVirtual, Visibility? visibility )
            : base( advice, targetType )
        {
            this.ContainingElement = targetType;
            this.TemplateMethod = templateMethod;
            this._scope = scope;
            this._name = name;
            this._isStatic = isStatic;
            this._isVirtual = isVirtual;
            this._visibility = visibility;
        }

        // TODO: Memo for methods?
        private MemberDeclarationSyntax? _declaration;

        public override MemberDeclarationSyntax GetDeclaration()
        {
            if ( this._declaration != null )
            {
                return this._declaration;
            }

            var templateSyntax = (MethodDeclarationSyntax) this.TemplateMethod!.GetSyntaxNode()!;

            this._declaration = MethodDeclaration(
                List<AttributeListSyntax>(), // TODO: Copy some attributes?
                templateSyntax.Modifiers,
                templateSyntax.ReturnType,
                templateSyntax.ExplicitInterfaceSpecifier!,
                templateSyntax.Identifier,
                templateSyntax.TypeParameterList!,
                templateSyntax.ParameterList,
                templateSyntax.ConstraintClauses,
                Block( ThrowStatement( ObjectCreationExpression( ParseTypeName( "System.NotImplementedException" ) ) ) ),
                null,
                templateSyntax.SemicolonToken );

            return this._declaration;
        }

        public override CSharpSyntaxNode GetSyntaxNode() => this.GetDeclaration();

        public override IEnumerable<CSharpSyntaxNode> GetSyntaxNodes() => new[] { (CSharpSyntaxNode) this.GetDeclaration() };

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class IntroducedParameter : IntroducedElement, IParameter
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

        public override CodeElement? ContainingElement { get; }

        public override IImmutableList<Attribute> Attributes => this._template.Attributes;

        public override CodeElementKind ElementKind => this._template.ElementKind;

        public IntroducedParameter( IAdvice advice, IMethod containingMethod, IParameter template ) : base( advice )
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

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class IntroducedGenericParameter : IntroducedElement, IGenericParameter
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

        public override CodeElement? ContainingElement { get; }

        public override IImmutableList<Attribute> Attributes => this._template.Attributes;

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

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }
    }
}

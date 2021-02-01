using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Caravela.Framework.Impl.Transformations
{

    abstract class IntroducedElement : Transformation, ICodeElement
    {
        public abstract ICodeElement? ContainingElement { get; }

        public abstract IReactiveCollection<IAttribute> Attributes { get; }

        public abstract CodeElementKind Kind { get; }

        public abstract SyntaxNode Declaration { get; }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null );
    }

    abstract class IntroducedMember : IntroducedElement, IMember
    {
        public abstract string Name { get; }
        public abstract bool IsStatic { get; }
        public abstract bool IsVirtual { get; }
        public abstract INamedType? DeclaringType { get; }
    }

    sealed class IntroducedMethod : IntroducedMember, IMethod, IToSyntax
    {
        public override ICodeElement? ContainingElement { get; }

        public IMethod? TemplateMethod { get; }
        public IMethod? OverriddenMethod { get; }
        public BlockSyntax MethodBodyOverride { get; }

        [Memo]
        public IParameter? ReturnParameter => this.TemplateMethod!.ReturnParameter;

        public IType ReturnType => this.TemplateMethod!.ReturnType;

        [Memo]
        public IImmutableList<IMethod> LocalFunctions => this.TemplateMethod!.LocalFunctions;

        [Memo]
        public IImmutableList<IParameter> Parameters => this.TemplateMethod!.Parameters!.Select(x => (IParameter)new IntroducedParameter(this, x)).ToImmutableList();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters => this.TemplateMethod!.GenericParameters!.Select( x => (IGenericParameter) new IntroducedGenericParameter( this, x ) ).ToImmutableList();

        Code.MethodKind IMethod.Kind => this.OverriddenMethod?.Kind ?? this.TemplateMethod!.Kind;

        CSharpSyntaxNode IToSyntax.GetSyntaxNode() => (CSharpSyntaxNode)this.Declaration;
        IEnumerable<CSharpSyntaxNode> IToSyntax.GetSyntaxNodes() => new[] { (CSharpSyntaxNode) this.Declaration };

        public override SyntaxNode Declaration
        {
            get
            {
                var templateSyntax = (MethodDeclarationSyntax)this.TemplateMethod!.GetSyntaxNode()!;

                return MethodDeclaration(
                    List<AttributeListSyntax>(), // TODO: Copy some attributes?
                    templateSyntax.Modifiers, templateSyntax.ReturnType, templateSyntax.ExplicitInterfaceSpecifier!, templateSyntax.Identifier, templateSyntax.TypeParameterList!,
                    templateSyntax.ParameterList, templateSyntax.ConstraintClauses, this.MethodBodyOverride!, null, templateSyntax.SemicolonToken 
                    );
            }
        }

        public override string Name => this.TemplateMethod!.Name;

        public override bool IsStatic => this.TemplateMethod!.IsStatic;

        public override bool IsVirtual => this.TemplateMethod!.IsVirtual;

        public override INamedType? DeclaringType => this.TemplateMethod!.DeclaringType;

        public override IReactiveCollection<IAttribute> Attributes => this.TemplateMethod!.Attributes;

        public override CodeElementKind Kind => CodeElementKind.Method;

        public IntroducedMethod( INamedType targetType, IMethod? overriddenMethod, IMethod? templateMethod, BlockSyntax methodBodyOverride )
        {
            this.ContainingElement = targetType;
            this.OverriddenMethod = overriddenMethod;
            this.TemplateMethod = templateMethod;
            this.MethodBodyOverride = methodBodyOverride;
        }

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

        public override CodeElementKind Kind => this._template.Kind;

        public override SyntaxNode Declaration => this._template.GetSyntaxNode()!;

        public IntroducedParameter( IMethod containingMethod, IParameter template )
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

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

        Code.TypeKind IType.Kind => Code.TypeKind.GenericParameter;

        public override ICodeElement? ContainingElement { get; }

        public override IReactiveCollection<IAttribute> Attributes => this._template.Attributes;

        public override CodeElementKind Kind => CodeElementKind.GenericParameter;

        public override SyntaxNode Declaration => this._template.GetSyntaxNode()!;

        public IntroducedGenericParameter( IMethod containingMethod, IGenericParameter template )
        {
            this.ContainingElement = containingMethod;
            this._template = template;
        }

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

﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    /// <summary>
    /// Base class implementing <see cref="IDeclarationBuilder"/>. These classes are returned by introduction advice methods so the user can continue
    /// specifying the introduced declaration. They are bound to the <see cref="CompilationModel"/> that created them, but implement
    /// <see cref="ISdkRef{T}"/> so they can resolve, using <see cref="DeclarationFactory"/>, to the consuming <see cref="CompilationModel"/>.
    /// 
    /// </summary>
    internal abstract class DeclarationBuilder : IDeclarationBuilderImpl, IDeclarationImpl
    {
        protected DeclarationBuilder( Advice parentAdvice )
        {
            this.ParentAdvice = parentAdvice;
        }

        public Advice ParentAdvice { get; }

        public virtual bool IsDesignTime => true;

        public IDeclarationOrigin Origin => this.ParentAdvice;

        public abstract IDeclaration? ContainingDeclaration { get; }

        IAttributeCollection IDeclaration.Attributes => this.Attributes;

        public AttributeBuilderCollection Attributes { get; } = new();

        public abstract DeclarationKind DeclarationKind { get; }

        public virtual bool IsImplicitlyDeclared => false;

        int IDeclaration.Depth => this.GetDepthImpl();

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public CompilationModel Compilation
            => (CompilationModel?) this.ContainingDeclaration?.Compilation
               ?? throw new AssertionFailedException( $"Declaration '{this}' has no containing declaration." );

        public bool IsFrozen { get; private set; }

        protected void CheckNotFrozen()
        {
            if ( this.IsFrozen )
            {
                throw new InvalidOperationException( $"You can no longer modify '{this.ToDisplayString()}'." );
            }
        }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public void AddAttribute( AttributeConstruction attribute )
        {
            this.CheckNotFrozen();

            this.Attributes.Add( new AttributeBuilder( this.ParentAdvice, this, attribute ) );
        }

        public void AddAttributes( IEnumerable<AttributeConstruction> attributes )
        {
            this.CheckNotFrozen();

            this.Attributes.AddRange( attributes.Select( a => new AttributeBuilder( this.ParentAdvice, this, a ) ) );
        }

        public void RemoveAttributes( INamedType type )
        {
            this.CheckNotFrozen();

            this.Attributes.RemoveAll( a => a.Type.Is( type ) );
        }

        public virtual void Freeze() => this.IsFrozen = true;

        public Ref<IDeclaration> ToRef() => Ref.FromBuilder( this );

        public SerializableDeclarationId ToSerializableId()
            => throw new NotImplementedException( "The method is not implemented for introduced declarations." );

        IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

        ISymbol? ISdkDeclaration.Symbol => null;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
            => ((IDeclarationImpl?) this.ContainingDeclaration)?.DeclaringSyntaxReferences ?? ImmutableArray<SyntaxReference>.Empty;

        public abstract bool CanBeInherited { get; }

        public virtual SyntaxTree? PrimarySyntaxTree => this.ContainingDeclaration.AssertNotNull().GetPrimarySyntaxTree();

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.ToDisplayString( CodeDisplayFormat.MinimallyQualified );

        public IDeclaration OriginalDefinition => this;

        public IAssembly DeclaringAssembly => this.Compilation.DeclaringAssembly;

        // TODO: should we locate diagnostic on the aspect attribute?
        public Location? DiagnosticLocation => null;

        public TExtension GetMetric<TExtension>()
            where TExtension : IMetric
            => this.GetCompilationModel().MetricManager.GetMetric<TExtension>( this );

        protected virtual SyntaxKind AttributeTargetSyntaxKind => SyntaxKind.None;

        public SyntaxList<AttributeListSyntax> GetAttributeLists( MemberInjectionContext context, IDeclaration? declaration = null )
        {
            var attributes = context.SyntaxGenerator.AttributesForDeclaration(
                (declaration ?? this).ToTypedRef(),
                context.Compilation,
                this.AttributeTargetSyntaxKind );

            if ( declaration is IMethod method )
            {
                attributes = attributes.AddRange(
                    context.SyntaxGenerator.AttributesForDeclaration(
                        method.ReturnParameter.ToTypedRef<IDeclaration>(),
                        context.Compilation,
                        SyntaxKind.ReturnKeyword ) );

                if ( method.MethodKind is MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.PropertySet )
                {
                    attributes = attributes.AddRange(
                        context.SyntaxGenerator.AttributesForDeclaration(
                            method.Parameters[0].ToTypedRef<IDeclaration>(),
                            context.Compilation,
                            SyntaxKind.ParamKeyword ) );
                }
            }
            else if ( declaration is IProperty { IsAutoPropertyOrField: true } )
            {
                // TODO: field-level attributes
            }

            return attributes;
        }

        public bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );
    }
}
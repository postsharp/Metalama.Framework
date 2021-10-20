// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TypedConstant = Caravela.Framework.Code.TypedConstant;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// Base class implementing <see cref="IDeclarationBuilder"/>. These classes are returned by introduction advices so the user can continue
    /// specifying the introduced declaration. They are bound to the <see cref="CompilationModel"/> that created them, but implement
    /// <see cref="IDeclarationRef{T}"/> so they can resolve, using <see cref="DeclarationFactory"/>, to the consuming <see cref="CompilationModel"/>.
    /// 
    /// </summary>
    internal abstract class DeclarationBuilder : IDeclarationBuilder, IDeclarationImpl, ITransformation
    {
        internal Advice ParentAdvice { get; }

        public DeclarationOrigin Origin => DeclarationOrigin.Aspect;

        public abstract IDeclaration? ContainingDeclaration { get; }

        IAttributeList IDeclaration.Attributes => this.Attributes;

        public AttributeBuilderList Attributes { get; } = new();

        public abstract DeclarationKind DeclarationKind { get; }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public CompilationModel Compilation => (CompilationModel?) this.ContainingDeclaration?.Compilation ?? throw new AssertionFailedException();

        public bool IsFrozen { get; private set; }

        Advice ITransformation.Advice => this.ParentAdvice;

        public DeclarationBuilder( Advice parentAdvice )
        {
            this.ParentAdvice = parentAdvice;
        }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public IAttributeBuilder AddAttribute( INamedType type, params object?[] constructorArguments )
        {
            /* We are interested in the fact that there is a matching ctor. 
               If there are multiple we don't care at this point as we will generate code eventually and C# will resolve the correct one.
               Of course this is a bit strange for the user, but currently it's not important.
            */

            var ctor = type.Constructors.OfCompatibleSignature( constructorArguments.Select( x => x?.GetType() ).ToList() ).FirstOrDefault();

            if ( ctor == null )
            {
                throw GeneralDiagnosticDescriptors.CompatibleAttributeConstructorDoesNotExist.CreateException(
                    (this.ParentAdvice.Aspect.AspectClass.ShortName, this, type) );
            }

            var ctorArguments = constructorArguments.Select( ( _, i ) => new TypedConstant( ctor.Parameters[i].Type, constructorArguments[i] ) )
                .ToList();

            return new AttributeBuilder( this, ctor, ctorArguments );
        }

        public void RemoveAttributes( INamedType type ) => throw new NotImplementedException();

        public virtual void Freeze() => this.IsFrozen = true;

        public IDiagnosticLocation? DiagnosticLocation => this.ContainingDeclaration?.DiagnosticLocation;

        public DeclarationRef<IDeclaration> ToRef() => DeclarationRef.FromBuilder( this );

        ISymbol? ISdkDeclaration.Symbol => null;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
            => ((IDeclarationImpl?) this.ContainingDeclaration)?.DeclaringSyntaxReferences ?? ImmutableArray<SyntaxReference>.Empty;

        public abstract bool CanBeInherited { get; }

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.ToDisplayString( CodeDisplayFormat.MinimallyQualified );

        public IDeclaration OriginalDefinition => this;

        public IAssembly DeclaringAssembly => this.Compilation.DeclaringAssembly;
    }
}
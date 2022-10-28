// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Linking;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a member to be introduced in a type and encapsulates the information needed by the <see cref="AspectLinker"/>
    /// to perform the linking.
    /// </summary>
    internal class InjectedMember
    {
        public DeclarationKind Kind { get; }

        /// <summary>
        /// Gets the <see cref="IInjectMemberTransformation" /> that created this object.
        /// </summary>
        public IInjectMemberTransformation Transformation { get; }

        public IDeclarationBuilder? DeclarationBuilder => (this.Transformation as IIntroduceDeclarationTransformation)?.DeclarationBuilder;

        /// <summary>
        /// Gets the syntax of the introduced member.
        /// </summary>
        public MemberDeclarationSyntax Syntax { get; }

        /// <summary>
        /// Gets the <see cref="AspectLayerId"/> that emitted the current <see cref="InjectedMember"/>.
        /// </summary>
        public AspectLayerId AspectLayerId { get; }

        /// <summary>
        /// Gets the semantic of the introduced member as supported by the linker.
        /// </summary>
        public InjectedMemberSemantic Semantic { get; }

        /// <summary>
        /// Gets the declaration (overriden or introduced) that corresponds to the current <see cref="InjectedMember"/>.
        /// This is used to associate diagnostic suppressions to the introduced member. If <c>null</c>, diagnostics
        /// are not suppressed from the introduced member.
        /// </summary>
        public IMemberOrNamedType? Declaration { get; }

        public InjectedMember(
            IInjectMemberTransformation injectMemberTransformation,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            InjectedMemberSemantic semantic,
            MemberBuilder declaration ) : this(
            injectMemberTransformation,
            declaration.DeclarationKind,
            syntax,
            aspectLayerId,
            semantic,
            declaration ) { }

        public InjectedMember(
            OverrideMemberTransformation overrideMemberTransformation,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            InjectedMemberSemantic semantic,
            IMemberOrNamedType? declaration ) : this(
            overrideMemberTransformation,
            overrideMemberTransformation.OverriddenDeclaration.DeclarationKind,
            syntax,
            aspectLayerId,
            semantic,
            declaration ) { }

        protected InjectedMember(
            InjectedMember prototype,
            MemberDeclarationSyntax syntax ) : this(
            prototype.Transformation,
            prototype.Kind,
            syntax,
            prototype.AspectLayerId,
            prototype.Semantic,
            prototype.Declaration ) { }

        internal InjectedMember(
            IInjectMemberTransformation transformation,
            DeclarationKind kind,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            InjectedMemberSemantic semantic,
            IMemberOrNamedType? declaration )
        {
            this.Transformation = transformation;
            this.Syntax = syntax;
            this.AspectLayerId = aspectLayerId;
            this.Semantic = semantic;
            this.Declaration = declaration;
            this.Kind = kind;
        }

        public override string? ToString() => this.Transformation.ToString();

        internal InjectedMember WithSyntax( MemberDeclarationSyntax newSyntax )
        {
            return new InjectedMember( this, newSyntax );
        }
    }
}
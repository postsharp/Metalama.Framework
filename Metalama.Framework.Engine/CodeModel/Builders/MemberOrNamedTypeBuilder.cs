// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal abstract class MemberOrNamedTypeBuilder : DeclarationBuilder, IMemberOrNamedTypeBuilder, IIntroduceMemberTransformation, IObservableTransformation
    {
        private Accessibility _accessibility;
        private string _name;
        private bool _isSealed;
        private bool _isNew;
        private bool _isAbstract;
        private bool _isStatic;

        public bool IsSealed
        {
            get => this._isSealed;
            set
            {
                this.CheckNotFrozen();
                this._isSealed = value;
            }
        }

        public bool IsNew
        {
            get => this._isNew;
            set
            {
                this.CheckNotFrozen();

                this._isNew = value;
            }
        }

        public INamedType DeclaringType { get; }

        public MemberInfo ToMemberInfo() => throw new NotImplementedException();

        public Accessibility Accessibility
        {
            get => this._accessibility;
            set
            {
                this.CheckNotFrozen();

                this._accessibility = value;
            }
        }

        public virtual string Name
        {
            get => this._name;
            set
            {
                this.CheckNotFrozen();

                this._name = value;
            }
        }

        public bool IsAbstract
        {
            get => this._isAbstract;
            set
            {
                this.CheckNotFrozen();

                this._isAbstract = value;
            }
        }

        public bool IsStatic
        {
            get => this._isStatic;
            set
            {
                this.CheckNotFrozen();

                this._isStatic = value;
            }
        }

        public sealed override IDeclaration ContainingDeclaration => this.DeclaringType;

        public abstract bool IsDesignTime { get; }

        public MemberOrNamedTypeBuilder( Advice parentAdvice, INamedType declaringType, string name ) : base( parentAdvice )
        {
            this.DeclaringType = declaringType;
            this._name = name;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context );

        public InsertPosition InsertPosition => this.ToInsertPosition();


    }
}
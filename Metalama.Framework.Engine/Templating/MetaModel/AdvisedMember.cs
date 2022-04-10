// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal abstract class AdvisedMember<T> : AdvisedDeclaration<T>, IMember
        where T : IMember, IDeclarationImpl
    {
        protected AdvisedMember( T underlying ) : base( underlying ) { }

        public Accessibility Accessibility => this.Underlying.Accessibility;

        public string Name => this.Underlying.Name;

        public bool IsAbstract => this.Underlying.IsAbstract;

        public bool IsStatic => this.Underlying.IsStatic;

        public bool IsSealed => this.Underlying.IsSealed;

        public bool IsNew => this.Underlying.IsNew;

        public bool IsVirtual => this.Underlying.IsVirtual;

        public bool IsAsync => this.Underlying.IsAsync;

        public bool IsOverride => this.Underlying.IsOverride;

        public bool IsImplicit => this.Underlying.IsImplicit;

        public bool IsExplicitInterfaceImplementation => this.Underlying.IsExplicitInterfaceImplementation;

        public INamedType DeclaringType => this.Underlying.DeclaringType;

        public MemberInfo ToMemberInfo() => this.Underlying.ToMemberInfo();

        public IUserReceiver This
            => new ThisInstanceUserReceiver(
                this.DeclaringType,
                new AspectReferenceSpecification( UserCodeExecutionContext.Current.AspectLayerId!.Value, AspectReferenceOrder.Base ) );
    }
}
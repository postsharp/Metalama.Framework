// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Linking;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceMember<T> : AdviceDeclaration<T>, IMember
        where T : IMember
    {
        public AdviceMember( T underlying ) : base( underlying ) { }

        public Accessibility Accessibility => this.Underlying.Accessibility;

        public string Name => this.Underlying.Name;

        public bool IsAbstract => this.Underlying.IsAbstract;

        public bool IsStatic => this.Underlying.IsStatic;

        public bool IsSealed => this.Underlying.IsSealed;

        public bool IsReadOnly => this.Underlying.IsReadOnly;

        public bool IsNew => this.Underlying.IsNew;

        public bool IsVirtual => this.Underlying.IsVirtual;

        public bool IsAsync => this.Underlying.IsAsync;

        public bool IsOverride => this.Underlying.IsOverride;

        public INamedType DeclaringType => this.Underlying.DeclaringType;

        public MemberInfo ToMemberInfo() => this.Underlying.ToMemberInfo();

        public IDynamicReceiver This
            => new ThisInstanceDynamicReceiver(
                this.DeclaringType,
                new LinkerAnnotation( this.Underlying.GetCompilationModel().AspectLayerId, LinkingOrder.Original ) );
    }
}
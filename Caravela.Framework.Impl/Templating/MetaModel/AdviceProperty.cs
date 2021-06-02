// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects.AdvisedCode;
using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceProperty : AdviceFieldOrProperty<IProperty>, IAdviceProperty
    {
        public AdviceProperty( IProperty underlying ) : base( underlying ) { }

        public RefKind RefKind => this.Underlying.RefKind;

        public bool IsByRef => this.Underlying.IsByRef;

        public bool IsRef => this.Underlying.IsRef;

        public bool IsRefReadonly => this.Underlying.IsRefReadonly;

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Underlying );

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public PropertyInfo ToPropertyInfo() => this.Underlying.ToPropertyInfo();

        public new IPropertyInvoker? BaseInvoker => this.Underlying.BaseInvoker;

        public new IPropertyInvoker Invoker => this.Underlying.Invoker;
    }
}
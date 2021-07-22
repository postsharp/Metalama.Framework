// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedProperty : AdvisedFieldOrProperty<IProperty>, IAdvisedProperty
    {
        public AdvisedProperty( IProperty underlying ) : base( underlying ) { }

        public RefKind RefKind => this.Underlying.RefKind;

        [Memo]
        public IAdvisedParameterList Parameters => new AdvisedParameterList( this.Underlying );

        IParameterList IHasParameters.Parameters => this.Underlying.Parameters;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public PropertyInfo ToPropertyInfo() => this.Underlying.ToPropertyInfo();

        public new IInvokerFactory<IPropertyInvoker> Invokers => this.Underlying.Invokers;

        public IProperty? OverriddenProperty => this.Underlying.OverriddenProperty;
    }
}
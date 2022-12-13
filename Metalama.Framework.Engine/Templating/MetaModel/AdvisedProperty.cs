// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal sealed class AdvisedProperty : AdvisedFieldOrProperty<IPropertyImpl>, IAdvisedProperty
    {
        public AdvisedProperty( IProperty underlying ) : base( (IPropertyImpl) underlying ) { }

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public PropertyInfo ToPropertyInfo() => this.Underlying.ToPropertyInfo();

        public new IInvokerFactory<IFieldOrPropertyInvoker> Invokers => this.Underlying.Invokers;

        public IProperty? OverriddenProperty => this.Underlying.OverriddenProperty;
    }
}
// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedProperty : AdvisedFieldOrProperty<IPropertyImpl>, IAdvisedProperty
    {
        public AdvisedProperty( IProperty underlying ) : base( (IPropertyImpl) underlying ) { }

        public RefKind RefKind => this.Underlying.RefKind;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.Underlying.ExplicitInterfaceImplementations;

        public PropertyInfo ToPropertyInfo() => this.Underlying.ToPropertyInfo();

        public new IInvokerFactory<IFieldOrPropertyInvoker> Invokers => this.Underlying.Invokers;

        public IProperty? OverriddenProperty => this.Underlying.OverriddenProperty;
    }
}
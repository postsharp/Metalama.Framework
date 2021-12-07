// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedProperty : AdvisedFieldOrProperty<IPropertyImpl>, IAdvisedProperty
    {
        public AdvisedProperty( IProperty underlying ) : base( (IPropertyImpl) underlying ) { }

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
// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdvisedField : AdvisedFieldOrProperty<IFieldImpl>, IAdvisedField
    {
        public AdvisedField( IField underlying ) : base( (IFieldImpl) underlying ) { }

        public FieldInfo ToFieldInfo() => this.Underlying.ToFieldInfo();
    }
}
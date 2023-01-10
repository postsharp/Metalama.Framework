// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal sealed class AdvisedField : AdvisedFieldOrProperty<IFieldImpl>, IAdvisedField
    {
        public AdvisedField( IField underlying ) : base( (IFieldImpl) underlying ) { }

        public FieldInfo ToFieldInfo() => this.Underlying.ToFieldInfo();

        public TypedConstant? ConstantValue => this.Underlying.ConstantValue;
    }
}
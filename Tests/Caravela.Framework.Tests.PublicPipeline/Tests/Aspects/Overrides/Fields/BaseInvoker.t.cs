using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Overrides.Fields.BaseInvoker
{
    class Aspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic OverrideProperty { get => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time."); set => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time."); }

    }

    class TargetCode
    {


private global::System.Int32 _field1;


private global::System.Int32 field {get    {
        return this.field_Source;
    }

set    {
        this.field_Source= value;
    }
}
private global::System.Int32 field_Source
{
    get
    {
        return this._field1;
    }

    set
    {
        this._field1 = value;
    }
}        
    }
}

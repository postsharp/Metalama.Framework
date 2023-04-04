// Warning CS8618 on `NRT`: `Non-nullable field 'NRT' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
// Warning CS8618 on `RT`: `Non-nullable field 'RT' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
// Warning CS8618 on `RTOCT`: `Non-nullable field 'RTOCT' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.`
[Aspect]
class TargetClass
{
    private global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes.RT NRT;
    private global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes.RTOCT? NRTOCT;
    private global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes.RT RT;
    private global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes.RTOCT RTOCT;
}

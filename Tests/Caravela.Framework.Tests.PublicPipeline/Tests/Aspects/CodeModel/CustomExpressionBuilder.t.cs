[Aspect]
        int Method(int a)
{
    global::System.Console.WriteLine(new global::Caravela.Framework.Tests.PublicPipeline.Aspects.CodeModel.CustomExpressionBuilder.Fixture("Hello, world."));
    var runTimeList = new global::System.Collections.Generic.List<global::Caravela.Framework.Tests.PublicPipeline.Aspects.CodeModel.CustomExpressionBuilder.Fixture>{new global::Caravela.Framework.Tests.PublicPipeline.Aspects.CodeModel.CustomExpressionBuilder.Fixture("Hello, world.")};
            return a;
}
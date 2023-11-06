// Final Compilation.Emit failed.
// Warning LAMA0282 on ``: `The template 'TheAspect.AuxiliaryTemplate()' may not be applicable to 'Target.M()', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Warning LAMA0282 on ``: `The template 'TheAspect.AuxiliaryTemplate()' may not be applicable to 'Target.M()', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Error CS8936 on `"""aux"""`: `Feature 'raw string literals' is not available in C# 10.0. Please use language version 11.0 or greater.`
// Error CS8936 on `"""aux"""`: `Feature 'raw string literals' is not available in C# 10.0. Please use language version 11.0 or greater.`
[TheAspect]
class Target
{
    private void M()
    {
        global::System.Console.WriteLine("""aux""");
        global::System.Console.WriteLine("""aux""");
    }
}

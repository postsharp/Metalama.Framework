// Final Compilation.Emit failed.
// Warning LAMA0282 on ``: `The template 'TheAspect.Event1.add' may not be applicable to 'Target.Event1.add', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Warning LAMA0282 on ``: `The template 'TheAspect.Event1.remove' may not be applicable to 'Target.Event1.remove', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Warning LAMA0282 on ``: `The template 'TheAspect.Event2.add' may not be applicable to 'Target.Event2.add', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Warning LAMA0282 on ``: `The template 'TheAspect.Event2.remove' may not be applicable to 'Target.Event2.remove', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Error CS8936 on `"""add"""`: `Feature 'raw string literals' is not available in C# 10.0. Please use language version 11.0 or greater.`
// Error CS8936 on `"""remove"""`: `Feature 'raw string literals' is not available in C# 10.0. Please use language version 11.0 or greater.`
[TheAspect]
class Target
{
    public event global::System.EventHandler Event1
    {
        add
        {
            global::System.Console.WriteLine("""add""");
        }
        remove
        {
        }
    }
    public event global::System.EventHandler Event2
    {
        add
        {
        }
        remove
        {
            global::System.Console.WriteLine("""remove""");
        }
    }
}

// Final Compilation.Emit failed.
// Warning LAMA0282 on ``: `The template 'TheAspect.Field' may not be applicable to 'Target.Field', because it uses C# 11.0, while the project uses C# 10.0. Consider specifying <LangVersion>11.0</LangVersion> in this project, or removing newer language features from the template and then specifying <MetalamaTemplateLanguageVersion> in the aspect project.`
// Error CS8936 on `""" This is a long message. It has several lines. Some are indented more than others. Some should start at the first column. Some have "quoted text" in them. """`: `Feature 'raw string literals' is not available in C# 10.0. Please use language version 11.0 or greater.`
[TheAspect]
class Target
{
    public global::System.String Field = (global::System.String)"""
        This is a long message.
        It has several lines.
            Some are indented
                    more than others.
        Some should start at the first column.
        Some have "quoted text" in them.
        """;
}

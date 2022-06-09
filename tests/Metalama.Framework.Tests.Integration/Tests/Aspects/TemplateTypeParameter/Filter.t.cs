// Final Compilation.Emit failed. 
// Error CS0592 on `global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.Filter.MyAspect`: `Attribute 'global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.Filter.MyAspect' is not valid on this declaration type. It is only valid on 'field' declarations.`
internal class Target
{


private global::System.String _q1;


[global::Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.Filter.MyAspect]
private global::System.String q 
{ get
{ 
        return this._q1;
}
set
{ 
        global::System.Console.WriteLine(typeof(global::System.String).Name);
        this._q1=value;
}
}}
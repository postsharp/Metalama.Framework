using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods;

[assembly: AspectOrder(
    typeof(OverrideAfterInvokerAspectAttribute), 
    typeof(InvokerAspectAttribute), 
    typeof(IntroductionAspectAttribute), 
    typeof(InvokerAspectBeforeIntroductionAttribute), 
    typeof(OverrideBeforeInvokerAspectAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods;

/// <summary>
/// Overrides a declaration using a template with invokers.
/// </summary>
public class InvokerAspectAttribute : MethodAspect
{
    public bool InvokeParameterInstance { get; init; }

    public bool OverrideTargetBefore { get; init; }

    public bool OverrideTargetAfter { get; init; }

    public string? TargetName { get; init; }

    public int TargetLevel { get; init; }

    public InvokerAspectAttribute()
    {
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var type = builder.Target.DeclaringType!;

        for (int i = 0; i < this.TargetLevel; i++)
        {
            type = type.BaseType;

            if (type == null)
            {
                throw new InvalidOperationException("Invalid invokerTargetBaseLevel.");
            }
        }

        var invokerTargetMethod =
            this.TargetName != null
            ? type.Methods.OfName(this.TargetName).Single()
            : builder.Target;

        if (this.OverrideTargetBefore)
        {
            builder.Advice.WithTemplateProvider(Templates.Instance).Override(invokerTargetMethod, nameof(Templates.OverrideTemplate), args: new { name = "before" });
        }

        builder.Advice.WithTemplateProvider(Templates.Instance).Override(builder.Target, nameof(Templates.InvokerOverrideTemplate), args: new { targetMethod = invokerTargetMethod, invokeParameterInstance = this.InvokeParameterInstance });

        if (this.OverrideTargetAfter)
        {
            builder.Advice.WithTemplateProvider(Templates.Instance).Override(invokerTargetMethod, nameof(Templates.OverrideTemplate), new { name = "after" });
        }
    }
}

public class InvokerAspectBeforeIntroductionAttribute : InvokerAspectAttribute
{
}

/// <summary>
/// Introduces a declaration using a template with invokers.
/// </summary>
public class IntroductionAspectAttribute : TypeAspect
{
    private readonly string _introducedMethodName;
    private readonly Type _introducedMethodParameterType;
    private readonly OverrideStrategy _overrideStrategy;

    public string? InvokerTargetName { get; init; }

    public int InvokerTargetBaseLevel { get; init; }

    public bool OverrideTargetAfter { get; init; }

    public IntroductionAspectAttribute(
        string introducedMethodName,
        Type introducedMethodParameterType,
        OverrideStrategy overrideStrategy )
    {
        this._introducedMethodName = introducedMethodName;
        this._introducedMethodParameterType = introducedMethodParameterType;
        this._overrideStrategy = overrideStrategy;
    }

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var type = builder.Target.DeclaringType!;

        for (int i = 0; i < this.InvokerTargetBaseLevel; i++)
        {
            type = type.BaseType;

            if (type == null)
            {
                throw new InvalidOperationException("Invalid invokerTargetBaseLevel.");
            }
        }

        var invokerTargetMethod =
            this.InvokerTargetName != null
            ? type.Methods.OfName(this.InvokerTargetName).SingleOrDefault()
            : null;

        var introducedMethod = 
            builder.Advice.WithTemplateProvider(Templates.Instance)
            .IntroduceMethod(
                builder.Target, 
                nameof(Templates.InvokerOverrideTemplate), 
                args: new { targetMethod = invokerTargetMethod, invokeParameterInstance = false },
                buildMethod: m =>
                {
                    m.Name = this._introducedMethodName;
                    m.ReturnType = TypeFactory.GetType(SpecialType.Void);
                    m.AddParameter("instance", this._introducedMethodParameterType);
                    m.AddParameter("value", TypeFactory.GetType(SpecialType.String));
                },
                whenExists:this._overrideStrategy)
            .Declaration;

        if (this.OverrideTargetAfter)
        {
            builder.Advice.WithTemplateProvider(Templates.Instance).Override(introducedMethod, nameof(Templates.OverrideTemplate), new { name = "after" });
        }
    }
}

public class Templates : ITemplateProvider
{
    public static Templates Instance { get; } = new();

    [Template]
    public dynamic? InvokerOverrideTemplate([CompileTime] IMethod? targetMethod, [CompileTime] bool invokeParameterInstance)
    {
        targetMethod ??= meta.Target.Method;

        if (invokeParameterInstance)
        {
            meta.InsertComment("This instance");
        }

        targetMethod.With(InvokerOptions.Default).Invoke(meta.This, "Default");
        targetMethod.With(InvokerOptions.Default | InvokerOptions.NullConditional).Invoke(meta.This, "Default-NullConditional");
        targetMethod.With(InvokerOptions.Base).Invoke(meta.This, "Base");
        targetMethod.With(InvokerOptions.Base | InvokerOptions.NullConditional).Invoke(meta.This, "Base-NullConditional");
        targetMethod.With(InvokerOptions.Current).Invoke(meta.This, "Current");
        targetMethod.With(InvokerOptions.Current | InvokerOptions.NullConditional).Invoke(meta.This, "Current-NullConditional");
        targetMethod.With(InvokerOptions.Final).Invoke(meta.This, "Final");
        targetMethod.With(InvokerOptions.Final | InvokerOptions.NullConditional).Invoke(meta.This, "Final-NullConditional");

        if (invokeParameterInstance)
        {
            meta.InsertComment("Another instance");
            targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Default).Invoke(meta.This, "Default");
            targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Default | InvokerOptions.NullConditional).Invoke(meta.This, "Default-NullConditional");
            targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Final).Invoke(meta.This, "Final");
            targetMethod.With(meta.Target.Parameters[0], InvokerOptions.Final | InvokerOptions.NullConditional).Invoke(meta.This, "Final-NullConditional");
        }

        return meta.Proceed();
    }

    [Template]
    public dynamic? OverrideTemplate([CompileTime] string name)
    {
        Console.WriteLine($"Invoker aspect override {name} begins");
        var x = meta.Proceed();
        Console.WriteLine($"Invoker aspect override {name} ends");
        return x;
    }
}

public class OverrideBeforeInvokerAspectAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Aspect before begins");
        var x = meta.Proceed();
        Console.WriteLine("Aspect before ends");
        return x;
    }
}

public class OverrideAfterInvokerAspectAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Aspect after begins");
        var x = meta.Proceed();
        Console.WriteLine("Aspect after ends");
        return x;
    }
}

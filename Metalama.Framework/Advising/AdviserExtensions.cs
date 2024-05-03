// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using System;

namespace Metalama.Framework.Advising;

public static class AdviserExtensions
{
    /// <summary>
    /// Overrides the implementation of a method.
    /// </summary>
    /// <param name="adviser">The advisable method.</param>
    /// <param name="template">Name of a method in the aspect class whose implementation will be used as a template.
    ///     This property must be annotated with <see cref="TemplateAttribute"/>. To select a different templates according to the kind of target method
    ///     (such as async or iterator methods), use the constructor of the <see cref="MethodTemplateSelector"/> type. To specify a single
    ///     template for all methods, pass a string.</param>
    /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template method.</param>
    /// <param name="tags">An optional opaque object of anonymous type passed to the template method and exposed under the <see cref="meta.Tags"/> property
    ///     of the <see cref="meta"/> API.</param>
    /// <seealso href="@overriding-methods"/>
    public static IOverrideAdviceResult<IMethod> Override(
        this IAdviser<IMethod> adviser,
        in MethodTemplateSelector template,
        object? args = null,
        object? tags = null )
        => ((IAdvisableInternal) adviser).AdviceFactory.Override( adviser.Target, template, args, tags );

    /// <summary>
    /// Introduces a new method or overrides the implementation of the existing one.
    /// </summary>
    /// <param name="adviser">An adviser for a member or named type.</param>
    /// <param name="template">Name of the method of the aspect class that will be used as a template for the introduced method. This method must be
    ///     annotated with <see cref="TemplateAttribute"/>. This method can parameters and a return type. The actual parameters and return type
    ///     of the introduced method can be modified using the <see cref="IMethodBuilder"/> returned by this method.</param>
    /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
    ///     method. The default scope depends on the scope of the template method.
    ///     If the method is static, the introduced method is static. However, if the template method is non-static, then the introduced method
    ///     copies of the scope of the target declaration of the aspect.</param>
    /// <param name="whenExists">Determines the implementation strategy when a method of the same name and signature is already declared in the target type.
    ///     The default strategy is to fail with a compile-time error.</param>
    /// <param name="buildMethod"></param>
    /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
    /// <param name="tags">An optional opaque object of anonymous type passed to the template method and exposed under the <see cref="meta.Tags"/> property
    ///     of the <see cref="meta"/> API.</param>
    /// <returns>An <see cref="IMethodBuilder"/> that allows to modify the name or signature, or to add custom attributes.</returns>
    /// <seealso href="@introducing-members"/>
    public static IIntroductionAdviceResult<IMethod> IntroduceMethod(
        this IAdviser<IMemberOrNamedType> adviser,
        string template,
        IntroductionScope scope = IntroductionScope.Default,
        OverrideStrategy whenExists = OverrideStrategy.Default,
        Action<IMethodBuilder>? buildMethod = null,
        object? args = null,
        object? tags = null )
        => ((IAdvisableInternal) adviser).AdviceFactory.IntroduceMethod(
            adviser.Target.GetClosestNamedType() ?? throw new ArgumentOutOfRangeException(),
            template,
            scope,
            whenExists,
            buildMethod,
            args,
            tags );
}
// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using SymbolExtensions = Metalama.Framework.Engine.Utilities.Roslyn.SymbolExtensions;

namespace Metalama.Framework.Engine.Templating;

internal class TemplateMemberSymbolClassifier
{
    protected ITypeSymbol MetaType { get; }

    public ISymbolClassifier SymbolClassifier { get; }

    public TemplateMemberSymbolClassifier(
        Compilation runTimeCompilation,
        ProjectServiceProvider serviceProvider )
    {
        var compilationServices = serviceProvider.GetRequiredService<CompilationServicesFactory>().GetInstance( runTimeCompilation );
        this.SymbolClassifier = compilationServices.SymbolClassifier;
        var reflectionMapper = compilationServices.ReflectionMapper;
        this.MetaType = reflectionMapper.GetTypeSymbol( typeof(meta) );
    }

    public bool RequiresCompileTimeExecution( ISymbol? symbol )
        => symbol != null && this.SymbolClassifier.GetTemplatingScope( symbol ).GetExpressionExecutionScope()
            == TemplatingScope.CompileTimeOnly;

    public static bool IsDynamicParameter( ITypeSymbol? type )
        => type switch
        {
            null => false,
            IDynamicTypeSymbol => true,
            IArrayTypeSymbol { ElementType: IDynamicTypeSymbol } => true,
            _ => false
        };

    public static bool IsTemplateParameter( IParameterSymbol parameter )
        => parameter.ContainingSymbol is IMethodSymbol { MethodKind: not MethodKind.LambdaMethod and not MethodKind.AnonymousFunction } or IPropertySymbol
            or IEventSymbol;

    public bool IsRunTimeTemplateParameter( IParameterSymbol parameter )
        => IsTemplateParameter( parameter )
           && this.SymbolClassifier.GetTemplatingScope( parameter ).GetExpressionExecutionScope() != TemplatingScope.CompileTimeOnly;

    public static bool IsTemplateTypeParameter( ITypeParameterSymbol parameter )
        => parameter.ContainingSymbol is IMethodSymbol { MethodKind: not MethodKind.LambdaMethod and not MethodKind.AnonymousFunction } or IPropertySymbol
            or IEventSymbol;

    public bool IsCompileTimeTemplateTypeParameter( ITypeParameterSymbol typeParameter )
        => IsTemplateTypeParameter( typeParameter ) && this.SymbolClassifier.GetTemplatingScope( typeParameter ).GetExpressionExecutionScope()
            == TemplatingScope.CompileTimeOnly;

    public bool IsRunTimeTemplateTypeParameter( ITypeParameterSymbol typeParameter )
        => IsTemplateTypeParameter( typeParameter ) && this.SymbolClassifier.GetTemplatingScope( typeParameter ).GetExpressionExecutionScope()
            != TemplatingScope.CompileTimeOnly;

    public bool IsCompileTimeParameter( IParameterSymbol parameter )
        => this.SymbolClassifier.GetTemplatingScope( parameter ).GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly;

    public bool IsCompileTimeParameter( ITypeParameterSymbol parameter )
        => this.SymbolClassifier.GetTemplatingScope( parameter ).GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly;

    public bool IsRunTimeMethod( IMethodSymbol symbol )
        => symbol.Name == nameof(meta.RunTime) &&
           symbol.ContainingType.GetDocumentationCommentId() == this.MetaType.GetDocumentationCommentId();

    public bool HasTemplateKeywordAttribute( ISymbol symbol )
        => symbol.GetAttributes()
            .Any( a => a.AttributeClass != null && SymbolExtensions.AnyBaseType( a.AttributeClass, t => t.Name == nameof(TemplateKeywordAttribute) ) );

    public bool ReferencesCompileTemplateTypeParameter( ITypeSymbol symbol )
        => symbol switch
        {
            ITypeParameterSymbol typeParameter => this.IsCompileTimeTemplateTypeParameter( typeParameter ),
            IPointerTypeSymbol pointerType => this.ReferencesCompileTemplateTypeParameter( pointerType.PointedAtType ),
            IArrayTypeSymbol arrayType => this.ReferencesCompileTemplateTypeParameter( arrayType.ElementType ),
            INamedTypeSymbol namedType => namedType.TypeArguments.Any( this.ReferencesCompileTemplateTypeParameter ),
            _ => false
        };
}
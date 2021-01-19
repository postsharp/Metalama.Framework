using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Code
{
    [CompileTime]
    public interface IType : IDisplayable
    {
        TypeKind Kind { get; }

        bool Is( IType other );
        
        bool Is( Type other );

        IArrayType MakeArrayType( int rank = 1 );

        IPointerType MakePointerType();
    }

    [CompileTime]
    public interface IDisplayable
    {
        string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null );
    }

    /// <summary>
    /// 
    /// </summary>
    [CompileTime]
    public sealed class CodeDisplayFormat
    {
        
    }
    
    /// <summary>
    /// Specifies the context for which the display string must be generated.
    /// </summary>
    [CompileTime]
    public sealed class CodeDisplayContext
    {
        
    }
}
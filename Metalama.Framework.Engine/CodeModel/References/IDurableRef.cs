using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// An <see cref="IRef"/> that does not keep a reference to a <see cref="CompilationContext"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface IDurableRef<out T> : IRefImpl<T>
    where T : class, ICompilationElement { }
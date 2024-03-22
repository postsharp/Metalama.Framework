class C
{
  /// <inheritdoc/>
  [Range(1, 1000)]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia_DocComment.RequiredAttribute(ErrorMessage = "REQUIRED")]
  public int? PageSize { get; init; }
}
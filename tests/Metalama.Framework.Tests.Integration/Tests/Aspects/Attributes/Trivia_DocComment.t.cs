// Warning CS8618 on `ErrorMessage`: `Non-nullable property 'ErrorMessage' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.`
class C
{
  /// <inheritdoc/>
  [Range(1, 1000)]
  [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia_DocComment.RequiredAttribute(ErrorMessage = "REQUIRED")]
  public int? PageSize { get; init; }
}
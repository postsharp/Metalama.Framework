partial class TestTypes
{
  /// <summary>
  /// </summary>
  [Test]
  class C
  {
    /// <summary>
    /// </summary>
    [Test]
    public int? Field1;
    /// <summary>
    /// </summary>
    [Test]
    public int? Field2;
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    public int? Field3;
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    public int? Field4;
    /// <summary>
    /// </summary>
    [Test]
    public int? Property1 { get; }
    /// <summary>
    /// </summary>
    [Test]
    public int? Property2 { get; }
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    public int? Property3 { get; }
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    public int? Property4 { get; }
  }
  /// <summary>
  /// </summary>
  [Test]
  enum E
  {
    /// <summary>
    /// </summary>
    [Test]
    Value1,
    /// <summary>
    /// </summary>
    [Test]
    Value2,
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    Value3,
    /// <summary>
    /// </summary>
    [Existing]
    [Test]
    Value4
  }
}
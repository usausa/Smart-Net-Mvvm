namespace Smart.Mvvm.Generator.Tests;

using SourceGenerateHelper.Testing;

public sealed class PipelineCacheTests
{
    private const string Source =
        """
        using Smart.Mvvm;

        public partial class MyViewModel : ObservableObject
        {
            [ObservableProperty]
            public partial string Name { get; set; }
        }
        """;

    private const string UnrelatedSource =
        """
        namespace Other;

        internal sealed class Unrelated;
        """;

    private const string AddedTargetSource =
        """
        using Smart.Mvvm;

        public partial class AddedViewModel : ObservableObject
        {
            [ObservableProperty]
            public partial string Value { get; set; }
        }
        """;

    // ------------------------------------------------------------
    // Cache
    // ------------------------------------------------------------

    [Fact]
    public void UnrelatedEditKeepsModelCached()
    {
        // Arrange & Act
        var result = GeneratorTestHelper.RunIncremental(Source, UnrelatedSource);

        // Assert
        Assert.Equal(result.FirstGeneratedText, result.SecondGeneratedText);
        Assert.NotEmpty(result.OutputReasons);
        Assert.DoesNotContain(result.OutputReasons, static x => x.IsChanged());
    }

    [Fact]
    public void TargetEditRebuildsModel()
    {
        // Arrange & Act
        var result = GeneratorTestHelper.RunIncremental(Source, AddedTargetSource);

        // Assert
        Assert.Contains(result.OutputReasons, static x => x.IsChanged());
    }
}

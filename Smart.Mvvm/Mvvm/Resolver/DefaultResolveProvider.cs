namespace Smart.Mvvm.Resolver;

using System.Diagnostics.CodeAnalysis;

public sealed class DefaultResolveProvider : IServiceProvider
{
    [RequiresUnreferencedCode("Activator.CreateInstance may not work in trimmed applications. Use SetResolver to provide an AOT-compatible factory.")]
    [RequiresDynamicCode("Activator.CreateInstance requires dynamic code generation. Use SetResolver to provide an AOT-compatible factory.")]
    private static object? DefaultFactory(Type type) => Activator.CreateInstance(type);

    private Func<Type, object?> defaultResolver = DefaultFactory;

    public static DefaultResolveProvider Default { get; } = new();

    private DefaultResolveProvider()
    {
    }

    public void SetResolver(Func<Type, object?> resolver)
    {
        defaultResolver = resolver;
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "The default resolver can be replaced via SetResolver with an AOT-compatible implementation.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "The default resolver can be replaced via SetResolver with an AOT-compatible implementation.")]
    public object? GetService(Type serviceType) => defaultResolver(serviceType);
}

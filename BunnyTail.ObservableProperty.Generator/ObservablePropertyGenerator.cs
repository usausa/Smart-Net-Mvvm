namespace BunnyTail.ObservableProperty.Generator;

using System;
using System.Collections.Immutable;
using System.Text;

using BunnyTail.ObservableProperty.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using SourceGenerateHelper;

[Generator]
public sealed class ObservablePropertyGenerator : IIncrementalGenerator
{
    private const string AttributeName = "BunnyTail.ObservableProperty.ObservablePropertyAttribute";

    private const string NotifyAlsoAttributeName = "BunnyTail.ObservableProperty.NotifyAlsoAttribute";

    private const string NotifyPropertyChangedName = "System.ComponentModel.INotifyPropertyChanged";

    private const string PropertyChangedEventName = "PropertyChanged";
    private const string PropertyChangedEventHandlerName = "System.ComponentModel.PropertyChangedEventHandler";
    private const string PropertyChangedEventHandlerNullableName = "System.ComponentModel.PropertyChangedEventHandler?";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertyProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (syntax, _) => IsPropertySyntax(syntax),
                static (context, _) => GetPropertyModel(context))
            .Collect();

        context.RegisterImplementationSourceOutput(
            propertyProvider,
            static (context, provider) => Execute(context, provider));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static bool IsPropertySyntax(SyntaxNode syntax) =>
        syntax is PropertyDeclarationSyntax;

    private static Result<PropertyModel> GetPropertyModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (PropertyDeclarationSyntax)context.TargetNode;
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IPropertySymbol symbol)
        {
            return Results.Error<PropertyModel>(null);
        }

        // Validate property definition
        if (!symbol.IsPartialDefinition)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidPropertyDefinition, syntax.GetLocation(), symbol.Name));
        }

        // Validate type definition
        var containingType = symbol.ContainingType;
        if (!IsImplementNotifyPropertyChanged(containingType))
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidTypeDefinition, syntax.GetLocation(), containingType.Name));
        }

        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var hasPropertyChangedEvent = HasPropertyChangedEvent(containingType);
        var notifyAlso = GetNotifyAlsoPropertyNames(symbol);

        return Results.Success(new PropertyModel(
            ns,
            containingType.GetClassName(),
            containingType.IsValueType,
            hasPropertyChangedEvent,
            symbol.DeclaredAccessibility,
            symbol.Type.ToDisplayString(),
            symbol.Name,
            new EquatableArray<string>(notifyAlso.ToArray())));
    }

    private static bool IsImplementNotifyPropertyChanged(INamedTypeSymbol typeSymbol)
    {
        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            if (@interface.ToDisplayString() == NotifyPropertyChangedName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasPropertyChangedEvent(INamedTypeSymbol typeSymbol)
    {
        var symbol = typeSymbol;
        while (symbol is not null)
        {
            foreach (var member in symbol.GetMembers(PropertyChangedEventName))
            {
                if (member is IEventSymbol eventSymbol)
                {
                    var eventType = eventSymbol.Type.ToDisplayString();
                    if ((eventType == PropertyChangedEventHandlerName) ||
                        (eventType == PropertyChangedEventHandlerNullableName))
                    {
                        return true;
                    }
                }
            }

            symbol = symbol.BaseType;
        }

        return false;
    }

    private static string[] GetNotifyAlsoPropertyNames(IPropertySymbol typeSymbol)
    {
        var notifyAlso = new List<string>();

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == NotifyAlsoAttributeName)
            {
                foreach (var argument in attribute.ConstructorArguments)
                {
                    if (argument.Kind == TypedConstantKind.Array)
                    {
                        foreach (var value in argument.Values)
                        {
                            if (value.Value is string strValue)
                            {
                                notifyAlso.Add(strValue);
                            }
                        }
                    }
                    else if (argument.Value is string value)
                    {
                        notifyAlso.Add(value);
                    }
                }
            }
        }

        return notifyAlso.ToArray();
    }

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, ImmutableArray<Result<PropertyModel>> properties)
    {
        foreach (var info in properties.SelectError())
        {
            context.ReportDiagnostic(info);
        }

        var builder = new SourceBuilder();
        foreach (var group in properties.SelectValue().GroupBy(static x => new { x.Namespace, x.ClassName }))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            builder.Clear();
            BuildSource(builder, group.ToList());

            var filename = MakeFilename(group.Key.Namespace, group.Key.ClassName);
            var source = builder.ToString();
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static void BuildSource(SourceBuilder builder, List<PropertyModel> properties)
    {
        var ns = properties[0].Namespace;
        var className = properties[0].ClassName;
        var isValueType = properties[0].IsValueType;

        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            builder.Namespace(ns);
            builder.NewLine();
        }

        // class
        builder
            .Indent()
            .Append("partial ")
            .Append(isValueType ? "struct " : "class ")
            .Append(className)
            .NewLine();
        builder.BeginScope();

        var first = true;
        foreach (var property in properties)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.NewLine();
            }

            // property
            // TODO
            builder
                .Indent()
                .Append(property.PropertyAccessibility.ToText())
                .Append(" partial ")
                .Append(property.PropertyType)
                .Append(' ')
                .Append(property.PropertyName)
                .NewLine();
            builder.BeginScope();

            // getter
            builder
                .Indent()
                .Append("get => field;")
                .NewLine();
            // setter
            builder
                .Indent()
                .Append("set")
                .NewLine();
            builder.BeginScope();
            // TODO
            builder
                .Indent()
                .Append("field = value;")
                .NewLine();
            builder.EndScope();

            builder.EndScope();
        }

        builder.EndScope();
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className.Replace('<', '[').Replace('>', ']'));
        buffer.Append(".g.cs");

        return buffer.ToString();
    }
}

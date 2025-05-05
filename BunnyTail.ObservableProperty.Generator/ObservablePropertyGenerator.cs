namespace BunnyTail.ObservableProperty.Generator;

using System;
using System.Collections.Immutable;
using System.Text;

using BunnyTail.ObservableProperty.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

    private const string DefaultRaiseMethodName = "RaisePropertyChanged";

    private const string TriggerMethodNamesOption = "ObservablePropertyTriggerMethodNames";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertyProvider = context
            .ForAttributeWithMetadataNameWithOptions(
                AttributeName,
                static (syntax, _) => IsPropertySyntax(syntax),
                static (context, options, _) => GetPropertyModel(context, options))
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

    private static Result<PropertyModel> GetPropertyModel(GeneratorAttributeSyntaxContext context, AnalyzerConfigOptions options)
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

        if (symbol.SetMethod is null)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.PropertySetterRequired, syntax.GetLocation(), symbol.Name));
        }

        // Validate type definition
        var containingType = symbol.ContainingType;
        if (!IsImplementNotifyPropertyChanged(containingType))
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidTypeDefinition, syntax.GetLocation(), containingType.Name));
        }

        // Find trigger method
        var triggerMethod = FindTriggerMethod(containingType, options.GetValue<string>(TriggerMethodNamesOption));
        if (!triggerMethod.Find)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.TriggerMethodNotFound, syntax.GetLocation(), containingType.Name));
        }

        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var getterAccessibility = GetMethodAccessibility(symbol.GetMethod, symbol.DeclaredAccessibility);
        var setterAccessibility = GetMethodAccessibility(symbol.SetMethod, symbol.DeclaredAccessibility);
        var notifyAlso = GetNotifyAlsoPropertyNames(symbol);

        return Results.Success(new PropertyModel(
            ns,
            containingType.GetClassName(),
            containingType.IsValueType,
            triggerMethod.ImplementType,
            triggerMethod.MethodType,
            triggerMethod.MethodName,
            symbol.DeclaredAccessibility,
            symbol.Type.ToDisplayString(),
            symbol.Name,
            symbol.GetMethod is not null,
            getterAccessibility,
            setterAccessibility,
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

    private static (bool Find, ImplementType ImplementType, MethodType MethodType, string MethodName) FindTriggerMethod(INamedTypeSymbol typeSymbol, string methods)
    {
        var implementType = FindEventImplementType(typeSymbol);
        if (implementType is null)
        {
            return (true, ImplementType.None, MethodType.ByEventArgs, DefaultRaiseMethodName);
        }

        if (SymbolEqualityComparer.Default.Equals(typeSymbol, implementType))
        {
            // TODO 対象メソッドのライズメソッドを検索
            // ある EventTrigger ＆ その名前
            // なし Event、デフォルト名

            return (true, ImplementType.EventTrigger, MethodType.ByEventArgs, "RaisePropertyChanged");
        }

        // TODO 対象メソッドのライズメソッドを検索
        // ある EventTrigger ＆ その名前
        // なし エラー
        return (true, ImplementType.EventTrigger, MethodType.ByEventArgs, "RaisePropertyChanged");
    }

    private static INamedTypeSymbol? FindEventImplementType(INamedTypeSymbol typeSymbol)
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
                        return symbol;
                    }
                }
            }

            symbol = symbol.BaseType;
        }

        return null;
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

    private static Accessibility? GetMethodAccessibility(IMethodSymbol? symbol, Accessibility defaultAccessibility)
    {
        return ((symbol is not null) && (symbol.DeclaredAccessibility != defaultAccessibility)) ? symbol.DeclaredAccessibility : null;
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
        var implementType = properties[0].ImplementType;
        var methodType = properties[0].MethodType;
        var methodName = properties[0].MethodName;

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

        // event args
        var names = properties
            .Select(static x => x.PropertyName)
            .Concat(properties.SelectMany(static x => x.NotifyAlso.ToArray()))
            .Distinct()
            .OrderBy(static x => x);
        foreach (var name in names)
        {
            builder
                .Indent()
                .Append("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .NewLine();
            builder
                .Indent()
                .Append("private static readonly global::System.ComponentModel.PropertyChangedEventArgs ")
                .Append(GetEventArgsPropertyName(name))
                .Append(" = new(\"")
                .Append(name)
                .Append("\");")
                .NewLine();
        }

        if (!implementType.HasEvent())
        {
            builder
                .Indent()
                .Append("public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;")
                .NewLine();
            builder.NewLine();
        }

        if (!implementType.HasTrigger())
        {
            builder
                .Indent()
                .Append("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .NewLine();
            builder
                .Indent()
                .Append("private void ")
                .Append(methodName)
                .Append("(global::System.ComponentModel.PropertyChangedEventArgs args)")
                .NewLine();
            builder.BeginScope();
            builder
                .Indent()
                .Append("PropertyChanged?.Invoke(this, args);")
                .NewLine();
            builder.EndScope();
        }

        foreach (var property in properties)
        {
            builder.NewLine();

            // property
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
            if (property.HasGetter)
            {
                builder.Indent();
                if (property.GetterAccessibility is not null)
                {
                    builder.Append(property.GetterAccessibility.Value.ToText()).Append(" ");
                }
                builder
                    .Append("get => field;")
                    .NewLine();
            }

            // setter
            builder.Indent();
            if (property.SetterAccessibility is not null)
            {
                builder.Append(property.SetterAccessibility.Value.ToText()).Append(" ");
            }
            builder
                .Append("set")
                .NewLine();
            builder.BeginScope();
            builder
                .Indent()
                .Append("if (!global::System.Collections.Generic.EqualityComparer<")
                .Append(property.PropertyType)
                .Append(">.Default.Equals(field, value))")
                .NewLine();
            builder.BeginScope();
            builder
                .Indent()
                .Append("field = value;")
                .NewLine();
            foreach (var name in new[] { property.PropertyName }.Concat(property.NotifyAlso.ToArray()).Distinct())
            {
                builder
                    .Indent()
                    .Append(methodName)
                    .Append('(');
                if (methodType is MethodType.ByEventArgsWithSender or MethodType.ByNameWithSender)
                {
                    builder.Append("this, ");
                }
                if (methodType is MethodType.ByName or MethodType.ByNameWithSender)
                {
                    builder.Append("\"").Append(name).Append("\"");
                }
                else
                {
                    builder
                        .Append(GetEventArgsPropertyName(name));
                }
                builder
                    .Append(");")
                    .NewLine();
            }
            builder.EndScope();
            builder.EndScope();

            builder.EndScope();
        }

        builder.EndScope();
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static string GetEventArgsPropertyName(string name) =>
        $"__{name}ChangedEventArgs";

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

namespace Smart.Mvvm.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Smart.Mvvm.Generator.Models;

using SourceGenerateHelper;

[Generator]
public sealed class ObservablePropertyGenerator : IIncrementalGenerator
{
    private const string AttributeName = "Smart.Mvvm.ObservablePropertyAttribute";
    private const string NotifyAlsoPropertyName = "NotifyAlso";

    private const string ObservableGeneratorOptionAttributeName = "Smart.Mvvm.ObservableGeneratorOptionAttribute";
    private const string ReactivePropertyName = "Reactive";
    private const string ViewModelPropertyName = "ViewModel";

    private const string ObservableObjectName = "Smart.Mvvm.ObservableObject";
    private const string ViewModelBaseName = "Smart.Mvvm.ViewModels.ViewModelBase";
    private const string TriggerMethodName = "RaisePropertyChanged";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertyProvider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (syntax, _) => IsPropertySyntax(syntax),
                static (context, _) => GetPropertyModel(context))
            .Collect();

        var treeProvider = context.ForAttributeWithMetadataNameSyntaxTrees(
            AttributeName,
            static (syntax, _) => IsPropertySyntax(syntax));

        context.RegisterSourceOutput(
            propertyProvider.Combine(treeProvider),
            static (context, provider) => ReportDiagnostics(context, provider.Left, provider.Right));

        var types = propertyProvider.SelectMany(static (properties, _) => SelectTypes(properties));
        context.RegisterImplementationSourceOutput(types, static (context, type) => Execute(context, type));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static bool IsPropertySyntax(SyntaxNode syntax) =>
        syntax is PropertyDeclarationSyntax or IndexerDeclarationSyntax;

    private static Result<PropertyModel> GetPropertyModel(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is IndexerDeclarationSyntax indexer)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidPropertyDefinition, indexer.ThisKeyword.GetLocation(), "this[]"));
        }

        var syntax = (PropertyDeclarationSyntax)context.TargetNode;
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not { } symbol)
        {
            return Results.Errors<PropertyModel>();
        }

        // Validate property definition
        if (!symbol.IsPartialDefinition || (symbol.PartialImplementationPart is not null) || symbol.IsStatic)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidPropertyDefinition, syntax.Identifier.GetLocation(), symbol.Name));
        }

        string? getter = null;
        string? setter = null;
        foreach (var accessor in syntax.AccessorList?.Accessors ?? default)
        {
            if (accessor.Keyword.IsKind(SyntaxKind.GetKeyword))
            {
                getter = accessor.GetImplementationAccessor();
            }
            else if (accessor.Keyword.IsKind(SyntaxKind.SetKeyword) || accessor.Keyword.IsKind(SyntaxKind.InitKeyword))
            {
                setter = accessor.GetImplementationAccessor();
            }
        }

        if ((symbol.SetMethod is null) || (setter is null))
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.PropertySetterRequired, syntax.Identifier.GetLocation(), symbol.Name));
        }

        // Validate type definition
        var containingType = symbol.ContainingType;
        var (isObservableObject, isViewModelBase) = GetBaseTypes(containingType);
        if (!isObservableObject)
        {
            return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.InvalidTypeDefinition, syntax.Identifier.GetLocation(), containingType.Name));
        }

        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();
        var (isReactive, isViewModel, optionOwner, optionLocation) = GetGeneratorOptions(containingType);
        var notifyAlso = GetNotifyAlsoPropertyNames(symbol);

        // Build the containing type hierarchy
        string[] containingTypes;
        string typeKey;
        if (containingType.ContainingType is null)
        {
            if (!IsPartialType(containingType) || containingType.IsFileLocal)
            {
                return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.PartialContainingTypeRequired, syntax.Identifier.GetLocation(), containingType.Name));
            }

            containingTypes = [$"{containingType.GetDeclarationKeyword()} {containingType.GetClassName()}"];
            typeKey = containingType.MetadataName;
        }
        else
        {
            // Nested type
            List<INamedTypeSymbol> typeHierarchy = [.. containingType.GetContainingTypes(), containingType];

            containingTypes = new string[typeHierarchy.Count];
            var keyParts = new string[typeHierarchy.Count];
            for (var i = 0; i < typeHierarchy.Count; i++)
            {
                var type = typeHierarchy[i];
                if (!IsPartialType(type) || type.IsFileLocal)
                {
                    return Results.Error<PropertyModel>(new DiagnosticInfo(Diagnostics.PartialContainingTypeRequired, syntax.Identifier.GetLocation(), type.Name));
                }

                containingTypes[i] = $"{type.GetDeclarationKeyword()} {type.GetClassName()}";
                keyParts[i] = type.MetadataName;
            }

            typeKey = String.Join(".", keyParts);
        }

        return Results.Success(new PropertyModel(
            ns,
            typeKey,
            new EquatableArray<string>(containingTypes),
            containingType.IsSealed,
            isReactive,
            isViewModel,
            isViewModelBase,
            optionOwner,
            optionLocation,
            containingType.ToDisplayString(),
            containingType.Locations.FirstOrDefault() is { } typeLocation ? LocationInfo.CreateFrom(typeLocation) : null,
            symbol.GetImplementationSignature(syntax),
            symbol.Type.ToDisplayString(SymbolDisplayFormats.FullyQualifiedNullable),
            symbol.Name,
            getter,
            setter,
            new EquatableArray<string>(notifyAlso)));
    }

    private static bool IsPartialType(INamedTypeSymbol symbol)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            if ((reference.GetSyntax() is TypeDeclarationSyntax declaration) &&
                !declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return false;
            }
        }

        return true;
    }

    private static (bool IsObservableObject, bool IsViewModelBase) GetBaseTypes(INamedTypeSymbol typeSymbol)
    {
        var isViewModelBase = false;
        var symbol = typeSymbol.BaseType;
        while (symbol is not null)
        {
            var name = symbol.ToDisplayString();
            if (name == ViewModelBaseName)
            {
                isViewModelBase = true;
            }
            else if (name == ObservableObjectName)
            {
                return (true, isViewModelBase);
            }

            symbol = symbol.BaseType;
        }

        return (false, false);
    }

    private static string[] GetNotifyAlsoPropertyNames(IPropertySymbol symbol)
    {
        var list = new List<string>();

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != AttributeName)
            {
                continue;
            }

            if (attribute.TryGetNamedArgument(NotifyAlsoPropertyName, out var names) && names.TryGetValues<string>(out var values))
            {
                list.AddRange(values);
            }
        }

#pragma warning disable IDE0028
        return list.ToArray();
#pragma warning restore IDE0028
    }

    private static (bool IsReactive, bool IsViewModel, string Owner, LocationInfo? Location) GetGeneratorOptions(ITypeSymbol typeSymbol)
    {
        var symbol = typeSymbol;
        while (symbol is not null)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                var isReactive = false;
                var isViewModel = false;

                if (attribute.AttributeClass?.ToDisplayString() != ObservableGeneratorOptionAttributeName)
                {
                    continue;
                }

                foreach (var argument in attribute.NamedArguments)
                {
                    if ((argument.Key == ReactivePropertyName) && argument.Value.TryGetValue<bool>(out var reactive))
                    {
                        isReactive = reactive;
                    }
                    else if ((argument.Key == ViewModelPropertyName) && argument.Value.TryGetValue<bool>(out var viewModel))
                    {
                        isViewModel = viewModel;
                    }
                }

                var location = attribute.ApplicationSyntaxReference?.GetSyntax() is { } node ? LocationInfo.CreateFrom(node) : null;
                return (isReactive, isViewModel, symbol.ToDisplayString(), location);
            }

            symbol = symbol.BaseType;
        }

        return (false, false, string.Empty, null);
    }

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    private static void ReportDiagnostics(SourceProductionContext context, ImmutableArray<Result<PropertyModel>> properties, ImmutableArray<SyntaxTree> trees)
    {
        var diagnostics = properties.SelectError().ToList();

        foreach (var group in properties.SelectValue().GroupBy(static x => new { x.Namespace, x.TypeKey }))
        {
            var model = group.First();

            // The ViewModel option only produces Subscribe methods, which require the Reactive option
            if (model.IsViewModel && (!model.IsReactive || !model.IsViewModelBase))
            {
                diagnostics.Add(model.OptionLocation is not null
                    ? new DiagnosticInfo(Diagnostics.ViewModelOptionRequiresReactive, model.OptionLocation, model.OptionOwner)
                    : new DiagnosticInfo(Diagnostics.ViewModelOptionRequiresReactive, model.TypeLocation, model.TypeName));
            }
        }

        context.ReportDiagnostics(diagnostics.Concat(FindHintNameCollisions(properties).Values).Distinct(), trees);
    }

    private static ImmutableArray<TypeModel> SelectTypes(ImmutableArray<Result<PropertyModel>> properties)
    {
        var collisions = FindHintNameCollisions(properties);
        return properties.SelectValue()
            .GroupBy(static x => new { x.Namespace, x.TypeKey })
            .Where(x => !collisions.ContainsKey(GetHintName(x.Key.Namespace, x.Key.TypeKey)))
            .Select(static x => new TypeModel(x.Key.Namespace, x.Key.TypeKey, new EquatableArray<PropertyModel>(x)))
            .ToImmutableArray();
    }

    private static Dictionary<string, DiagnosticInfo> FindHintNameCollisions(ImmutableArray<Result<PropertyModel>> properties)
    {
        var collisions = new Dictionary<string, DiagnosticInfo>(StringComparer.Ordinal);
        var firsts = new Dictionary<string, (string HintName, string TypeName)>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in properties.SelectValue().OrderBy(static x => GetHintName(x.Namespace, x.TypeKey), StringComparer.Ordinal))
        {
            var hintName = GetHintName(property.Namespace, property.TypeKey);
            if (!firsts.TryGetValue(hintName, out var first))
            {
                firsts.Add(hintName, (hintName, property.TypeName));
            }
            else if ((first.HintName != hintName) && !collisions.ContainsKey(hintName))
            {
                collisions.Add(hintName, new DiagnosticInfo(Diagnostics.HintNameCollision, (Location?)null, property.TypeName, first.TypeName));
            }
        }

        return collisions;
    }

    private static string GetHintName(string ns, string typeKey) =>
        HintNameBuilder.Build(ns, typeKey);

    private static void Execute(SourceProductionContext context, TypeModel model)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var builder = new SourceBuilder();
#pragma warning disable IDE0028
        BuildSource(builder, model.Properties.ToList());
#pragma warning restore IDE0028

        context.AddSource(GetHintName(model.Namespace, model.TypeKey), builder);
    }

    private static void BuildSource(SourceBuilder builder, List<PropertyModel> properties)
    {
        var ns = properties[0].Namespace;
        var containingTypes = properties[0].ContainingTypes;
        var isSealed = properties[0].IsSealed;
        var isReactive = properties[0].IsReactive;
        var isViewModel = properties[0].IsViewModel && properties[0].IsViewModelBase;

        builder.AutoGenerated();
        builder.EnableNullable();
        builder.Disable("CS8618");
        builder.Disable("CS9264");
        builder.Disable("CS0612, CS0618");
        builder.NewLine();

        // namespace
        if (!String.IsNullOrEmpty(ns))
        {
            builder.Namespace(ns);
            builder.NewLine();
        }

        // type (including the containing type hierarchy for nested types)
        foreach (var containingType in containingTypes)
        {
            builder
                .Indent()
                .Append("partial ")
                .Append(containingType)
                .NewLine();
            builder.BeginScope();
        }

        // event args
        var names = properties
            .Select(static x => x.PropertyName)
            .Concat(properties.SelectMany(static x => x.NotifyAlso))
            .Distinct()
            .OrderBy(static x => x, StringComparer.Ordinal)
            .ToList();
        var eventArgsFields = BuildEventArgsFieldNames(names);
        foreach (var name in names)
        {
            builder
                .Indent()
                .Append("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .NewLine();
            builder
                .Indent()
                .Append("private static readonly global::System.ComponentModel.PropertyChangedEventArgs ")
                .Append(GetEventArgsField(eventArgsFields, name))
                .Append(" = new(")
                .Append(SymbolDisplay.FormatLiteral(name, true))
                .Append(");")
                .NewLine();
        }

        foreach (var property in properties)
        {
            builder.NewLine();

            // property
            builder
                .Indent()
                .Append(property.Signature)
                .NewLine();
            builder.BeginScope();

            // getter
            if (property.Getter is not null)
            {
                builder
                    .Indent()
                    .Append(property.Getter)
                    .Append(" => field;")
                    .NewLine();
            }

            // setter
            builder
                .Indent()
                .Append(property.Setter)
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
            foreach (var name in new[] { property.PropertyName }.Concat(property.NotifyAlso).Distinct())
            {
                builder
                    .Indent()
                    .Append(TriggerMethodName)
                    .Append('(')
                    .Append(GetEventArgsField(eventArgsFields, name))
                    .Append(");")
                    .NewLine();
            }
            builder.EndScope();
            builder.EndScope();

            builder.EndScope();
        }

        // Reactive option
        if (isReactive)
        {
            foreach (var property in properties)
            {
                builder.NewLine();

                // method
                builder
                    .Indent()
                    .Append(isSealed ? "private" : "protected")
                    .Append(" global::System.IObservable<")
                    .Append(property.PropertyType)
                    .Append("> Observe")
                    .Append(property.PropertyName)
                    .Append("()")
                    .NewLine();
                builder.BeginScope();

                builder
                    .Indent()
                    .Append("return global::System.Reactive.Linq.Observable.Select(")
                    .NewLine();
                builder.IndentLevel++;
                builder
                    .Indent()
                    .Append("global::System.Reactive.Linq.Observable.Where(")
                    .NewLine();
                builder.IndentLevel++;
                builder
                    .Indent()
                    .Append("global::System.Reactive.Linq.Observable.FromEvent<global::System.ComponentModel.PropertyChangedEventHandler, global::System.ComponentModel.PropertyChangedEventArgs>(")
                    .NewLine();
                builder.IndentLevel++;
                builder
                    .Indent()
                    .Append("static h => (_, e) => h(e),")
                    .NewLine();
                builder
                    .Indent()
                    .Append("h => PropertyChanged += h,")
                    .NewLine();
                builder
                    .Indent()
                    .Append("h => PropertyChanged -= h),")
                    .NewLine();
                builder.IndentLevel--;
                builder
                    .Indent()
                    .Append("static x => x.PropertyName == ")
                    .Append(SymbolDisplay.FormatLiteral(property.PropertyName, true))
                    .Append("),")
                    .NewLine();
                builder.IndentLevel--;
                builder
                    .Indent()
                    .Append("_ => ")
                    .Append(CSharpIdentifier.Escape(property.PropertyName))
                    .Append(");")
                    .NewLine();
                builder.IndentLevel--;

                builder.EndScope();
            }
        }

        // ViewModel option
        if (isViewModel && isReactive)
        {
            foreach (var property in properties)
            {
                builder.NewLine();

                // method
                builder
                    .Indent()
                    .Append(isSealed ? "private" : "protected")
                    .Append(" void Subscribe")
                    .Append(property.PropertyName)
                    .Append("(global::System.Action<")
                    .Append(property.PropertyType)
                    .Append("> action)")
                    .NewLine();
                builder.BeginScope();

                builder
                    .Indent()
                    .Append("Disposables.Add(global::System.ObservableExtensions.Subscribe(Observe")
                    .Append(property.PropertyName)
                    .Append("(), action));")
                    .NewLine();

                builder.EndScope();
            }
        }

        // close the type hierarchy
        for (var i = 0; i < containingTypes.Count; i++)
        {
            builder.EndScope();
        }
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static Dictionary<string, string>? BuildEventArgsFieldNames(List<string> names)
    {
        var requiresFallback = false;
        foreach (var name in names)
        {
            if (!SyntaxFacts.IsValidIdentifier(name))
            {
                requiresFallback = true;
                break;
            }
        }

        if (!requiresFallback)
        {
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);
        var sequence = 0;

        foreach (var name in names)
        {
            var candidate = SyntaxFacts.IsValidIdentifier(name) ? $"__{name}ChangedEventArgs" : null;
            if ((candidate is null) || !used.Add(candidate))
            {
                do
                {
                    candidate = $"__NotifyEventArgs{sequence}";
                    sequence++;
                }
                while (!used.Add(candidate));
            }

            map.Add(name, candidate);
        }

        return map;
    }

    private static string GetEventArgsField(Dictionary<string, string>? fieldNames, string name) =>
        fieldNames is not null ? fieldNames[name] : $"__{name}ChangedEventArgs";
}

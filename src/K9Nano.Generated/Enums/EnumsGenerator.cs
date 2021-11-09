using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace K9Nano.Generated
{
    /// <summary>
    /// 为枚举生成获取描述的扩展方法
    /// </summary>
    [Generator]
    public class EnumsGenerator : ISourceGenerator
    {
        private const string AttributeText = @"
using System;
namespace K9Nano.Generated
{
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class DescriptionGeneratorAttribute : Attribute
    {
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            Debug.WriteLine("Initalize code generator");
#endif
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // add the attribute text
            context.AddSource("DescriptionGeneratorAttribute", AttributeText);

            // retreive the populated receiver 
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;
            var sb = new StringBuilder();
            sb.AppendLine(@"
using System;
using System.Text;
using System.Collections.Generic;

namespace K9Nano.Generated
{
    public static class EnumExtensions
    {"
            );

            foreach (var enumDeclarationSyntax in receiver.CandidateClasses)
            {
                var namespaceName = GetNamespace(enumDeclarationSyntax.Parent);
                if (namespaceName == null)
                {
                    throw new Exception($"Can not find namespace of {enumDeclarationSyntax.Identifier.ValueText}");
                }
                var methodSourceText = GenerateMethod(namespaceName, enumDeclarationSyntax);
                sb.AppendLine(methodSourceText);
            }

            sb.AppendLine(@"
    }
}
");
            var sourceText = sb.ToString();
            context.AddSource("EnumExtensionsGenerated", sourceText);
        }

        private static string? GetNamespace(SyntaxNode? node)
        {
            var ns = node;
            while (true)
            {
                if (ns == null) return null;

                if (ns is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    return namespaceDeclarationSyntax.Name.ToString();
                }

                ns = ns.Parent;
            }
        }

        private static string GenerateMethod(string namespaceName, EnumDeclarationSyntax enumDeclarationSyntax)
        {
            var typeName = namespaceName + '.' + enumDeclarationSyntax.Identifier.ValueText;

            var fields = enumDeclarationSyntax.Members.Select(member =>
                {
                    var name = member.Identifier.Text;

                    var descriptionAttribute = member.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .FirstOrDefault(a => a.Name is IdentifierNameSyntax n && n.Identifier.Text == "Description");
                    var description = descriptionAttribute?.ArgumentList?.Arguments.FirstOrDefault()?.ToString()
                                      ?? $"\"{name}\"";
                    return new EnumFieldMetadata(name, description);
                })
                .ToList();

            var sb = new StringBuilder();

            // GetDescription
            sb.AppendLine($@"
public static string GetDescription(this {typeName} obj)
{{
    return obj switch
    {{
"
            );

            foreach (var f in fields)
            {
                sb.AppendLine($"{typeName}.{f.Name} => {f.Description},");
            }

            sb.AppendLine(@"                
        _ => obj.ToString()
    };
}"
            );

            sb.AppendLine($@"

public static IEnumerable<KeyValuePair<int, string>> GetValuesAndDescriptions(this {typeName} obj)
{{
    return new KeyValuePair<int, string>[]
    {{
"
            );

            foreach (var f in fields)
            {
                sb.AppendLine($"new ((int){typeName}.{f.Name}, {f.Description}),");
            }

            sb.AppendLine(@"
    };
}"
            );

            var sourceText = sb.ToString();
            return sourceText;
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<EnumDeclarationSyntax> CandidateClasses { get; } = new();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is EnumDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Any(
                        a => a.Attributes
                            .Any(b => (b.Name as IdentifierNameSyntax)?.Identifier.ValueText == "DescriptionGenerator")))
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }

        private class EnumFieldMetadata
        {
            public string Name { get; }
            
            public string Description { get; }

            public EnumFieldMetadata(string name, string description)
            {
                Name = name;
                Description = description;
            }
        }
    }
}
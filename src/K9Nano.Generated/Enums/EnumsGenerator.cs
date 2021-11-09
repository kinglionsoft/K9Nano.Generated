using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            //Debug.WriteLine("Initalize code generator");
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


            foreach (var enumDeclarationSyntax in receiver.CandidateClasses)
            {
                var className = enumDeclarationSyntax.Identifier.ValueText;
                var namespaceName = GetNamespace(enumDeclarationSyntax.Parent);
                var sourceText = Generate(namespaceName, enumDeclarationSyntax);
                var fileName = namespaceName.Replace('.', '_') + '_' + className;
                context.AddSource($"{fileName}Generated", sourceText);
            }
        }

        private static string GetNamespace(SyntaxNode node)
        {
            var ns = node;
            while (true)
            {
                if (ns is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    if (namespaceDeclarationSyntax.Name is IdentifierNameSyntax identifierNameSyntax)
                    {
                        return identifierNameSyntax.Identifier.ToString();
                    }
                }

                ns = ns.Parent;
                if (ns == null) return null;
            }
        }

        private static string Generate(string namespaceName, EnumDeclarationSyntax enumDeclarationSyntax)
        {
            var typeName = namespaceName + '.' + enumDeclarationSyntax.Identifier.ValueText;

            var sb = new StringBuilder();
            sb.AppendLine($@"
using System;
using System.Text;

namespace K9Nano.Generated
{{
    public static class EnumExtensions
    {{
        public static string GetDescription(this {typeName} obj)
        {{
            return obj switch
            {{
"
            );

            foreach (var member in enumDeclarationSyntax.Members)
            {
                var name = member.Identifier.Text;
                var description = member.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .FirstOrDefault(a => a.Name is IdentifierNameSyntax n && n.Identifier.Text == "Description");
                var text = description?.ArgumentList?.Arguments.FirstOrDefault()?.ToString()
                           ?? $"\"{name}\"";

                sb.AppendLine($"{typeName}.{name} => {text},");
            }

            sb.AppendLine(@"                
                _ => obj.ToString()
            };
        }
    }
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
    }
}
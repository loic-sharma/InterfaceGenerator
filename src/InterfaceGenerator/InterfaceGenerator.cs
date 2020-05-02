using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace InterfaceGenerator
{
    [Generator]
    public class InterfaceGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            var syntaxReceiver = (SyntaxReceiver)context.SyntaxReceiver;

            foreach (var classDeclaration in syntaxReceiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var interfaceName = "I" + classDeclaration.Identifier.Text;

                if (ShouldGenerateInterface(model, classDeclaration, interfaceName))
                {
                    var interfaceSource = GenerateInterfaceSource(model, classDeclaration, interfaceName);

                    context.AddSource($"{interfaceName}.cs", SourceText.From(interfaceSource, Encoding.UTF8));
                }
            }
        }

        private bool ShouldGenerateInterface(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            string interfaceName)
        {
            // Determine if the class declaration implements an interface that should be generated.
            foreach (var baseTypeSyntax in classDeclaration.BaseList.Types)
            {
                // Check the name of the interface.
                if (!(baseTypeSyntax.Type is IdentifierNameSyntax baseName))
                {
                    continue;
                }

                if (!baseName.Identifier.ValueText.Equals(interfaceName))
                {
                    continue;
                }

                // Check that the interface does not exist.
                var baseTypeInfo = model.GetTypeInfo(baseTypeSyntax.Type);

                return baseTypeInfo.Type.TypeKind == TypeKind.Error;
            }

            return false;
        }

        private string GenerateInterfaceSource(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            string interfaceName)
        {
            var classInfo = model.GetDeclaredSymbol(classDeclaration);
            var namespaceName = classInfo.ContainingNamespace.Name;

            var source = new StringBuilder($@"
namespace {namespaceName}
{{
    public interface {interfaceName}
    {{");

            foreach (var member in classDeclaration.Members)
            {
                if (!(member is MethodDeclarationSyntax methodDeclaration)) continue;

                source.Append(methodDeclaration.ReturnType.ToFullString());
                source.Append(methodDeclaration.Identifier.Text);
                source.Append(methodDeclaration.TypeParameterList?.ToFullString() ?? string.Empty);
                source.Append(methodDeclaration.ParameterList.ToFullString());
                source.Append(";");
                source.AppendLine();
            }

            source.Append("    }");
            source.Append("}");

            return source.ToString();
        }
    }

    public class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is ClassDeclarationSyntax classDeclaration))
            {
                return;
            }

            if (classDeclaration.BaseList == null)
            {
                return;
            }

            if (!classDeclaration.BaseList.Types.Any())
            {
                return;
            }

            // Only add this class if it extends a type prefixed with "I".
            var targetType = "I" + classDeclaration.Identifier.Text;

            foreach (var baseType in classDeclaration.BaseList.Types)
            {
                if (baseType.Type is IdentifierNameSyntax parentName)
                {
                    if (parentName.Identifier.ValueText == targetType)
                    {
                        CandidateClasses.Add(classDeclaration);
                        return;
                    }
                }
            }
        }
    }

}
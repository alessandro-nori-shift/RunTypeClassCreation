using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DynamicClassCreation.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynClassCreation.RoslynSyntaxTree;

public class RoslynTypeBuilder
{
    private static readonly IEnumerable<MetadataReference> DefaultReferences =
        new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };
    
    private readonly string _namespace;
    private readonly string _className;
    private readonly CSharpSyntaxNode _syntaxTreeRoot;

    private Type? _resultType;

    public RoslynTypeBuilder(string className, params Property[] properties)
    {
        _namespace = "MyNewNamespace." + className;
        _className = className;
        _syntaxTreeRoot = GetSyntaxTreeRoot(className, properties);
    }

    private CompilationUnitSyntax GetSyntaxTreeRoot(string className, IEnumerable<Property> properties)
    {
        // Create CompilationUnitSyntax
        var root = SyntaxFactory.CompilationUnit();

        // using System;
        root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

        // namespace MyNewNamespace.<className>
        var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_namespace));


        // public <className>
        var classDeclaration = SyntaxFactory.ClassDeclaration(className);
        classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var propertyDeclarations =
            properties.Select(p => SyntaxFactory
                    .PropertyDeclaration(SyntaxFactory.ParseTypeName(p.ValueType.ToString()), p.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), SyntaxFactory
                        .AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))))
                .Cast<MemberDeclarationSyntax>()
                .ToArray();

        classDeclaration = classDeclaration.AddMembers(propertyDeclarations);
        @namespace = @namespace.AddMembers(classDeclaration);

        root = root.AddMembers(@namespace).NormalizeWhitespace();

        return root;
    }

    public string ClassCode() => _syntaxTreeRoot.ToFullString();

    private Type? CompileInternal()
    {
        var compilation = CSharpCompilation
            .Create(_namespace)
            .AddSyntaxTrees(_syntaxTreeRoot.SyntaxTree)
            .AddReferences(DefaultReferences)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            throw new ApplicationException(failures.First().GetMessage());
            var exceptions = failures.Select(
                    diagnostic => new CompilationException(diagnostic.Id, diagnostic.GetMessage()))
                .ToList();

            throw new AggregateException("Encountered the following exceptions while trying to compile:", exceptions);
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var newTypeFullName = $"{_namespace}.{_className}";

        var type = assembly.GetType(newTypeFullName);

        return type;
    }

    public Type? Compile()
    {
        return _resultType ??= CompileInternal();
    }

    public class CompilationException : Exception
    {
        private readonly string _id;
        private readonly string _message;

        public CompilationException(string id, string message)
        {
            _id = id;
            _message = message;
        }

        public override string ToString() => $"{_id} : {_message}";
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LoxSharp.Generated
{
    [Generator]
    public class GrammarGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            GenerateExpr(context);

            var grammar = new string[] {
                "Binary   : Expr Left, Token Op, Expr Right",
                "Grouping : Expr Expression",
                "Literal  : Object Value",
                "Unary    : Token Op, Expr Right"
            };

            var classDefinitions = grammar.Select(ParseDefinition).ToList();

            GenerateVisitorInterface(classDefinitions,"Expr", context);
            classDefinitions.ForEach(def => GenerateClass(def, "Expr", context));
        }

        private void GenerateExpr(GeneratorExecutionContext context)
        {
            var source = @"
namespace LoxSharp.Grammar
{
    public abstract class Expr
    {
        public abstract T Accept<T>(Visitor<T> visitor);
    }
}
";

            context.AddSource("Expr", SourceText.From(source, Encoding.UTF8));
        }

        private (string, IEnumerable<string>) ParseDefinition(string definition)
        {
            var definitionArray = definition.Split(':')
                .Select(s => s.Trim())
                .ToArray();

            var className = definitionArray[0];
            var properties = definitionArray[1].Split(',');

            return (className, properties);
        }

        private void GenerateVisitorInterface(List<(string ClassName, IEnumerable<string> Properties)> classDefinitions, string baseName, GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using System;
using LoxSharp.Common.Parser;

namespace LoxSharp.Grammar
{
    public interface Visitor<T> 
    {
");

            foreach (var classDefinition in classDefinitions)
            {
                var methodName = $"Visit{classDefinition.ClassName}{baseName}";
                sourceBuilder.AppendLine($"\tT {methodName}({classDefinition.ClassName} {classDefinition.ClassName.ToLower().First()});");
            }

            sourceBuilder.AppendLine(@"
    }
}");

            context.AddSource("Visitor", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private void GenerateClass((string className, IEnumerable<string> properties) classDefinition, string baseName, GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder($@"
using System;
using LoxSharp.Common.Parser;

namespace LoxSharp.Grammar
{{
    public class {classDefinition.className} : Expr
    {{
            ");

            var ctorParams = new List<string>();
            var assignments = new List<string>();

            foreach (var propStr in classDefinition.properties)
            {
                var propArray = propStr.Trim().Split(' ');
                var propType = propArray[0].Trim();
                var propName = propArray[1].Trim();
                
                sourceBuilder.Append($@"
                    public {propType} {propName} {{ get; set; }}
                ");

                ctorParams.Add($"{propType} {propName.ToLower()}");
                assignments.Add($"{propName} = {propName.ToLower()};");
            }

            sourceBuilder.Append($@"
                public {classDefinition.className}(
                    {string.Join(",", ctorParams)})
                {{
                    {string.Join("\n", assignments)}
                }}
                ");

            sourceBuilder.AppendLine($@"
                public override T Accept<T>(Visitor<T> visitor) {{
                    return visitor.Visit{classDefinition.className}{baseName}(this);
                }}");

            sourceBuilder.AppendLine(@"
    }
}
");

            //var xxx = sourceBuilder.ToString();
            //Debugger.Launch();

            context.AddSource(classDefinition.className, SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // deliberately left empty - not needed
        }
    }

}

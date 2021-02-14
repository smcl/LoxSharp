using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoxSharp.Generated
{
    public abstract class BaseGrammarGenerator 
    {
        protected void GenerateGrammar(GeneratorExecutionContext context, string baseName, string[] elements)
        {
            var classDefinitions = elements.Select(ParseDefinition).ToList();
            GenerateBaseClass(baseName, context);
            GenerateVisitorInterface(classDefinitions, baseName, context);
            classDefinitions.ForEach(def => GenerateClass(def, baseName, context));
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

        private void GenerateBaseClass(string baseName, GeneratorExecutionContext context)
        {
            var source = $@"
namespace LoxSharp.Grammar
{{
    public abstract class {baseName}
    {{
        public abstract T Accept<T>({baseName}Visitor <T> visitor);
    }}
}}";

            context.AddSource(baseName, SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateVisitorInterface(List<(string ClassName, IEnumerable<string> Properties)> classDefinitions, string baseName, GeneratorExecutionContext context)
        {
            var classContents = new StringBuilder();

            foreach (var (className, _) in classDefinitions)
            {
                var methodName = $"Visit{className}{baseName}";
                classContents.AppendLine($"\tT {methodName}({className} {baseName.ToLower().First()});");
            }

            var source = $@"
using System;
using LoxSharp.Common.Parser;

namespace LoxSharp.Grammar
{{
            public interface {baseName}Visitor<T> 
            {{
                {classContents}
            }}
}}";

            context.AddSource($"{baseName}Visitor", SourceText.From(source, Encoding.UTF8));
        }

        private void GenerateClass((string className, IEnumerable<string> properties) classDefinition, string baseName, GeneratorExecutionContext context)
        {
            var properties = new List<string>();
            var ctorParams = new List<string>();
            var assignments = new List<string>();

            foreach (var propStr in classDefinition.properties)
            {
                var propArray = propStr.Trim().Split(' ');
                var propType = propArray[0].Trim();
                var propName = propArray[1].Trim();

                properties.Add($"public {propType} {propName} {{ get; set; }}");
                ctorParams.Add($"{propType} {propName.ToLower()}");
                assignments.Add($"{propName} = {propName.ToLower()};");
            }

            var classBuilder = new StringBuilder($@"
using System;
using System.Collections.Generic;
using LoxSharp.Common.Parser;

namespace LoxSharp.Grammar
{{
    public class {classDefinition.className} : {baseName}
    {{
        {string.Join("\n", properties)}

        public {classDefinition.className}({string.Join(",", ctorParams)}) 
        {{
            {string.Join("\n", assignments)}
        }}

        public override T Accept<T>({baseName}Visitor <T> visitor) {{
            return visitor.Visit{classDefinition.className}{baseName}(this);
        }}
    }}
}}");

            context.AddSource(classDefinition.className, SourceText.From(classBuilder.ToString(), Encoding.UTF8));
        }
    }
}

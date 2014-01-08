using System;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using System.Text.RegularExpressions;

public static class Calculator
{
    private static CSharpCodeProvider provider = null;
    static Calculator()
    {
        provider = new CSharpCodeProvider();

        // get all System.Math class members
        // so user doesn't need to add "Math." before them
        // and make them case-insensitive (pi, Pi, PI)
        PopulateMathLibrary();
    }

    private static CompilerParameters CreateCompilerParameters()
    {
        var compilerParams = new CompilerParameters
                                 {
                                     CompilerOptions = "/target:library /optimize",
                                     GenerateExecutable = false,
                                     GenerateInMemory = true,
                                     IncludeDebugInformation = false
                                 };
        compilerParams.ReferencedAssemblies.Add("System.dll");
        return compilerParams;
    }
    private static string GenerateCode(string expression)
    {
        var source = new StringBuilder();
        var sw = new StringWriter(source);
        var options = new CodeGeneratorOptions();
        var myNamespace = new CodeNamespace("ExpressionEvaluator");
        myNamespace.Imports.Add(new CodeNamespaceImport("System"));
        var classDeclaration = new CodeTypeDeclaration { IsClass = true, Name = "Calculator", Attributes = MemberAttributes.Public };
        var myMethod = new CodeMemberMethod { Name = "Calculate", ReturnType = new CodeTypeReference(typeof(double)), Attributes = MemberAttributes.Public };
        myMethod.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(expression)));
        classDeclaration.Members.Add(myMethod);
        myNamespace.Types.Add(classDeclaration);
        provider.GenerateCodeFromNamespace(myNamespace, sw, options);
        sw.Flush();
        sw.Close();
        return source.ToString();
    }
    private static CompilerResults CompileCode(string source)
    {
        CompilerParameters parms = CreateCompilerParameters();
        return provider.CompileAssemblyFromSource(parms, source);
    }
    private static string RunCode(CompilerResults results)
    {
        Assembly executingAssembly = results.CompiledAssembly;
        object assemblyInstance = executingAssembly.CreateInstance("ExpressionEvaluator.Calculator");
        return assemblyInstance.GetType().GetMethod("Calculate").Invoke(assemblyInstance, new object[] { }).ToString();
    }
    public static string Evaluate(string expression)
    {
        string processed_expression = ProcessExpression(expression);
        string source = GenerateCode(processed_expression);

        CompilerResults results = CompileCode(source);
        if ((results == null) || (results.Errors.Count != 0) || (results.CompiledAssembly == null))
        {
            return expression;
        }

        return RunCode(results);
    }

    private static Hashtable s_math_library = new Hashtable();
    private static void PopulateMathLibrary()
    {
        // get a reflected assembly of the System assembly
        Assembly systemAssembly = Assembly.GetAssembly(typeof(System.Math));
        try
        {
            // cannot call the entry method if the assembly is null
            if (systemAssembly != null)
            {
                // use reflection to get a reference to the Math class

                Module[] modules = systemAssembly.GetModules(false);
                Type[] types = modules[0].GetTypes();

                // loop through each class that was defined and look for the first occurrance of the Math class
                foreach (Type type in types)
                {
                    if (type.Name == "Math")
                    {
                        // get all of the members of the math class and map them to the same member
                        // name in uppercase
                        MemberInfo[] mis = type.GetMembers();
                        foreach (MemberInfo mi in mis)
                        {
                            s_math_library[mi.Name.ToUpper()] = mi.Name;
                        }
                    }
                    // if the entry point method does return in Int32, then capture it and return it
                }

                // if it got here, then there was no entry point method defined.  Tell user about it
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error:  An exception occurred while executing the script", ex);
        }
    }
    private static string ProcessExpression(string expression)
    {
        // look for regular expressions with only letters
        Regex regex = new Regex("[a-zA-Z_]+");

        // track all math functions and constants in expression so we don't replace them again
        ArrayList replaced_items = new ArrayList();

        // find all alpha words inside the evaluation function that are possible functions
        MatchCollection matches = regex.Matches(expression);
        foreach (Match match in matches)
        {
            // if the word is found in the math member map, add a Math prefix to it
            bool in_math_library = s_math_library[match.Value.ToUpper()] != null;
            if (replaced_items.Contains(match.Value) == false && in_math_library)
            {
                expression = expression.Replace(match.Value, "Math." + s_math_library[match.Value.ToUpper()]);
            }

            // we matched it already, so don't allow us to replace it again
            replaced_items.Add(match.Value);
        }
        return expression;
    }
}

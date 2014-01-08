using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
    public class Expression
    {
        private string expression;
        private ParseTreeEvaluator tree;

        public ParseErrors Errors
        {
            get
            {
                if (tree != null)
                    return tree.Errors;
                else 
                    return null;
            }
        }

        public Expression(string exp)
        {
            expression = exp;
            Scanner scanner = new Scanner();
            Parser parser = new Parser(scanner);
            tree = new ParseTreeEvaluator(Context.Default);
            tree = parser.Parse(expression, tree) as ParseTreeEvaluator;
        }

        public object Evaluate()
        {
            object result = tree.Evaluate(null);
            if (tree.Context.CurrentStackSize > 0)
                Errors.Add(new ParseError("Stacksize is not empty", 0, null));
            return result;
        }

        public static object Evaluate(string expression)
        {
            return Expression.Evaluate<object>(expression);
        }

        public static T Evaluate<T>(string expression) 
        {
            object result = null;
            try
            {
                Expression exp = new Expression(expression);
                
                if (exp.tree.Errors.Count > 0)
                    result = exp.tree.Errors[0].Message;
                else
                    result = exp.Evaluate();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result != null ? ((T)(result)) : default(T);
        }
    }
}

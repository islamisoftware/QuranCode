using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
    public delegate object FunctionDelegate(object[] parameters);

    public abstract class Function
    {

        /// <summary>
        /// define the arguments of the dynamic function
        /// </summary>
        public Variables Arguments { get; protected set; }

        /// <summary>
        /// name of the function
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// minimum number of allowed parameters (default = 0)
        /// </summary>
        public int MaxParameters { get; protected set; }

        /// <summary>
        /// maximum number of allowed parameters (default = 0)
        /// </summary>
        public int MinParameters { get; protected set; }

        public abstract object Evaluate(object[] parameters, ParseTreeEvaluator tree);

    }

    public class DynamicFunction : Function
    {
        /// <summary>
        /// points to the RHS of the assignment of this function
        /// this branch will be evaluated each time this function is executed
        /// </summary>
        private ParseNode Node;

        /// <summary>
        /// the list of parameters must correspond the the required set of Arguments
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override object Evaluate(object[] parameters, ParseTreeEvaluator tree)
        {

            // create a new scope for the arguments
            Variables pars = Arguments.Clone();
            // now push a copy of the function arguments on the stack
            tree.Context.PushScope(pars);

            // assign the parameters to the current function scope variables            
            int i = 0;
            foreach (string key in pars.Keys)
                pars[key] = parameters[i++];

            // execute the function here

            object result = Node.Evaluate(tree, null);

            // clean up the stack
            tree.Context.PopScope();

            return result;
        }

        public DynamicFunction(string name, ParseNode node, Variables args)
            : this(name, node, args, 0, 0)
        {
        }
        public DynamicFunction(string name, ParseNode node, Variables args, int minParameters, int maxParameters)
        {
            Node = node;
            Arguments = args;
            MinParameters = minParameters;
            MaxParameters = maxParameters;
        }
    }

    public class StaticFunction : Function
    {
        /// <summary>
        /// the actual function implementation
        /// </summary>
        public FunctionDelegate FunctionDelegate { get; private set; }

        public override object Evaluate(object[] parameters, ParseTreeEvaluator tree)
        {
            tree.Context.PushScope(null);
            object result = FunctionDelegate(parameters);
            tree.Context.PopScope();
            return result;
        }

        public StaticFunction(string name, FunctionDelegate function)
            : this(name, function, 0, 0)
        {
        }
        public StaticFunction(string name, FunctionDelegate function, int minParameters, int maxParameters)
        {
            Name = name;
            FunctionDelegate = function;
            MinParameters = minParameters;
            MaxParameters = maxParameters;
            Arguments = new Variables();
        }
    }

}

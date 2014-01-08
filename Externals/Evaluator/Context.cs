using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
    public class Context
    {
        private static Context defaultContext;
        public static Context Default
        {
            get
            {
                if (defaultContext == null)
                    defaultContext = new Context();
                return defaultContext;
            }
        }

        // list of functions currently in scope during an evaluation
        // note that this typically is NOT thread safe.

        // contains a list of variables that is in scope. Scope is used only for DynamicFunctions (for now)
        private List<Variables> inScope;

        public Functions Functions { get; private set; }
        public Variables Globals { get; private set; }
        /// <summary>
        /// check current stacksize
        /// is used for debugging purposes and error handling
        /// to prevent stackoverflows
        /// </summary>
        public int CurrentStackSize
        {
            get
            {
                return inScope.Count;
            }
        }

        public Variables CurrentScope
        {
            get
            {
                if (inScope.Count <= 0)
                    return null;

                return inScope[inScope.Count - 1];
            }
        }

        public void PushScope(Variables vars)
        {
            inScope.Add(vars);
        }

        public Variables PopScope()
        {
            if (inScope.Count <= 0)
                return null;

            Variables vars = inScope[inScope.Count - 1];
            inScope.RemoveAt(inScope.Count - 1);
            return vars;
        }

        public Context()
        {
            Reset();
        }

        /// <summary>
        /// resets the context to its defaults
        /// </summary> 
        public void Reset()
        {
            inScope = new List<Variables>();
            Functions = new Functions();
            Globals = new Variables();
            Functions.InitDefaults();
            Globals["pi"]  = 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679; // Math.Pi is not very precise
            Globals["e"]   = 2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274;  // Math.E is not very precise
            Globals["phi"] = 1.6180339887498948482045868343656381177203091798057628621354486227052604628189024497072072041893911374;  // no Math.Phi at all
        }
    }
}

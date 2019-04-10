using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace adesso.BusinessProcesses.ConfigurationSubstitution.Templating
{
    internal static class SchemeToken
    {
        public readonly static Regex Substitution = new Regex(@"{[^{()]+(?(?=\()\([^{}(]+\))*}");
        public readonly static Regex FailoverSubstitution = new Regex(@"^{\?");
        public readonly static Regex Variable = new Regex(@"(?(?<={\?)[^}]+(?=})|(?<={)(?!\?)[^}]+(?=}))");
        public readonly static Regex VariableName = new Regex("^[a-zA-Z]+");
        public readonly static Regex OperationParameter = new Regex(@"(?<=\()[^)}{]+(?=\))");
        public readonly static Regex SplitFailoverParameter = new Regex(@"(?(?<=\?)[^)}{?]+|[^)}{?]+)");
        public readonly static Regex ReplaceParameter = new Regex(@"(?<=\[)[^\[\]]+,[^\[\],]*(?=\])");
        public readonly static Regex SelectionList = new Regex("([0-9](?(?=,)|))");

        public readonly static Regex SplitDecisionParameter = new Regex("[ ]+");
        public readonly static Regex ChildSplitDecisionParamter = new Regex("-+");

        public const string FillerParameter = "";
        public const string FailoverStart = "FAILOVER_START";
    }
}

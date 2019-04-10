using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace adesso.BusinessProcesses.ConfigurationSubstitution.Templating
{
    internal static class Operations
    {
        public static ISet<string> Apply(string raw, string value)
        {
            var orderedValues = new HashSet<string>();
            TreeEntry operationTree = new TreeEntry(false);

            if (!SchemeToken.OperationParameter.IsMatch(raw))
            {
                orderedValues.Add(value);
                return orderedValues;
            }

            MatchCollection matchedOperations = SchemeToken.OperationParameter.Matches(raw);
            var failoverParameters = new List<List<string>>();

            foreach (Match operation in matchedOperations)
            {
                bool isFailover = false;
                string operationParameter = operation.Value;

                MatchCollection splitParameters = SchemeToken.SplitFailoverParameter.Matches(operationParameter);
                var parameters = new List<string>(splitParameters.Count);

                for (int sI = 0; sI < splitParameters.Count; sI++)
                {
                    if (sI == 0 && splitParameters[sI].Index == 1 && operationParameter.StartsWith("?"))
                        isFailover = true;
                    parameters.Add(splitParameters[sI].Value);
                }

                if (!isFailover)
                    AddParameterEntry(operationTree, parameters);
                else
                    failoverParameters.Add(parameters);
            }

            if (failoverParameters.Count > 0)
                AddParameterEntry(operationTree, failoverParameters);

            //Console.WriteLine(operationTree.ToString());


            return TreeEntry.Resolve(new string[] { value }, operationTree);
        }

        private static void AddParameterEntry(TreeEntry current, List<List<string>> failover)
        {
            if (current.Children.Count > 0)
                foreach (TreeEntry child in current.Children.Values)
                    AddParameterEntry(child, failover);
            else
            {
                var childEntry = new TreeEntry(true);
                int failKey = 0;
                current.Children.Add(failKey, childEntry);

                foreach (List<string> fail in failover)
                    AddParameterEntry(childEntry, fail);
            }
        }
        private static void AddParameterEntry(TreeEntry current, List<string> parameters)
        {
            if (current.Children.Count > 0)
                foreach (TreeEntry child in current.Children.Values)
                    AddParameterEntry(child, parameters);
            else
                for (int pI = 0; pI < parameters.Count; pI++)
                    current.Children.Add(pI, new TreeEntry(parameters[pI]));
        }

        public static List<string> Split(string value)
        {
            List<string> splitValues = new List<string>();

            string[] split = SchemeToken.SplitDecisionParameter.Split(value);
            for (int sI = split.Length - 1; sI >= 0; sI--)
            {
                splitValues.Add(split[sI]);
                if (!SchemeToken.ChildSplitDecisionParamter.IsMatch(split[sI]))
                    continue;

                string[] childSplit = SchemeToken.ChildSplitDecisionParamter.Split(split[sI]);
                for (int cI = childSplit.Length - 1; cI >= 0; cI--)
                    splitValues.Add(childSplit[cI]);
            }

            return splitValues;
        }

        public static List<string> Count(bool isUp, string value)
        {
            List<string> countedValues = new List<string>();

            int countMax = 9;

            if (isUp)
                for (int count = 1; count <= countMax; count++)
                    countedValues.Add(value + count);
            else
                for (int count = countMax; count >= 1; count--)
                    countedValues.Add(value + count);

            return countedValues;
        }

        public static string Replace(string parameter, string value)
        {
            if (!SchemeToken.ReplaceParameter.IsMatch(parameter))
                throw new InvalidOperationException();

            string pattern, substitution;
            pattern = substitution = String.Empty;

            string replaceOperation = SchemeToken.ReplaceParameter.Match(parameter).Value;
            int seperation = replaceOperation.IndexOf(",");

            if (seperation != 0)
                pattern = replaceOperation.Substring(0, seperation);

            if (seperation < replaceOperation.Length - 1)
                substitution = replaceOperation.Substring(seperation + 1, replaceOperation.Length - (seperation + 1));

            return new Regex(pattern).Replace(value, substitution);
        }

        public static string Select(string parameter, string value)
        {
            if (!SchemeToken.SelectionList.IsMatch(parameter))
                throw new InvalidOperationException();

            string selectedResult = String.Empty;
            MatchCollection positionParameters = SchemeToken.SelectionList.Matches(parameter);


            foreach (Match position in positionParameters)
            {
                int select = 0;
                if (int.TryParse(position.Value, out select))
                    selectedResult += value[select];
            }

            return selectedResult;
        }
    }
}

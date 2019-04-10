using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace adesso.BusinessProcesses.ConfigurationSubstitution.Templating
{
    sealed internal class Substitution
    {
        #region Attributes
        public string Name { get; private set; }
        public ISet<string> Values { get; private set; }
        public string Scheme { get; private set; }

        public bool IsFailover { get; private set; }
        #endregion

        #region Methods

        public static Dictionary<int, Substitution> GetAll(string rawScheme)
        {
            Dictionary<int, Substitution> result = new Dictionary<int, Substitution>();

            if (String.IsNullOrWhiteSpace(rawScheme))
                return result;

            MatchCollection matches = SchemeToken.Substitution.Matches(rawScheme);

            foreach (Match match in matches)
                result.Add(match.Index, new Substitution(String.Empty, new HashSet<string>(), match.Value, SchemeToken.FailoverSubstitution.IsMatch(match.Value)));

            return result;
        }

        public static void InitVariableData(IEnumerable<Substitution> substitutions, NamingData namingData)
        {
            if (substitutions == null)
                return;

            if (namingData == null)
                throw new ArgumentNullException("Supplied null namingData");

            foreach (var substitution in substitutions)
                substitution.SetVariable(namingData);
        }

        private void SetVariable(NamingData namingData)
        {
            //TODO : ERROR
            if (!SchemeToken.Variable.IsMatch(Scheme))
                return;

            string variableInformation = SchemeToken.Variable.Match(Scheme).Value;

            //TODO : ERROR
            if (!SchemeToken.VariableName.IsMatch(variableInformation))
                return;

            this.Name = SchemeToken.VariableName.Match(variableInformation).Value;
            string variableValue = String.Empty;

            if (!namingData.TryGet(this.Name, ref variableValue))
                throw new InvalidOperationException("Missing " + this.Name + " inside NamingData!");

            if (String.IsNullOrEmpty(this.Name))
                throw new ArgumentException(this.Name + " is configured with empty value!");

            this.Values = Operations.Apply(variableInformation, variableValue);

            if (this.Values.Count == 0)
                throw new ArgumentException(this.Name + " has no valid values configured.");

        }

        private static void AddValue(OrderedDictionary tree, List<string> values)
        {
            if (tree.Count == 0)
                for (int vI = 0; vI < values.Count; vI++)
                    tree.Add(values[vI], new OrderedDictionary());
            else
                for (var existingEnum = tree.GetEnumerator(); existingEnum.MoveNext();)
                    AddValue(((OrderedDictionary)((DictionaryEntry)existingEnum.Current).Value), values);
        }

        public static string[] GetFinalValues(Dictionary<int, Substitution> substitutions, string schemeString)
        {
            if (substitutions == null)
                return new string[] { };

            var valueTree = new OrderedDictionary();

            foreach (var substitution in substitutions.Values)
            {
                var variableValues = (substitution.IsFailover) ? new List<string> { { SchemeToken.FillerParameter } } : new List<string>(substitution.Values.Count);
                variableValues.AddRange(substitution.Values);

                AddValue(valueTree, variableValues);
            }

            //Console.WriteLine(JsonConvert.SerializeObject(valueTree));

            int offset = 0;
            return ConstructValues(valueTree, substitutions, schemeString, ref offset);
        }

        private static string[] ConstructValues(OrderedDictionary valueTree, Dictionary<int, Substitution> substitutions, string scheme, ref int offset)
        {
            int baseOffset = 0;
            var uniqueValues = new HashSet<string>();
            int[] subKeys = substitutions.Keys.ToArray();
            int lastSub = subKeys.Length - 1;

            for (var valueEnum = valueTree.GetEnumerator(); valueEnum.MoveNext();)
            {
                OrderedDictionary child = ((OrderedDictionary)((DictionaryEntry)valueEnum.Current).Value);
                string tail, prefix;
                string[] childValues = new string[] { };
                tail = prefix = String.Empty;

                if (child.Count >= 1)
                    childValues = ConstructValues(child, substitutions, scheme, ref offset);

                int subKey = lastSub - offset;
                if (subKey < 0 || subKey >= subKeys.Length)
                    throw new ArgumentException("Invalid subkey with value of " + subKey);

                int subLength = substitutions[subKeys[subKey]].Scheme.Length;
                int schemeLength = scheme.Length;
                int schemeOffset = subKeys[subKey] + subLength;

                if (offset != 0)
                    schemeLength = subKeys[subKey + 1];

                if (subKey == 0)
                    prefix = scheme.Substring(0, subKeys[subKey]);

                if (schemeOffset != schemeLength)
                    tail = prefix + valueEnum.Key + scheme.Substring(schemeOffset, schemeLength - schemeOffset);
                else
                    tail = prefix + valueEnum.Key;

                if (childValues.Length > 0)
                    foreach (string childValue in childValues)
                    {
                        uniqueValues.Add(tail + childValue);
                    }
                else
                    uniqueValues.Add(tail);

                if (offset > baseOffset)
                    baseOffset = offset;
                offset = 0;
            }

            offset = baseOffset + 1;

            return uniqueValues.ToArray();
        }

        public override string ToString()
        {
            var builder = new StringBuilder("Substitution : " + Name + " IsFailover : " + IsFailover + Environment.NewLine);
            builder.AppendLine("Values :" + this.Values.Aggregate("", (acc, v) => (acc.Equals("")) ? v : acc + "," + v));
            return builder.ToString();
        }
        #endregion

        #region Constructor
        public Substitution(string name, ISet<string> values, string scheme, bool isFailover)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Values = values ?? throw new ArgumentNullException(nameof(values));
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            IsFailover = isFailover;
        }
        #endregion
    }
}

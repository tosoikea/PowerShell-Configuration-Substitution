using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace adesso.BusinessProcesses.ConfigurationSubstitution.Templating
{
    class TreeEntry
    {
        #region Attributes
        public string Parameter { get; private set; }
        public Dictionary<int, TreeEntry> Children { get; private set; }
        #endregion

        #region Methods
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


        public static ISet<string> Resolve(IEnumerable<string> values, TreeEntry tree)
        {
            var orderedValues = new HashSet<string>();
            var additions = new List<string>();

            bool isAddAllowed = false;
            bool isChildren = tree.Children.Count > 0;

            foreach (string value in values)
            {
                switch (tree.Parameter)
                {
                    case "lower":
                        additions.Add(value.ToLower());
                        break;
                    case "upper":
                        additions.Add(value.ToUpper());
                        break;
                    case "split":
                        additions.AddRange(Operations.Split(value));
                        break;
                    case "countUP":
                        additions.AddRange(Operations.Count(true, value));
                        break;
                    case "countDOWN":
                        additions.AddRange(Operations.Count(true, value));
                        break;
                    case SchemeToken.FailoverStart:
                        additions.Add(value);
                        isAddAllowed = true;
                        break;
                    case SchemeToken.FillerParameter:
                        additions.Add(value);
                        break;
                    default:
                        if (SchemeToken.ReplaceParameter.IsMatch(tree.Parameter))
                            additions.Add(Operations.Replace(tree.Parameter, value));
                        else if (SchemeToken.SelectionList.IsMatch(tree.Parameter))
                            additions.Add(Operations.Select(tree.Parameter, value));
                        else
                            throw new NotImplementedException(tree.Parameter);
                        break;
                }
            }

            additions = additions.Where(val => !String.IsNullOrEmpty(val)).ToList();

            if (!isChildren || isAddAllowed)
                additions.ForEach(addition => orderedValues.Add(addition));

            if (!isChildren)
                return orderedValues;

            var sortedIndex = new List<int>(tree.Children.Keys);
            sortedIndex.Sort();

            foreach (int index in sortedIndex)
            {
                ISet<string> childSet = Resolve(additions, tree.Children[index]);
                foreach (string childEntry in childSet)
                    orderedValues.Add(childEntry);
            }

            return orderedValues;
        }
        #endregion

        #region Constructors
        public TreeEntry(bool isFailover)
        {
            Parameter = (isFailover) ? SchemeToken.FailoverStart : SchemeToken.FillerParameter;
            Children = new Dictionary<int, TreeEntry>();
        }

        public TreeEntry(string parameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            Children = new Dictionary<int, TreeEntry>();
        }
        #endregion
    }
}

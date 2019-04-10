using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adesso.BusinessProcesses.ConfigurationSubstitution.Templating
{
    public static class Resolver
    {
        public static Hashtable ConvertSchemeToDictionary(string scheme, NamingData namingData)
        {
            string[] data = ConvertSchemeToArray(scheme, namingData);
            if (data.Length == 0)
                throw new ArgumentException("Empty generated data.");

            var result = new Hashtable { { "value", data[0] } };
            if (data.Length > 1)
                result["failover"] = data.Skip(1).ToArray();

            return result;
        }
        public static string[] ConvertSchemeToArray(string scheme, NamingData namingData)
        {
            Dictionary<int, Substitution> substitutions = Substitution.GetAll(scheme);

            if (substitutions.Count == 0)
                return new string[] { scheme };

            Substitution.InitVariableData(substitutions.Values, namingData);

            return Substitution.GetFinalValues(substitutions, scheme);
        }
    }
}

using System;
using System.Linq;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter
{
    public class NamedObjectFilterRule : FilterRule
    {
        public NamedObjectFilterRule(FilterOperation operation, string match)
            : base(operation, match)
        {
            
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass type)
        {
            var objectName = name.Parts.LastOrDefault();

            return !String.IsNullOrEmpty(objectName) && Regex.IsMatch(objectName);
        }
    }
}
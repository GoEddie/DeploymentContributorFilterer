using System;
using System.Linq;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
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
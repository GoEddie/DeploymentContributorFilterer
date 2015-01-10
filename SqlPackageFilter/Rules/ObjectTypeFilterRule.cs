using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class ObjectTypeFilterRule : FilterRule
    {
        public ObjectTypeFilterRule(FilterOperation operation, string match) : base(operation, match)
        {
            
        }

        public override  bool Matches(ObjectIdentifier name, ModelTypeClass type)
        {
            return Regex.IsMatch(type.Name);
        }
    }
}
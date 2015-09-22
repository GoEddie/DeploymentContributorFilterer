using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class ObjectTypeFilterRule : FilterRule
    {
        public ObjectTypeFilterRule(FilterOperation operation, string match, MatchType matchType) : base(operation, match, matchType)
        {
            
        }

        public override  bool Matches(ObjectIdentifier name, ModelTypeClass type, DeploymentStep step = null)
        {
            return Matches(type.Name);
        }
    }
}
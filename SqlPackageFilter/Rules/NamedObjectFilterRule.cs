using System;
using System.Linq;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class NamedObjectFilterRule : FilterRule
    {
        public NamedObjectFilterRule(FilterOperation operation, string match, MatchType matchType)
            : base(operation, match, matchType)
        {
            
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass type, DeploymentStep step = null)
        {
            foreach (var part in name.Parts)
                if (!String.IsNullOrEmpty(part) && (Matches(part)))
                    return true;

            return false;
        }
    }
}
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;
using AgileSqlClub.SqlPackageFilter.DacExtensions;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class ObjectTypeFilterRule : FilterRule
    {
        protected readonly string Schema;

        public ObjectTypeFilterRule(FilterOperation operation, string match, MatchType matchType, List<string> options = null) : base(operation, match, matchType)
        {
            if (options != null && options.Count > 0)
            {
                Schema = options[0];
            }
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass type, DeploymentStep step = null)
        {
            bool result = (Schema == null || Schema.Equals(name.GetSchemaName(type)))
                && Matches(type.Name);

            return result;
        }
    }
}
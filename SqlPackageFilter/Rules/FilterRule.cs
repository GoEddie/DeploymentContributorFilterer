using System.Text.RegularExpressions;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class FilterRule
    {
        protected readonly string Match;
        protected readonly Regex Regex;

        public FilterRule()
        {
            
        }

        public FilterRule(FilterOperation operation, string match)
        {
            RuleFilterOperation = operation;
            Match = match;
            Regex = new Regex(Match, RegexOptions.Compiled);
        }

        public virtual bool Matches(ObjectIdentifier name, ModelTypeClass objectType)
        {
            return false;
        }

        protected FilterOperation RuleFilterOperation;

        public virtual FilterOperation Operation() { return RuleFilterOperation; }
    }
}
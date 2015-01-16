using System.Text.RegularExpressions;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class FilterRule
    {
        protected readonly string Match;
        protected readonly MatchType MatchType;
        protected readonly Regex Regex;

        public FilterRule()
        {
            
        }

        public FilterRule(FilterOperation operation, string match, MatchType matchType)
        {
            RuleFilterOperation = operation;
            Match = match;
            MatchType = matchType;
            Regex = new Regex(Match, RegexOptions.Compiled);
        }

        public virtual bool Matches(ObjectIdentifier name, ModelTypeClass objectType)
        {
            return false;
        }

        protected bool Matches(string text)
        {
            var matches = Regex.IsMatch(text);
            
            if (matches && MatchType == MatchType.DoesMatch)
                return true;

            if (!matches && MatchType == MatchType.DoesNotMatch)
                return true;

            return false;
        }

        protected FilterOperation RuleFilterOperation;

        public virtual FilterOperation Operation() { return RuleFilterOperation; }
    }
}
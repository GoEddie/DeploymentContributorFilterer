using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter
{
    public class KeeperDecider
    {
        private readonly IEnumerable<FilterRule> _rules;

        public KeeperDecider(IEnumerable<FilterRule> rules)
        {
            _rules = rules;
        }

        public bool ShouldRemoveFromPlan(TSqlObject definition, bool isDrop)
        {
            foreach (var rule in _rules)
            {
                if ( ((rule.Operation() == FilterOperation.Keep && isDrop) || rule.Operation() == FilterOperation.Ignore) && rule.Matches(definition))
                    return true;
            }

            return false;
        }
    }

    public enum FilterOperation
    {
        Keep,
        Ignore
    }

    public enum FilterType
    {
        Schema,
        Name,
        Type
    }

    public class FilterRule
    {
        protected readonly string Match;
        protected readonly Regex Regex;

        public FilterRule()
        {
            
        }

        public FilterRule(string match)
        {
            Match = match;
            Regex = new Regex(Match, RegexOptions.Compiled);
        }

        public virtual bool Matches(TSqlObject defintion)
        {
            return false;
        }

        protected FilterOperation RuleFilterOperation;

        public virtual FilterOperation Operation() { return RuleFilterOperation; }
    }

    public class IgnoreSchemaFilterRule : FilterRule
    {
        public IgnoreSchemaFilterRule(string match) : base(match)
        {
            RuleFilterOperation = FilterOperation.Ignore;
        }

        public override bool Matches(TSqlObject defintion)
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoreNamedObjectFilterRule : FilterRule
    {
        public IgnoreNamedObjectFilterRule(string match) : base(match)
        {
            RuleFilterOperation = FilterOperation.Ignore;
        }

        public override bool Matches(TSqlObject defintion)
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoreObjectTypeFilterRule : FilterRule
    {

        public IgnoreObjectTypeFilterRule(string match) : base(match)
        {
            RuleFilterOperation = FilterOperation.Ignore;
        }

        public override  bool Matches(TSqlObject defintion)
        {
            throw new NotImplementedException();
        }
    }
}

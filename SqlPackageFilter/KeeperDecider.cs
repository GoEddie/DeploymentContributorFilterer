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

        /// <summary>
        /// 
        /// Source	    Destination DropObjectsNotInSource	FilterType	Generates	Result
        /// dbo.Table	Missing	    TRUE	                Keep	    Create	    Leave in deploy
        /// Missing	    dbo.Table	TRUE	                Keep	    Drop	    Remove from deploy
        /// dbo.Table	Missing	    TRUE	                Ignore	    Create	    Remove from deploy
        /// Missing	    dbo.Table	TRUE	                Ignore	    Drop	    Remove from deploy
        /// dbo.Table	Missing	    TRUE	                None	    Create	    Leave in deploy
        /// Missing	    dbo.Table	TRUE	                None	    Drop	    Leave in deploy

        /// </summary>
        /// <param name="definition"></param>
        /// <param name="stepType"></param>
        /// <returns></returns>
        public bool ShouldRemoveFromPlan(TSqlObject definition, StepType stepType)
        {
            if (stepType == StepType.Other)
                return false;

            foreach (var rule in _rules)
            {
                var operation = rule.Operation();

                if (operation == FilterOperation.Ignore && rule.Matches(definition))
                {
                    return true;
                }
                
                if (operation == FilterOperation.Keep && stepType == StepType.Drop && rule.Matches(definition))
                {
                    return true;
                }
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

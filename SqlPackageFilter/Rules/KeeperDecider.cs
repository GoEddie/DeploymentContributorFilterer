using System;
using System.Collections.Generic;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
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
        /// <param name="name"></param>
        /// <param name="objectType"></param>
        /// <param name="stepType"></param>
        /// <returns></returns>
        public bool ShouldRemoveFromPlan(ObjectIdentifier name, ModelTypeClass objectType, StepType stepType)
        {

            if (stepType == StepType.Other)
                return false;

            foreach (var rule in _rules)
            {
                var operation = rule.Operation();
                
                if (operation == FilterOperation.Ignore && rule.Matches(name, objectType))
                {
                    return true;
                }

                if (operation == FilterOperation.Keep && stepType == StepType.Drop && rule.Matches(name, objectType))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}

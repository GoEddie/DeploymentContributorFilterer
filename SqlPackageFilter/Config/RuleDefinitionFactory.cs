using System;
using System.Collections.Generic;
using AgileSqlClub.SqlPackageFilter.Filter;
using AgileSqlClub.SqlPackageFilter.Rules;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    public class RuleDefinitionFactory
    {
        public List<FilterRule> BuildRules(Dictionary<string, string> contextArgs)
        {
            var defintions = BuildRuleDefinitions(contextArgs);
            var rules = new List<FilterRule>();

            foreach (var ruleDefinition in defintions)
            {
                switch (ruleDefinition.Type)
                {
                    case FilterType.Schema:
                        rules.Add(new SchemaFilterRule(ruleDefinition.Operation, ruleDefinition.Match ));
                        break;
                    case FilterType.Name:
                        rules.Add(new NamedObjectFilterRule(ruleDefinition.Operation, ruleDefinition.Match));
                        break;
                    case FilterType.Type:
                        rules.Add(new ObjectTypeFilterRule(ruleDefinition.Operation, ruleDefinition.Match));
                        break;
                }
            }

            return rules;

        }

        private List<RuleDefinition> BuildRuleDefinitions(Dictionary<string, string> contextArgs)
        {
            var rules = new List<RuleDefinition>();

            foreach (var arg in contextArgs)
            {
                var definitionType = FilterDefinitionTypeParser.GetDefinitionType(arg.Key);

                if(definitionType == FilterDefinitionType.CommandLine)
                    rules.Add(new CommandLineFilterParser().GetDefinitions(arg.Value));

                if (definitionType == FilterDefinitionType.XmlFile)
                    rules.AddRange(new XmlFilterParser().GetDefinitions(arg.Value));
            }

            return rules;
        } 
    }
}
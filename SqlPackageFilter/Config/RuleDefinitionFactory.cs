using System;
using System.Collections.Generic;
using AgileSqlClub.SqlPackageFilter.Filter;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Extensibility;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    public class RuleDefinitionFactory
    {
        private readonly IDisplayMessageHandler _messageHandler;

        public RuleDefinitionFactory(IDisplayMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        public List<FilterRule> BuildRules(Dictionary<string, string> contextArgs, DeploymentFilter deploymentFilter)
        {
            var defintions = BuildRuleDefinitions(contextArgs, deploymentFilter);

            var rules = new List<FilterRule>();

            foreach (var ruleDefinition in defintions)
            {

                FilterRule rule;
                switch (ruleDefinition.FilterType)
                {
                    case FilterType.Schema:
                        rule = new SchemaFilterRule(ruleDefinition.Operation, ruleDefinition.Match,
                            ruleDefinition.MatchType);
                        break;
                    case FilterType.Name:
                        rule = new NamedObjectFilterRule(ruleDefinition.Operation, ruleDefinition.Match,
                            ruleDefinition.MatchType);
                        break;
                    case FilterType.Type:
                        rule = new ObjectTypeFilterRule(ruleDefinition.Operation, ruleDefinition.Match,
                            ruleDefinition.MatchType, ruleDefinition.Options);
                        break;
                    case FilterType.TableColumns:
                        rule = new TableColumnFilterRule(ruleDefinition.Operation, ruleDefinition.Match,
                            ruleDefinition.MatchType, deploymentFilter);
                        break;
                    case FilterType.MultiPartName:
                        rule = new MultiPartNamedObjectFilterRule(ruleDefinition.Operation, ruleDefinition.Match,
                            ruleDefinition.MatchType, deploymentFilter);
                        break;
                    default:
                        rule = null;
                        break;
                }

                if (rule == null)
                {
                    deploymentFilter.ShowMessage($" - unknown type for ruleDefinition: {ruleDefinition.Match}", DisplayMessageLevel.Warning);
                }
                else
                {
                    deploymentFilter.ShowMessage($" - adding ruleDefinition: {ruleDefinition.FilterType}", DisplayMessageLevel.Info);
                    rules.Add(rule);
                }
            }

            return rules;
        }

        public IEnumerable<RuleDefinition> BuildRuleDefinitions(Dictionary<string, string> contextArgs,
            DeploymentFilter deploymentFilter)
        {
            var rules = new List<RuleDefinition>();

            foreach (var arg in contextArgs)
            {
                try
                {
                    var definitionType = FilterDefinitionTypeParser.GetDefinitionType(arg.Key);

                    if (definitionType == FilterDefinitionType.CommandLine)
                    {
                        var rule = new CommandLineFilterParser(_messageHandler).GetDefinitions(arg.Value);
                        rules.Add(rule);
                        deploymentFilter.ShowMessage($" - Command Line Filter: {arg.Value}", DisplayMessageLevel.Info);
                    }

                    if (definitionType == FilterDefinitionType.XmlFile)
                    {
                        deploymentFilter.ShowMessage($" - Loading Filters from file: {arg.Value}", DisplayMessageLevel.Info);
                        rules.AddRange(
                            new XmlFilterParser(new FileGateway(), _messageHandler).GetDefinitions(arg.Value));
                    }

                    if (definitionType == FilterDefinitionType.Logging)
                    {
                        var level = DisplayMessageLevel.None;
                        if (Enum.TryParse(arg.Value, true, out level))
                        {
                            _messageHandler.SetMessageLevel(level);
                        }
                    }
                }
                catch (Exception e)
                {
                    deploymentFilter.ShowMessage(string.Format("Error decoding command line arg: {0}, error: {1}",
                        arg.Value, e.Message), DisplayMessageLevel.Errors);
                }
            }

            return rules;
        }
    }
}
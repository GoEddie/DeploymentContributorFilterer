using System.Collections.Generic;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    public class RuleDefinitionFactory
    {
        public List<RuleDefinition> BuildRules(Dictionary<string, string> contextArgs)
        {
            var rules = new List<RuleDefinition>();

            foreach (var arg in contextArgs)
            {
                var definitionType = FilterDefinitionTypeParser.GetDefinitionType(arg.Key);

                if(definitionType == FilterDefinitionType.CommandLine)
                    rules.Add(new CommandLineFilterParser().GetDefinitions(arg.Value));

                if (definitionType == FilterDefinitionType.CommandLine)
                    rules.AddRange(new XmlFilterParser().GetDefinitions(arg.Value));
            }

            return rules;
        } 
    }
}
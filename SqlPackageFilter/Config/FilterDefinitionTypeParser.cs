using System;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    class FilterDefinitionTypeParser
    {
        private const string RuleDefinition = "SqlPackageFilter";
        private const string RuleDefinitionXmlFile = "SqlPackageFilterFile";

        public static FilterDefinitionType GetDefinitionType(string definition)
        {
            if (definition.StartsWith(RuleDefinition, StringComparison.OrdinalIgnoreCase))
                return FilterDefinitionType.CommandLine;

            if (definition.StartsWith(RuleDefinitionXmlFile, StringComparison.OrdinalIgnoreCase))
                return FilterDefinitionType.XmlFile;

            return FilterDefinitionType.NotFilter;
        }
    }
}
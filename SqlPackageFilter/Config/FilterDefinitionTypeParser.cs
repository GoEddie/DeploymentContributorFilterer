using System;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    class FilterDefinitionTypeParser
    {
        private const string RuleDefinition = "SqlPackageFilter";
        private const string RuleDefinitionXmlFile = "SqlPackageXmlFilterFile";
        private const string LoggingLevel = "SqlPackageLogging";

        public static FilterDefinitionType GetDefinitionType(string definition)
        {
            if (definition.StartsWith(RuleDefinition, StringComparison.OrdinalIgnoreCase))
                return FilterDefinitionType.CommandLine;

            if (definition.StartsWith(RuleDefinitionXmlFile, StringComparison.OrdinalIgnoreCase))
                return FilterDefinitionType.XmlFile;

            if (definition.StartsWith(LoggingLevel, StringComparison.OrdinalIgnoreCase))
                return FilterDefinitionType.Logging;

            return FilterDefinitionType.NotFilter;
        }
    }
}
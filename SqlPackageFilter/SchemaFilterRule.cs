using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter
{
    public class SchemaFilterRule : FilterRule
    {
        public SchemaFilterRule(FilterOperation operation, string match) : base(operation, match)
        {
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass type)
        {
            var schemaName = name.GetSchemaName();
            
            if (string.IsNullOrEmpty(schemaName))
                return false;

            return Regex.IsMatch(schemaName);
        }
    }
}
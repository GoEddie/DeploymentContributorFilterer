using System;
using AgileSqlClub.SqlPackageFilter.Filter;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    public class CommandLineFilterParser
    {
        public RuleDefinition GetDefinitions(string value)
        {
            var operation = GetOperation(value);

            var remove = operation == FilterOperation.Ignore ? 6 : 4;

            value = value.Substring(remove);

            var type = GetFilterType(value);

            switch (type)
            {
                case FilterType.Schema:
                    remove = 6;
                    break;
                case FilterType.Name:
                    remove = 4;
                    break;
                case FilterType.Type:
                    remove = 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            value = value.Substring(remove);

            var match = value.Trim(new []{'(',')'});

            var definiton = new RuleDefinition()
            {
                Operation = operation,
                Type = type,
                Match = match
            };

            return definiton;
        }

        private FilterType GetFilterType(string value)
        {
            if (value.StartsWith("Schema", StringComparison.OrdinalIgnoreCase))
            {
                return FilterType.Schema;
            }
            
            if (value.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
            {
                return FilterType.Name;
            }

            if (value.StartsWith("Type", StringComparison.OrdinalIgnoreCase))
            {
                return FilterType.Type;
            }

            throw new ArgumentException(string.Format("Could not get filter type, either Schema, Name or Type from: {0}", value));

        }

        private FilterOperation GetOperation(string value)
        {
            if (value.StartsWith("Ignore", StringComparison.OrdinalIgnoreCase))
                return FilterOperation.Ignore;

            if (value.StartsWith("Keep", StringComparison.OrdinalIgnoreCase))
                return FilterOperation.Keep;

            throw new ArgumentException(string.Format("Could not get filter operation, either Ignore or Keep from: {0}", value));
        }


    }
}
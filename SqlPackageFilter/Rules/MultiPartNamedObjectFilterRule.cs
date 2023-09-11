using System;
using System.Text.RegularExpressions;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class MultiPartNamedObjectFilterRule : FilterRule
    {
        private readonly DeploymentFilter _df;
        internal const char Separator = ',';
        private readonly Regex[] _matchParts;

        public MultiPartNamedObjectFilterRule(FilterOperation operation, string match, MatchType matchType, DeploymentFilter df = null)
        // Note that base.Match is unused in this implementation.
        : base(operation, ".*", matchType)
        {
            _df = df;
            _df?.ShowMessage("multipart filtering : {name.ToString()} : {matches}");

            // This assumes that a literal ',' never appears in a part name.
            _matchParts = Array.ConvertAll(match.Split(Separator), s => new Regex(s, RegexOptions.Compiled));
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass type, DeploymentStep step = null)
        {

            bool matches = true;
            for (int i = 0; i < name.Parts.Count && i < _matchParts.Length; i++)
            {
                // Match names right-to-left, since specificity of a SQL identifier is right-to-left,
                // and so this behaves identically to NamedObjectFilterRule if the requested match
                // contains only one argument.
                var part = name.Parts[name.Parts.Count - 1 - i];
                var match = _matchParts[_matchParts.Length - 1 - i];
                if (!match.IsMatch(part))
                {
                    matches = false;
                    break;
                }
            }
            _df?.ShowMessage($"multipart filtering : {string.Join(",",name.Parts)} : {matches}");

            if (matches && MatchType == MatchType.DoesMatch)
                return true;

            return !matches && MatchType == MatchType.DoesNotMatch;
        }
    }
}
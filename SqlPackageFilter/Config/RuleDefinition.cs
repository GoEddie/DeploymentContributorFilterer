using AgileSqlClub.SqlPackageFilter.Filter;
using System.Collections.Generic;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    public class RuleDefinition
    {
        public FilterOperation Operation;
        public FilterType FilterType;
        public string Match;
        public MatchType MatchType;
        public List<string> Options;
    }


    
}

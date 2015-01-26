using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Config
{
    //get the file name to the xml file with the filters in, parse and return
    public class XmlFilterParser
    {
        private readonly IFileGateway _gateway;

        public XmlFilterParser(IFileGateway gateway)
        {
            _gateway = gateway;
        }

        public IEnumerable<RuleDefinition> GetDefinitions(string path)
        {
            var definitions = new List<RuleDefinition>();

            var doc = XDocument.Parse(_gateway.GetContents(path));
            
            foreach (var node in doc.XPathSelectElements("//Filter"))
            {
                var ruleDefinition = new RuleDefinition();

                var operation = node.Attribute("Operation");
                if (operation != null)
                {
                    Enum.TryParse(operation.Value, true, out ruleDefinition.Operation);
                }

                var type = node.Attribute("Type");
                if (type != null)
                {
                    Enum.TryParse(type.Value, true, out ruleDefinition.FilterType);
                }

                var matchType = node.Attribute("MatchType");
                if (matchType != null)
                {
                    Enum.TryParse(matchType.Value, true, out ruleDefinition.MatchType);
                }

                ruleDefinition.Match = node.Value;

                if (!string.IsNullOrEmpty(ruleDefinition.Match))
                {
                    if (ruleDefinition.Match[0] == '!')
                    {
                        if(ruleDefinition.MatchType == MatchType.DoesMatch)
                            throw new InvalidOperationException("Cannot have a Include MatchType with a ! specified before the match");

                        ruleDefinition.Match = ruleDefinition.Match.Substring(1);
                    }

                    ruleDefinition.Match = ruleDefinition.Match.Trim('(', ')');
                }

                definitions.Add(ruleDefinition);
            }

            return definitions;
        }
    }
}
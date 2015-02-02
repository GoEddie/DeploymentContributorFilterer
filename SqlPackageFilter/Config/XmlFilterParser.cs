using System;
using System.Collections.Generic;
using System.Xml;
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
        private readonly IDisplayMessageHandler _messageHandler;
        private const string SecurityFilterMatch = @"^(User|UserDefinedServerRole|ApplicationRole|BuiltInServerRole|Permission|Role|RoleMembership|ServerRoleMembership|User|UserDefinedServerRole)$";

        public XmlFilterParser(IFileGateway gateway, IDisplayMessageHandler messageHandler)
        {
            _gateway = gateway;
            _messageHandler = messageHandler;
        }

        public IEnumerable<RuleDefinition> GetDefinitions(string path)
        {
        
            var definitions = new List<RuleDefinition>();

            XDocument doc = null;

            try
            {
                doc = XDocument.Parse(_gateway.GetContents(path));
            }
            catch (XmlException e)
            {
                _messageHandler.ShowMessage(string.Format("Error parsing Xml Filter File: {0}", e.Message), DisplayMessageLevel.Errors);
                return definitions;
            }
            
            foreach (var node in doc.XPathSelectElements("//Filter"))
            {
                var ruleDefinition = new RuleDefinition();

                var operation = node.Attribute("Operation");
                if (operation == null)
                {
                    _messageHandler.ShowMessage("Error getting filter from xml: Operation attribute was not specified", DisplayMessageLevel.Errors);
                    continue;
                }

                if (!Enum.TryParse(operation.Value, true, out ruleDefinition.Operation))
                {
                    _messageHandler.ShowMessage(string.Format("Found Filter in Xml: but Operation attribute not understood: \"{0}\"", operation.Value), DisplayMessageLevel.Errors);
                    continue;
                }


                var type = node.Attribute("Type");
                if (type == null)
                {
                    _messageHandler.ShowMessage("Error getting filter from xml: Type attribute was not specified", DisplayMessageLevel.Errors);
                    continue;
                }

                if(!Enum.TryParse(type.Value, true, out ruleDefinition.FilterType))
                {
                    _messageHandler.ShowMessage(string.Format("Found Filter in Xml: but Type attribute not understood: \"{0}\"", type.Value), DisplayMessageLevel.Errors);
                    continue;
                }
                

                var matchType = node.Attribute("MatchType");
                if (matchType == null)
                {
                    _messageHandler.ShowMessage("Error getting filter from xml: Type MatchType was not specified", DisplayMessageLevel.Errors);
                    continue;
                }
                
                if (!Enum.TryParse(matchType.Value, true, out ruleDefinition.MatchType))
                {
                    _messageHandler.ShowMessage(string.Format("Found Filter in Xml: but MatchType attribute not understood: \"{0}\"", matchType.Value), DisplayMessageLevel.Errors);
                    continue;
                }

                ruleDefinition.Match = node.Value;

                if (!string.IsNullOrEmpty(ruleDefinition.Match))
                {
                    if (ruleDefinition.Match[0] == '!')
                    {
                        if (ruleDefinition.MatchType == MatchType.DoesMatch)
                        {
                            _messageHandler.ShowMessage("Cannot have a DoesMatch MatchType with a ! specified before the match", DisplayMessageLevel.Errors);
                            continue;
                        }

                        ruleDefinition.Match = ruleDefinition.Match.Substring(1);
                    }

                    ruleDefinition.Match = ruleDefinition.Match.Trim('(', ')');
                }

                _messageHandler.ShowMessage(string.Format("Found Filter in Xml: Operation: {0} FilterType: {1} MatchType: {2} Match: ({3})", ruleDefinition.Operation, ruleDefinition.FilterType, ruleDefinition.MatchType, ruleDefinition.Match), DisplayMessageLevel.Info);

                if (ruleDefinition.FilterType == FilterType.Security)
                {
                    ruleDefinition = new RuleDefinition()
                    {
                        Operation = ruleDefinition.Operation,
                        FilterType = FilterType.Type,
                        Match = SecurityFilterMatch,
                        MatchType = MatchType.DoesMatch //TODO: dup code between this and command line parser - unite them!
                    };
                }
                
                definitions.Add(ruleDefinition);
            }

            if (definitions.Count == 0)
            {
                _messageHandler.ShowMessage("There were no filters found in the Xml config file", DisplayMessageLevel.Errors);
            }

            return definitions;
        }
    }
}
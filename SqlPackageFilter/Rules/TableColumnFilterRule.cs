using System.Linq;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class TableColumnFilterRule : FilterRule
    {
        private readonly FilterOperation _operation;

        public TableColumnFilterRule(FilterOperation operation, string match, MatchType matchType) : base(operation, match, matchType)
        {
            _operation = operation;
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass objectType, DeploymentStep step = null)
        {
            if (step == null)
                return false;

            if (_operation != FilterOperation.Keep)
                return false;

            if (!Matches(name.Parts.LastOrDefault()))
            {
                return false;
            }

            var alterStep = step as AlterElementStep;
            
            if (alterStep == null)
                return false;

            var script = (TSqlScript)alterStep.Script;

            if (script == null)
                return false;

            var batch = script.Batches.FirstOrDefault();

            if (batch == null)
                return false;
            
            var statement = batch.Statements.FirstOrDefault();

            var dropTableElementStatement = statement as AlterTableDropTableElementStatement;

            if (dropTableElementStatement == null)
                return false;

            var toRemove = dropTableElementStatement.AlterTableDropTableElements.Where(p => p.TableElementType == TableElementType.Column).ToList();

            foreach (var alterTableDropTableElement in toRemove)
            {
                dropTableElementStatement.AlterTableDropTableElements.Remove(alterTableDropTableElement);
            }

            return dropTableElementStatement.AlterTableDropTableElements.Count == 0;  //This is a strange one, we remove the bits we want from the drop table element but there might be other things like constraints that should be dropped

        }
    }
}
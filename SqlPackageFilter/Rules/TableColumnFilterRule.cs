using System.Linq;
using AgileSqlClub.SqlPackageFilter.DacExtensions;
using AgileSqlClub.SqlPackageFilter.Filter;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AgileSqlClub.SqlPackageFilter.Rules
{
    public class TableColumnFilterRule : FilterRule
    {
        private readonly FilterOperation _operation;
        private readonly DeploymentFilter _deploymentFilter;
        private readonly string _schemaForMatch = null;

        public TableColumnFilterRule(FilterOperation operation, string match, MatchType matchType,
            DeploymentFilter deploymentFilter) : base(operation, match.Split(',',2).Last(), matchType)
        {
            _operation = operation;
            _deploymentFilter = deploymentFilter;
            if (match.Contains(','))
                _schemaForMatch = match.Split(',', 2).First();
#if DEBUG
            _deploymentFilter?.ShowMessage($"columnfiltering on {match}");
#endif
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass objectType, DeploymentStep step = null)
        {

            if (step == null)
                return false;

            if (_operation != FilterOperation.Keep)
                return false;
#if DEBUG
            _deploymentFilter?.ShowMessage($"filtering {string.Join(",",name.Parts)}");
#endif
            if (_schemaForMatch is not null && name.GetSchemaName(objectType) != _schemaForMatch)
            {
                return false; // it is a different schema
            }

            if (!Matches(name.Parts.LastOrDefault()))
            {
                return false; // it is a different name part.
            }
                
            if (step is SqlTableMigrationStep)
            {
                return true;   //we can't allow a table migration on this table as it would drop our extra columns....
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
            
            //is there a create table statement for our table? if so abort the whole thing
            
            var statement = batch.Statements.FirstOrDefault();

            var dropTableElementStatement = statement as AlterTableDropTableElementStatement;

            if (dropTableElementStatement == null)
                return false;

            var toRemove = dropTableElementStatement.AlterTableDropTableElements.Where(p => p.TableElementType == TableElementType.Column).ToList();

            foreach (var alterTableDropTableElement in toRemove)
            {
                dropTableElementStatement.AlterTableDropTableElements.Remove(alterTableDropTableElement);
            }
       
            if (dropTableElementStatement.AlterTableDropTableElements.Count > 0)
            {
                return false;
            }  //This is a strange one, we remove the bits we want from the drop table element but there might be other things like constraints that should be dropped

            script.Batches.RemoveAt(0);   

            return script.Batches.Count == 0;

        }
    }
}
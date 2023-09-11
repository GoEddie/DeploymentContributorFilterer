using System;
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
            DeploymentFilter deploymentFilter) : base(operation, match.SplitAtFirst(',').Last(), matchType)
        {
            _operation = operation;
            _deploymentFilter = deploymentFilter;
            if (match.Contains(','))
            {
                _schemaForMatch = match.SplitAtFirst(',').First();
#if DEBUG
                _deploymentFilter?.ShowMessage($" -> Table Column Filter - schema: {_schemaForMatch}, rule: {Match}, ");
#endif
            }
            else
            {
#if DEBUG
                _deploymentFilter?.ShowMessage($" -> Table Column Filter - Single part rule: {match} ");
#endif
            }
        }

        public override bool Matches(ObjectIdentifier name, ModelTypeClass objectType, DeploymentStep step = null)
        {

            if (step == null)
                return false;

            if (_operation != FilterOperation.Keep)
                return false;
#if DEBUG
            //_deploymentFilter?.ShowMessage($" -- checking ColumnFilter filter for {string.Join(".",name.Parts)}");
#endif
            if ( !string.IsNullOrWhiteSpace( _schemaForMatch) && name.GetSchemaName(objectType) != _schemaForMatch)
            {
               // _deploymentFilter?.ShowMessage($" - different schema");
                return false; // it is a different schema
            }

            if (!Matches(name.Parts.LastOrDefault()))
            {
                //_deploymentFilter?.ShowMessage($" - different name");
                return false; // it is a different name part.
            }
                
            if (step is SqlTableMigrationStep sts)
            {
/*#if DEBUG
                var sourceTable = sts.SourceElement.GetScript();
                var targetTable = sts.TargetElement.GetScript();

                foreach ( var singleBatch in ((TSqlScript)sts.Script).Batches)
                {
                    _deploymentFilter?.ShowMessage(
                        $@"    - steps : {singleBatch.Statements.Count}");
                }
#else*/
                var sourceTable = string.Join(".", sts.SourceTable?.Name?.Parts??Enumerable.Empty<string>());
                var targetTable = string.Join(".", sts.TargetTable?.Name?.Parts ?? Enumerable.Empty<string>());
                _deploymentFilter?.ShowMessage(sts.TargetTable == null
                    ? $@"  - REMOVED: SqlTableMigrationStep for {sourceTable}"
                    : $@"  - REMOVED: SqlTableMigrationStep for {sourceTable} => currently = {targetTable}");
                //#endif


                return true;   //we can't allow a table migration on this table as it would drop our extra columns....
            }

            if (!(step is AlterElementStep alterStep))
            {
                //_deploymentFilter?.ShowMessage($" - null AlterStep");
                return false;
            }

            var script = (TSqlScript)alterStep.Script;

            if (script == null)
            {
                //_deploymentFilter?.ShowMessage($" - null Script");
                return false;
            }

            var batch = script.Batches.FirstOrDefault();
            if (batch == null)
            {
                //_deploymentFilter?.ShowMessage($" - null Batch");
                return false;
            }

            //is there a create table statement for our table? if so abort the whole thing
            
            var statement = batch.Statements.FirstOrDefault();

            if (!(statement is AlterTableDropTableElementStatement dropTableElementStatement))
            {
                //_deploymentFilter?.ShowMessage($" - not a AlterTableDropTableElementStatement - it is a {dropTableElementStatement.GetType().Name}");
                return false;
            }

            var toRemove = dropTableElementStatement.AlterTableDropTableElements.Where(p => p.TableElementType == TableElementType.Column).ToList();

            foreach (var alterTableDropTableElement in toRemove)
            {
                dropTableElementStatement.AlterTableDropTableElements.Remove(alterTableDropTableElement);
                _deploymentFilter?.ShowMessage($" -- Keeping {name}.[{alterTableDropTableElement.Name.Value}]");
            }
       
            if (dropTableElementStatement.AlterTableDropTableElements.Count > 0)
            {
                foreach (var keptElement in dropTableElementStatement.AlterTableDropTableElements)
                {
                    _deploymentFilter?.ShowMessage($"keeping {keptElement.Name.Value}");
                }
                _deploymentFilter?.ShowMessage($" - cleaned statements");
                return false;
            }  //This is a strange one, we remove the bits we want from the drop table element but there might be other things like constraints that should be dropped

            script.Batches.RemoveAt(0);
            //_deploymentFilter?.ShowMessage($" - batches remaining : {script.Batches.Count}");

            return script.Batches.Count == 0;

        }
    }

    public static class StringExtn
    {
        public static string[] SplitAtFirst(this string s, char splitter)
        {
            int charIndex = s.IndexOf(splitter);
            if (charIndex < 0)
                return new[] { s};
            return new[] { s.Substring(0, charIndex), s.Substring(charIndex + 1) };
        }
    }
    
}
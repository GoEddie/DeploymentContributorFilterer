using AgileSqlClub.SqlPackageFilter.Rules;
using AgileSqlClub.SqlPackageFilter.Extensions;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
    public static class DeploymentStepDecider
    {
        public static DeploymentStepDecision Decide(DeploymentStep step, KeeperDecider decider)
        {
            return RemoveCreateElement(step, decider) ?? RemoveDropStep(step, decider) ?? RemoveAlterStep(step, decider) ?? RemoveDataLossCheckStep(step, decider);
        }

        private static DeploymentStepDecision RemoveDataLossCheckStep(DeploymentStep step, KeeperDecider decider)
        {
            var dlcStep = new DataLossCheckStep(step);

            if (dlcStep == null || !dlcStep.IsDataLossCheck)
                return null;

            var objectNames = GetAllIdentifiersFromStep(step);

            return new DeploymentStepDecision()
            {
                Remove = decider.ShouldRemoveFromPlan(new ObjectIdentifier(objectNames), ModelSchema.Table, StepType.Drop),
                StepType = StepType.DataLossCheck,
                ObjectNames = objectNames
            };
        }

        private static DeploymentStepDecision RemoveAlterStep(DeploymentStep step, KeeperDecider decider)
        {
            var alterStep = step as AlterElementStep;

            if (alterStep == null)
                return null;

            var objectNames = GetAllIdentifiersFromStep(step);

            return new DeploymentStepDecision()
            {
                Remove = decider.ShouldRemoveFromPlan(new ObjectIdentifier(objectNames), alterStep.TargetElement?.ObjectType, StepType.Alter, alterStep),
                StepType = StepType.Alter,
                ObjectNames = GetAllIdentifiersFromStep(step)
            };
        }

        private static DeploymentStepDecision RemoveDropStep(DeploymentStep step, KeeperDecider decider)
        {
            var dropStep = step as DropElementStep;

            if (dropStep == null)
                return null;

            var objectNames = GetAllIdentifiersFromStep(step);

            return new DeploymentStepDecision()
            {
                Remove = decider.ShouldRemoveFromPlan(new ObjectIdentifier(objectNames), dropStep.TargetElement?.ObjectType, StepType.Drop),
                StepType = StepType.Drop,
                ObjectNames = objectNames
            };
        }

        private static DeploymentStepDecision RemoveCreateElement(DeploymentStep step, KeeperDecider decider)
        {
            var createStep = step as CreateElementStep;

            if (createStep == null)
                return null;

            var objectNames = GetAllIdentifiersFromStep(step);

            return new DeploymentStepDecision()
            {
                Remove = decider.ShouldRemoveFromPlan(new ObjectIdentifier(objectNames), createStep.SourceElement?.ObjectType, StepType.Create),
                StepType = StepType.Create,
                ObjectNames = objectNames
            };
        }

        private static Optional<string> GetColumnFromAlterTableStatement(TSqlStatement statement)
        {
            return statement
                .AsOptional(s => s as AlterTableAddTableElementStatement)
                .ValueOrDefault(ats => ats?.Definition?.TableConstraints)
                .OptionalAt(0).ValueOrDefault(tc => tc as UniqueConstraintDefinition)
                .ValueOrDefault(ucd => ucd?.Columns)
                .OptionalAt(0).ValueOrDefault(c => c?.Column?.MultiPartIdentifier?.Identifiers)
                .OptionalAt(0).ValueOrDefault(i => i?.Value);
        }

        private static Optional<string> GetTableTokensFromAlterTableStatement(TSqlStatement statement)
        {
            var returnValue = String.Empty;

            var optional = statement
                .AsOptional(s => s as AlterTableAddTableElementStatement)
                .ValueOrDefault(ats => ats?.SchemaObjectName?.Identifiers?.Select(i => i.Value))
                .Default(() => new List<String>())
                .Evaluate();

            return String.Join(".", optional).AsOptional();
        }

        private static string[] GetAllIdentifiersFromStep(DeploymentStep sourceStep)
        {
            var tokens = new List<string>();

            if (sourceStep is CreateElementStep)
            {
                var step = sourceStep as CreateElementStep;

                var statement = step.Script
                    .AsOptional(s => s as TSqlScript)
                    .ValueOrDefault(s => s?.Batches)
                    .OptionalAt(0).ValueOrDefault(b => b?.Statements)
                    .OptionalAt(0);

                var column = GetColumnFromAlterTableStatement(statement.Evaluate());
                var tableTokens = GetTableTokensFromAlterTableStatement(statement.Evaluate());

                tokens.Add(step.SourceElement?.Name?.ToString() ?? "");
                tokens.Add(column.Evaluate());
                tokens.Add(tableTokens.Evaluate());
            }
            else if (sourceStep is DropElementStep)
            {
                var step = sourceStep as DropElementStep;
                //todo optional stuff
                tokens.Add(step.TargetElement?.Name?.ToString() ?? "");

            }
            else if (sourceStep is AlterElementStep)
            {
                var step = sourceStep as AlterElementStep;
                //todo optional stuff
                tokens.Add(step.TargetElement?.Name?.ToString() ?? "");
            }
            else
            {
                var step = new DataLossCheckStep(sourceStep);
                if (step != null && step.IsDataLossCheck)
                {
                    //todo optional stuff
                    tokens.Add(step.ObjectName.ToString());
                }
            }

            return tokens.ToArray();
        }
    }
}
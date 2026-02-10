using System;
using System.Collections.Generic;
using System.Linq;
using AgileSqlClub.SqlPackageFilter.ReleaseStep;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;
using AgileSqlClub.SqlPackageFilter.DacExtensions;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
    public static class DeploymentStepDecider
    {
        public static DeploymentStepDecision Decide(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
        {
            return RemoveCreateElement(step, decider, logSink) ??
                   RemoveDropStep(step, decider, logSink) ??
                   RemoveAlterStep(step, decider, logSink) ??
                   RemoveDataLossCheckStep(step, decider, logSink)
                   ;
        }

        private static DeploymentStepDecision RemoveDataLossCheckStep(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
        {
            var dlcStep = new DataLossCheckStep(step);
            if (dlcStep.IsDataLossCheck)
            {
                return new DeploymentStepDecision()
                {
                    Remove = decider.ShouldRemoveFromPlan(dlcStep.ObjectName, ModelSchema.Table, StepType.Drop, null, logSink),
                    StepType = StepType.DataLossCheck,
                    ObjectName = dlcStep.ObjectName.ToString()
                };
            }
            return null;
        }

        private static List<string> DroppedObjects = new List<string>();
        private static DeploymentStepDecision RemoveAlterStep(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
        {
            if (!(step is AlterElementStep alterStep)) return null;

            var remove = decider.ShouldRemoveFromPlan(alterStep.TargetElement?.Name ?? new ObjectIdentifier(),
                alterStep.TargetElement?.ObjectType, StepType.Alter, alterStep, logSink);

            var objectName = alterStep.TargetElement?.Name?.ToString() ?? "";
            if (remove && alterStep is SqlTableMigrationStep)
            {
                logSink($"    -- {objectName} flagged for future consideration", DisplayMessageLevel.Info);
                DroppedObjects.Add(objectName);
            }

            return new DeploymentStepDecision()
            {
                Remove = remove,
                StepType = StepType.Alter,
                ObjectName = objectName
            };
        }

        private static DeploymentStepDecision RemoveDropStep(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
        {
            if (step is DropElementStep dropStep)
            {
                var targetElementName = dropStep.TargetElement?.Name;

                //special handling to detect unnamed check and default constraints... we need to create a dummy name for these with at least the same schema as the owning object
                if ((dropStep.TargetElement?.ObjectType.Name == ModelSchema.CheckConstraint.Name) || (dropStep.TargetElement?.ObjectType.Name == ModelSchema.DefaultConstraint.Name))
                {
                    if (targetElementName == null || !targetElementName.HasName)
                    {
                        var owner = dropStep.TargetElement?.GetParent();
                        var schema = owner?.GetSchemaName();
                        logSink($" detected unnamed {dropStep.TargetElement?.ObjectType.Name} constraint on '{owner.Name}'", DisplayMessageLevel.Info);
                        targetElementName = new ObjectIdentifier(schema, "unnamed constraint");
                    }
                }
               
                return new DeploymentStepDecision()
                {
                    Remove = decider.ShouldRemoveFromPlan(targetElementName ?? new ObjectIdentifier(), dropStep.TargetElement?.ObjectType, StepType.Drop, null, logSink),
                    StepType = StepType.Drop,
                    ObjectName = targetElementName?.ToString() ?? ""
                };
            }
            return null;
        }

        private static DeploymentStepDecision RemoveCreateElement(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
        {
            if (!(step is CreateElementStep createStep)) return null;

            //special handling to detect unnamed check and default constraints... we need to create a dummy name for these with at least the same schema as the owning object
            var sourceElementName = createStep.SourceElement?.Name;
            
            if ((createStep.SourceElement?.ObjectType.Name == ModelSchema.CheckConstraint.Name) || (createStep.SourceElement?.ObjectType.Name == ModelSchema.DefaultConstraint.Name))
            {
                if (sourceElementName == null || !sourceElementName.HasName)
                {
                    var owner = createStep.SourceElement?.GetParent();
                    string schema = owner?.GetSchemaName();
                    logSink($" detected unnamed {createStep.SourceElement?.ObjectType.Name} constraint on {owner?.Name}", DisplayMessageLevel.Info);
                    sourceElementName = new ObjectIdentifier(schema, "unnamed constraint");
                }
            }

            var shouldRemove = decider.ShouldRemoveFromPlan(sourceElementName ?? new ObjectIdentifier(),
            createStep.SourceElement?.ObjectType, StepType.Create, null, logSink);
            var objectName = createStep.SourceElement?.Name?.ToString() ?? "";

            DeploymentStep replaceDeploymentStep = null;
            // the only object that contain [dbo].[tablename]. is an index with FQN of [dbo].[tablename].[indexName]
            if (DroppedObjects.Any(c => objectName.StartsWith(c)))
            {
                if (createStep.SourceElement?.ObjectType.Name == "Index")
                {
                    replaceDeploymentStep = new TryDropIndexDeploymentStep(createStep);
                    logSink($"    -- {objectName} of type {createStep.SourceElement.ObjectType.Name} has been replaced with an alternate {replaceDeploymentStep.GetType().Name}", DisplayMessageLevel.Info);
                }
                else
                {
                    logSink($"    -- {objectName} of type {createStep.SourceElement?.ObjectType.Name ?? "<unknown>"} has a dependency but not replacement", DisplayMessageLevel.Info);
                }
            }

            return new DeploymentStepDecision()
            {
                Remove = shouldRemove || replaceDeploymentStep != null,
                StepType = StepType.Create,
                ObjectName = objectName,
                ReplacementStep = replaceDeploymentStep
            };
        }
    }
}
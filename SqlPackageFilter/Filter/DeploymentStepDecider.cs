using System;
using System.Collections.Generic;
using System.Linq;
using AgileSqlClub.SqlPackageFilter.ReleaseStep;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
  public static class DeploymentStepDecider
  {
    public static DeploymentStepDecision Decide(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
    {
      return RemoveCreateElement(step, decider, logSink) ?? 
             RemoveDropStep(step, decider) ?? 
             RemoveAlterStep(step, decider, logSink) ?? 
             RemoveDataLossCheckStep(step, decider) 
             ;
    }

    private static DeploymentStepDecision RemoveDataLossCheckStep(DeploymentStep step, KeeperDecider decider)
    {
      var dlcStep = new DataLossCheckStep(step);
      return !dlcStep.IsDataLossCheck ? null : new DeploymentStepDecision() {
        Remove = decider.ShouldRemoveFromPlan(dlcStep.ObjectName, ModelSchema.Table, StepType.Drop),
        StepType = StepType.DataLossCheck,
        ObjectName = dlcStep.ObjectName.ToString()
      };
    }

    private static List<string> DroppedObjects = new List<string>();
    private static DeploymentStepDecision RemoveAlterStep(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
    {
        if (!(step is AlterElementStep alterStep)) return null;

      var remove = decider.ShouldRemoveFromPlan(alterStep.TargetElement?.Name ?? new ObjectIdentifier(),
          alterStep.TargetElement?.ObjectType, StepType.Alter, alterStep);

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

    private static DeploymentStepDecision RemoveDropStep(DeploymentStep step, KeeperDecider decider)
    {
        return !(step is DropElementStep dropStep) ? null : new DeploymentStepDecision()
      {
        Remove = decider.ShouldRemoveFromPlan(dropStep.TargetElement?.Name ?? new ObjectIdentifier(), dropStep.TargetElement?.ObjectType, StepType.Drop),
        StepType = StepType.Drop,
        ObjectName = dropStep.TargetElement?.Name?.ToString() ?? ""
      };
    }

    private static DeploymentStepDecision RemoveCreateElement(DeploymentStep step, KeeperDecider decider, Action<string, DisplayMessageLevel> logSink)
    {
        if (!(step is CreateElementStep createStep)) return null;
            var shouldRemove = decider.ShouldRemoveFromPlan(createStep.SourceElement?.Name ?? new ObjectIdentifier(),
            createStep.SourceElement?.ObjectType, StepType.Create);
        var objectName = createStep.SourceElement?.Name?.ToString() ?? "";
        //logSink($"    -- {objectName} should {(shouldRemove ? "" : "NOT ")}be removed");

        DeploymentStep replaceDeploymentStep = null;
        // the only object that contain [dbo].[tablename]. is an index with FQN of [dbo].[tablename].[indexName]
        if (DroppedObjects.Any(c => objectName.StartsWith(c)))
        {
            if (createStep.SourceElement?.ObjectType.Name == "Index")
            {
                replaceDeploymentStep = new TryDropIndexDeploymentStep(createStep);
                logSink(
                    $"    -- {objectName} of type {createStep.SourceElement.ObjectType.Name} has been replaced with an alternate {replaceDeploymentStep.GetType().Name}", DisplayMessageLevel.Info);
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
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

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
      return !dlcStep.IsDataLossCheck ? null : new DeploymentStepDecision() {
        Remove = decider.ShouldRemoveFromPlan(dlcStep.ObjectName, ModelSchema.Table, StepType.Drop),
        StepType = StepType.DataLossCheck,
        ObjectName = dlcStep.ObjectName.ToString()
      };
    }

    private static DeploymentStepDecision RemoveAlterStep(DeploymentStep step, KeeperDecider decider)
    {
      var alterStep = step as AlterElementStep;

      return alterStep == null ? null : new DeploymentStepDecision()
      {
        Remove = decider.ShouldRemoveFromPlan(alterStep.TargetElement?.Name ?? new ObjectIdentifier(), alterStep.TargetElement?.ObjectType, StepType.Alter, alterStep),
        StepType = StepType.Alter,
        ObjectName = alterStep.TargetElement?.Name?.ToString() ?? ""
      };
    }

    private static DeploymentStepDecision RemoveDropStep(DeploymentStep step, KeeperDecider decider)
    {
      var dropStep = step as DropElementStep;

      return dropStep == null ? null : new DeploymentStepDecision()
      {
        Remove = decider.ShouldRemoveFromPlan(dropStep.TargetElement?.Name ?? new ObjectIdentifier(), dropStep.TargetElement?.ObjectType, StepType.Drop),
        StepType = StepType.Drop,
        ObjectName = dropStep.TargetElement?.Name?.ToString() ?? ""
      };
    }

    private static DeploymentStepDecision RemoveCreateElement(DeploymentStep step, KeeperDecider decider)
    {
      var createStep = step as CreateElementStep;

      return createStep == null ? null : new DeploymentStepDecision()
      {
        Remove = decider.ShouldRemoveFromPlan(createStep.SourceElement?.Name ?? new ObjectIdentifier(), createStep.SourceElement?.ObjectType, StepType.Create),
        StepType = StepType.Create,
        ObjectName = createStep.SourceElement?.Name?.ToString() ?? ""
      };
    }
  }
}
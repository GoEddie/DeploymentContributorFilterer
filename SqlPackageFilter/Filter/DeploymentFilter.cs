using System;
using System.Diagnostics;
using System.Dynamic;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
  [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "1.4.4.1")]
  public class DeploymentFilter : DeploymentPlanModifier, IDisplayMessageHandler
  {
    private DisplayMessageLevel _displayLevel = DisplayMessageLevel.Errors;

    public void ShowMessage(string message, DisplayMessageLevel level)
    {
      if (_displayLevel >= level)
        ShowMessage(message);
    }

    public void SetMessageLevel(DisplayMessageLevel level)
    {
      _displayLevel = level;
    }

    public void ShowMessage(string message)
    {
      PublishMessage(new ExtensibilityError(message, Severity.Message));
    }

    protected override void OnExecute(DeploymentPlanContributorContext context)
    {            
      try
      {

        PublishMessage(new ExtensibilityError("Starting AgileSqlClub.DeploymentFilterContributor", Severity.Message));
        /*
        foreach (var contextArgument in context.Arguments)
        {
            ShowMessage($"{contextArgument.Key}:{contextArgument.Value}");
        }*/

        var rules = new RuleDefinitionFactory(this).BuildRules(context.Arguments, this);

        var decider = new KeeperDecider(rules);

        var next = context.PlanHandle.Head;

        while (next != null)
        {

          var current = next;
          next = current.Next;

          var stepDecider = DeploymentStepDecider.Decide(current, decider);

          if (stepDecider != null)
          {
            if (stepDecider.Remove)
            {
              Remove(context.PlanHandle, current);
              PublishMessage(new ExtensibilityError($"Step removed from deployment by SqlPackageFilter, object: {stepDecider.ObjectName}, step type: {stepDecider.StepType}", Severity.Message));
            }
            else if (_displayLevel == DisplayMessageLevel.Info)
              PublishMessage(new ExtensibilityError($"Step has not been removed from deployment, object: {stepDecider.ObjectName} type: {stepDecider.StepType}", Severity.Message));

          }
        }

        PublishMessage(new ExtensibilityError("Completed AgileSqlClub.DeploymentFilterContributor", Severity.Message));
      }
      catch (Exception e)
      {
        //global exception as we don't want to break sqlpackage.exe
        PublishMessage(new ExtensibilityError($"Error in DeploymentFilter: {e.Message}\r\nStack: {e.StackTrace}", Severity.Error));
      }
    }
  }
}
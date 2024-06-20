using System;
using System.Diagnostics;
using System.Dynamic;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
  [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "1.4.4.1")]
  public class DeploymentFilter : DeploymentPlanModifier, IDisplayMessageHandler
  {
    private DisplayMessageLevel _displayLevel = DisplayMessageLevel.Info;

    public void ShowMessage(string message, DisplayMessageLevel level)
    {
        if (level >= _displayLevel)
        {
            Severity severity = Severity.Message;
            if (level == DisplayMessageLevel.Errors)
            {
                severity = Severity.Error;
            }
            else if (level == DisplayMessageLevel.Warning)
            {
                severity = Severity.Warning;
            }
            
            PublishMessage(new ExtensibilityError(message, severity));
        }
    }

    public void SetMessageLevel(DisplayMessageLevel level)
    {
      _displayLevel = level;
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
            
            if (_displayLevel == DisplayMessageLevel.Debug)
            {
                ShowMessage($" -- checking filter for a {current.GetType().Name}", DisplayMessageLevel.Debug);
                if (current is CreateElementStep ce)
                {
                    ShowMessage($"    -- for creating a {ce.SourceElement.Name}", DisplayMessageLevel.Debug);
                }
            }

          var stepDecider = DeploymentStepDecider.Decide(current, decider, this.ShowMessage);
          
          if (stepDecider != null)
          {
            if (stepDecider.Remove)
            {
                if (stepDecider.ReplacementStep != null)
                {
                    AddBefore(context.PlanHandle, current, stepDecider.ReplacementStep);

                    Remove(context.PlanHandle, current);
                    ShowMessage($"Step REPLACED from deployment by SqlPackageFilter, object: {stepDecider.ObjectName}, step type: {stepDecider.StepType} , replaced with type: {stepDecider.ReplacementStep}",
                        DisplayMessageLevel.Info);
                }
                else
                {

                    Remove(context.PlanHandle, current);
                    ShowMessage($"Step removed from deployment by SqlPackageFilter, object: {stepDecider.ObjectName}, step type: {stepDecider.StepType}",
                        DisplayMessageLevel.Info);
                }
            }
            else
            {
                ShowMessage($"Step has not been removed from deployment, object: {stepDecider.ObjectName} type: {stepDecider.StepType}", DisplayMessageLevel.Debug);
            }

          }
        }

        PublishMessage(new ExtensibilityError("Completed AgileSqlClub.DeploymentFilterContributor", Severity.Message));
      }
      catch (Exception e)
      {
          ShowMessage($"Error in DeploymentFilter: {e.Message}\r\nStack: {e.StackTrace}", DisplayMessageLevel.Errors);
          throw;
      }
    }
  }
}
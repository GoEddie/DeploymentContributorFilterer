using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
    [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "1.4.1.0")]
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
                PublishMessage(new ExtensibilityError("Starting AgileSqlClub.DeploymentFilterContributor",
                    Severity.Message));

                var rules = new RuleDefinitionFactory(this).BuildRules(context.Arguments, this);

                var decider = new KeeperDecider(rules);

                var next = context.PlanHandle.Head;
                while (next != null)
                {
                    var current = next;
                    next = current.Next;

                    var name = "";

                    var shouldRemove = false;
                    var stepType = StepType.Other;

                    var createStep = current as CreateElementStep;
                    if (createStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(createStep.SourceElement.Name,
                            createStep.SourceElement.ObjectType, StepType.Create);
                        stepType = StepType.Create;
                        name = createStep.SourceElement.Name.ToString();
                    }

                    var dropStep = current as DropElementStep;
                    if (dropStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(dropStep.TargetElement.Name,
                            dropStep.TargetElement.ObjectType, StepType.Drop);
                        stepType = StepType.Drop;
                        name = dropStep.TargetElement.Name.ToString();
                    }

                    var alterStep = current as AlterElementStep;
                    if (alterStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(alterStep.TargetElement.Name,
                            alterStep.TargetElement.ObjectType, StepType.Alter, alterStep);
                        stepType = StepType.Alter;
                        name = alterStep.SourceElement.Name.ToString();
                        
                    }


                    if (shouldRemove)
                    {
                        Remove(context.PlanHandle, current);
                        PublishMessage(
                            new ExtensibilityError(
                                string.Format("Step removed from deployment by SqlPackageFilter, object: {0}", name),
                                Severity.Message));
                    }
                    else
                    {
                        if (_displayLevel == DisplayMessageLevel.Info)
                        {
                            PublishMessage(
                                new ExtensibilityError(
                                    string.Format("Step has not been removed from deployment, object: {0} type: {1}",
                                        name, stepType),
                                    Severity.Message));
                        }
                    }
                }

                PublishMessage(new ExtensibilityError("Completed AgileSqlClub.DeploymentFilterContributor",
                    Severity.Message));
            }
            catch (Exception e)
            {
                //global exception as we don't want to break sqlpackage.exe
                PublishMessage(
                    new ExtensibilityError(
                        string.Format("Error in DeploymentFilter: {0}\r\nStack: {1}", e.Message, e.StackTrace),
                        Severity.Error));
            }
        }
    }
}
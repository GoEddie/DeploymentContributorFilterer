using System;
using System.Windows.Forms;
using AgileSqlClub.SqlPackageFilter.Config;
using AgileSqlClub.SqlPackageFilter.Rules;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
    [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "0.1.0.0")]
    public class DeploymentFilter : DeploymentPlanModifier, IDisplayMessageHandler
    {
        public void ShowMessage(string message)
        {
            base.PublishMessage(new ExtensibilityError(message, Severity.Message));
        }

        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            try
            {
                base.PublishMessage(new ExtensibilityError("Starting AgileSqlClub.DeploymentFilterContributor", Severity.Message));

                var rules = new RuleDefinitionFactory(this).BuildRules(context.Arguments, this);

                var decider = new KeeperDecider(rules);

                var next = context.PlanHandle.Head;
                while (next != null)
                {
                    var current = next;
                    next = current.Next;

                    var name = "";

                    bool shouldRemove = false;
                    var stepType = StepType.Other;

                    var createStep = current as CreateElementStep;
                    if (createStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(createStep.SourceElement.Name, createStep.SourceElement.ObjectType, StepType.Create);
                        stepType = StepType.Create;
                        name = createStep.SourceElement.Name.ToString();

                    }
                    
                    var dropStep = current as DropElementStep;
                    if (dropStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(dropStep.TargetElement.Name, dropStep.TargetElement.ObjectType, StepType.Drop);
                        stepType = StepType.Drop;
                        name = dropStep.TargetElement.Name.ToString();
                    }

                    var alterStep = current as AlterElementStep;
                    if (alterStep != null)
                    {
                        shouldRemove = decider.ShouldRemoveFromPlan(alterStep.TargetElement.Name, alterStep.TargetElement.ObjectType, StepType.Alter);
                        stepType = StepType.Alter;
                        name = alterStep.SourceElement.Name.ToString();
                    }


                    if (shouldRemove)
                    {
                        base.Remove(context.PlanHandle, current);
                        base.PublishMessage(
                            new ExtensibilityError(
                                string.Format("Step removed from deployment by SqlPackageFilter, object: {0}", name),
                                Severity.Message));
                    }
                    else
                    {
                        if (_displayLevel == DisplayMessageLevel.Info)
                        {
                            base.PublishMessage(
                                    new ExtensibilityError(
                                        string.Format("Step has not been removed from deployment, object: {0} type: {1}", name, stepType),
                                        Severity.Message));
                            
                        }
                    }

                }

                base.PublishMessage(new ExtensibilityError("Completed AgileSqlClub.DeploymentFilterContributor", Severity.Message));

            }
            catch (Exception e)
            {   //global exception as we don't want to break sqlpackage.exe
                base.PublishMessage(new ExtensibilityError(string.Format("Error in DeploymentFilter: {0}\r\nStack: {1}", e.Message, e.StackTrace), Severity.Error));
            }
        }

        public void ShowMessage(string message, DisplayMessageLevel level)
        {
            if(_displayLevel >= level)
               ShowMessage(message);
        }

        private DisplayMessageLevel _displayLevel = DisplayMessageLevel.Errors;

        public void SetMessageLevel(DisplayMessageLevel level)
        {
            _displayLevel = level;
        }
    }
    
}

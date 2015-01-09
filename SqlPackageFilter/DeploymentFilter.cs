using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Extensibility;

namespace AgileSqlClub.SqlPackageFilter
{
    public enum StepType
    {
        Other,
        Create,
        Alter,
        Drop
    }

    public class StepTypeSniffer
    {
        public StepType GetStepType(DeploymentStep step)
        {
            if (step as CreateElementStep != null)
                return StepType.Create;

            if (step as DropElementStep != null)
                return StepType.Drop;

            if (step as AlterElementStep != null)
                return StepType.Alter;

            return StepType.Other;
        }
    }

    [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "0.1.0.0")]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            
            var next = context.PlanHandle.Head;
            while (next != null)
            {
                var current = next;
                next = current.Next;

                Console.WriteLine(current.GetType());
                
                var createStep = current as CreateElementStep;
                if (createStep != null )//&& ShouldFilter(createStep))
                {
                    Console.WriteLine(context.PlanHandle + " : " + createStep.SourceElement.Name.ToString());
                    //base.PublishMessage(new ExtensibilityError("removing item from deployment..", Severity.Message));
                }

                var dropStep = current as DropElementStep;
                if (dropStep != null)
                {
                    Console.WriteLine("DROP: " + context.PlanHandle + " : " + dropStep.TargetElement.Name.ToString());
                }
            }
                    
        }
    }
    
}

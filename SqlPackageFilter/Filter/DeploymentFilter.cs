using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dac.Deployment;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
    [ExportDeploymentPlanModifier("AgileSqlClub.DeploymentFilterContributor", "0.1.0.0")]
    public class DeploymentFilter : DeploymentPlanModifier
    {
        protected override void OnExecute(DeploymentPlanContributorContext context)
        {
            foreach (var arg in context.Arguments)
            {
                MessageBox.Show(arg.Key + " : " + arg.Value);
            }
           
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

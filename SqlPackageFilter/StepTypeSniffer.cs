using Microsoft.SqlServer.Dac.Deployment;

namespace AgileSqlClub.SqlPackageFilter
{
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
}
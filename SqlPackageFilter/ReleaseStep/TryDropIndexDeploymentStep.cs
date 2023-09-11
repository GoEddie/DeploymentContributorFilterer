using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Deployment;

namespace AgileSqlClub.SqlPackageFilter.ReleaseStep
{
    public class TryDropIndexDeploymentStep: DeploymentStep
    {
        private CreateElementStep wrappedStep;
        public TryDropIndexDeploymentStep(CreateElementStep wrappedStep)
        {
            this.wrappedStep = wrappedStep;
        }

        public override IList<string> GenerateTSQL()
        {
            var name = wrappedStep.SourceElement.Name;
            var dropStatement = $"DROP INDEX IF EXISTS {string.Join(".", name)}";
            var statementList = new List<string>();
            statementList.Add(dropStatement);
            statementList.AddRange(wrappedStep.GenerateTSQL());

            return statementList;
        }
    }
}
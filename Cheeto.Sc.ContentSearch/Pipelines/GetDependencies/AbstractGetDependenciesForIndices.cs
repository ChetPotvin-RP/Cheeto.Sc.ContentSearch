using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Pipelines.GetDependencies;

namespace Cheeto.Sc.ContentSearch.Pipelines.GetDependencies
{
    public abstract class AbstractGetDependenciesForIndices : BaseProcessor
    {
        public abstract void ExecuteProcess(GetDependenciesArgs args);

        public override void Process(GetDependenciesArgs args)
        {
            string job = Sitecore.Context.Job.Name;
            ExecuteProcess(args);
        }
    }
}

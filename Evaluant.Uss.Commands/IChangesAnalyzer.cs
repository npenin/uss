using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
    public interface IChangesAnalyzer
    {
        CommandCollection Commands { get; set; }

        void ComputeChanges(Entity entity);
        void ComputeChanges(IEnumerable<Entity> entities);

        void ComputeChanges(Entity entity, bool ignoreReferences);
        void ComputeChanges(IEnumerable<Entity> entities, bool ignoreReferences);

        void ComputeChanges(Entity entity, ICollection<Entity> alreadyProcessedEntities);
    }
}

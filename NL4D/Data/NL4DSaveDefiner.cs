using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace NL4D.Data
{
    /// <summary>
    /// Registers custom saveable types and container definitions with TaleWorlds.SaveSystem.
    /// Base ID 2849170 is assigned uniquely to Not Left 4 Dead.
    /// </summary>
    public sealed class NL4DSaveDefiner : SaveableTypeDefiner
    {
        public NL4DSaveDefiner() : base(2849170)
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(RescuedSurvivorRecord), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<RescuedSurvivorRecord>));
        }
    }
}

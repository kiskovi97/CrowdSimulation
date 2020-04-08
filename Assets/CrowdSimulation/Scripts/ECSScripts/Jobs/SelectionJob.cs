using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using static AnimatorSystem;
public struct SelectionJob : IJobForEachWithEntity<Selection>
{

    public void Execute(Entity entity, int index, ref Selection selection)
    {
      
    }
}

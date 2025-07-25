using System.Collections.Generic;

namespace BehaviorTree
{
    public class Sequence : BTNode
    {
        private List<BTNode> children;

        public Sequence(List<BTNode> children) => this.children = children;

        public override BTResult Tick()
        {
            foreach (var child in children)
            {
                var result = child.Tick();
                if (result != BTResult.Success)
                    return result;
            }
            return BTResult.Success;
        }
    }
}
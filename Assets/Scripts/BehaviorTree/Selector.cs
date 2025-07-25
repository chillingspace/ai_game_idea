using System.Collections.Generic;

namespace BehaviorTree
{
    public class Selector : BTNode
    {
        private List<BTNode> children;

        public Selector(List<BTNode> children) => this.children = children;

        public override BTResult Tick()
        {
            foreach (var child in children)
            {
                var result = child.Tick();
                if (result != BTResult.Failure)
                    return result;
            }
            return BTResult.Failure;
        }
    }
}

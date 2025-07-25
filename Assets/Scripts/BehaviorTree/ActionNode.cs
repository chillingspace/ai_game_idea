using System;

namespace BehaviorTree
{
    public class ActionNode : BTNode
    {
        private Func<BTResult> action;

        public ActionNode(Func<BTResult> action) => this.action = action;

        public override BTResult Tick() => action();
    }
}

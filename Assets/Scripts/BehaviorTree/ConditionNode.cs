using System;

namespace BehaviorTree
{
    public class ConditionNode : BTNode
    {
        private Func<bool> condition;

        public ConditionNode(Func<bool> condition) => this.condition = condition;

        public override BTResult Tick() => condition() ? BTResult.Success : BTResult.Failure;
    }
}

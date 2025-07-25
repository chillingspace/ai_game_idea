namespace BehaviorTree
{
    public enum BTResult { Success, Failure, Running }

    public abstract class BTNode
    {
        public abstract BTResult Tick();
    }
}
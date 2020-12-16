
namespace TotalAI
{
    public abstract class Level
    {
        protected LevelType levelType;
        protected float level;
        protected bool disabled;

        public abstract float GetLevel();

        // Returns the actual amount that was changed
        public abstract float ChangeLevel(float amount);

        public virtual void SetActive(Agent agent, bool active)
        {
            disabled = !active;
        }

        public virtual bool GetStatus()
        {
            return !disabled;
        }

    }
}
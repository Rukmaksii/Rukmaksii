namespace GameScene.model
{
    public interface IKillable
    {
        /**
         * <summary>inflicts <see cref="damage"/> to the <see cref="IKillable"/></summary>
         * <param name="damage">the damages to be inflicted</param>
         * <returns>true if the <see cref="IKillable"/> is still alive, false otherwise</returns>
         */
        public bool TakeDamage(int damage);

        /**
         * <summary>called when an <see cref="IKillable"/> is killed</summary>
         */
        public void OnKill();
    }
}
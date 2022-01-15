namespace model
{

    public enum WeaponType
    {
        CloseRange,
        Light,
        Heavy
    }
    
    
    public interface IWeapon
    {
        public bool Fire();

        public void Reload();
    }
}
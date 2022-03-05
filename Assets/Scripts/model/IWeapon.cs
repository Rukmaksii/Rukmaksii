namespace model
{

    public enum WeaponType
    {
        CloseRange = 0,
        Light = 1,
        Heavy = 2
    }
    
    
    public interface IWeapon
    {
        public void Fire();
        public void Reload();
    }
}
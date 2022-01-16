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
        public void Reload();
    }
}
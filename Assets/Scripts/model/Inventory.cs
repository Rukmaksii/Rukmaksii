using PlayerControllers;
using Weapons;

namespace model
{
    public class Inventory
    {
        private PlayerController Player;

        public Jetpack Jetpack { get; set; }

        private BaseWeapon closeRangeWeapon;
        private BaseWeapon heavyWeapon;
        private BaseWeapon lightWeapon;

        private WeaponType selectedType;

        public BaseWeapon CurrentWeapon
        {
            get
            {
                BaseWeapon availableWeapon = null;
                if (closeRangeWeapon != null)
                {
                    if (selectedType == closeRangeWeapon.Type)
                    {
                        return closeRangeWeapon;
                    }
                    else
                    {
                        availableWeapon = closeRangeWeapon;
                    }
                }
                else if (lightWeapon != null)
                {
                    if (selectedType == lightWeapon.Type)
                    {
                        return lightWeapon;
                    }
                    else
                    {
                        availableWeapon = lightWeapon;
                    }
                }
                else if (heavyWeapon != null)
                {
                    if (selectedType == heavyWeapon.Type)
                    {
                        return heavyWeapon;
                    }
                    else
                    {
                        availableWeapon = heavyWeapon;
                    }
                }

                return availableWeapon;
            }
        }

        public Inventory(PlayerController player)
        {
            this.Player = player;
        }


        public void AddWeapon(BaseWeapon newWeapon)
        {
            switch (newWeapon.Type)
            {
                case WeaponType.Heavy:
                    heavyWeapon = newWeapon;
                    break;
                case WeaponType.Light:
                    lightWeapon = newWeapon;
                    break;
                case WeaponType.CloseRange:
                    closeRangeWeapon = newWeapon;
                    break;
            }
        }
        
        
        public bool SwitchWeapon(WeaponType type)
        {
            bool switched = false;

            switch (type)
            {
                case WeaponType.Heavy:
                    if (heavyWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
                case WeaponType.Light:
                    if (lightWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
                case WeaponType.CloseRange:
                    if (closeRangeWeapon != null)
                    {
                        selectedType = type;
                        switched = true;
                    }

                    break;
            }

            return switched;
        }
    }
}
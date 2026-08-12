namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponMuzzle
    {
        public WeaponPoint Position { get; }
        public WeaponDirection Forward { get; }

        public WeaponMuzzle(
            in WeaponPoint position,
            in WeaponDirection forward)
        {
            Position = position;
            Forward = forward;
        }
    }
}

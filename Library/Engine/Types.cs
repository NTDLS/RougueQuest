﻿namespace Library.Engine.Types
{
    public enum EquipSlot
    {
        Pack,
        Belt,
        RightRing,
        Weapon,
        Bracers,
        Armor,
        Boots,
        Necklace,
        Garment,
        Helment,
        Shield,
        Gauntlets,
        FreeHand,
        LeftRing
    }

    public enum ActorClassName
    {
        ActorBuilding,
        ActorFriendyBeing,
        ActorHostileBeing,
        ActorItem,
        ActorPlayer,
        ActorTerrain,
        ActorTextBlock
    }

    public enum ActorSubType
    {
        Unspecified,
        Pack,
        Belt,
        Ring,
        Weapon,
        Bracers,
        Armor,
        Necklace,
        Garment,
        Helment,
        Shield,
        Gauntlets,
        Boots,
        Potion,
        Scroll,
        Wand,
        Money
    }

    public enum RotationMode
    {
        None, //Almost free.
        Clip, //Expensive...
        Upsize //Hella expensive!
    }
}

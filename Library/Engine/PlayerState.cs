﻿using Library.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Library.Engine
{
    public class PlayerState
    {
        private EngineCoreBase _core;

        public PlayerState(EngineCoreBase core)
        {
            _core = core;
            Equipment = new List<Equip>();
        }

        public void SetCore(EngineCoreBase core)
        {
            _core = core;
        }

        public List<Equip> Equipment { get; set; }
        public Guid UID { get; set; }
        public string Name { get; set; }
        public int Avatar { get; set; }

        //These are the attributes that the player started with from character creation.
        public int StartingConstitution { get; set; }
        public int StartingDexterity { get; set; }
        public int StartingIntelligence { get; set; }
        public int StartingStrength { get; set; }
        public List<TileIdentifier> KnownSpells { get; set; } = new List<TileIdentifier>();


        #region Starting + Augmented attributes.

        [JsonIgnore]
        public int Armorclass => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.ArmorClass).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.ArmorClass).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int Speed => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.Speed).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.Speed).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int Constitution => StartingConstitution
            + (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.Constitution).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.Constitution).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int Dexterity => StartingDexterity
            + (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.Dexterity).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.Dexterity).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int Intelligence => StartingIntelligence
            + (_core?.State?.ActorStates?.States(UID)?.Where(o => o.State == StateOfBeing.Intelligence).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.Intelligence).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int Strength => StartingStrength
            + (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.Strength).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.Strength).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int EarthResistance => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.EarthResistance).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.EarthResistance).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int LightningResistance => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.LightningResistance).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.LightningResistance).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int FireResistance => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.FireResistance).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.FireResistance).Sum(o => o.Value) ?? 0);

        [JsonIgnore]
        public int ColdResistance => (_core?.State?.ActorStates?.States(UID).Where(o => o.State == StateOfBeing.ColdResistance).Sum(o => o.ModificationAmount) ?? 0)
            + (_core?.State?.Character?.Equipment?.Where(o => o.Tile != null && o.Tile.Meta.Effects != null).SelectMany(o => o.Tile.Meta?.Effects)?.Where(o => o.EffectType == ItemEffect.ColdResistance).Sum(o => o.Value) ?? 0);

        #endregion

        public int Mana => (6 * Level) + Intelligence;
        public int Hitpoints => (6 * Level) + Constitution;
        public int MaxWeight => (Level * 20) + Strength;
        public int Experience { get; set; }
        public int NextLevelExperience { get; set; }
        public int Level { get; set; }

        private int _availableMana;
        public int AvailableMana
        {
            get
            {
                return _availableMana;
            }
            set
            {
                _availableMana = value < 0 ? 0 : value;
                if (_availableMana > Mana)
                {
                    _availableMana = Mana;
                }
            }
        }

        private int _availableHitpoints;
        public int AvailableHitpoints
        {
            get
            {
                return _availableHitpoints;
            }
            set
            {
                _availableHitpoints = value < 0 ? 0 : value;
                if (_availableHitpoints > Hitpoints)
                {
                    _availableHitpoints = Hitpoints;
                }
            }
        }

        /// <summary>
        /// Gets the aggregate amount of money in gold equivalent.
        /// </summary>
        public int Money
        {
            get
            {
                //Get the purse that is equiped.
                var equipSlot = _core.State.Character.GetEquipSlot(EquipSlot.Purse);
                if (equipSlot != null && equipSlot.Tile != null)
                {
                    //Find all the money in the purse.
                    var money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();
                    int value = (int)(money.Sum(o => o.Tile.Meta.Quantity * o.Tile.Meta.Value) ?? 0.0);

                    return value;
                }
                return 0;
            }
        }

        public double Bulk
        {
            get
            {
                double totalBulk = 0;

                foreach (var equip in Equipment)
                {
                    if (equip.Tile != null)
                    {
                        totalBulk += (equip.Tile.Meta.Bulk ?? 0);
                    }
                }

                var pack = _core.State.Character.GetEquipSlot(EquipSlot.Pack);
                if (pack != null && pack.Tile != null)
                {

                    double packBulk = _core.State.Items.Where(o => o.ContainerId == pack.Tile.Meta.UID).Sum(o => (o.Tile.Meta.Bulk ?? 0) * (o.Tile.Meta.Quantity ?? 1));

                    totalBulk += packBulk;
                }

                return totalBulk;
            }
        }

        public double Weight
        {
            get
            {
                double totalWeight = 0;

                foreach (var equip in Equipment)
                {
                    if (equip.Tile != null)
                    {
                        totalWeight += (equip.Tile.Meta.Weight ?? 0);
                    }
                }

                var pack = _core.State.Character.GetEquipSlot(EquipSlot.Pack);
                if (pack != null && pack.Tile != null)
                {

                    double packWeight = _core.State.Items.Where(o => o.ContainerId == pack.Tile.Meta.UID).Sum(o => (o.Tile.Meta.Weight ?? 0) * (o.Tile.Meta.Quantity ?? 1));

                    totalWeight += packWeight;
                }

                return totalWeight;
            }
        }

        public void AddKnownSpell(TileIdentifier spellTile)
        {
            var scroll = _core.Materials.Where(o => o.Meta.SubType == ActorSubType.Scroll && o.Meta.SpellName == spellTile.Meta.SpellName).First();
            var spell = scroll.DeriveCopy();
            spell.Meta.Mana = spellTile.Meta.Mana;
            spell.Meta.IsMemoriziedSpell = true;
            KnownSpells.Add(spell);
        }

        public void AddMoney(TileIdentifier moneyToAdd)
        {
            var equipSlot = _core.State.Character.GetEquipSlot(EquipSlot.Purse);
            if (equipSlot != null && equipSlot.Tile != null)
            {
                //Find all the money in the purse.
                var money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();

                //If we dont have a the coin type, add a zero quantity one.
                if (money.Where(o => o.Tile.Meta.Name.Contains(moneyToAdd.Meta.Name)).FirstOrDefault() == null)
                {
                    var addedTile = _core.Materials.Where(o => o.Meta.SubType == ActorSubType.Money && o.Meta.Name == moneyToAdd.Meta.Name).First().Clone(true);
                    _core.State.Items.Add(new CustodyItem() { Tile = addedTile, ContainerId = equipSlot.Tile.Meta.UID });

                    money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();
                }

                var tileToModify = money.Where(o => o.Tile.Meta.Name == moneyToAdd.Meta.Name).First().Tile;

                tileToModify.Meta.Quantity = (tileToModify.Meta.Quantity ?? 0) + moneyToAdd.Meta.Quantity;
            }
        }

        public void AddMoney(int amountOfMoneyToAdd)
        {
            var equipSlot = _core.State.Character.GetEquipSlot(EquipSlot.Purse);
            if (equipSlot != null && equipSlot.Tile != null)
            {
                //Find all the money in the purse.
                var money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();

                //If we dont have a gold coin, add a zero quantity one.
                if (money.Where(o => o.Tile.Meta.Name.Contains("Gold")).FirstOrDefault() == null)
                {
                    var goldTile = _core.Materials.Where(o => o.Meta.SubType == ActorSubType.Money && o.Meta.Name.Contains("Gold")).First().Clone(true);
                    _core.State.Items.Add(new CustodyItem() { Tile = goldTile, ContainerId = equipSlot.Tile.Meta.UID });

                    money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();
                }

                var goldTiles = money.Where(o => o.Tile.Meta.Name.Contains("Gold")).First().Tile;

                goldTiles.Meta.Quantity = (goldTiles.Meta.Quantity ?? 0) + amountOfMoneyToAdd;
            }
        }

        public void DeductMoney(int amountOfGoldToDeduct)
        {
            if (amountOfGoldToDeduct > this.Money)
            {
                return; //We don't have enough.
            }

            var equipSlot = _core.State.Character.GetEquipSlot(EquipSlot.Purse);
            if (equipSlot != null && equipSlot.Tile != null)
            {
                //Find all the money in the purse.
                var money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();

                //If we dont have a gold coin, add a zero quantity one.
                if (money.Where(o => o.Tile.Meta.Name.Contains("Gold")).FirstOrDefault() == null)
                {
                    var goldTile = _core.Materials.Where(o => o.Meta.SubType == ActorSubType.Money && o.Meta.Name.Contains("Gold")).First().Clone(true);
                    _core.State.Items.Add(new CustodyItem() { Tile = goldTile, ContainerId = equipSlot.Tile.Meta.UID });

                    money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();
                }

                int goldWeHave = (int)(money.Where(o => o.Tile.Meta.Name.Contains("Gold")).Sum(o => o.Tile.Meta.Quantity) ?? 0.0);

                if (goldWeHave < amountOfGoldToDeduct)
                {
                    MakeChangeUntilGoldAvailable(amountOfGoldToDeduct);
                }

                var goldTiles = money.Where(o => o.Tile.Meta.Name.Contains("Gold")).First().Tile;

                goldTiles.Meta.Quantity = (goldTiles.Meta.Quantity ?? 0) - amountOfGoldToDeduct;
            }
        }

        public void MakeChangeUntilGoldAvailable(int amountOfGoldToMake)
        {
            if (amountOfGoldToMake > this.Money)
            {
                return; //We don't have enough.
            }

            var equipSlot = _core.State.Character.GetEquipSlot(EquipSlot.Purse);
            if (equipSlot != null && equipSlot.Tile != null)
            {
                //Find all the money in the purse.
                var money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();

                //If we dont have a gold coin, add a zero quantity one.
                if (money.Where(o => o.Tile.Meta.Name.Contains("Gold")).FirstOrDefault() == null)
                {
                    var goldTile = _core.Materials.Where(o => o.Meta.SubType == ActorSubType.Money && o.Meta.Name.Contains("Gold")).First().Clone(true);
                    _core.State.Items.Add(new CustodyItem() { Tile = goldTile, ContainerId = equipSlot.Tile.Meta.UID });

                    money = _core.State.Items.Where(o => o.ContainerId == equipSlot.Tile.Meta.UID).ToList();
                }

                int goldWeHave = 0;

                do
                {
                    var copper = money.Where(o => o.Tile.Meta.Name.Contains("Copper")).FirstOrDefault()?.Tile.Meta;
                    var silver = money.Where(o => o.Tile.Meta.Name.Contains("Silver")).FirstOrDefault()?.Tile.Meta;
                    var gold = money.Where(o => o.Tile.Meta.Name.Contains("Gold")).FirstOrDefault()?.Tile.Meta;
                    var platinum = money.Where(o => o.Tile.Meta.Name.Contains("Platinum")).FirstOrDefault()?.Tile.Meta;

                    if (copper != null && copper.Quantity * copper.Value > 1)
                    {
                        int amountToDeduct = (int)(1.0 / copper.Value);
                        copper.Quantity -= amountToDeduct;
                        gold.Quantity = (gold.Quantity ?? 0) + 1;
                    }
                    else if (silver != null && silver.Quantity * silver.Value > 1)
                    {
                        int amountToDeduct = (int)(1.0 / silver.Value);
                        silver.Quantity -= amountToDeduct;
                        gold.Quantity = (gold.Quantity ?? 0) + 1;
                    }
                    else if (platinum != null && platinum.Quantity * platinum.Value > 1)
                    {
                        int amountToDeduct = (int)(1.0 / platinum.Value);
                        platinum.Quantity -= amountToDeduct;
                        gold.Quantity = (gold.Quantity ?? 0) + 1;
                    }

                    goldWeHave = (int)(money.Where(o => o.Tile.Meta.Name.Contains("Gold")).Sum(o => o.Tile.Meta.Quantity) ?? 0.0);
                } while (goldWeHave < amountOfGoldToMake);
            }
        }

        public void InitializeState()
        {
            AvailableHitpoints = Hitpoints;
            AvailableMana = Mana;
            NextLevelExperience = 300;
        }

        public void LevelUp()
        {
            Level++;
            NextLevelExperience = (int)(((float)NextLevelExperience) * 1.5f);
            AvailableHitpoints = Hitpoints;
            AvailableMana = Mana;
        }

        public Equip FindEquipSlotByItem(TileIdentifier tile)
        {
            if (tile.Meta.UID != null)
            {
                return FindEquipSlotByItemId((Guid)tile.Meta.UID);
            }
            return null;
        }

        public Equip FindEquipSlotByItemId(Guid? itemUid)
        {
            if (itemUid == null)
            {
                return null;
            }

            return Equipment.Where(o => o.Tile != null && o.Tile.Meta != null && o.Tile.Meta.UID == (Guid)itemUid).FirstOrDefault();
        }

        public Equip GetEquipSlot(EquipSlot slot)
        {
            var equip = Equipment.Where(o => o.Slot == slot).FirstOrDefault();
            if (equip == null)
            {
                equip = new Equip() { Slot = slot };
                Equipment.Add(equip);
            }

            return equip;
        }

        public Equip GetQuiverSlotOfType(ProjectileType projectileType)
        {
            var q1 = GetEquipSlot(EquipSlot.Projectile1);
            if (q1.Tile != null && q1.Tile.Meta.ProjectileType == projectileType) return q1;
            var q2 = GetEquipSlot(EquipSlot.Projectile2);
            if (q2.Tile != null && q2.Tile.Meta.ProjectileType == projectileType) return q2;
            var q3 = GetEquipSlot(EquipSlot.Projectile3);
            if (q3.Tile != null && q3.Tile.Meta.ProjectileType == projectileType) return q3;
            var q4 = GetEquipSlot(EquipSlot.Projectile4);
            if (q4.Tile != null && q4.Tile.Meta.ProjectileType == projectileType) return q4;
            var q5 = GetEquipSlot(EquipSlot.Projectile5);
            if (q5.Tile != null && q5.Tile.Meta.ProjectileType == projectileType) return q5;

            return null;
        }

        public CustodyItem GetInventoryItemFromQuiverSlotOfType(ProjectileType projectileType)
        {
            var quiver = GetQuiverSlotOfType(projectileType);
            if (quiver != null && quiver.Tile != null)
            {
                return _core.State.Items.Where(o => o.Tile.Meta.UID == quiver.Tile.Meta.UID).First();
            }
            return null;
        }

        /// <summary>
        /// Updates the items in equipment slots with the items from inventory.
        /// </summary>
        public void PushFreshInventoryItemsToEquipSlots()
        {
            foreach (var suit in Enum.GetValues<EquipSlot>())
            {
                var slot = GetEquipSlot(suit);
                if (slot != null && slot.Tile != null)
                {
                    slot.Tile = _core.State.Items.Where(o => o.Tile.Meta.UID == slot.Tile.Meta.UID).FirstOrDefault()?.Tile;
                }
            }
        }
    }
}

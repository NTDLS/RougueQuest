﻿using Assets;
using Game.Actors;
using Game.Engine;
using Library.Engine;
using Library.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game.Classes
{
    public static class StoreAndInventory
    {
        private static ImageList _imageList;

        public static ImageList ImageList
        {
            get
            {
                if (_imageList == null)
                {
                    _imageList = new ImageList();
                }
                return _imageList;
            }
        }

        public static string GetImageKey(string imagePath)
        {
            if (ImageList.Images.Keys.Contains(imagePath))
            {
                return imagePath;
            }

            var bitmap = SpriteCache.GetBitmapCached(imagePath);
            ImageList.Images.Add(imagePath, bitmap);

            return imagePath;
        }

        public static ListViewItem FindListViewObjectByUid(ListView listView, Guid uid)
        {
            foreach (ListViewItem obj in listView.Items)
            {
                var item = obj.Tag as EquipTag;

                if (item.Tile.Meta.UID == uid)
                {
                    return obj;
                }
            }

            return null;
        }

        public static string GetItemTip(EngineCore core, ActorBase tile, bool includeOfferPrice = false, bool includeAskingPrice = false)
        {
            return GetItemTip(core, new TileIdentifier(tile.TilePath, tile.Meta), includeOfferPrice, includeAskingPrice);
        }

        public static string GetItemTip(EngineCore core, TileIdentifier tile, bool includeOfferPrice = false, bool includeAskingPrice = false)
        {
            string text = tile.Meta.DisplayName;

            if ((tile.Meta.Enchantment != null && (tile.Meta.IsIdentified ?? false) == false) == false)
            {
                text += $" ({tile.Meta.RarityText})";
            }
            else
            {
                text += $" (UNIDENTIFIED)";
            }

            text += "\r\n" + $"Type: {tile.Meta.SubType}";

            if (tile.Meta.Enchantment == EnchantmentType.Enchanted && (tile.Meta.IsIdentified ?? false) == true)
                text += "\r\n" + $"Enchantment: ENCHANTED!!";
            else if (tile.Meta.Enchantment == EnchantmentType.Cursed && (tile.Meta.IsIdentified ?? false) == true)
                text += "\r\n" + $"Enchantment: CURSED!!";

            if (tile.Meta.Weight != null) text += "\r\n" + $"Weight: {(tile.Meta.Weight * (tile.Meta.Quantity ?? 1)):N2}";
            if (tile.Meta.Bulk != null) text += "\r\n" + $"Bulk: {(tile.Meta.Bulk * (tile.Meta.Quantity ?? 1)):N2}";

            if (tile.Meta.CanStack == true && tile.Meta.Quantity > 0)
                text += "\r\n" + $"Quantity: {tile.Meta.Quantity}";

            if ((tile.Meta.Enchantment != null && (tile.Meta.IsIdentified ?? false) == false) == false)
            {
                if (tile.Meta.CanStack == false && tile.Meta.Charges > 0)
                    text += "\r\n" + $"Charges: {tile.Meta.Charges}";
                if (tile.Meta.AC != null) text += "\r\n" + $"AC: {tile.Meta.AC:N0}";
                if (tile.Meta.SubType == ActorSubType.MeleeWeapon || tile.Meta.SubType == ActorSubType.RangedWeapon)
                    text += "\r\n" + $"Stats: {tile.Meta.DndDamageText}";
            }

            if (tile.Meta.SubType == ActorSubType.Money)
                text += "\r\n" + $"Value: {((int)((tile.Meta.Quantity ?? 0) * tile.Meta.Value)):N0} gold";

            if (includeOfferPrice) text += "\r\n" + $"Offer: {StoreAndInventory.OfferPrice(core, tile):N0}";
            if (includeAskingPrice) text += "\r\n" + $"Asking Price: {StoreAndInventory.AskingPrice(core, tile):N0}";

            return text;
        }

        public static void AddItemToListView(ListView listView, ActorItem item)
        {
            AddItemToListView(listView, new TileIdentifier(item.TilePath, item.Meta));
        }

        public static void AddItemToListView(ListView listView, TileIdentifier tile)
        {
            string text = tile.Meta.DisplayName;

            if (tile.Meta.CanStack == true && tile.Meta.Quantity > 0)
            {
                text += $" ({tile.Meta.Quantity})";
            }
            else if (tile.Meta.CanStack == false && tile.Meta.Charges > 0)
            {
                text += $" ({tile.Meta.Charges})";
            }

            var equipTag = new EquipTag()
            {
                Tile = new TileIdentifier(tile.TilePath, tile.Meta)
            };

            equipTag.AcceptTypes.Add((ActorSubType)tile.Meta.SubType);

            ListViewItem item = new ListViewItem(text);
            item.ImageKey = GetImageKey(tile.ImagePath);
            item.Tag = equipTag;
            listView.Items.Add(item);
        }

        public static double UnitPrice(EngineCoreBase core, TileIdentifier tile)
        {
            if (tile.Meta.Value == null)
            {
                return 0;
            }

            double intelligenceDiscount = (core.State.Character.Intelligence / 100.0);
            return ((tile.Meta.Value ?? 0.0) - ((tile.Meta.Value ?? 0.0) * intelligenceDiscount));
        }

        public static int AskingPrice(EngineCoreBase core, TileIdentifier tile, int? quantity)
        {
            if (tile.Meta.Value == null)
            {
                return 0;
            }

            double value = UnitPrice(core, tile);
            double qty = (double)(quantity ?? 0);

            if (qty > 0)
            {
                value *= qty;
            }

            if (tile.Meta.Enchantment == EnchantmentType.Enchanted && tile.Meta.IsIdentified == true)
            {
                value *= 3;
            }

            if (value >= 1)
            {
                return (int)value;
            }

            return 1;
        }


        public static int AskingPrice(EngineCoreBase core, TileIdentifier tile)
        {
            if (tile.Meta.Value == null)
            {
                return 0;
            }

            return AskingPrice(core, tile, (tile.Meta.Charges ?? 0) + (tile.Meta.Quantity ?? 0));
        }

        public static int OfferPrice(EngineCoreBase core, TileIdentifier tile)
        {
            if (tile.Meta.Value == null)
            {
                return 0;
            }

            double intelligenceBonus = (core.State.Character.Intelligence / 100.0);
            double halfValue = ((tile.Meta.Value ?? 0.0) / 2.0);
            double value = (halfValue + (halfValue * intelligenceBonus));
            int qty = (tile.Meta.Charges ?? 0) + (tile.Meta.Quantity ?? 0);

            if (qty > 0)
            {
                value *= qty;
            }

            //If the item is unidentified, then we will waaaaaaay undercut the offer price.
            if (tile.Meta.Enchantment != null && (tile.Meta.IsIdentified ?? false) == false)
            {
                value /= 3;
            }
            else if (tile.Meta.Enchantment == EnchantmentType.Cursed && tile.Meta.IsIdentified == true)
            {
                value /= 10;
            }
            else if (tile.Meta.Enchantment == EnchantmentType.Enchanted && tile.Meta.IsIdentified == true)
            {
                value *= 3;
            }

            if (value >= 1)
            {
                return (int)value;
            }

            return 1;
        }
    }
}

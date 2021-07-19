﻿using Assets;
using Library.Engine.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace Library.Engine
{
    public class TileMetadata
    {
        /// <summary>
        /// This is only populated for tiles that need it.
        /// </summary>
        public Guid? UID { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public bool? SnapToGrid { get; set; }
        public int Quantity { get; set; } //Stacking because things like money only really matter in multiples.
        public bool? CanWalkOn { get; set; }
        public bool? CanTakeDamage { get; set; }
        public int? Experience { get; set; }
        public int? HitPoints { get; set; }
        public int? AC { get; set; } //Armor Class. yay.
        public int? DamageDice { get; set; }
        public int? DamageDiceFaces { get; set; }
        public int? DamageAdditional { get; set; }
        public int? OriginalHitPoints { get; set; }
        public bool? IsContainer { get; set; }
        public bool? CanStack { get; set; }
        public int? BulkCapacity { get; set; }
        public int? WeightCapacity { get; set; }
        public int? Weight { get; set; }
        public int? Bulk { get; set; }
        public int? Dexterity { get; set; }
        public int? Strength { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActorClassName? SpawnType { get; set; } //Used by the ActorSpawner.

        [JsonConverter(typeof(StringEnumConverter))]
        public ActorSubType? SpawnSubType { get; set; } //Used by the ActorSpawner.
        public int? MinLevel { get; set; } //Used by the ActorSpawner
        public int? MaxLevel { get; set; } //Used by the ActorSpawner
        public int? Level { get; set; } //Used to know when we should show items, enemies and what to populate in shops.
        public int? Value { get; set; } //Rough monetary value of the item in a shop.
        /// <summary>
        /// Used for level warp tiles. This tells the engine which level to load.
        /// </summary>
        public string LevelWarpName { get; set; }
        /// Used for level warp tiles. This tells the engine which tile to spawn to.
        public Guid LevelWarpTargetTileUID { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActorClassName? ActorClass { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ActorSubType? SubType { get; set; }

        /// <summary>
        /// This is what the object will say to the player when approached.
        /// </summary>
        public string Dialog { get; set; }
        /// <summary>
        /// Will the dialog be shown every time this tile is intersected?
        /// </summary>
        public bool? OnlyDialogOnce { get; set; }

        public void OverrideWith(TileMetadata with)
        {
            this.UID = with.UID ?? this.UID;
            this.Tag = with.Tag ?? this.Tag;
            this.CanWalkOn = with.CanWalkOn ?? this.CanWalkOn;
            this.CanTakeDamage = with.CanTakeDamage ?? this.CanTakeDamage;
            this.HitPoints = with.HitPoints ?? this.HitPoints;
            this.Experience = with.Experience ?? this.Experience;
            this.ActorClass = with.ActorClass ?? this.ActorClass;
            this.Name = with.Name ?? this.Name;
            this.Dialog = with.Dialog ?? this.Dialog;
            this.OnlyDialogOnce = with.OnlyDialogOnce ?? this.OnlyDialogOnce;
            this.IsContainer = with.IsContainer ?? this.IsContainer;
            this.CanStack = with.CanStack ?? this.CanStack;
            this.SubType = with.SubType ?? this.SubType;
            this.AC = with.AC ?? this.AC;
            this.BulkCapacity = with.BulkCapacity ?? this.BulkCapacity;
            this.WeightCapacity = with.WeightCapacity ?? this.WeightCapacity;
            this.Bulk = with.Bulk ?? this.Bulk;
            this.Weight = with.Weight ?? this.Weight;
            this.DamageDice = with.DamageDice ?? this.DamageDice;
            this.DamageDiceFaces = with.DamageDiceFaces ?? this.DamageDiceFaces;
            this.DamageAdditional = with.DamageAdditional ?? this.DamageAdditional;
            this.Strength = with.Strength ?? this.Strength;
            this.Dexterity = with.Dexterity ?? this.Dexterity;
            this.SnapToGrid = with.SnapToGrid ?? this.SnapToGrid;
            this.SpawnType = with.SpawnType ?? this.SpawnType;
            this.SpawnSubType = with.SpawnSubType ?? this.SpawnSubType;
            this.MinLevel = with.MinLevel ?? this.MinLevel;
            this.MaxLevel = with.MaxLevel ?? this.MaxLevel;
            this.Level = with.Level ?? this.Level;
            this.Value = with.Value ?? this.Value;
        }

        /// <summary>
        /// Traverses up the directories looking for metadata files that describe the assets and allowing for overriding of metadata values.
        /// </summary>
        /// <param name="tilePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static TileMetadata FindFirstMetafile(string tilePath, string fileName)
        {
            string globalMetaFile = Path.Combine(tilePath, fileName);

            if (File.Exists(globalMetaFile))
            {
                var text = File.ReadAllText(globalMetaFile);
                return JsonConvert.DeserializeObject<TileMetadata>(text);
            }
            else
            {
                string parentPath = Directory.GetParent(tilePath)?.FullName;
                if (string.IsNullOrWhiteSpace(parentPath) == false)
                {
                    return FindFirstMetafile(parentPath, fileName);
                }
            }

            return null;
        }

        /// <summary>
        /// Reads metadata from the asset directories and refreshes the values stored with the actor.
        /// </summary>
        public static TileMetadata GetFreshMetadata(string tilePath)
        {
            string fileSystemPath = Path.GetDirectoryName(Constants.GetAssetPath($"{tilePath}"));
            string exactMetaFileName = Constants.GetAssetPath($"{tilePath}.txt");

            var meta = FindFirstMetafile(fileSystemPath, "_GlobalMeta.txt") ?? new TileMetadata();

            var localMeta = FindFirstMetafile(fileSystemPath, "_LocalMeta.txt");
            if (localMeta != null)
            {
                meta.OverrideWith(localMeta);
            }

            if (File.Exists(exactMetaFileName))
            {
                var text = File.ReadAllText(exactMetaFileName);
                var exactMeta = JsonConvert.DeserializeObject<TileMetadata>(text);
                if (exactMeta != null)
                {
                    meta.OverrideWith(exactMeta);
                }
            }

            if (meta.UID == null && meta.ActorClass != ActorClassName.ActorTerrain)
            {
                meta.UID = Guid.NewGuid();
            }

            meta.OriginalHitPoints = meta.HitPoints;

            return meta;
        }
    }
}

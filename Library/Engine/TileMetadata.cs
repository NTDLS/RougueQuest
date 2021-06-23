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
        public int Quantity { get; set; } //Stacking because things like money only really matter in quantities
        public bool? CanWalkOn { get; set; }
        public bool? CanTakeDamage { get; set; }
        public int? Experience { get; set; }
        public int? HitPoints { get; set; }
        public int? OriginalHitPoints { get; set; }        
        public bool? IsContainer { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BasicTileType? BasicType { get; set; }

        public void OverrideWith(TileMetadata with)
        {
            this.Tag = with.Tag ?? this.Tag;
            this.CanWalkOn = with.CanWalkOn ?? this.CanWalkOn;
            this.CanTakeDamage = with.CanTakeDamage ?? this.CanTakeDamage;
            this.HitPoints = with.HitPoints ?? this.HitPoints;
            this.Experience = with.Experience ?? this.Experience;
            this.BasicType = with.BasicType ?? this.BasicType;
            this.Name = with.Name ?? this.Name;
            this.IsContainer = with.IsContainer ?? this.IsContainer;
        }

        /// <summary>
        /// Traverses up the directories looking for metadata files that describe the assets and allowing for overriding of metadat values.
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

            meta.OriginalHitPoints = meta.HitPoints;

            return meta;
        }

    }
}
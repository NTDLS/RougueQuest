﻿using System.Collections.Generic;
using System.Linq;

namespace Library.Engine
{
    public class GameState
    {
        private EngineCoreBase _core;

        public GameState(EngineCoreBase core)
        {
            _core = core;
            Character = new PlayerState(core);
        }

        public void SetCore(EngineCoreBase core)
        {
            _core = core;
            Character.SetCore(core);
            ActiveThreadCount = 0;
        }

        /// <summary>
        /// A list of recently attacked enemies or ones that have seen the player.
        /// </summary>
        public List<RecentlyEngagedHostile> RecentlyEngagedHostiles = new List<RecentlyEngagedHostile>();
        public ActorStates ActorStates { get; set; } = new ActorStates();
        public PlayerState Character { get; set; }

        /// <summary>
        /// Items in the world and who/what they belong to. This is items in chests, in bags, in your inventory and belonging to creatures.
        /// </summary>
        public List<CustodyItem> Items { get; private set; } = new List<CustodyItem>();

        /// <summary>
        /// A list of items that the player has identified via spell or store so they will be auto-identified when picked up later.
        /// For items with special traits such as armor, rings, weapons, etc. this will only auto-identify when the item enchantment is "normal".
        /// We use string instead of TileIdentifier because we want to key on Name and not TilePath so that we can auto-identify like items like "Ring of Adornment".
        /// </summary>
        public List<string> IdentifiedItems { get; private set; } = new List<string>();

        /// <summary>
        /// The level/map-number that the player is currently on.
        /// </summary>
        public int CurrentLevel { get; set; }
        public int DefaultLevel { get; set; }
        public bool IsDialogActive { get; set; }
        public int ActiveThreadCount { get; private set; } = 0;
        public int TimePassed { get; set; } = 0; //Number of seconds passed in the game. (not real time, game time).
        public List<PersistentStore> Stores { get; set; } = new List<PersistentStore>();
        public object ActiveThreadCountLock { get; private set; } = new object();

        public void AddThreadReference()
        {
            lock (ActiveThreadCountLock)
            {
                ActiveThreadCount++;
            }
        }

        public void RemoveThreadReference()
        {
            lock (ActiveThreadCountLock)
            {
                ActiveThreadCount--;
            }
        }

        public CustodyItem GetOrCreateInventoryItem(TileIdentifier tile)
        {
            if (tile == null)
            {
                return null;
            }

            var inventoryItem = _core.State.Items.Where(o => o.Tile.Meta.UID == tile.Meta.UID).FirstOrDefault();
            if (inventoryItem == null)
            {
                inventoryItem = new CustodyItem()
                {
                    Tile = tile.Clone()
                };

                _core.State.Items.Add(inventoryItem);
            }
            return inventoryItem;
        }

        public CustodyItem GetInventoryItem(TileIdentifier tile)
        {
            var inventoryItem = _core.State.Items.Where(o => o.Tile.Meta.UID == tile.Meta.UID).FirstOrDefault();

            return inventoryItem;
        }
    }
}

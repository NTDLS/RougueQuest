﻿using Library.Types;
using Game.Actors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library.Engine;

namespace Game.Engine
{
    public class EngineCore : EngineCoreBase
    {
        public delegate void BeforeTickEvent(EngineCore core, Types.TickInput input);
        public event BeforeTickEvent BeforeTick;

        public delegate void AfterTickEvent(EngineCore core, Types.TickInput input, Point<double> offsetApplied);
        public event AfterTickEvent AfterTick;

        public EngineTickController Tick { get; set; }

        public ActorPlayer Player { get; set; }

        public EngineCore(Control drawingSurface, Size visibleSize)
            : base (drawingSurface, visibleSize)
        {
            Tick = new EngineTickController(this);
        }

        public void NewGame(string characterName, int dexterity, int constitution, int intelligence, int strength)
        {
            this.QueueAllForDelete();
            this.PurgeAllDeletedTiles();

            MapPersistence.Load(this, Assets.Constants.GetAssetPath(@"Maps\MapHome.rqm"), true);

            this.State = new GameState()
            {
                CurrentMap = "MapHome"
            };

            this.State.Character = new PlayerCharacter()
            {
                UID = Guid.NewGuid(),
                Experience = 0,
                Name = characterName,
                Level = 1,
                StartingDexterity = dexterity,
                StartingConstitution = constitution,
                StartingIntelligence = intelligence,
                StartingStrength = strength
            };

            this.Player = Actors.OfType<ActorPlayer>().FirstOrDefault();
        }

        public void LoadGame(string fileName)
        {
            this.QueueAllForDelete();
            this.PurgeAllDeletedTiles();
            MapPersistence.Load(this, fileName);
            this.Player = Actors.OfType<ActorPlayer>().FirstOrDefault();
        }

        public void SaveGame(string fileName)
        {
            MapPersistence.Save(this, fileName, this.State);
        }

        public ActorTextBlock AddNewTextBlock(string font, Brush color, double size, double x, double y, bool isPositionStatic, string text = "")
        {
            lock (CollectionSemaphore)
            {
                var obj = new ActorTextBlock(this, font, color, size, new Point<double>(x, y), isPositionStatic)
                {
                    Text = text
                };
                Actors.Add(obj);
                return obj;
            }
        }

        public override void HandleSingleKeyPress(Keys key)
        {
            if (Player == null || Player.Visible == false)
            {
                return;
            }

            var input = new Types.TickInput()
            {
                InputType = Types.TickInputType.Keyboard,
                Key = key
            };

            BeforeTick?.Invoke(this, input);
            var offsetApplied = Tick.Advance(input);
            AfterTick?.Invoke(this, input, offsetApplied);
        }
    }
}

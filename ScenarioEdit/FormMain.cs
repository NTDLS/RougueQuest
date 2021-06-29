﻿using Assets;
using ScenarioEdit.Engine;
using Library.Engine;
using Library.Engine.Types;
using Library.Types;
using Library.Utility;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScenarioEdit
{
    public partial class FormMain : Form
    {
        /// <summary>"
        /// The action that will be performed when clicking the left mouse button.
        /// </summary>
        public enum PrimaryMode
        {
            Insert,
            Select,
            Shape
        }

        public enum ShapeFillMode
        {
            Insert,
            Delete,
            Select,
        }

        private bool _firstShown = true;
        private EngineCore _core;
        private bool _fullScreen = false;
        private bool _hasBeenModified = false;
        private string _currentMapFilename = string.Empty;
        private int _newFilenameIncrement = 1;
        private ToolTip _interrogationTip = new ToolTip();
        private Rectangle? shapeSelectionRect = null;

        #region Settings.

        private int tilePaintOverlap = 5;
        private bool highlightSelectedTile = true;
        private bool highlightHoverTile = true;

        #endregion

        /// <summary>
        /// Where the mouse was when the user started dragging the map.
        /// </summary>
        private Point<double> dragStartMouse = new Point<double>();
        /// <summary>
        /// Where the mouse was when the user started drawing
        /// </summary>
        private Point<double> insertStartMousePosition = new Point<double>();
        /// <summary>
        /// Where the mouse was when the user started drawing a fill shape.
        /// </summary>
        private Point<double> shapeInsertStartMousePosition = new Point<double>();
        /// <summary>
        /// What the background offset was then the user started dragging the map.
        /// </summary>
        private Point<double> dragStartOffset = new Point<double>();
        /// <summary>
        /// The location that the last block was placed (real world location).
        /// </summary>
        private Point<double> drawLastLocation = new Point<double>();
        /// <summary>
        /// Whether the user is dragging with the right or left mouse buttons when in shape fill mode
        /// </summary>
        private ShapeFillMode shapeFillMode = ShapeFillMode.Insert;
        private Size lastPlacedItemSize = new Size(0, 0);
        private ActorBase selectedTile = null;
        private ActorBase previousHoverTile = null;
        private PrimaryMode CurrentPrimaryMode { get; set; } = PrimaryMode.Select;

        //This really shouldn't be necessary! :(
        protected override CreateParams CreateParams
        {
            get
            {
                //Paints all descendants of a window in bottom-to-top painting order using double-buffering.
                // For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. 
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000; //WS_EX_COMPOSITED       
                return handleParam;
            }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        private Control drawingsurface = new Control();


        private void FormMain_Load(object sender, EventArgs e)
        {
            _core = new EngineCore(drawingsurface, new Size(drawingsurface.Width, drawingsurface.Height));

            DoubleBuffered = true;
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            if (_fullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.Width = Screen.PrimaryScreen.Bounds.Width;
                this.Height = Screen.PrimaryScreen.Bounds.Height;
                this.ShowInTaskbar = true;
                //this.TopMost = true;
                this.WindowState = FormWindowState.Maximized;
            }

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.UserPaint, true);

            System.Reflection.PropertyInfo controlProperty = typeof(Control)
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(drawingsurface, true, null);

            this.Shown += FormMain_Shown;

            splitContainerBody.Panel1.Controls.Add(drawingsurface);
            drawingsurface.Dock = DockStyle.Fill;

            drawingsurface.BackColor = Color.FromArgb(60, 60, 60);
            drawingsurface.PreviewKeyDown += drawingsurface_PreviewKeyDown;
            drawingsurface.Paint += new PaintEventHandler(drawingsurface_Paint);
            drawingsurface.MouseClick += new MouseEventHandler(drawingsurface_MouseClick);
            drawingsurface.MouseDoubleClick += new MouseEventHandler(drawingsurface_MouseDoubleClick);
            drawingsurface.MouseDown += new MouseEventHandler(drawingsurface_MouseDown);
            drawingsurface.MouseMove += new MouseEventHandler(drawingsurface_MouseMove);
            drawingsurface.MouseUp += new MouseEventHandler(drawingsurface_MouseUp);

            drawingsurface.Select();
            drawingsurface.Focus();

            PreviewKeyDown += drawingsurface_PreviewKeyDown;
            listViewProperties.PreviewKeyDown += drawingsurface_PreviewKeyDown;
            toolStrip.PreviewKeyDown += drawingsurface_PreviewKeyDown;
            treeViewTiles.PreviewKeyDown += drawingsurface_PreviewKeyDown;

            toolStripButtonInsertMode.Click += ToolStripButtonInsertMode_Click;
            toolStripButtonSelectMode.Click += ToolStripButtonSelectMode_Click;
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
            saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            toolStripButtonSave.Click += ToolStripButtonSave_Click;
            toolStripButtonOpen.Click += ToolStripButtonOpen_Click;
            toolStripButtonClose.Click += ToolStripButtonClose_Click;
            toolStripButtonNew.Click += ToolStripButtonNew_Click;
            toolStripButtonMoveTileUp.Click += ToolStripButtonMoveTileUp_Click;
            toolStripButtonMoveTileDown.Click += ToolStripButtonMoveTileDown_Click;
            toolStripButtonShapeMode.Click += ToolStripButtonShapeMode_Click;
            toolStripButtonPlayMap.Click += ToolStripButtonPlayMap_Click;
            toolStripMenuItemResetAllTileMeta.Click += ToolStripMenuItemResetAllTileMeta_Click;
            treeViewTiles.MouseDown += TreeViewTiles_MouseDown;
            treeViewTiles.MouseUp += TreeViewTiles_MouseUp;

            toolStripMenuItemAddLevel.Click += ToolStripMenuItemAddLevel_Click;
            toolStripMenuItemChangeLevel.Click += ToolStripMenuItemChangeLevel_Click;

            toolStripMenuItemSetDefaultLevel.Click += ToolStripMenuItemSetDefaultLevel_Click;
            toolStripMenuItemDeleteLevel.Click += ToolStripMenuItemDeleteLevel_Click;
            toolStripMenuItemViewWorldItems.Click += ToolStripMenuItemViewWorldItems_Click;

            PopulateMaterials();

            ToolStripButtonSelectMode_Click(new object(), new EventArgs());

            NewToolStripMenuItem_Click(null, null);
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            if (_firstShown)
            {
                using (var form = new FormWelcome())
                {
                    var result = form.ShowDialog();

                    if (result == DialogResult.Yes)
                    {
                        NewToolStripMenuItem_Click(null, null);
                    }
                    else if (result == DialogResult.OK)
                    {
                        _currentMapFilename = form.SelectedFileName;
                        _core.LoadLevlesAndPopCurrent(_currentMapFilename);
                        FormWelcome.AddToRecentList(_currentMapFilename);
                        _hasBeenModified = false;
                    }
                }

                _firstShown = false;
            }
        }

        private void drawingsurface_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.QueueForDelete();
                    }
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.X--;
                    }
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.X++;
                    }
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.Y--;
                    }
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.Y++;
                    }
                }
            }


            else if (e.KeyCode == Keys.D)
            {
                if (CurrentPrimaryMode == PrimaryMode.Select)
                {
                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    foreach (var tile in selectedTiles)
                    {
                        tile.SelectedHighlight = false;
                    }

                    foreach (var tile in selectedTiles)
                    {
                        var actor = new ActorBase(_core)
                        {
                            Meta = tile.Meta,
                            X = tile.X + 15,
                            Y = tile.Y + 15
                        };

                        actor.SetImage(tile.GetImage());

                        actor.SelectedHighlight = true;

                        _core.Actors.Add(actor);
                    }
                }
            }

        }

        #region Menu Clicks.

        private void ToolStripMenuItemViewWorldItems_Click(object sender, EventArgs e)
        {
            if (CheckForNeededSave())
            {
                using (var form = new FormWorldItems(_core))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                    }
                }
            }
        }

        private void ToolStripMenuItemDeleteLevel_Click(object sender, EventArgs e)
        {
            using (var form = new FormDeleteLevel(_core))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int levelIndex = form.SelectedLevelIndex;
                    _core.DeleteLevel(levelIndex);
                }
            }
        }

        private void ToolStripMenuItemSetDefaultLevel_Click(object sender, EventArgs e)
        {
            using (var form = new FormSetDefaultLevel(_core))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int levelIndex = form.SelectedLevelIndex;
                    _core.SetDefaultLevel(levelIndex);
                }
            }
        }

        private void ToolStripMenuItemChangeLevel_Click(object sender, EventArgs e)
        {
            using (var form = new FormSelectLevel(_core))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int levelIndex = form.SelectedLevelIndex;
                    _core.SelectLevel(levelIndex);
                }
            }
        }

        private void ToolStripMenuItemAddLevel_Click(object sender, EventArgs e)
        {
            using (var form = new FormAddNewLevel(_core))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int levelIndex = _core.Levels.AddNew(form.LevelName);
                    _core.SelectLevel(levelIndex);
                }
            }
        }

        private void ToolStripMenuItemResetAllTileMeta_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Reset the meta data of all tiles on this map with their default values?",
                "Reset all metadata?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _core.ResetAllTilesMetadata();
                MessageBox.Show("Complete.");
            }
        }

        private void ToolStripButtonPlayMap_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentMapFilename))
            {
                _hasBeenModified = true;
            }

            if (CheckForNeededSave())
            {
                var gameApp = new System.Diagnostics.Process();
                gameApp.StartInfo.FileName = @"..\..\..\..\Game\bin\Debug\net5.0-windows\Game.exe";
                gameApp.StartInfo.Arguments = $"/levelfile:\"{_currentMapFilename}\" /levelindex:{_core.State.CurrentLevel}";
                gameApp.Start();
            }
        }

        private void ToolStripButtonMoveTileDown_Click(object sender, EventArgs e)
        {
            if (selectedTile != null)
            {
                selectedTile.DrawOrder--;
                PopulateSelectedItemProperties();
            }
        }

        private void ToolStripButtonMoveTileUp_Click(object sender, EventArgs e)
        {
            if (selectedTile != null)
            {
                selectedTile.DrawOrder++;
                PopulateSelectedItemProperties();
            }
        }

        /// <summary>
        /// Do not continue if this returns false.
        /// </summary>
        /// <returns></returns>
        bool CheckForNeededSave()
        {
            if (_hasBeenModified)
            {
                var result = MessageBox.Show("The current file has been modified. Save it first?",
                    "Save before continuing?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    return TrySave();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            NewToolStripMenuItem_Click(sender, e);
        }

        private void ToolStripButtonClose_Click(object sender, EventArgs e)
        {
            NewToolStripMenuItem_Click(sender, e);
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            OpenToolStripMenuItem_Click(sender, e);
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e)
        {
            SaveToolStripMenuItem_Click(sender, e);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForNeededSave() == false)
            {
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Rogue Quest Scenario (*.rqs)|*.rqs|All files (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _currentMapFilename = dialog.FileName;
                    _core.LoadLevlesAndPopCurrent(_currentMapFilename);
                    FormWelcome.AddToRecentList(_currentMapFilename);
                    _hasBeenModified = false;
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //If we already have an open file, then just save it.
            if (string.IsNullOrWhiteSpace(_currentMapFilename) == false)
            {
                _core.PushLevelAndSave(_currentMapFilename);
                FormWelcome.AddToRecentList(_currentMapFilename);
                _hasBeenModified = false;
            }
            else //If we do not have a current open file, then we need to "Save As".
            {
                TrySave();
            }
        }

        bool TrySave()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = $"Newfile {_newFilenameIncrement++}";
                dialog.Filter = "RQ Map (*.qrm)|*.rqm|All files (*.*)|*.*";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _currentMapFilename = dialog.FileName;
                    _core.PushLevelAndSave(_currentMapFilename);
                    FormWelcome.AddToRecentList(_currentMapFilename);
                    _hasBeenModified = false;
                    return true;
                }
            }
            return false;
        }


        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrySave();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForNeededSave() == false)
            {
                return;
            }

            _core.Reset();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForNeededSave() == false)
            {
                return;
            }
        }

        private void ToolStripButtonShapeMode_Click(object sender, EventArgs e)
        {
            CurrentPrimaryMode = PrimaryMode.Shape;
            toolStripButtonInsertMode.Checked = false;
            toolStripButtonSelectMode.Checked = false;
            toolStripButtonShapeMode.Checked = true;

            ClearMultiSelection();

            if (selectedTile != null) selectedTile.SelectedHighlight = false;
        }

        private void ToolStripButtonSelectMode_Click(object sender, EventArgs e)
        {
            CurrentPrimaryMode = PrimaryMode.Select;
            toolStripButtonInsertMode.Checked = false;
            toolStripButtonSelectMode.Checked = true;
            toolStripButtonShapeMode.Checked = false;

            ClearMultiSelection();

            if (selectedTile != null) selectedTile.SelectedHighlight = false;
        }

        private void ToolStripButtonInsertMode_Click(object sender, EventArgs e)
        {
            CurrentPrimaryMode = PrimaryMode.Insert;
            toolStripButtonInsertMode.Checked = true;
            toolStripButtonSelectMode.Checked = false;
            toolStripButtonShapeMode.Checked = false;

            ClearMultiSelection();

            if (selectedTile != null) selectedTile.SelectedHighlight = false;
        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //e.ClickedItem
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CheckForNeededSave() == false)
            {
                e.Cancel = true;
                return;
            }

            _core.Stop();
        }

        #endregion

        #region TreeViewTiles.

        private void TreeViewTiles_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _interrogationTip.Hide(sender as Control);
            }
        }

        private void TreeViewTiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var lv = sender as TreeView;
                var selectedItem = lv.GetNodeAt(e.X, e.Y);

                if (selectedItem == null)
                {
                    return;
                }

                var meta = TileMetadata.GetFreshMetadata(selectedItem.FullPath);
                if (meta.ActorClass == ActorClassName.ActorItem)
                {
                    string text = meta.Name;

                    if (meta.SubType == ActorSubType.Weapon)
                    {
                        text += "\r\n" + $"Damage: {meta.DamageDice:N0}d{meta.DamageDiceFaces:N0}";
                        if (meta.DamageAdditional > 0)
                        {
                            text += $" +{meta.DamageAdditional:N0}";
                        }
                    }
                    else if (meta.SubType == ActorSubType.Armor || meta.SubType == ActorSubType.Boots
                        || meta.SubType == ActorSubType.Garment || meta.SubType == ActorSubType.Gauntlets
                        || meta.SubType == ActorSubType.Helment || meta.SubType == ActorSubType.Shield
                        || meta.SubType == ActorSubType.Necklace || meta.SubType == ActorSubType.Belt
                        || meta.SubType == ActorSubType.Bracers)
                    {
                        text += "\r\n" + $"Armor Class: {meta.AC:N0}";
                    }
                    else if (meta.SubType == ActorSubType.Chest || meta.SubType == ActorSubType.Pack)
                    {
                        text += "\r\n" + $"Max Weight: {meta.WeightCapacity:N0}";
                        text += "\r\n" + $"Bulk Weight: {meta.BulkCapacity:N0}";
                    }

                    text += "\r\n" + $"Weight: {meta.Bulk:N0}";
                    text += "\r\n" + $"Bulk: {meta.Weight:N0}";


                    if (string.IsNullOrWhiteSpace(text) == false)
                    {
                        var location = new Point(e.X + 10, e.Y - 25);
                        _interrogationTip.Show(text, lv, location, 5000);
                    }
                }
            }
        }

        #endregion

        #region drawingsurface.

        private void ClearMultiSelection()
        {
            var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
            foreach (var tile in selectedTiles)
            {
                tile.SelectedHighlight = false;
            }
        }

        private void drawingsurface_MouseDown(object sender, MouseEventArgs e)
        {
            drawingsurface.Select();
            drawingsurface.Focus();

            double x = e.X + _core.Display.BackgroundOffset.X;
            double y = e.Y + _core.Display.BackgroundOffset.Y;

            var hoverTile = _core.Actors.Intersections(new Point<double>(x, y), new Point<double>(1, 1)).OrderBy(o => o.DrawOrder).LastOrDefault();

            if (e.Button == MouseButtons.Middle)
            {
                dragStartOffset = new Point<double>(_core.Display.BackgroundOffset.X, _core.Display.BackgroundOffset.Y);
                dragStartMouse = new Point<double>(e.X, e.Y);
            }

            //We have a bunch of tiles selected but now are clicking on a different tile while not holding shift or control.
            //Since this is not adding to the selection (holsing shift) or removing from the selection (holding control) and not
            //dragging (since we arent over a selected tile, then assume this is a new operation and deslect all tiles.
            if (IsShiftDown() == false && IsControlDown() == false && (hoverTile == null || hoverTile?.SelectedHighlight == false))
            {
                ClearMultiSelection();
                selectedTile = null;
            }

            if (CurrentPrimaryMode == PrimaryMode.Select && e.Button == MouseButtons.Right)
            {
                shapeInsertStartMousePosition = new Point<double>(e.X, e.Y);
            }

            if (CurrentPrimaryMode == PrimaryMode.Shape && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
            {
                shapeInsertStartMousePosition = new Point<double>(e.X, e.Y);
            }

            if (e.Button == MouseButtons.Left && CurrentPrimaryMode == PrimaryMode.Insert)
            {
                //Single item placement with left button.
                insertStartMousePosition = new Point<double>(e.X, e.Y);
                PlaceSelectedItem(x, y);
            }

            if (e.Button == MouseButtons.Left && CurrentPrimaryMode == PrimaryMode.Select)
            {
                if (selectedTile != hoverTile && hoverTile != null)
                {
                    if (selectedTile != null && highlightSelectedTile)
                    {
                        selectedTile.SelectedHighlight = false;
                    }

                    //Place the mouse over the exact center of the tile.
                    Win32.POINT p = new Win32.POINT((int)hoverTile.ScreenX, (int)hoverTile.ScreenY);
                    Win32.ClientToScreen(drawingsurface.Handle, ref p);
                    Win32.SetCursorPos(p.x, p.y);

                    selectedTile = hoverTile;
                    PopulateSelectedItemProperties();
                }

                if (selectedTile != null && highlightSelectedTile)
                {
                    selectedTile.SelectedHighlight = true;
                }
            }
        }

        public static bool IsShiftDown()
        {
            return (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
        }

        public static bool IsControlDown()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        private void drawingsurface_MouseClick(object sender, MouseEventArgs e)
        {
            double x = e.X + _core.Display.BackgroundOffset.X;
            double y = e.Y + _core.Display.BackgroundOffset.Y;

            var hoverTile = _core.Actors.Intersections(new Point<double>(x, y), new Point<double>(1, 1)).OrderBy(o => o.DrawOrder).LastOrDefault();

            //Single item deletion with right button.
            if (e.Button == MouseButtons.Right && CurrentPrimaryMode == PrimaryMode.Insert)
            {
                if (hoverTile != null)
                {
                    hoverTile.QueueForDelete();
                    _hasBeenModified = true;
                }
            }
        }

        private void drawingsurface_Paint(object sender, PaintEventArgs e)
        {
            var image = _core.Render();

            e.Graphics.DrawImage(image, 0, 0);

            if (shapeSelectionRect != null)
            {
                Pen pen = new Pen(Color.Yellow, 2);

                if (shapeFillMode == ShapeFillMode.Delete)
                {
                    pen = new Pen(Color.Red, 2);
                }

                e.Graphics.DrawRectangle(pen, (Rectangle)shapeSelectionRect);
            }
        }

        private void drawingsurface_MouseMove(object sender, MouseEventArgs e)
        {
            double x = e.X + _core.Display.BackgroundOffset.X;
            double y = e.Y + _core.Display.BackgroundOffset.Y;

            toolStripStatusLabelMouseXY.Text = $"{x}x,{y}y";

            if (e.Button == MouseButtons.Middle)
            {
                _core.Display.BackgroundOffset.X = dragStartOffset.X - (e.X - dragStartMouse.X);
                _core.Display.BackgroundOffset.Y = dragStartOffset.Y - (e.Y - dragStartMouse.Y);
                _core.Display.DrawingSurface.Invalidate();
            }
            else
            {
                var hoverTile = _core.Actors.Intersections(new Point<double>(x, y),
                    new Point<double>(1, 1)).OrderBy(o => o.DrawOrder).LastOrDefault();

                if (e.Button != MouseButtons.Left) //Dont change the selection based on hover location while dragging.
                {
                    if (previousHoverTile != null && highlightHoverTile)
                    {
                        previousHoverTile.HoverHighlight = false;
                    }

                    if (hoverTile != null)
                    {
                        string hoverText = $"[{hoverTile.TilePath}]";

                        if (hoverTile.Meta?.CanStack == true)
                        {
                            hoverText += $" ({hoverTile.Meta.Quantity:N0})";
                        }

                        toolStripStatusLabelHoverObject.Text = hoverText;

                        if (highlightHoverTile && CurrentPrimaryMode != PrimaryMode.Shape)
                        {
                            hoverTile.HoverHighlight = true;
                        }

                        previousHoverTile = hoverTile;
                    }
                    else
                    {
                        toolStripStatusLabelHoverObject.Text = "";
                    }
                }

                if ((CurrentPrimaryMode == PrimaryMode.Shape && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
                    || (CurrentPrimaryMode == PrimaryMode.Select && e.Button == MouseButtons.Right))
                {
                    shapeSelectionRect = GraphicsUtility.SortRectangle(new Rectangle(
                                        (int)shapeInsertStartMousePosition.X,
                                        (int)shapeInsertStartMousePosition.Y,
                                        (int)(e.X - shapeInsertStartMousePosition.X),
                                        (int)(e.Y - shapeInsertStartMousePosition.Y)));

                    var rc = (Rectangle)shapeSelectionRect;
                    drawingsurface.Invalidate();
                    toolStripStatusLabelDebug.Text = $"{rc.X}x,{rc.Y}y->{rc.Width}x,{rc.Height}y";

                    if (CurrentPrimaryMode == PrimaryMode.Shape)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            shapeFillMode = ShapeFillMode.Insert;
                        }
                        if (e.Button == MouseButtons.Right)
                        {
                            shapeFillMode = ShapeFillMode.Delete;
                        }
                    }
                    else if (CurrentPrimaryMode == PrimaryMode.Select)
                    {
                        shapeFillMode = ShapeFillMode.Select;
                    }
                }

                //Paint with left button.
                if (e.Button == MouseButtons.Left && CurrentPrimaryMode == PrimaryMode.Insert)
                {
                    double drawDeltaX = e.X - (drawLastLocation.X - _core.Display.BackgroundOffset.X);
                    double drawDeltaY = e.Y - (drawLastLocation.Y - _core.Display.BackgroundOffset.Y);

                    if (lastPlacedItemSize.Width > 0)
                    {
                        if (Math.Abs(drawDeltaX) > lastPlacedItemSize.Width - tilePaintOverlap
                            || Math.Abs(drawDeltaY) > lastPlacedItemSize.Height - tilePaintOverlap)
                        {
                            PlaceSelectedItem(x, y);
                        }
                    }
                }

                //Drag item.
                if (e.Button == MouseButtons.Left && CurrentPrimaryMode == PrimaryMode.Select)
                {
                    if (selectedTile == null)
                    {
                        selectedTile = hoverTile;
                    }

                    var selectedTiles = _core.Actors.Tiles.Where(o => o.SelectedHighlight == true).ToList();
                    if (selectedTiles.Count == 1)
                    {
                        if (selectedTile != null)
                        {
                            selectedTile.X = x;
                            selectedTile.Y = y;
                            _hasBeenModified = true;
                        }
                    }
                    else if (selectedTiles.Count > 1)
                    {
                        int deltaX = (int)(selectedTile.X - x);
                        int deltaY = (int)(selectedTile.Y - y);

                        selectedTile.X = x;
                        selectedTile.Y = y;

                        foreach (var tile in selectedTiles)
                        {
                            if (tile != selectedTile)
                            {
                                tile.X -= deltaX;
                                tile.Y -= deltaY;
                            }
                        }
                    }
                }

                //Paint deletion with right button.
                if (e.Button == MouseButtons.Right && CurrentPrimaryMode == PrimaryMode.Insert)
                {
                    if (hoverTile != null)
                    {
                        hoverTile.QueueForDelete();
                        _hasBeenModified = true;
                    }
                }
            }
        }

        private void drawingsurface_MouseUp(object sender, MouseEventArgs e)
        {
            PopulateSelectedItemProperties();

            if (shapeFillMode == ShapeFillMode.Select && (IsShiftDown() || IsControlDown()))
            {
                double x = e.X + _core.Display.BackgroundOffset.X;
                double y = e.Y + _core.Display.BackgroundOffset.Y;

                var hoverTile = _core.Actors.Intersections(new Point<double>(x, y), new Point<double>(1, 1)).OrderBy(o => o.DrawOrder).LastOrDefault();

                if (hoverTile != null)
                {
                    if (IsShiftDown())
                    {
                        hoverTile.SelectedHighlight = true;
                    }
                    else if (IsControlDown())
                    {
                        hoverTile.SelectedHighlight = false;
                    }
                }
            }

            if (
                shapeSelectionRect != null &&
                   (
                        (CurrentPrimaryMode == PrimaryMode.Shape && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right))
                        || (CurrentPrimaryMode == PrimaryMode.Select && e.Button == MouseButtons.Right)
                   )
                )
            {
                if (shapeFillMode == ShapeFillMode.Select)
                {
                    var rc = (Rectangle)shapeSelectionRect;
                    double x = rc.X + _core.Display.BackgroundOffset.X;
                    double y = rc.Y + _core.Display.BackgroundOffset.Y;

                    var intersections = _core.Actors.Intersections(x, y, rc.Width, rc.Height);

                    if (IsControlDown() == true)
                    {
                        foreach (var obj in intersections)
                        {
                            obj.SelectedHighlight = false;
                        }
                    }
                    else
                    {
                        foreach (var obj in intersections)
                        {
                            obj.SelectedHighlight = true;
                        }
                    }

                }
                else if (shapeFillMode == ShapeFillMode.Insert)
                {
                    var rc = (Rectangle)shapeSelectionRect;
                    double x = rc.X + _core.Display.BackgroundOffset.X;
                    double y = rc.Y + _core.Display.BackgroundOffset.Y;

                    if (rc.Height > 0 && rc.Width > 0)
                    {
                        int minHeight = 0;

                        for (int py = 0; py < rc.Height;)
                        {
                            for (int px = 0; px < rc.Width;)
                            {
                                var placedTile = PlaceSelectedItem(x + px, y + py);
                                if (placedTile == null) //No tiles are selected in the explorer.
                                {
                                    shapeSelectionRect = null;
                                    drawingsurface.Invalidate();
                                    return;
                                }

                                px += placedTile.Size.Width;
                                //We keep track of the min height because I perfer overlap over gaps if the tiles are irregular.
                                if (placedTile.Size.Height > minHeight)
                                {
                                    minHeight = placedTile.Size.Height;
                                }
                            }
                            py += minHeight;
                        }
                    }
                }
                else if (shapeFillMode == ShapeFillMode.Delete)
                {
                    var rc = (Rectangle)shapeSelectionRect;
                    double x = rc.X + _core.Display.BackgroundOffset.X;
                    double y = rc.Y + _core.Display.BackgroundOffset.Y;

                    var intersections = _core.Actors.Intersections(x, y, rc.Width, rc.Height);

                    foreach (var obj in intersections)
                    {
                        obj.QueueForDelete();
                    }
                }

                shapeSelectionRect = null;
                drawingsurface.Invalidate();
            }
        }

        private void drawingsurface_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double x = e.X + _core.Display.BackgroundOffset.X;
            double y = e.Y + _core.Display.BackgroundOffset.Y;

            var hoverTile = _core.Actors.Intersections(new Point<double>(x, y), new Point<double>(1, 1)).OrderBy(o => o.DrawOrder).LastOrDefault();

            if (hoverTile == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Left && CurrentPrimaryMode == PrimaryMode.Select)
            {
                if (hoverTile.Meta?.IsContainer == true)
                {
                    EditorContainer((Guid)hoverTile.Meta.UID);
                }
                else if (hoverTile.Meta?.ActorClass == ActorClassName.ActorLevelWarp)
                {
                    using (var form = new FormSelectLevel(_core))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            int levelIndex = form.SelectedLevelIndex;
                            var level = _core.Levels.ByIndex(levelIndex);
                            selectedTile.Meta.LevelWarpName = level.Name;
                            PopulateSelectedItemProperties();
                        }
                    }
                }
            }
        }

        #endregion

        #region Form Events.

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            _core.ResizeDrawingSurface(new Size(drawingsurface.Width, drawingsurface.Height));
        }

        #endregion

        #region ListViewProperties.

        private void listViewProperties_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (listViewProperties.SelectedItems?.Count > 0)
                {
                    var selectedRow = listViewProperties.SelectedItems[0];

                    if (selectedRow.Text == "Contents")
                    {
                        if (selectedTile.Meta?.IsContainer == true)
                        {
                            EditorContainer((Guid)selectedTile.Meta.UID);
                        }
                    }
                    else if (selectedRow.Text == "Warp to Level")
                    {
                        using (var form = new FormSelectLevel(_core))
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                int levelIndex = form.SelectedLevelIndex;
                                var level = _core.Levels.ByIndex(levelIndex);
                                selectedTile.Meta.LevelWarpName = level.Name;
                                PopulateSelectedItemProperties();
                            }
                        }
                    }
                    else
                    {
                        using (var dialog = new FormTileProperties(selectedRow.Text, selectedRow.SubItems[1].Text.ToString()))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                if (selectedRow.Text == "Location")
                                {
                                    var coords = dialog.PropertyValue.Split(",");
                                    if (coords.Length == 2)
                                    {
                                        var x = double.Parse(coords[0]);
                                        var y = double.Parse(coords[1]);

                                        selectedTile.X = x;
                                        selectedTile.Y = y;
                                    }
                                }
                                else if (selectedRow.Text == "Can Take Damage")
                                {
                                    bool result = false;

                                    if (bool.TryParse(dialog.PropertyValue, out result) == false)
                                    {
                                        result = int.Parse(dialog.PropertyValue) != 0;
                                    }

                                    selectedTile.Meta.CanTakeDamage = result;
                                }
                                else if (selectedRow.Text == "Can Walk On")
                                {
                                    bool result = false;

                                    if (bool.TryParse(dialog.PropertyValue, out result) == false)
                                    {
                                        result = int.Parse(dialog.PropertyValue) != 0;
                                    }

                                    selectedTile.Meta.CanWalkOn = result;
                                }
                                else if (selectedRow.Text == "Name")
                                {
                                    selectedTile.Meta.Name = dialog.PropertyValue;
                                }
                                else if (selectedRow.Text == "Tag")
                                {
                                    selectedTile.Meta.Tag = dialog.PropertyValue;
                                }
                                else if (selectedRow.Text == "Hit Points")
                                {
                                    selectedTile.Meta.HitPoints = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Experience")
                                {
                                    selectedTile.Meta.Experience = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Quantity")
                                {
                                    selectedTile.Meta.Quantity = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Angle")
                                {
                                    selectedTile.Velocity.Angle.Degrees = double.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "z-Order")
                                {
                                    selectedTile.DrawOrder = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Armor Class")
                                {
                                    selectedTile.Meta.AC = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Damage Dice")
                                {
                                    selectedTile.Meta.DamageDice = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Damage Dice Faces")
                                {
                                    selectedTile.Meta.DamageDiceFaces = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Damage Additional")
                                {
                                    selectedTile.Meta.DamageAdditional = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Bulk")
                                {
                                    selectedTile.Meta.Bulk = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Bulk Capacity")
                                {
                                    selectedTile.Meta.BulkCapacity = int.Parse(dialog.PropertyValue);
                                }
                                else if (selectedRow.Text == "Weight Capacity")
                                {
                                    selectedTile.Meta.WeightCapacity = int.Parse(dialog.PropertyValue);
                                }

                                PopulateSelectedItemProperties();
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        void PopulateSelectedItemProperties()
        {
            listViewProperties.Items.Clear();

            if (selectedTile != null && selectedTile.Visible)
            {
                listViewProperties.Items.Add("Name").SubItems.Add(selectedTile.Meta?.Name);
                listViewProperties.Items.Add("Tile").SubItems.Add(selectedTile.TilePath);
                listViewProperties.Items.Add("Tag").SubItems.Add(selectedTile.Meta?.Tag);
                listViewProperties.Items.Add("Actor Class").SubItems.Add(selectedTile.Meta?.ActorClass.ToString());
                listViewProperties.Items.Add("Sub Type").SubItems.Add(selectedTile.Meta?.SubType.ToString());
                listViewProperties.Items.Add("Can Walk On").SubItems.Add(selectedTile.Meta?.CanWalkOn.ToString());
                listViewProperties.Items.Add("Angle").SubItems.Add(selectedTile.Velocity.Angle.Degrees.ToString());
                listViewProperties.Items.Add("z-Order").SubItems.Add(selectedTile.DrawOrder.ToString());
                listViewProperties.Items.Add("Size").SubItems.Add(selectedTile.Size.ToString());

                if (selectedTile.Meta.ActorClass == ActorClassName.ActorLevelWarp)
                {
                    listViewProperties.Items.Add("Warp to Level").SubItems.Add(selectedTile.Meta?.LevelWarpName?.ToString());
                }

                if (selectedTile.Meta.ActorClass == ActorClassName.ActorHostileBeing)
                {
                    listViewProperties.Items.Add("Can Take Damage").SubItems.Add(selectedTile.Meta?.CanTakeDamage.ToString());
                    listViewProperties.Items.Add("Hit Points").SubItems.Add(selectedTile.Meta?.HitPoints.ToString());
                    listViewProperties.Items.Add("Experience").SubItems.Add(selectedTile.Meta?.Experience.ToString());
                }

                if (selectedTile.Meta.SubType == ActorSubType.Armor || selectedTile.Meta.SubType == ActorSubType.Gauntlets
                    || selectedTile.Meta.SubType == ActorSubType.Helment || selectedTile.Meta.SubType == ActorSubType.Bracers
                    || selectedTile.Meta.SubType == ActorSubType.Boots || selectedTile.Meta.SubType == ActorSubType.Shield
                    || selectedTile.Meta.SubType == ActorSubType.Ring || selectedTile.Meta.SubType == ActorSubType.Garment
                    || selectedTile.Meta.SubType == ActorSubType.Belt || selectedTile.Meta.SubType == ActorSubType.Necklace)
                {
                    listViewProperties.Items.Add("Armor Class").SubItems.Add(selectedTile.Meta?.AC.ToString());
                }

                if (selectedTile.Meta.SubType == ActorSubType.Wand || selectedTile.Meta.SubType == ActorSubType.Weapon)
                {
                    listViewProperties.Items.Add("Damage Dice").SubItems.Add(selectedTile.Meta?.DamageDice.ToString());
                    listViewProperties.Items.Add("Damage Dice Faces").SubItems.Add(selectedTile.Meta?.DamageDiceFaces.ToString());
                    listViewProperties.Items.Add("Damage Additional").SubItems.Add(selectedTile.Meta?.DamageAdditional.ToString());
                }

                if (selectedTile.Meta.SubType == ActorSubType.Pack)
                {
                    listViewProperties.Items.Add("BulkCapacity").SubItems.Add(selectedTile.Meta?.BulkCapacity.ToString());
                    listViewProperties.Items.Add("WeightCapacity").SubItems.Add(selectedTile.Meta?.WeightCapacity.ToString());
                }

                listViewProperties.Items.Add("Bulk").SubItems.Add(selectedTile.Meta?.Bulk.ToString());
                listViewProperties.Items.Add("Weight").SubItems.Add(selectedTile.Meta?.Weight.ToString());

                listViewProperties.Items.Add("Location").SubItems.Add($"{selectedTile.Location.X},{selectedTile.Location.Y}");

                if (selectedTile.Meta?.IsContainer == true)
                {
                    listViewProperties.Items.Add("Contents").SubItems.Add("<open>");
                }
                if (selectedTile.Meta?.CanStack == true)
                {
                    listViewProperties.Items.Add("Quantity").SubItems.Add(selectedTile.Meta?.Quantity.ToString());
                }

                listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }

        #endregion

        public TreeNode CreateImageListAndAssets(ImageList imageList, TreeNode parent, string basePath, string partialPath)
        {
            TreeNode node = null;

            if (parent == null)
            {
                node = treeViewTiles.Nodes.Add(Path.GetFileName(partialPath));
            }
            else
            {
                node = parent.Nodes.Add(Path.GetFileName(partialPath));
            }

            foreach (var f in Directory.GetFiles(basePath + partialPath, "*.png"))
            {
                if (Path.GetFileName(f).StartsWith("@"))
                {
                    continue;
                }
                var file = new FileInfo(f);

                string fileKey = $"{partialPath}\\{Path.GetFileNameWithoutExtension(file.Name)}";

                imageList.Images.Add(fileKey, SpriteCache.GetBitmapCached(file.FullName));

                node.Nodes.Add(fileKey, Path.GetFileNameWithoutExtension(file.Name), fileKey, fileKey);
            }
            foreach (string d in Directory.GetDirectories(basePath + partialPath))
            {
                var directory = Path.GetFileName(d);
                if (directory.StartsWith("@") || directory.ToLower() == "player")
                {
                    continue;
                }
                var addedNode = CreateImageListAndAssets(imageList, node, basePath, partialPath + "\\" + directory);

                //Set the folder image to the first image in the children.
                TreeNode imageFind = addedNode;
                while (String.IsNullOrWhiteSpace(imageFind.ImageKey))
                {
                    if (imageFind.Nodes.Count > 0)
                    {
                        imageFind = imageFind.Nodes[0];
                    }
                    else
                    {
                        break;
                    }
                }
                if (imageFind != null)
                {
                    addedNode.ImageKey = imageFind.ImageKey;
                    addedNode.SelectedImageKey = imageFind.SelectedImageKey;
                }
            }

            return node;
        }

        void PopulateMaterials()
        {
            ImageList imageList = new ImageList();
            treeViewTiles.ImageList = imageList;
            CreateImageListAndAssets(imageList, null, Assets.Constants.BaseAssetPath, "Tiles");
            if (treeViewTiles.Nodes.Count > 0)
            {
                treeViewTiles.Nodes[0].Expand();
            }
        }

        TreeNode GetRandomChildNode(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                int nodeIndex = MathUtility.RandomNumber(0, node.Nodes.Count);

                if (node.Nodes[nodeIndex].Nodes.Count > 0)
                {
                    return GetRandomChildNode(node.Nodes[nodeIndex]);
                }
                else
                {
                    return node.Nodes[nodeIndex];
                }
            }

            return node;
        }

        private ActorBase PlaceSelectedItem(double x, double y)
        {
            ActorBase insertedTile = null;

            _hasBeenModified = true;

            if (treeViewTiles.SelectedNode == null)
            {
                return null;
            }

            var selectedItem = GetRandomChildNode(treeViewTiles.SelectedNode);
            if (selectedItem != null)
            {
                insertedTile = _core.Actors.AddNew<ActorBase>(x, y, selectedItem.FullPath);

                insertedTile.RefreshMetadata(false);

                if (insertedTile.Meta.ActorClass == ActorClassName.ActorSpawnPoint)
                {
                    insertedTile.DrawOrder = _core.Actors.Tiles.Max(o => o.DrawOrder) + 1;

                    var otherSpawnPoints = _core.Actors.Tiles.Where(o =>
                        o.Meta.ActorClass == ActorClassName.ActorSpawnPoint && o.Meta.UID != insertedTile.Meta.UID).ToList();

                    foreach (var tile in otherSpawnPoints)
                    {
                        tile.QueueForDelete();
                    }

                    _core.PurgeAllDeletedTiles();
                }

                //No need to create GUIDs for every terrain tile.
                if (insertedTile.Meta.ActorClass != ActorClassName.ActorTerrain)
                {
                    insertedTile.Meta.UID = Guid.NewGuid();
                }

                if (insertedTile.Meta.CanTakeDamage != null && ((bool)insertedTile.Meta.CanTakeDamage) == true)
                {
                    if (insertedTile.Meta.HitPoints == null)
                    {
                        insertedTile.Meta.HitPoints = 10;
                    }

                    if (insertedTile.Meta.Experience == null)
                    {
                        insertedTile.Meta.Experience = 10;
                    }
                }

                if (insertedTile.Meta.CanStack == true && insertedTile.Meta.Quantity == 0)
                {
                    insertedTile.Meta.Quantity = 1;
                }

                lastPlacedItemSize = insertedTile.Size;
                drawLastLocation = new Point<double>(x, y);
            }

            return insertedTile;
        }

        private void EditorContainer(Guid containerId)
        {
            using (var form = new FormEditContainer(_core, containerId))
            {
                form.ShowDialog();
            }
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
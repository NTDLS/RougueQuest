﻿using Library.Engine;
using Library.Engine.Types;
using ScenarioEdit.Engine;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ScenarioEdit
{
    public partial class FormEditItemSpawner : Form
    {
        public EngineCore Core { get; set; }

        public List<ActorSubType> SelectedSubTypes { get; set; } = new List<ActorSubType>();

        public FormEditItemSpawner(EngineCore core, List<ActorSubType> selectedTypes)
        {
            InitializeComponent();
            SelectedSubTypes = selectedTypes;

            if (SelectedSubTypes == null)
            {
                SelectedSubTypes = new List<ActorSubType>();
            }

            Core = core;
        }

        public FormEditItemSpawner()
        {
            InitializeComponent();
        }

        private void FormEditSpawner_Load(object sender, EventArgs e)
        {
            this.AcceptButton = buttonSave;
            this.CancelButton = buttonCancel;

            foreach (var obj in Utility.RandomDropSubTypes)
            {
                var item = new ListViewItem(obj.ToString())
                {
                    Tag = obj,
                    Checked = SelectedSubTypes.Contains(obj)
                };

                listViewContainer.Items.Add(item);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SelectedSubTypes.Clear();

            foreach (ListViewItem obj in listViewContainer.Items)
            {
                if (obj.Checked)
                {
                    SelectedSubTypes.Add((ActorSubType)obj.Tag);
                }
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem obj in listViewContainer.Items)
            {
                obj.Checked = true;
            }
        }

        private void buttonNone_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem obj in listViewContainer.Items)
            {
                obj.Checked = false;
            }
        }
    }
}

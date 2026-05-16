namespace SampleDataMaker.WinForm.Views
{
    partial class ConnectionOperationView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer = new SplitContainer();
            OverBaseSplitContainer = new SplitContainer();
            OverSplitContainer = new SplitContainer();
            dgvTables = new DataGridView();
            ColumnsDataGridView = new DataGridView();
            SelectTableDataGridView = new DataGridView();
            label1 = new Label();
            CreateCountTextBox = new TextBox();
            TemplateNameTextBox = new TextBox();
            TemplateComboBox = new ComboBox();
            TemplateButton = new Button();
            Create2Button = new Button();
            CreateButton = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)OverBaseSplitContainer).BeginInit();
            OverBaseSplitContainer.Panel1.SuspendLayout();
            OverBaseSplitContainer.Panel2.SuspendLayout();
            OverBaseSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)OverSplitContainer).BeginInit();
            OverSplitContainer.Panel1.SuspendLayout();
            OverSplitContainer.Panel2.SuspendLayout();
            OverSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTables).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ColumnsDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SelectTableDataGridView).BeginInit();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(OverBaseSplitContainer);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(label1);
            splitContainer.Panel2.Controls.Add(CreateCountTextBox);
            splitContainer.Panel2.Controls.Add(TemplateNameTextBox);
            splitContainer.Panel2.Controls.Add(TemplateComboBox);
            splitContainer.Panel2.Controls.Add(TemplateButton);
            splitContainer.Panel2.Controls.Add(Create2Button);
            splitContainer.Panel2.Controls.Add(CreateButton);
            splitContainer.Size = new Size(941, 540);
            splitContainer.SplitterDistance = 471;
            splitContainer.TabIndex = 0;
            // 
            // OverBaseSplitContainer
            // 
            OverBaseSplitContainer.Dock = DockStyle.Fill;
            OverBaseSplitContainer.Location = new Point(0, 0);
            OverBaseSplitContainer.Name = "OverBaseSplitContainer";
            OverBaseSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // OverBaseSplitContainer.Panel1
            // 
            OverBaseSplitContainer.Panel1.Controls.Add(OverSplitContainer);
            // 
            // OverBaseSplitContainer.Panel2
            // 
            OverBaseSplitContainer.Panel2.Controls.Add(SelectTableDataGridView);
            OverBaseSplitContainer.Size = new Size(941, 471);
            OverBaseSplitContainer.SplitterDistance = 342;
            OverBaseSplitContainer.TabIndex = 3;
            // 
            // OverSplitContainer
            // 
            OverSplitContainer.Dock = DockStyle.Fill;
            OverSplitContainer.Location = new Point(0, 0);
            OverSplitContainer.Name = "OverSplitContainer";
            // 
            // OverSplitContainer.Panel1
            // 
            OverSplitContainer.Panel1.Controls.Add(dgvTables);
            // 
            // OverSplitContainer.Panel2
            // 
            OverSplitContainer.Panel2.Controls.Add(ColumnsDataGridView);
            OverSplitContainer.Size = new Size(941, 342);
            OverSplitContainer.SplitterDistance = 272;
            OverSplitContainer.SplitterWidth = 5;
            OverSplitContainer.TabIndex = 2;
            // 
            // dgvTables
            // 
            dgvTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvTables.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvTables.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTables.Dock = DockStyle.Fill;
            dgvTables.Location = new Point(0, 0);
            dgvTables.Name = "dgvTables";
            dgvTables.RowHeadersWidth = 20;
            dgvTables.Size = new Size(272, 342);
            dgvTables.TabIndex = 0;
            // 
            // ColumnsDataGridView
            // 
            ColumnsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ColumnsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ColumnsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ColumnsDataGridView.Dock = DockStyle.Fill;
            ColumnsDataGridView.Location = new Point(0, 0);
            ColumnsDataGridView.Name = "ColumnsDataGridView";
            ColumnsDataGridView.RowHeadersWidth = 20;
            ColumnsDataGridView.Size = new Size(664, 342);
            ColumnsDataGridView.TabIndex = 1;
            // 
            // SelectTableDataGridView
            // 
            SelectTableDataGridView.AllowUserToAddRows = false;
            SelectTableDataGridView.AllowUserToDeleteRows = false;
            SelectTableDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            SelectTableDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            SelectTableDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            SelectTableDataGridView.Dock = DockStyle.Fill;
            SelectTableDataGridView.Location = new Point(0, 0);
            SelectTableDataGridView.Name = "SelectTableDataGridView";
            SelectTableDataGridView.ReadOnly = true;
            SelectTableDataGridView.RowHeadersWidth = 20;
            SelectTableDataGridView.Size = new Size(941, 125);
            SelectTableDataGridView.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(694, 24);
            label1.Name = "label1";
            label1.Size = new Size(69, 18);
            label1.TabIndex = 7;
            label1.Text = "作成件数 : ";
            // 
            // CreateCountTextBox
            // 
            CreateCountTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CreateCountTextBox.Location = new Point(769, 21);
            CreateCountTextBox.Name = "CreateCountTextBox";
            CreateCountTextBox.Size = new Size(74, 25);
            CreateCountTextBox.TabIndex = 6;
            // 
            // TemplateNameTextBox
            // 
            TemplateNameTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TemplateNameTextBox.Location = new Point(12, 5);
            TemplateNameTextBox.Name = "TemplateNameTextBox";
            TemplateNameTextBox.Size = new Size(464, 25);
            TemplateNameTextBox.TabIndex = 5;
            // 
            // TemplateComboBox
            // 
            TemplateComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TemplateComboBox.FormattingEnabled = true;
            TemplateComboBox.Location = new Point(12, 34);
            TemplateComboBox.Name = "TemplateComboBox";
            TemplateComboBox.Size = new Size(464, 26);
            TemplateComboBox.TabIndex = 4;
            // 
            // TemplateButton
            // 
            TemplateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TemplateButton.Location = new Point(482, 13);
            TemplateButton.Name = "TemplateButton";
            TemplateButton.Size = new Size(120, 40);
            TemplateButton.TabIndex = 3;
            TemplateButton.Text = "テンプレート";
            TemplateButton.UseVisualStyleBackColor = true;
            // 
            // Create2Button
            // 
            Create2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Create2Button.Location = new Point(608, 13);
            Create2Button.Name = "Create2Button";
            Create2Button.Size = new Size(80, 40);
            Create2Button.TabIndex = 2;
            Create2Button.Text = "種類作成";
            Create2Button.UseVisualStyleBackColor = true;
            // 
            // CreateButton
            // 
            CreateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CreateButton.Location = new Point(849, 13);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(80, 40);
            CreateButton.TabIndex = 1;
            CreateButton.Text = "作成";
            CreateButton.UseVisualStyleBackColor = true;
            // 
            // ConnectionOperationView
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(941, 540);
            Controls.Add(splitContainer);
            Font = new Font("メイリオ", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ConnectionOperationView";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ConnectionOperationView";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            OverBaseSplitContainer.Panel1.ResumeLayout(false);
            OverBaseSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)OverBaseSplitContainer).EndInit();
            OverBaseSplitContainer.ResumeLayout(false);
            OverSplitContainer.Panel1.ResumeLayout(false);
            OverSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)OverSplitContainer).EndInit();
            OverSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTables).EndInit();
            ((System.ComponentModel.ISupportInitialize)ColumnsDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)SelectTableDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer;
        private DataGridView dgvTables;
        private Button CreateButton;
        private Button Create2Button;
        private Button TemplateButton;
        private DataGridView ColumnsDataGridView;
        private SplitContainer OverSplitContainer;
        private ComboBox TemplateComboBox;
        private TextBox TemplateNameTextBox;
        private TextBox CreateCountTextBox;
        private Label label1;
        private SplitContainer OverBaseSplitContainer;
        private DataGridView SelectTableDataGridView;
    }
}
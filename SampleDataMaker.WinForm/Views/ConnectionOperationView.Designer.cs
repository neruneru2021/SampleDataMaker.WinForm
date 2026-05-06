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
            OverSplitContainer = new SplitContainer();
            dgvTables = new DataGridView();
            ColumnsDataGridView = new DataGridView();
            TemplateNameTextBox = new TextBox();
            TemplateComboBox = new ComboBox();
            TemplateButton = new Button();
            Create2Button = new Button();
            CreateButton = new Button();
            CreateCountTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)OverSplitContainer).BeginInit();
            OverSplitContainer.Panel1.SuspendLayout();
            OverSplitContainer.Panel2.SuspendLayout();
            OverSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTables).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ColumnsDataGridView).BeginInit();
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
            splitContainer.Panel1.Controls.Add(OverSplitContainer);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(CreateCountTextBox);
            splitContainer.Panel2.Controls.Add(TemplateNameTextBox);
            splitContainer.Panel2.Controls.Add(TemplateComboBox);
            splitContainer.Panel2.Controls.Add(TemplateButton);
            splitContainer.Panel2.Controls.Add(Create2Button);
            splitContainer.Panel2.Controls.Add(CreateButton);
            splitContainer.Size = new Size(800, 540);
            splitContainer.SplitterDistance = 471;
            splitContainer.TabIndex = 0;
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
            OverSplitContainer.Size = new Size(800, 471);
            OverSplitContainer.SplitterDistance = 266;
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
            dgvTables.Size = new Size(266, 471);
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
            ColumnsDataGridView.Size = new Size(529, 471);
            ColumnsDataGridView.TabIndex = 1;
            // 
            // TemplateNameTextBox
            // 
            TemplateNameTextBox.Location = new Point(180, 21);
            TemplateNameTextBox.Name = "TemplateNameTextBox";
            TemplateNameTextBox.Size = new Size(74, 25);
            TemplateNameTextBox.TabIndex = 5;
            // 
            // TemplateComboBox
            // 
            TemplateComboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TemplateComboBox.FormattingEnabled = true;
            TemplateComboBox.Location = new Point(260, 21);
            TemplateComboBox.Name = "TemplateComboBox";
            TemplateComboBox.Size = new Size(230, 26);
            TemplateComboBox.TabIndex = 4;
            // 
            // TemplateButton
            // 
            TemplateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TemplateButton.Location = new Point(496, 13);
            TemplateButton.Name = "TemplateButton";
            TemplateButton.Size = new Size(120, 40);
            TemplateButton.TabIndex = 3;
            TemplateButton.Text = "テンプレート";
            TemplateButton.UseVisualStyleBackColor = true;
            // 
            // Create2Button
            // 
            Create2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Create2Button.Location = new Point(622, 13);
            Create2Button.Name = "Create2Button";
            Create2Button.Size = new Size(80, 40);
            Create2Button.TabIndex = 2;
            Create2Button.Text = "種類作成";
            Create2Button.UseVisualStyleBackColor = true;
            // 
            // CreateButton
            // 
            CreateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CreateButton.Location = new Point(708, 13);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(80, 40);
            CreateButton.TabIndex = 1;
            CreateButton.Text = "作成";
            CreateButton.UseVisualStyleBackColor = true;
            // 
            // CreateCountTextBox
            // 
            CreateCountTextBox.Location = new Point(89, 22);
            CreateCountTextBox.Name = "CreateCountTextBox";
            CreateCountTextBox.Size = new Size(74, 25);
            CreateCountTextBox.TabIndex = 6;
            // 
            // ConnectionOperationView
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 540);
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
            OverSplitContainer.Panel1.ResumeLayout(false);
            OverSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)OverSplitContainer).EndInit();
            OverSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTables).EndInit();
            ((System.ComponentModel.ISupportInitialize)ColumnsDataGridView).EndInit();
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
    }
}
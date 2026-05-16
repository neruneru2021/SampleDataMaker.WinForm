namespace SampleDataMaker.WinForm.Views
{
    partial class ForeignKeySelectView
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
            ForeignKeyTableDataGridView = new DataGridView();
            ForeignKeyColumnDataGridView = new DataGridView();
            ForeignKeyDataGridView = new DataGridView();
            TblSplitContainer = new SplitContainer();
            KeyTableSplitContainer = new SplitContainer();
            BaseSplitContainer = new SplitContainer();
            UpperSplitContainer = new SplitContainer();
            SelectFKeyDataGridView = new DataGridView();
            ConfirmedButton = new Button();
            ((System.ComponentModel.ISupportInitialize)ForeignKeyTableDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ForeignKeyColumnDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ForeignKeyDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TblSplitContainer).BeginInit();
            TblSplitContainer.Panel1.SuspendLayout();
            TblSplitContainer.Panel2.SuspendLayout();
            TblSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)KeyTableSplitContainer).BeginInit();
            KeyTableSplitContainer.Panel1.SuspendLayout();
            KeyTableSplitContainer.Panel2.SuspendLayout();
            KeyTableSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BaseSplitContainer).BeginInit();
            BaseSplitContainer.Panel1.SuspendLayout();
            BaseSplitContainer.Panel2.SuspendLayout();
            BaseSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)UpperSplitContainer).BeginInit();
            UpperSplitContainer.Panel1.SuspendLayout();
            UpperSplitContainer.Panel2.SuspendLayout();
            UpperSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)SelectFKeyDataGridView).BeginInit();
            SuspendLayout();
            // 
            // ForeignKeyTableDataGridView
            // 
            ForeignKeyTableDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ForeignKeyTableDataGridView.Dock = DockStyle.Fill;
            ForeignKeyTableDataGridView.Location = new Point(0, 0);
            ForeignKeyTableDataGridView.Margin = new Padding(3, 4, 3, 4);
            ForeignKeyTableDataGridView.Name = "ForeignKeyTableDataGridView";
            ForeignKeyTableDataGridView.RowHeadersWidth = 20;
            ForeignKeyTableDataGridView.Size = new Size(268, 335);
            ForeignKeyTableDataGridView.TabIndex = 0;
            // 
            // ForeignKeyColumnDataGridView
            // 
            ForeignKeyColumnDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ForeignKeyColumnDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ForeignKeyColumnDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ForeignKeyColumnDataGridView.Dock = DockStyle.Fill;
            ForeignKeyColumnDataGridView.Location = new Point(0, 0);
            ForeignKeyColumnDataGridView.Margin = new Padding(3, 4, 3, 4);
            ForeignKeyColumnDataGridView.Name = "ForeignKeyColumnDataGridView";
            ForeignKeyColumnDataGridView.RowHeadersWidth = 20;
            ForeignKeyColumnDataGridView.Size = new Size(376, 335);
            ForeignKeyColumnDataGridView.TabIndex = 1;
            // 
            // ForeignKeyDataGridView
            // 
            ForeignKeyDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ForeignKeyDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ForeignKeyDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ForeignKeyDataGridView.Dock = DockStyle.Fill;
            ForeignKeyDataGridView.Location = new Point(0, 0);
            ForeignKeyDataGridView.Margin = new Padding(3, 4, 3, 4);
            ForeignKeyDataGridView.Name = "ForeignKeyDataGridView";
            ForeignKeyDataGridView.RowHeadersWidth = 20;
            ForeignKeyDataGridView.Size = new Size(280, 335);
            ForeignKeyDataGridView.TabIndex = 2;
            // 
            // TblSplitContainer
            // 
            TblSplitContainer.Dock = DockStyle.Fill;
            TblSplitContainer.Location = new Point(0, 0);
            TblSplitContainer.Name = "TblSplitContainer";
            // 
            // TblSplitContainer.Panel1
            // 
            TblSplitContainer.Panel1.Controls.Add(KeyTableSplitContainer);
            // 
            // TblSplitContainer.Panel2
            // 
            TblSplitContainer.Panel2.Controls.Add(ForeignKeyDataGridView);
            TblSplitContainer.Size = new Size(932, 335);
            TblSplitContainer.SplitterDistance = 648;
            TblSplitContainer.TabIndex = 3;
            // 
            // KeyTableSplitContainer
            // 
            KeyTableSplitContainer.Dock = DockStyle.Fill;
            KeyTableSplitContainer.Location = new Point(0, 0);
            KeyTableSplitContainer.Name = "KeyTableSplitContainer";
            // 
            // KeyTableSplitContainer.Panel1
            // 
            KeyTableSplitContainer.Panel1.Controls.Add(ForeignKeyTableDataGridView);
            // 
            // KeyTableSplitContainer.Panel2
            // 
            KeyTableSplitContainer.Panel2.Controls.Add(ForeignKeyColumnDataGridView);
            KeyTableSplitContainer.Size = new Size(648, 335);
            KeyTableSplitContainer.SplitterDistance = 268;
            KeyTableSplitContainer.TabIndex = 2;
            // 
            // BaseSplitContainer
            // 
            BaseSplitContainer.Dock = DockStyle.Fill;
            BaseSplitContainer.Location = new Point(0, 0);
            BaseSplitContainer.Name = "BaseSplitContainer";
            BaseSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // BaseSplitContainer.Panel1
            // 
            BaseSplitContainer.Panel1.Controls.Add(UpperSplitContainer);
            // 
            // BaseSplitContainer.Panel2
            // 
            BaseSplitContainer.Panel2.Controls.Add(ConfirmedButton);
            BaseSplitContainer.Size = new Size(932, 564);
            BaseSplitContainer.SplitterDistance = 490;
            BaseSplitContainer.TabIndex = 4;
            // 
            // UpperSplitContainer
            // 
            UpperSplitContainer.Dock = DockStyle.Fill;
            UpperSplitContainer.Location = new Point(0, 0);
            UpperSplitContainer.Name = "UpperSplitContainer";
            UpperSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // UpperSplitContainer.Panel1
            // 
            UpperSplitContainer.Panel1.Controls.Add(TblSplitContainer);
            // 
            // UpperSplitContainer.Panel2
            // 
            UpperSplitContainer.Panel2.Controls.Add(SelectFKeyDataGridView);
            UpperSplitContainer.Size = new Size(932, 490);
            UpperSplitContainer.SplitterDistance = 335;
            UpperSplitContainer.TabIndex = 4;
            // 
            // SelectFKeyDataGridView
            // 
            SelectFKeyDataGridView.AllowUserToAddRows = false;
            SelectFKeyDataGridView.AllowUserToDeleteRows = false;
            SelectFKeyDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            SelectFKeyDataGridView.Dock = DockStyle.Fill;
            SelectFKeyDataGridView.Location = new Point(0, 0);
            SelectFKeyDataGridView.Name = "SelectFKeyDataGridView";
            SelectFKeyDataGridView.ReadOnly = true;
            SelectFKeyDataGridView.RowHeadersWidth = 20;
            SelectFKeyDataGridView.Size = new Size(932, 151);
            SelectFKeyDataGridView.TabIndex = 0;
            // 
            // ConfirmedButton
            // 
            ConfirmedButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ConfirmedButton.Location = new Point(841, 23);
            ConfirmedButton.Name = "ConfirmedButton";
            ConfirmedButton.Size = new Size(80, 40);
            ConfirmedButton.TabIndex = 2;
            ConfirmedButton.Text = "確定";
            ConfirmedButton.UseVisualStyleBackColor = true;
            // 
            // ForeignKeySelectView
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(932, 564);
            Controls.Add(BaseSplitContainer);
            Font = new Font("メイリオ", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ForeignKeySelectView";
            Text = "ForeignKeySelectView";
            ((System.ComponentModel.ISupportInitialize)ForeignKeyTableDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)ForeignKeyColumnDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)ForeignKeyDataGridView).EndInit();
            TblSplitContainer.Panel1.ResumeLayout(false);
            TblSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TblSplitContainer).EndInit();
            TblSplitContainer.ResumeLayout(false);
            KeyTableSplitContainer.Panel1.ResumeLayout(false);
            KeyTableSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)KeyTableSplitContainer).EndInit();
            KeyTableSplitContainer.ResumeLayout(false);
            BaseSplitContainer.Panel1.ResumeLayout(false);
            BaseSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)BaseSplitContainer).EndInit();
            BaseSplitContainer.ResumeLayout(false);
            UpperSplitContainer.Panel1.ResumeLayout(false);
            UpperSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)UpperSplitContainer).EndInit();
            UpperSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)SelectFKeyDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView ForeignKeyTableDataGridView;
        private DataGridView ForeignKeyColumnDataGridView;
        private DataGridView ForeignKeyDataGridView;
        private SplitContainer TblSplitContainer;
        private SplitContainer BaseSplitContainer;
        private Button ConfirmedButton;
        private SplitContainer KeyTableSplitContainer;
        private SplitContainer UpperSplitContainer;
        private DataGridView SelectFKeyDataGridView;
    }
}
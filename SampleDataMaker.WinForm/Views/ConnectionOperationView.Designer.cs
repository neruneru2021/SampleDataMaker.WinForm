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
            dgvTables = new DataGridView();
            CreateButton = new Button();
            Create2Button = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTables).BeginInit();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(dgvTables);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(Create2Button);
            splitContainer.Panel2.Controls.Add(CreateButton);
            splitContainer.Size = new Size(800, 540);
            splitContainer.SplitterDistance = 438;
            splitContainer.TabIndex = 0;
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
            dgvTables.Size = new Size(438, 540);
            dgvTables.TabIndex = 0;
            // 
            // CreateButton
            // 
            CreateButton.Location = new Point(12, 12);
            CreateButton.Name = "CreateButton";
            CreateButton.Size = new Size(80, 40);
            CreateButton.TabIndex = 1;
            CreateButton.Text = "作成";
            CreateButton.UseVisualStyleBackColor = true;
            // 
            // Create2Button
            // 
            Create2Button.Location = new Point(98, 12);
            Create2Button.Name = "Create2Button";
            Create2Button.Size = new Size(80, 40);
            Create2Button.TabIndex = 2;
            Create2Button.Text = "種類作成";
            Create2Button.UseVisualStyleBackColor = true;
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
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTables).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer;
        private DataGridView dgvTables;
        private Button CreateButton;
        private Button Create2Button;
    }
}
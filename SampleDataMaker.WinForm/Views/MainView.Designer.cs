namespace SampleDataMaker.WinForm.Views
{
    partial class MainView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainSplitContainer = new SplitContainer();
            dgvConnections = new DataGridView();
            RegisterButton = new Button();
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).BeginInit();
            MainSplitContainer.Panel1.SuspendLayout();
            MainSplitContainer.Panel2.SuspendLayout();
            MainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConnections).BeginInit();
            SuspendLayout();
            // 
            // MainSplitContainer
            // 
            MainSplitContainer.Dock = DockStyle.Fill;
            MainSplitContainer.Location = new Point(0, 0);
            MainSplitContainer.Name = "MainSplitContainer";
            MainSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            MainSplitContainer.Panel1.Controls.Add(dgvConnections);
            // 
            // MainSplitContainer.Panel2
            // 
            MainSplitContainer.Panel2.Controls.Add(RegisterButton);
            MainSplitContainer.Size = new Size(531, 397);
            MainSplitContainer.SplitterDistance = 325;
            MainSplitContainer.TabIndex = 0;
            // 
            // dgvConnections
            // 
            dgvConnections.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvConnections.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvConnections.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvConnections.Dock = DockStyle.Fill;
            dgvConnections.Location = new Point(0, 0);
            dgvConnections.Name = "dgvConnections";
            dgvConnections.RowHeadersWidth = 15;
            dgvConnections.Size = new Size(531, 325);
            dgvConnections.TabIndex = 0;
            // 
            // RegisterButton
            // 
            RegisterButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            RegisterButton.Location = new Point(439, 16);
            RegisterButton.Name = "RegisterButton";
            RegisterButton.Size = new Size(80, 40);
            RegisterButton.TabIndex = 0;
            RegisterButton.Text = "登録";
            RegisterButton.UseVisualStyleBackColor = true;
            // 
            // MainView
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(531, 397);
            Controls.Add(MainSplitContainer);
            Font = new Font("メイリオ", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainView";
            StartPosition = FormStartPosition.CenterScreen;
            MainSplitContainer.Panel1.ResumeLayout(false);
            MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainSplitContainer).EndInit();
            MainSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConnections).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer MainSplitContainer;
        private Button RegisterButton;
        private DataGridView dgvConnections;
    }
}

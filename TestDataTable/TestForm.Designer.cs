namespace TestDataTable
{
    partial class TestForm
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
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.quitButton = new System.Windows.Forms.Button();
            this.sendButton = new System.Windows.Forms.Button();
            this.messages = new System.Windows.Forms.TextBox();
            this.layersButton = new System.Windows.Forms.Button();
            this.dataButton = new System.Windows.Forms.Button();
            this.layerListBox = new System.Windows.Forms.ListBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.sendCepButton = new System.Windows.Forms.Button();
            this.loadFeatureClassButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.featureClassTextBox = new System.Windows.Forms.Label();
            this.getGraphicsButton = new System.Windows.Forms.Button();
            this.sendGraphicsButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(200, 12);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(617, 343);
            this.dataGridView.TabIndex = 0;
            // 
            // quitButton
            // 
            this.quitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.quitButton.Location = new System.Drawing.Point(742, 509);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(75, 23);
            this.quitButton.TabIndex = 1;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // sendButton
            // 
            this.sendButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.sendButton.Location = new System.Drawing.Point(478, 509);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send GPS";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // messages
            // 
            this.messages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messages.Location = new System.Drawing.Point(13, 362);
            this.messages.Multiline = true;
            this.messages.Name = "messages";
            this.messages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.messages.Size = new System.Drawing.Size(804, 115);
            this.messages.TabIndex = 3;
            // 
            // layersButton
            // 
            this.layersButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.layersButton.Location = new System.Drawing.Point(113, 509);
            this.layersButton.Name = "layersButton";
            this.layersButton.Size = new System.Drawing.Size(69, 23);
            this.layersButton.TabIndex = 4;
            this.layersButton.Text = "Get Layers";
            this.layersButton.UseVisualStyleBackColor = true;
            this.layersButton.Click += new System.EventHandler(this.layersButton_Click);
            // 
            // dataButton
            // 
            this.dataButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.dataButton.Location = new System.Drawing.Point(188, 509);
            this.dataButton.Name = "dataButton";
            this.dataButton.Size = new System.Drawing.Size(58, 23);
            this.dataButton.TabIndex = 5;
            this.dataButton.Text = "Get Data";
            this.dataButton.UseVisualStyleBackColor = true;
            this.dataButton.Click += new System.EventHandler(this.dataButton_Click);
            // 
            // layerListBox
            // 
            this.layerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.layerListBox.FormattingEnabled = true;
            this.layerListBox.Location = new System.Drawing.Point(13, 13);
            this.layerListBox.Name = "layerListBox";
            this.layerListBox.Size = new System.Drawing.Size(181, 329);
            this.layerListBox.TabIndex = 6;
            // 
            // connectButton
            // 
            this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.connectButton.Location = new System.Drawing.Point(13, 509);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 7;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // clearButton
            // 
            this.clearButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.clearButton.Location = new System.Drawing.Point(640, 509);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 8;
            this.clearButton.Text = "Clear All";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // sendCepButton
            // 
            this.sendCepButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.sendCepButton.Location = new System.Drawing.Point(559, 509);
            this.sendCepButton.Name = "sendCepButton";
            this.sendCepButton.Size = new System.Drawing.Size(75, 23);
            this.sendCepButton.TabIndex = 9;
            this.sendCepButton.Text = "Send CEP";
            this.sendCepButton.UseVisualStyleBackColor = true;
            this.sendCepButton.Click += new System.EventHandler(this.sendCepButton_Click);
            // 
            // loadFeatureClassButton
            // 
            this.loadFeatureClassButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.loadFeatureClassButton.Location = new System.Drawing.Point(414, 509);
            this.loadFeatureClassButton.Name = "loadFeatureClassButton";
            this.loadFeatureClassButton.Size = new System.Drawing.Size(58, 23);
            this.loadFeatureClassButton.TabIndex = 10;
            this.loadFeatureClassButton.Text = "Load FC";
            this.loadFeatureClassButton.UseVisualStyleBackColor = true;
            this.loadFeatureClassButton.Click += new System.EventHandler(this.loadFeatureClassButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(94, 483);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(723, 20);
            this.textBox1.TabIndex = 11;
            // 
            // featureClassTextBox
            // 
            this.featureClassTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.featureClassTextBox.AutoSize = true;
            this.featureClassTextBox.Location = new System.Drawing.Point(14, 486);
            this.featureClassTextBox.Name = "featureClassTextBox";
            this.featureClassTextBox.Size = new System.Drawing.Size(74, 13);
            this.featureClassTextBox.TabIndex = 12;
            this.featureClassTextBox.Text = "Feature Class:";
            // 
            // getGraphicsButton
            // 
            this.getGraphicsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.getGraphicsButton.Location = new System.Drawing.Point(252, 509);
            this.getGraphicsButton.Name = "getGraphicsButton";
            this.getGraphicsButton.Size = new System.Drawing.Size(75, 23);
            this.getGraphicsButton.TabIndex = 13;
            this.getGraphicsButton.Text = "Get Graph";
            this.getGraphicsButton.UseVisualStyleBackColor = true;
            this.getGraphicsButton.Click += new System.EventHandler(this.getGraphicsButton_Click);
            // 
            // sendGraphicsButton
            // 
            this.sendGraphicsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.sendGraphicsButton.Location = new System.Drawing.Point(333, 509);
            this.sendGraphicsButton.Name = "sendGraphicsButton";
            this.sendGraphicsButton.Size = new System.Drawing.Size(75, 23);
            this.sendGraphicsButton.TabIndex = 14;
            this.sendGraphicsButton.Text = "Send Graph";
            this.sendGraphicsButton.UseVisualStyleBackColor = true;
            this.sendGraphicsButton.Click += new System.EventHandler(this.sendGraphicsButton_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 544);
            this.Controls.Add(this.sendGraphicsButton);
            this.Controls.Add(this.getGraphicsButton);
            this.Controls.Add(this.featureClassTextBox);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.loadFeatureClassButton);
            this.Controls.Add(this.sendCepButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.layerListBox);
            this.Controls.Add(this.dataButton);
            this.Controls.Add(this.layersButton);
            this.Controls.Add(this.messages);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.dataGridView);
            this.MinimumSize = new System.Drawing.Size(685, 38);
            this.Name = "TestForm";
            this.Text = "DNRGarmin - ArcMap Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox messages;
        private System.Windows.Forms.Button layersButton;
        private System.Windows.Forms.Button dataButton;
        private System.Windows.Forms.ListBox layerListBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button sendCepButton;
        private System.Windows.Forms.Button loadFeatureClassButton;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label featureClassTextBox;
        private System.Windows.Forms.Button getGraphicsButton;
        private System.Windows.Forms.Button sendGraphicsButton;
    }
}


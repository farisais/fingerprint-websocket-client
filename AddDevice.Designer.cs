namespace FingerprintSync
{
    partial class AddDevice
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.AddDeviceButton = new System.Windows.Forms.Button();
            this.RemoveDeviceButton = new System.Windows.Forms.Button();
            this.dataGridViewFP = new System.Windows.Forms.DataGridView();
            this.serial_number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ip_local = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fdid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFP)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewFP, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.97183F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.02817F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 284);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.AddDeviceButton);
            this.flowLayoutPanel1.Controls.Add(this.RemoveDeviceButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(451, 27);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // AddDeviceButton
            // 
            this.AddDeviceButton.Location = new System.Drawing.Point(3, 3);
            this.AddDeviceButton.Name = "AddDeviceButton";
            this.AddDeviceButton.Size = new System.Drawing.Size(32, 23);
            this.AddDeviceButton.TabIndex = 0;
            this.AddDeviceButton.Text = "+";
            this.AddDeviceButton.UseVisualStyleBackColor = true;
            this.AddDeviceButton.Click += new System.EventHandler(this.AddDeviceButton_Click);
            // 
            // RemoveDeviceButton
            // 
            this.RemoveDeviceButton.Location = new System.Drawing.Point(41, 3);
            this.RemoveDeviceButton.Name = "RemoveDeviceButton";
            this.RemoveDeviceButton.Size = new System.Drawing.Size(32, 23);
            this.RemoveDeviceButton.TabIndex = 1;
            this.RemoveDeviceButton.Text = "-";
            this.RemoveDeviceButton.UseVisualStyleBackColor = true;
            this.RemoveDeviceButton.Click += new System.EventHandler(this.RemoveDeviceButton_Click);
            // 
            // dataGridViewFP
            // 
            this.dataGridViewFP.AllowUserToAddRows = false;
            this.dataGridViewFP.AllowUserToDeleteRows = false;
            this.dataGridViewFP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFP.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.serial_number,
            this.ip_local,
            this.fdid,
            this.status});
            this.dataGridViewFP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewFP.Location = new System.Drawing.Point(3, 36);
            this.dataGridViewFP.Name = "dataGridViewFP";
            this.dataGridViewFP.ReadOnly = true;
            this.dataGridViewFP.Size = new System.Drawing.Size(451, 245);
            this.dataGridViewFP.TabIndex = 1;
            // 
            // serial_number
            // 
            this.serial_number.HeaderText = "Serial No.";
            this.serial_number.Name = "serial_number";
            this.serial_number.ReadOnly = true;
            // 
            // ip_local
            // 
            this.ip_local.HeaderText = "IP Address";
            this.ip_local.Name = "ip_local";
            this.ip_local.ReadOnly = true;
            // 
            // fdid
            // 
            this.fdid.HeaderText = "FP ID";
            this.fdid.Name = "fdid";
            this.fdid.ReadOnly = true;
            // 
            // status
            // 
            this.status.HeaderText = "Status";
            this.status.Name = "status";
            this.status.ReadOnly = true;
            // 
            // AddDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 284);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AddDevice";
            this.Text = "AddDevice";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddDevice_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFP)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button AddDeviceButton;
        private System.Windows.Forms.Button RemoveDeviceButton;
        public System.Windows.Forms.DataGridView dataGridViewFP;
        private System.Windows.Forms.DataGridViewTextBoxColumn serial_number;
        private System.Windows.Forms.DataGridViewTextBoxColumn ip_local;
        private System.Windows.Forms.DataGridViewTextBoxColumn fdid;
        private System.Windows.Forms.DataGridViewTextBoxColumn status;
    }
}
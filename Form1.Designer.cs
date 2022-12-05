namespace RebarCreate
{
    partial class Form1
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.crank_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(136, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 56);
            this.button2.TabIndex = 1;
            this.button2.Text = "Create Rebar";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 57);
            this.button1.TabIndex = 2;
            this.button1.Text = "Test Button";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // crank_btn
            // 
            this.crank_btn.Location = new System.Drawing.Point(254, 25);
            this.crank_btn.Name = "crank_btn";
            this.crank_btn.Size = new System.Drawing.Size(82, 52);
            this.crank_btn.TabIndex = 3;
            this.crank_btn.Text = "Crank";
            this.crank_btn.UseVisualStyleBackColor = true;
            this.crank_btn.Click += new System.EventHandler(this.crank_btn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 263);
            this.Controls.Add(this.crank_btn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button crank_btn;
    }
}


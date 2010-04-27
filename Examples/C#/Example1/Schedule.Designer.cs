namespace Example1
{
    public partial class Schedule
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
            this.listEvents = new System.Windows.Forms.ListView();
            this.clbTodo = new System.Windows.Forms.CheckedListBox();
            this.cbMonth = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbLocalTime = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listEvents
            // 
            this.listEvents.Location = new System.Drawing.Point(12, 66);
            this.listEvents.Name = "listEvents";
            this.listEvents.Size = new System.Drawing.Size(381, 394);
            this.listEvents.TabIndex = 1;
            this.listEvents.UseCompatibleStateImageBehavior = false;
            this.listEvents.View = System.Windows.Forms.View.List;
            // 
            // clbTodo
            // 
            this.clbTodo.FormattingEnabled = true;
            this.clbTodo.Location = new System.Drawing.Point(399, 66);
            this.clbTodo.Name = "clbTodo";
            this.clbTodo.Size = new System.Drawing.Size(262, 394);
            this.clbTodo.TabIndex = 2;
            // 
            // cbMonth
            // 
            this.cbMonth.FormattingEnabled = true;
            this.cbMonth.Items.AddRange(new object[] {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"});
            this.cbMonth.Location = new System.Drawing.Point(12, 19);
            this.cbMonth.Name = "cbMonth";
            this.cbMonth.Size = new System.Drawing.Size(129, 22);
            this.cbMonth.TabIndex = 3;
            this.cbMonth.SelectedIndexChanged += new System.EventHandler(this.cbMonth_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 14);
            this.label1.TabIndex = 4;
            this.label1.Text = "Choose A Month";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(273, 14);
            this.label2.TabIndex = 4;
            this.label2.Text = "This Month\'s Events";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(396, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(266, 14);
            this.label3.TabIndex = 4;
            this.label3.Text = "This Month\'s Todo List";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbLocalTime
            // 
            this.cbLocalTime.AutoSize = true;
            this.cbLocalTime.Location = new System.Drawing.Point(175, 19);
            this.cbLocalTime.Name = "cbLocalTime";
            this.cbLocalTime.Size = new System.Drawing.Size(170, 18);
            this.cbLocalTime.TabIndex = 5;
            this.cbLocalTime.Text = "Show Events in Local Time";
            this.cbLocalTime.UseVisualStyleBackColor = true;
            this.cbLocalTime.CheckedChanged += new System.EventHandler(this.cbLocalTime_CheckedChanged);
            // 
            // Schedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 472);
            this.Controls.Add(this.cbLocalTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbMonth);
            this.Controls.Add(this.clbTodo);
            this.Controls.Add(this.listEvents);
            this.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Schedule";
            this.Text = "Schedule";
            this.Load += new System.EventHandler(this.Schedule_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listEvents;
        private System.Windows.Forms.CheckedListBox clbTodo;
        private System.Windows.Forms.ComboBox cbMonth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbLocalTime;
    }
}


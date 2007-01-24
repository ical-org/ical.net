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
            this.SuspendLayout();
            // 
            // listEvents
            // 
            this.listEvents.Location = new System.Drawing.Point(208, 18);
            this.listEvents.Name = "listEvents";
            this.listEvents.Size = new System.Drawing.Size(232, 379);
            this.listEvents.TabIndex = 1;
            this.listEvents.UseCompatibleStateImageBehavior = false;
            this.listEvents.View = System.Windows.Forms.View.List;
            // 
            // clbTodo
            // 
            this.clbTodo.FormattingEnabled = true;
            this.clbTodo.Location = new System.Drawing.Point(459, 18);
            this.clbTodo.Name = "clbTodo";
            this.clbTodo.Size = new System.Drawing.Size(225, 379);
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
            this.cbMonth.Location = new System.Drawing.Point(12, 18);
            this.cbMonth.Name = "cbMonth";
            this.cbMonth.Size = new System.Drawing.Size(190, 21);
            this.cbMonth.TabIndex = 3;
            this.cbMonth.SelectedIndexChanged += new System.EventHandler(this.cbMonth_SelectedIndexChanged);
            // 
            // Schedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(705, 415);
            this.Controls.Add(this.cbMonth);
            this.Controls.Add(this.clbTodo);
            this.Controls.Add(this.listEvents);
            this.Name = "Schedule";
            this.Text = "Schedule";
            this.Load += new System.EventHandler(this.Schedule_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listEvents;
        private System.Windows.Forms.CheckedListBox clbTodo;
        private System.Windows.Forms.ComboBox cbMonth;
    }
}


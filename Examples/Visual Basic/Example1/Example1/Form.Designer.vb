<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.Label4 = New System.Windows.Forms.Label
        Me.txtDescription = New System.Windows.Forms.TextBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.txtSubject = New System.Windows.Forms.TextBox
        Me.btnAdd = New System.Windows.Forms.Button
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.datToDate = New System.Windows.Forms.DateTimePicker
        Me.datFromDate = New System.Windows.Forms.DateTimePicker
        Me.datToTime = New System.Windows.Forms.DateTimePicker
        Me.datFromTime = New System.Windows.Forms.DateTimePicker
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.gvSelectedDates = New System.Windows.Forms.DataGridView
        Me.btnMakeICS = New System.Windows.Forms.Button
        Me.GroupBox3 = New System.Windows.Forms.GroupBox
        Me.btnBrowse = New System.Windows.Forms.Button
        Me.txtSavePath = New System.Windows.Forms.TextBox
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.gvSelectedDates, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.txtDescription)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.txtSubject)
        Me.GroupBox1.Controls.Add(Me.btnAdd)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.datToDate)
        Me.GroupBox1.Controls.Add(Me.datFromDate)
        Me.GroupBox1.Controls.Add(Me.datToTime)
        Me.GroupBox1.Controls.Add(Me.datFromTime)
        Me.GroupBox1.Location = New System.Drawing.Point(9, 13)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(388, 157)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Select Dates and Times to Add to Your Calendar"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(11, 101)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(63, 13)
        Me.Label4.TabIndex = 10
        Me.Label4.Text = "Description:"
        '
        'txtDescription
        '
        Me.txtDescription.Location = New System.Drawing.Point(79, 98)
        Me.txtDescription.Name = "txtDescription"
        Me.txtDescription.Size = New System.Drawing.Size(294, 20)
        Me.txtDescription.TabIndex = 9
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(27, 75)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(46, 13)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Subject:"
        '
        'txtSubject
        '
        Me.txtSubject.Location = New System.Drawing.Point(80, 72)
        Me.txtSubject.Name = "txtSubject"
        Me.txtSubject.Size = New System.Drawing.Size(294, 20)
        Me.txtSubject.TabIndex = 7
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(297, 124)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(76, 27)
        Me.btnAdd.TabIndex = 6
        Me.btnAdd.Text = "Add"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(50, 49)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(23, 13)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "To:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(40, 23)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(33, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "From:"
        '
        'datToDate
        '
        Me.datToDate.Location = New System.Drawing.Point(79, 45)
        Me.datToDate.Name = "datToDate"
        Me.datToDate.Size = New System.Drawing.Size(200, 20)
        Me.datToDate.TabIndex = 3
        '
        'datFromDate
        '
        Me.datFromDate.Location = New System.Drawing.Point(79, 19)
        Me.datFromDate.Name = "datFromDate"
        Me.datFromDate.Size = New System.Drawing.Size(200, 20)
        Me.datFromDate.TabIndex = 2
        Me.datFromDate.Value = New Date(2008, 2, 6, 0, 0, 0, 0)
        '
        'datToTime
        '
        Me.datToTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.datToTime.Location = New System.Drawing.Point(285, 45)
        Me.datToTime.Name = "datToTime"
        Me.datToTime.ShowUpDown = True
        Me.datToTime.Size = New System.Drawing.Size(90, 20)
        Me.datToTime.TabIndex = 1
        '
        'datFromTime
        '
        Me.datFromTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.datFromTime.Location = New System.Drawing.Point(285, 19)
        Me.datFromTime.Name = "datFromTime"
        Me.datFromTime.ShowUpDown = True
        Me.datFromTime.Size = New System.Drawing.Size(90, 20)
        Me.datFromTime.TabIndex = 0
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.gvSelectedDates)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 176)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(385, 138)
        Me.GroupBox2.TabIndex = 2
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Currently Selected Dates"
        '
        'gvSelectedDates
        '
        Me.gvSelectedDates.AllowUserToAddRows = False
        Me.gvSelectedDates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gvSelectedDates.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gvSelectedDates.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.gvSelectedDates.Location = New System.Drawing.Point(3, 16)
        Me.gvSelectedDates.Name = "gvSelectedDates"
        Me.gvSelectedDates.Size = New System.Drawing.Size(379, 119)
        Me.gvSelectedDates.TabIndex = 0
        '
        'btnMakeICS
        '
        Me.btnMakeICS.Location = New System.Drawing.Point(274, 381)
        Me.btnMakeICS.Name = "btnMakeICS"
        Me.btnMakeICS.Size = New System.Drawing.Size(123, 25)
        Me.btnMakeICS.TabIndex = 3
        Me.btnMakeICS.Text = "Make .ics File"
        Me.btnMakeICS.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.btnBrowse)
        Me.GroupBox3.Controls.Add(Me.txtSavePath)
        Me.GroupBox3.Location = New System.Drawing.Point(12, 320)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(385, 55)
        Me.GroupBox3.TabIndex = 3
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Save To"
        '
        'btnBrowse
        '
        Me.btnBrowse.Location = New System.Drawing.Point(309, 20)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(61, 25)
        Me.btnBrowse.TabIndex = 1
        Me.btnBrowse.Text = "Browse..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtSavePath
        '
        Me.txtSavePath.Location = New System.Drawing.Point(14, 23)
        Me.txtSavePath.Name = "txtSavePath"
        Me.txtSavePath.Size = New System.Drawing.Size(280, 20)
        Me.txtSavePath.TabIndex = 0
        '
        'SaveFileDialog1
        '
        Me.SaveFileDialog1.Filter = "ICS Files|*.ics"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(405, 413)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.btnMakeICS)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "Create ICS File"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.gvSelectedDates, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents datFromTime As System.Windows.Forms.DateTimePicker
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents datToDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents datFromDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents datToTime As System.Windows.Forms.DateTimePicker
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents btnMakeICS As System.Windows.Forms.Button
    Friend WithEvents gvSelectedDates As System.Windows.Forms.DataGridView
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents txtSavePath As System.Windows.Forms.TextBox
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtDescription As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtSubject As System.Windows.Forms.TextBox
    Friend WithEvents btnAdd As System.Windows.Forms.Button

End Class

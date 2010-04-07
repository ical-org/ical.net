Imports DDay.iCal
Public Class Form

    'create a datatable to hold the dates that the user selects
    Dim dtSelectedDates As New DataTable("dtSelectedDates")


    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try

            'setup the controls for our first time through
            SaveFileDialog1.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            txtSavePath.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) & "\MyTest.ics"

            'Set date/time controls to today and now
            datFromDate.Value = Now.Date
            datToDate.Value = Now.Date

            datFromTime.Value = Now.ToLocalTime
            datToTime.Value = Now.ToLocalTime

            'create a datatable to hold the dates that the user selects.  I chose to use a datatable because
            'I plan on eventually grabbing the data for the ICS file from a database layer, which returns a datatable.
            dtSelectedDates.Columns.Add("Start", GetType(System.DateTime))
            dtSelectedDates.Columns.Add("End", GetType(System.DateTime))
            dtSelectedDates.Columns.Add("Summary", GetType(System.String))
            dtSelectedDates.Columns.Add("Description", GetType(System.String))

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

#Region "DDay ICal Stuff"
    ''' <summary>
    ''' Creates a DDay iCalendar object and saves selected dates to an ics file
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnMakeICS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMakeICS.Click

        'Create a new DDay.ICalendar
        Dim MyCal As New iCalendar

        'Set to version 1.0 to make it compatible with Outlook 2003
        MyCal.Version = "1.0"

        Try

            'did they select anything?
            If dtSelectedDates.Rows.Count = 0 Then
                MsgBox("You did not select any dates.", MsgBoxStyle.OkOnly, "No Dates Selected")
                Exit Sub
            End If

            If txtSavePath.Text = "" Then
                MsgBox("You did not specify a location to save to.", MsgBoxStyle.OkOnly, "Invalid Save Location")
                Exit Sub
            End If


            'Create a fileInfo object for error checking the user's provided path
            Dim SaveLocation As New System.IO.FileInfo(txtSavePath.Text)

            'Check that the user-supplied path exists 
            Select Case True

                Case Not (SaveLocation.Directory.Exists)
                    MsgBox("The Save directory you specified does not exist.", MsgBoxStyle.OkOnly, "Invalid Save Location")
                    Exit Sub

                Case (SaveLocation.Extension <> ".ics")
                    MsgBox("The file name must have an extension of .ics.", MsgBoxStyle.OkOnly, "Invalid Save Location")
                    Exit Sub
            End Select



            'Loop through each event that the user has created, and that has been
            'stored in the datatable 'dtSelectedDates'
            For Each Item As DataRow In dtSelectedDates.Rows

                'Create an event in thr iCalendar
                Dim MyEvent As DDay.iCal.Components.Event = MyCal.Create(Of DDay.iCal.Components.Event)()

                'Populate the properties
                With MyEvent
                    .Start = New DDay.iCal.DataTypes.iCalDateTime(DirectCast(Item("Start"), Date))
                    .End = New DDay.iCal.DataTypes.iCalDateTime(DirectCast(Item("End"), Date))
                    .Summary = New DDay.iCal.DataTypes.Text(DirectCast(Item("Summary"), String))
                    .Description = New DDay.iCal.DataTypes.Text(DirectCast(Item("Description"), String))
                End With

            Next


            'Serialize the calendar, and output it to the user-specified path
            Dim MySerializer As New DDay.iCal.Serialization.iCalendarSerializer(MyCal)
            MySerializer.Serialize(txtSavePath.Text)

            MsgBox("ICS Created at" & SaveLocation.FullName & "!", MsgBoxStyle.OkOnly, "Success")

        Catch ex As Exception
            MsgBox(ex.ToString)

        Finally
            MyCal.Dispose()
        End Try


    End Sub
#End Region


#Region "Other Stuff to get basic application working"
    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Try
            With SaveFileDialog1
                .InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                .ShowDialog()
            End With

            If SaveFileDialog1.FileName <> "" Then txtSavePath.Text = SaveFileDialog1.FileName
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub


    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        Try

            'Create a new row, and populate it with the values that the user has supplied
            Dim dr As DataRow = dtSelectedDates.NewRow
            With dr
                dr("Start") = datFromDate.Value.ToShortDateString & " " & datFromTime.Value.ToShortTimeString
                dr("End") = datToDate.Value.ToShortDateString & " " & datToTime.Value.ToShortTimeString
                dr("Summary") = txtSubject.Text
                dr("Description") = txtDescription.Text
            End With

            'Add the row to the datatable
            dtSelectedDates.Rows.Add(dr)

            'rebind the gridview
            gvSelectedDates.DataSource = dtSelectedDates

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub
#End Region


End Class

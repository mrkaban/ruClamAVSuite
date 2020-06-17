Imports System.ComponentModel
Imports System.Threading
Imports System.Net
Imports System.Security.AccessControl
Imports System.Xml

Public Class Form1
    Private drag As Boolean
    Private scanInAct As Boolean
    Private wifi As Boolean
    Private usb As Boolean
    Private mustStop As Boolean
    Private lastScan As String
    Private mousex As Integer
    Private mousey As Integer
    Private trd As Thread
    Shared backFinished As Boolean = False
    'BUTTON17 to verify...
    'Make user-manual
    'Modify file app.manifest to start clamav as administrator
    'Form events
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim temp1 As String()
        temp1 = guiOptionGetter(RichTextBox1, RichTextBox2)
        If temp1(0).Contains("Вкл") Then
            wifi = True
            Label4.Text = "Активен"
            Label4.Location = New Point(348, 103)
        Else
            wifi = False
            Label4.Text = "Не активен"
            Label4.Location = New Point(332, 103)
        End If
        If temp1(1).Contains("Вкл") Then
            usb = True
            Label5.Text = "Активен"
            Label5.Location = New Point(610, 103)
        Else
            usb = False
            Label5.Text = "Не активен"
            Label5.Location = New Point(595, 103)
        End If
        If temp1(2).Contains("Никогда") Then
            Label3.Text = "Последняя проверка: Никогда"
            lastScan = "Последняя проверка: Никогда"
            Label3.Location = New Point(65, 103)
            Me.Refresh()
        Else
            Label3.Text = temp1(2)
            Label3.Location = New Point(35, 103)
        End If
        If realTimeGet() = False Then
            PictureBox2.BackgroundImage = My.Resources.no
            Label2.Text = "Служба ClamAV не активна."
        Else
            PictureBox2.BackgroundImage = My.Resources.ok
            Label2.Text = "Служба ClamAV активна."
        End If
    End Sub

    Private Sub Button1_MouseHover(sender As Object, e As EventArgs) Handles Button1.MouseHover
        Button1.Image = My.Resources.close_selected 'Change image when mouse hover control buttons (close, minimize)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Hide()
        'Dim toastXml As XmlDocument = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04)

        '' Fill in the text elements
        'Dim stringElements As XmlNodeList = toastXml.GetElementsByTagName("text")
        'For i As Integer = 0 To stringElements.Count - 1
        '    stringElements(i).AppendChild(toastXml.CreateTextNode("Line " + i))
        'Next

        '' Specify the absolute path to an image
        'Dim imagePath As [String] = "file:///" + IO.Path.GetFullPath("toastImageAndText.png")
        'Dim imageElements As XmlNodeList = toastXml.GetElementsByTagName("image")

        'Dim toast As New ToastNotification(toastXml)
    End Sub

    Private Sub Button1_MouseLeave(sender As Object, e As EventArgs) Handles Button1.MouseLeave
        Button1.Image = My.Resources.close 'Restore image of control buttons
    End Sub

    Private Sub Button2_MouseHover(sender As Object, e As EventArgs) Handles Button2.MouseHover
        Button2.Image = My.Resources.down_selected 'Change image when mouse hover control buttons (close, minimize)
    End Sub

    Private Sub Button2_MouseLeave(sender As Object, e As EventArgs) Handles Button2.MouseLeave
        Button2.Image = My.Resources.down 'Restore image of control buttons
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub
    'End form events
    'Window move
    Private Sub Panel1_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel1.MouseDown
        drag = True 'Sets the variable drag to true.
        mousex = Windows.Forms.Cursor.Position.X - My.Forms.Form1.Left 'Sets variable mousex
        mousey = Windows.Forms.Cursor.Position.Y - My.Forms.Form1.Top 'Sets variable mousey
    End Sub

    Private Sub Panel1_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel1.MouseMove
        'If drag is set to true then move the form accordingly.
        If drag Then
            My.Forms.Form1.Top = Windows.Forms.Cursor.Position.Y - mousey
            My.Forms.Form1.Left = Windows.Forms.Cursor.Position.X - mousex
        End If
    End Sub

    Private Sub Panel1_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel1.MouseUp
        drag = False 'Sets drag to false, so the form does not move according to the code in MouseMove
    End Sub

    Private Sub Label1_MouseDown(sender As Object, e As MouseEventArgs) Handles Label1.MouseDown
        drag = True 'Sets the variable drag to true.
        mousex = Windows.Forms.Cursor.Position.X - My.Forms.Form1.Left 'Sets variable mousex
        mousey = Windows.Forms.Cursor.Position.Y - My.Forms.Form1.Top 'Sets variable mousey
    End Sub

    Private Sub Label1_MouseMove(sender As Object, e As MouseEventArgs) Handles Label1.MouseMove
        'If drag is set to true then move the form accordingly.
        If drag Then
            My.Forms.Form1.Top = Windows.Forms.Cursor.Position.Y - mousey
            My.Forms.Form1.Left = Windows.Forms.Cursor.Position.X - mousex
        End If
    End Sub

    Private Sub Label1_MouseUp(sender As Object, e As MouseEventArgs) Handles Label1.MouseUp
        drag = False 'Sets drag to false, so the form does not move according to the code in MouseMove
    End Sub
    'End window move
    'Guide and contributors
    Private Sub GuideToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuideToolStripMenuItem.Click
        'WARNING show user-manual
        MsgBox("Открыть руководство в PDF")
    End Sub
    Private Sub ClamAVSuiteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClamAVSuiteToolStripMenuItem.Click
        MsgBox("ruClamAV-Suite переведен на русский язык командой сайта КонтинентСвободы.рф", MsgBoxStyle.Information, "Информация")
    End Sub

    Private Sub ClamAVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClamAVToolStripMenuItem.Click
        Dim contrib As Form
        Dim names As RichTextBox
        names = New RichTextBox
        names.BackColor = Color.FromArgb(42, 42, 42)
        names.Rtf = "{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1040{\fonttbl{\f0\fnil\fcharset0 Calibri;}}
{\*\generator Riched20 10.0.14393}\viewkind4\uc1 
\b\lang16 ClamAV Team\b0\tab\tab\tab\tab\b Talos Group\b0\par
Joel Esler\tab\tab\tab\tab Andrea Allievi\tab\tab\tab\tab Alex McDonnell\par
Nigel Houghton\tab\tab\tab Jonathan Arneson\tab\tab\tab Kevin Miklavcic\par
Tom Judge\tab\tab\tab\tab Ben Baker\tab\tab\tab\tab Patrick Mullen\par
Kevin Lin\tab\tab\tab\tab Nathan Benson\tab\tab\tab Marcin Noga\par
Steve Morgan\tab\tab\tab\tab Andrew Blunk\tab\tab\tab\tab Katie Nolan\par
Matt Olney\tab\tab\tab\tab Kevin Brooks\tab\tab\tab\tab Carlos Pacho\par
Dave Raynor\tab\tab\tab\tab Jaime Filson\tab\tab\tab\tab Ryan Pentney\par
Samir Sapra\tab\tab\tab\tab Paul Frank\tab\tab\tab\tab Nick Randolph\par
Ryan Steinmetz\tab\tab\tab Erick Galinkin\tab\tab\tab\tab Marcos Rodriguez\par
Mickey Sola\tab\tab\tab\tab Richard Harman, Jr.\tab\tab\tab Geoff Serrao \par
Dave Suffling\tab\tab\tab\tab Nicholas Herbert\tab\tab\tab Brandon Stultz\par
Matt Watchinkski\tab\tab\tab Richard Johnson\tab\tab\tab Nick Suan\par
Alain Zidouemba\tab\tab\tab Alex Kambis\tab\tab\tab\tab Emmanuel Tacheau\par
\tab\tab\tab\tab\tab Brittany Lawler\tab\tab\tab Melissa Taylor\par
\b ClamAV QA\b0\tab\tab\tab\tab Chris Marczewski\tab\tab\tab Angel Villegas\par
Erin Germ\tab\tab\tab\tab Christopher Marshall\tab\tab\tab Yves Younan \par
Dragos Malene\tab\tab\tab Nick Mavis\par
Vijay Mistry\tab\tab\tab\tab Christopher McBee\par
Matt Donnan\tab\tab\tab\tab David McDaniel\par\par
\b\cf1 Contributors\cf0\b0\par
Aeriana, Andreas Cadhalpun, Mike Cathey, Michael Cichosz, Diego d'Ambra, Arnaud Jacques,\par
Tomasz Papszun, Bill Parker, Robert Scroggins, Sven Strickroth, Trog, Steve Basford,\par
Dennis de Messemacker, Jason Englander, Thomas Lamy, Thomas Masden, Boguslaw Brandys,\par
Anthony Havé, Andreas Faust, Sebastian Andrzej Siewior\par\par
\b ClamAV Emeritus\b0\par
Luca Gibelli, Török Edvin, Tomasz Kojm, Alberto Wu, Nigel Horne\par
}"
        names.ForeColor = Color.AntiqueWhite
        contrib = New Form()
        contrib.Height = 600
        contrib.Width = 800
        contrib.AccessibleName = "Contributors"
        contrib.Text = "Авторы"
        contrib.Icon = System.Drawing.Icon.FromHandle(My.Resources.ClamAV_Logo.GetHicon)
        contrib.DesktopLocation = New Point(My.Computer.Screen.Bounds.Width - 600, My.Computer.Screen.Bounds.Height - 400)
        contrib.Controls.Add(names)
        names.Height = contrib.Size.Height - 10
        names.Width = contrib.Size.Width - 10
        contrib.FormBorderStyle = FormBorderStyle.Fixed3D
        contrib.MaximizeBox = False
        contrib.Visible = False
        contrib.ShowIcon = True
        contrib.ShowDialog()
    End Sub
    'End guide and contributors
    'Check box in the app other checkbox are in settings section
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        'wifi
        If CheckBox1.Checked = True Then
            CheckBox1.Text = "ВКЛ"
            CheckBox1.ForeColor = Color.ForestGreen
            CheckBox1.RightToLeft = RightToLeft.Yes
        Else
            CheckBox1.Text = "ВЫКЛ"
            CheckBox1.ForeColor = Color.Red
            CheckBox1.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        'usb
        If CheckBox2.Checked = True Then
            CheckBox2.Text = "ВКЛ"
            CheckBox2.ForeColor = Color.ForestGreen
            CheckBox2.RightToLeft = RightToLeft.Yes
        Else
            CheckBox2.Text = "ВЫКЛ"
            CheckBox2.ForeColor = Color.Red
            CheckBox2.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox13_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox13.CheckedChanged
        'shutdown after a scan
        If CheckBox13.Checked = True Then
            CheckBox13.Text = "ВКЛ"
            CheckBox13.ForeColor = Color.ForestGreen
            CheckBox13.RightToLeft = RightToLeft.Yes
        Else
            CheckBox13.Text = "ВЫКЛ"
            CheckBox13.ForeColor = Color.Red
            CheckBox13.RightToLeft = RightToLeft.No
        End If
    End Sub
    'End check box in the app
    'Home buttons events
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Panel3.Width = 733
        Panel3.Height = 404
        Panel3.Location = New Point(3, 0)
        Label1.Text = "ClamAV - Анализ"
        Panel3.Visible = True
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Panel4.Width = 733
        Panel4.Height = 404
        Panel4.Location = New Point(3, 0)
        Label1.Text = "ClamAV - Защита Wi-Fi"
        Panel4.Visible = True
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Panel5.Width = 733
        Panel5.Height = 404
        Panel5.Location = New Point(3, 0)
        Label1.Text = "ClamAV - USB Контроллер"
        Panel5.Visible = True
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Panel6.Width = 733
        Panel6.Height = 404
        Panel6.Location = New Point(3, 0)
        Label1.Text = "ClamAV - Шифрование файлов и безопасное удаление"
        Panel6.Visible = True
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Panel7.Width = 733
        Panel7.Height = 404
        Panel7.Location = New Point(3, 0)
        Label1.Text = "ClamAV - Отчеты"
        Panel7.Visible = True
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Panel8.Width = 733
        Panel8.Height = 404
        Panel8.Location = New Point(3, 0)
        Label1.Text = "ClamAV - Настройки"
        Panel8.Visible = True
    End Sub
    'End home buttons events

    'Back to home buttons
    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        Label1.Text = "ClamAV - Главная"
        Panel3.Visible = False
    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        Label1.Text = "ClamAV - Главная"
        Panel4.Visible = False
    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        Label1.Text = "ClamAV - Главная"
        Panel5.Visible = False
    End Sub

    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        Label1.Text = "ClamAV - Главная"
        Panel6.Visible = False
    End Sub

    Private Sub Button21_Click(sender As Object, e As EventArgs) Handles Button21.Click
        Label1.Text = "ClamAV - Главная"
        Panel7.Visible = False
    End Sub

    Private Sub Button23_Click(sender As Object, e As EventArgs) Handles Button23.Click
        Label1.Text = "ClamAV - Главная"
        Panel8.Visible = False
    End Sub
    'End back to home buttons
    'Analysis panel

    Private Sub Panel3_VisibleChanged(sender As Object, e As EventArgs) Handles Panel3.VisibleChanged
        If Panel3.Visible = True Then
            If shutFileGet() Then
                CheckBox13.CheckState = CheckState.Checked
            Else
                CheckBox13.CheckState = CheckState.Unchecked
            End If
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        startScanGraphicsEffect()
        trd = New Thread(AddressOf fastScan)
        trd.IsBackground = True
        BackgroundWorker2.RunWorkerAsync()
        trd.Start()
        ProgressBar1.Value = 0
        scanInAct = True
        ProgressBar1.Style = ProgressBarStyle.Blocks
        ProgressBar1.ForeColor = Color.Red
        Timer1.Interval = 14000
        mustStop = False
        Timer1.Start()
        Label3.Text = "Последняя проверка: " & System.DateTime.Now
        lastScan = Label3.Text
        If Label3.Text.Contains("Никогда") Then
            Label3.Location = New Point(65, 103)
        Else
            Label3.Location = New Point(35, 103)
        End If
        guiOptionSetter(wifi, usb, lastScan)
    End Sub


    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Dim unitToPass As String
        Dim unitLetter As Char = ChrW(65)
        Dim drives(26) As Char
        Dim size As UInt64
        startScanGraphicsEffect()
        trd = New Thread(AddressOf fullScan)
        trd.IsBackground = True
        BackgroundWorker2.RunWorkerAsync()
        trd.Start()
        ProgressBar1.Value = 1
        scanInAct = True
        ProgressBar1.Style = ProgressBarStyle.Continuous
        ProgressBar1.ForeColor = Color.Red
        unitToPass = unitLetter
        size = 0
        For i As Int16 = 0 To 26
            If (Drive_Status(unitToPass) = True) Then
                size += GetDiskSpace(unitToPass)
            End If
            unitToPass = ChrW(AscW(unitLetter) + (i + 1))
        Next
        If size >= 60000000000 Then ' Used to set the timer, determined from hard disk size not free.
            Timer1.Interval = 400000
        ElseIf size <= 60000000000 And size >= 40000000000 Then
            Timer1.Interval = 200000
        ElseIf size < 40000000000 And size >= 20000000000 Then
            Timer1.Interval = 150000
        ElseIf size < 20000000000 Then
            Timer1.Interval = 100000
        End If
        mustStop = False
        Timer1.Start()
        Label3.Text = "Последняя проверка: " & System.DateTime.Now
        lastScan = Label3.Text
        If Label3.Text.Contains("Никогда") Then
            Label3.Location = New Point(65, 103)
        Else
            Label3.Location = New Point(35, 103)
        End If
        guiOptionSetter(wifi, usb, lastScan)
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Dim customizedScanPath As String
        If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            customizedScanPath = FolderBrowserDialog1.SelectedPath
            If Not customizedScanPath = "" And Not customizedScanPath = " " Then
                BackgroundWorker2.RunWorkerAsync()
                customScan(customizedScanPath) ' Set the path to scan
                startScanGraphicsEffect()
                ProgressBar1.Value = 0
                scanInAct = True
                ProgressBar1.Style = ProgressBarStyle.Continuous
                ProgressBar1.ForeColor = Color.Red
                Timer1.Interval = 3000
                mustStop = False
                Timer1.Start()
                Label3.Text = "Последняя проверка: " & System.DateTime.Now
                lastScan = Label3.Text
                If Label3.Text.Contains("Никогда") Then
                    Label3.Location = New Point(65, 103)
                Else
                    Label3.Location = New Point(35, 103)
                End If
                guiOptionSetter(wifi, usb, lastScan)
            End If
        End If
    End Sub

    Public Sub startScanGraphicsEffect()
        Button12.Enabled = True
        Button12.Text = "Пауза"
        Button13.Enabled = True
        Button9.Enabled = False
        Button10.Enabled = False
        Button11.Enabled = False
        ProgressBar1.UseWaitCursor = True
        Button9.Refresh()
        Button10.Refresh()
        Button11.Refresh()
        Button12.Refresh()
        Button13.Refresh()
    End Sub

    Public Sub stopScanGraphicsEffect()
        ProgressBar1.Value = 0
        ProgressBar1.UseWaitCursor = False
        Button12.Text = "Пауза"
        Button12.Enabled = False
        Button13.Enabled = False
        Button9.Enabled = True
        Button10.Enabled = True
        Button11.Enabled = True
        Button9.Refresh()
        Button10.Refresh()
        Button11.Refresh()
        Button12.Refresh()
        Button13.Refresh()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ProgressBar1.PerformStep() 'Increment of one unit
        If mustStop = True Then 'The scan is finished
            Timer1.Interval = 100 'Rapid progress bar
            mustStop = False
        End If
        If ProgressBar1.Value = 100 Then
            Timer1.Stop()
            If backFinished Then
                stopScanGraphicsEffect()
            End If
        End If
    End Sub


    Private Sub BackgroundWorker2_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        'Check if the scan is terminated
        backFinished = False
        Do While getScanFinished() = False
            Thread.Sleep(1500) '2500
        Loop
        If getScanFinished() = True Then
            mustStop = True
            backFinished = True 'If the progress bar finished his count first of clamscan process, the graphic effect not end.
        End If
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        If Button12.Text = "Пауза" Then
            pauseScan()
            Button12.Text = "Возобновить"
            Button12.Enabled = True
            Button13.Enabled = False
            ProgressBar1.UseWaitCursor = False
            Button12.Refresh()
            Button13.Refresh()
            Timer1.Stop()
        Else
            resumeScan()
            Button12.Text = "Пауза"
            Button12.Enabled = True
            Button13.Enabled = True
            ProgressBar1.UseWaitCursor = True
            Button12.Refresh()
            Button13.Refresh()
            Timer1.Start()
        End If
    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        'Break the scan
        breakScan()
        If Timer1.Enabled = True Then
            Timer1.Stop()
        Else
            MsgBox("Возможно, проверка завершена, но индикатор прогресса все еще активен.", MsgBoxStyle.Critical, "Ошибка")
        End If
        stopScanGraphicsEffect()
    End Sub

    Private Sub FolderBrowserDialog1_HelpRequest(sender As Object, e As EventArgs) Handles FolderBrowserDialog1.HelpRequest
        MsgBox("Выберите путь с помощью мыши и нажмите кнопку «ОК».", MsgBoxStyle.Information, "Руководство")
    End Sub

    Private Sub CheckBox13_Click(sender As Object, e As EventArgs) Handles CheckBox13.Click
        shutFileSet(CheckBox13.Checked)
    End Sub
    'End Analysis panel
    'Wifi protection panel
    Private Sub Panel4_VisibleChanged(sender As Object, e As EventArgs) Handles Panel4.VisibleChanged
        If Panel4.Visible = True Then
            Dim tempVet() As String
            tempVet = guiOptionGetter(RichTextBox1, RichTextBox2)
            If tempVet(0).Contains("on") Then
                wifi = True
                CheckBox1.CheckState = CheckState.Checked
            Else
                wifi = False
                CheckBox1.CheckState = CheckState.Unchecked
            End If
            If tempVet(1).Contains("on") Then
                usb = True
            Else
                usb = False
            End If
        End If
    End Sub

    Private Sub CheckBox1_Click(sender As Object, e As EventArgs) Handles CheckBox1.Click
        'User do click to change checkbox state, the new state is saved on the guiSettings file.
        Dim tempBool1 As String
        Dim tempBool2 As String
        If wifi = True Then
            'User has done a click so the state is changed!
            wifi = False
            tempBool1 = "wi-fi=off"
            Label4.Text = "Не активно"
            Label4.Location = New Point(332, 103)
        Else
            'wifi was set to false
            wifi = True
            tempBool1 = "wi-fi=on"
            Label4.Text = "Активно"
            Label4.Location = New Point(348, 103)
        End If
        If usb = True Then
            tempBool2 = "usb=on"
        Else
            tempBool2 = "usb=off"
        End If
        guiOptionSetter(tempBool1, tempBool2, lastScan)
    End Sub
    'End wifi protection panel
    'Usb panel
    Private Sub Panel5_VisibleChanged(sender As Object, e As EventArgs) Handles Panel5.VisibleChanged
        If Panel5.Visible = True Then
            Dim tempVet() As String
            tempVet = guiOptionGetter(RichTextBox1, RichTextBox2)
            If tempVet(0).Contains("on") Then
                wifi = True
            Else
                wifi = False
            End If
            If tempVet(1).Contains("on") Then
                usb = True
                CheckBox2.CheckState = CheckState.Checked
            Else
                usb = False
                CheckBox2.CheckState = CheckState.Unchecked
            End If
        End If
    End Sub

    Private Sub CheckBox2_Click(sender As Object, e As EventArgs) Handles CheckBox2.Click
        'User do click to change checkbox state, the new state is saved on the guiSettings file.
        Dim tempBool1 As String
        Dim tempBool2 As String
        If usb = True Then
            'User has done a click so the state is changed!
            usb = False
            tempBool1 = "usb=off"
            Label5.Text = "Не активно"
            Label5.Location = New Point(595, 103)
        Else
            'usb was set to false
            usb = True
            tempBool1 = "usb=on"
            Label5.Text = "Активно"
            Label5.Location = New Point(610, 103)
        End If
        If wifi = True Then
            tempBool2 = "wi-fi=on"
        Else
            tempBool2 = "wi-fi=off"
        End If
        guiOptionSetter(tempBool2, tempBool1, lastScan)
    End Sub
    'End usb panel
    'Settings panel
    Private Sub Button24_Click(sender As Object, e As EventArgs) Handles Button24.Click
        'Apply button for settings panel
        Select Case ListBox1.SelectedIndex
            Case 0
                'Upgrades
                MsgBox("Запланированные задачи выполнены!", MsgBoxStyle.Information, "Информация")
            Case 1
                'Scheduled scan
                If Date.Compare(DateTimePicker1.Value, System.DateTime.Now) > 0 Then
                    setDateSchedScan(DateTimePicker1.Value.Day, DateTimePicker1.Value.Month, DateTimePicker1.Value.Year, DateTimePicker2.Value.Hour, DateTimePicker2.Value.Minute)
                End If
            Case 2
                'Real time protection
                realTimeSet(CheckBox12.Checked)
                If realTimeGet() = False Then
                    PictureBox2.BackgroundImage = My.Resources.no
                    Label2.Text = "Служба ClamAV не активна."
                    stopClamService()
                Else
                    PictureBox2.BackgroundImage = My.Resources.ok
                    Label2.Text = "Служба ClamAV активна."
                    startClamService()
                End If
            Case 3
                'Parental control
                setParentalControlFiles(CheckBox14.CheckState, CheckBox15.CheckState, CheckBox16.CheckState)
            Case 4
                'Excluded paths
                Dim listPaths = New ArrayList()
                Try
                    If ListBox2.Items.Contains("Каталог не указан!") = False Then
                        For i As Int16 = 0 To (ListBox2.Items.Count - 1)
                            listPaths.Add(ListBox2.Items.Item(i).ToString)
                        Next
                        fileExSetter(listPaths.ToArray())
                    Else
                        removeExFile()
                    End If
                Catch ex As Exception
                    MsgBox("Извините, произошла ошибка!", MsgBoxStyle.Critical, "Ошибка")
                End Try
            Case 5
                'Other settings
                Dim vectOptions(9) As String
                If CheckBox3.CheckState = CheckState.Checked Then
                    vectOptions(0) = "-i"
                Else
                    vectOptions(0) = ""
                End If
                If CheckBox4.CheckState = CheckState.Checked Then
                    vectOptions(1) = "-r"
                Else
                    vectOptions(1) = ""
                End If
                If CheckBox5.CheckState = CheckState.Checked Then
                    vectOptions(2) = "-leaveTemps=yes"
                Else
                    vectOptions(2) = "-leaveTemps=no"
                End If
                If CheckBox6.CheckState = CheckState.Checked Then
                    vectOptions(3) = "-z"
                Else
                    vectOptions(3) = ""
                End If
                If CheckBox7.CheckState = CheckState.Checked Then
                    vectOptions(4) = "--cross-fs=yes"
                Else
                    vectOptions(4) = "--cross-fs=no"
                End If
                If CheckBox8.CheckState = CheckState.Checked Then
                    vectOptions(5) = "--remove=yes"
                Else
                    vectOptions(5) = "--remove=no"
                End If
                If CheckBox9.CheckState = CheckState.Checked Then
                    vectOptions(6) = "-move=" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\clamQuarantena"
                Else
                    vectOptions(6) = ""
                End If
                If CheckBox10.CheckState = CheckState.Checked Then
                    vectOptions(7) = "--detect-pua=yes"
                Else
                    vectOptions(7) = "--detect-pua=no"
                End If
                If CheckBox11.CheckState = CheckState.Checked Then
                    vectOptions(8) = "--detect-broken=yes"
                Else
                    vectOptions(8) = "--detect-broken=no"
                End If
                'The empty space in array is leave for detect structured option. (now removed)
                optionSetter(vectOptions)
            Case Else
                MsgBox("Невозможно завершить эту операцию!", MsgBoxStyle.Exclamation, "Предупреждение")
        End Select
    End Sub

    Private Sub Panel8_VisibleChanged(sender As Object, e As EventArgs) Handles Panel8.VisibleChanged
        If Panel8.Visible = True Then
            Dim vectOption(9) As String
            vectOption = optionGetter()
            For i As Int16 = 0 To 8
                If vectOption(i) IsNot "" And vectOption(i).Contains("=no") = False Then
                    Select Case (i + 3)
                        Case 3
                            CheckBox3.CheckState = CheckState.Checked
                        Case 4
                            CheckBox4.CheckState = CheckState.Checked
                        Case 5
                            CheckBox5.CheckState = CheckState.Checked
                        Case 6
                            CheckBox6.CheckState = CheckState.Checked
                        Case 7
                            CheckBox7.CheckState = CheckState.Checked
                        Case 8
                            CheckBox8.CheckState = CheckState.Checked
                        Case 9
                            CheckBox9.CheckState = CheckState.Checked
                        Case 10
                            CheckBox10.CheckState = CheckState.Checked
                        Case 11
                            CheckBox11.CheckState = CheckState.Checked
                    End Select
                Else
                    Select Case (i + 3)
                        Case 3
                            CheckBox3.CheckState = CheckState.Unchecked
                        Case 4
                            CheckBox4.CheckState = CheckState.Unchecked
                        Case 5
                            CheckBox5.CheckState = CheckState.Unchecked
                        Case 6
                            CheckBox6.CheckState = CheckState.Unchecked
                        Case 7
                            CheckBox7.CheckState = CheckState.Unchecked
                        Case 8
                            CheckBox8.CheckState = CheckState.Unchecked
                        Case 9
                            CheckBox9.CheckState = CheckState.Unchecked
                        Case 10
                            CheckBox10.CheckState = CheckState.Unchecked
                        Case 11
                            CheckBox11.CheckState = CheckState.Unchecked
                    End Select
                End If
            Next
        End If
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Select Case ListBox1.SelectedIndex
            Case 0
                Panel10.Location = New Point(226, 80)
                Panel10.Height = 256
                Panel10.Width = 475
                Panel9.Visible = False
                Panel11.Visible = False
                Panel12.Visible = False
                Panel13.Visible = False
                Panel14.Visible = False
                Panel10.Visible = True
            Case 1
                Panel11.Location = New Point(226, 80)
                Panel11.Height = 256
                Panel11.Width = 475
                Panel9.Visible = False
                Panel10.Visible = False
                Panel12.Visible = False
                Panel13.Visible = False
                Panel14.Visible = False
                Panel11.Visible = True
            Case 2
                Panel12.Location = New Point(226, 80)
                Panel12.Height = 256
                Panel12.Width = 475
                Panel9.Visible = False
                Panel10.Visible = False
                Panel11.Visible = False
                Panel13.Visible = False
                Panel14.Visible = False
                Panel12.Visible = True
            Case 3
                Panel14.Location = New Point(226, 80)
                Panel14.Height = 256
                Panel14.Width = 475
                Panel9.Visible = False
                Panel10.Visible = False
                Panel11.Visible = False
                Panel12.Visible = False
                Panel13.Visible = False
                Panel14.Visible = True
            Case 4
                Panel13.Location = New Point(226, 80)
                Panel13.Height = 256
                Panel13.Width = 475
                Panel9.Visible = False
                Panel10.Visible = False
                Panel11.Visible = False
                Panel12.Visible = False
                Panel14.Visible = False
                Panel13.Visible = True
            Case 5
                Panel9.Location = New Point(226, 80)
                Panel9.Height = 256
                Panel9.Width = 475
                Panel10.Visible = False
                Panel11.Visible = False
                Panel12.Visible = False
                Panel13.Visible = False
                Panel14.Visible = False
                Panel9.Visible = True
            Case Else
                MsgBox("Невозможно завершить эту операцию!", MsgBoxStyle.Exclamation, "Предупреждение")
        End Select
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        If CheckBox3.Checked = True Then
            CheckBox3.Text = "ВКЛ"
            CheckBox3.ForeColor = Color.ForestGreen
            CheckBox3.RightToLeft = RightToLeft.Yes
        Else
            CheckBox3.Text = "ВЫКЛ"
            CheckBox3.ForeColor = Color.Red
            CheckBox3.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        If CheckBox4.Checked = True Then
            CheckBox4.Text = "ВКЛ"
            CheckBox4.ForeColor = Color.ForestGreen
            CheckBox4.RightToLeft = RightToLeft.Yes
        Else
            CheckBox4.Text = "ВЫКЛ"
            CheckBox4.ForeColor = Color.Red
            CheckBox4.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If CheckBox5.Checked = True Then
            CheckBox5.Text = "ВКЛ"
            CheckBox5.ForeColor = Color.ForestGreen
            CheckBox5.RightToLeft = RightToLeft.Yes
        Else
            CheckBox5.Text = "ВЫКЛ"
            CheckBox5.ForeColor = Color.Red
            CheckBox5.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If CheckBox6.Checked = True Then
            CheckBox6.Text = "ВКЛ"
            CheckBox6.ForeColor = Color.ForestGreen
            CheckBox6.RightToLeft = RightToLeft.Yes
        Else
            CheckBox6.Text = "ВЫКЛ"
            CheckBox6.ForeColor = Color.Red
            CheckBox6.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        If CheckBox7.Checked = True Then
            CheckBox7.Text = "ВКЛ"
            CheckBox7.ForeColor = Color.ForestGreen
            CheckBox7.RightToLeft = RightToLeft.Yes
        Else
            CheckBox7.Text = "ВЫКЛ"
            CheckBox7.ForeColor = Color.Red
            CheckBox7.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox8_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox8.CheckedChanged
        If CheckBox8.Checked = True Then
            CheckBox8.Text = "ВКЛ"
            CheckBox8.ForeColor = Color.ForestGreen
            CheckBox8.RightToLeft = RightToLeft.Yes
        Else
            CheckBox8.Text = "ВЫКЛ"
            CheckBox8.ForeColor = Color.Red
            CheckBox8.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox9.CheckedChanged
        If CheckBox9.Checked = True Then
            CheckBox9.Text = "ВКЛ"
            CheckBox9.ForeColor = Color.ForestGreen
            CheckBox9.RightToLeft = RightToLeft.Yes
        Else
            CheckBox9.Text = "ВЫКЛ"
            CheckBox9.ForeColor = Color.Red
            CheckBox9.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox10_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox10.CheckedChanged
        If CheckBox10.Checked = True Then
            CheckBox10.Text = "ВКЛ"
            CheckBox10.ForeColor = Color.ForestGreen
            CheckBox10.RightToLeft = RightToLeft.Yes
        Else
            CheckBox10.Text = "ВЫКЛ"
            CheckBox10.ForeColor = Color.Red
            CheckBox10.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox11.CheckedChanged
        If CheckBox11.Checked = True Then
            CheckBox11.Text = "ВКЛ"
            CheckBox11.ForeColor = Color.ForestGreen
            CheckBox11.RightToLeft = RightToLeft.Yes
        Else
            CheckBox11.Text = "ВЫКЛ"
            CheckBox11.ForeColor = Color.Red
            CheckBox11.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox12_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox12.CheckedChanged
        If CheckBox12.Checked = True Then
            CheckBox12.Text = "ВКЛ"
            CheckBox12.ForeColor = Color.ForestGreen
            CheckBox12.RightToLeft = RightToLeft.Yes
        Else
            CheckBox12.Text = "ВЫКЛ"
            CheckBox12.ForeColor = Color.Red
            CheckBox12.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox14_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox14.CheckedChanged
        If CheckBox14.Checked = True Then
            CheckBox14.Text = "ВКЛ"
            CheckBox14.ForeColor = Color.ForestGreen
            CheckBox14.RightToLeft = RightToLeft.Yes
            CheckBox15.Text = "ВКЛ"
            CheckBox15.ForeColor = Color.ForestGreen
            CheckBox15.RightToLeft = RightToLeft.Yes
            CheckBox16.Text = "ВКЛ"
            CheckBox16.ForeColor = Color.ForestGreen
            CheckBox16.RightToLeft = RightToLeft.Yes
        Else
            CheckBox14.Text = "ВЫКЛ"
            CheckBox14.ForeColor = Color.Red
            CheckBox14.RightToLeft = RightToLeft.No
        End If
    End Sub

    Private Sub CheckBox15_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox15.CheckedChanged
        If CheckBox15.Checked = True Then
            CheckBox15.Text = "ВКЛ"
            CheckBox15.ForeColor = Color.ForestGreen
            CheckBox15.RightToLeft = RightToLeft.Yes
            CheckBox16.Text = "ВКЛ"
            CheckBox16.ForeColor = Color.ForestGreen
            CheckBox16.RightToLeft = RightToLeft.Yes
        Else
            If CheckBox14.Checked = False Then
                CheckBox15.Text = "ВЫКЛ"
                CheckBox15.ForeColor = Color.Red
                CheckBox15.RightToLeft = RightToLeft.No
            End If
        End If
    End Sub

    Private Sub CheckBox16_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox16.CheckedChanged
        If CheckBox16.Checked = True Then
            CheckBox16.Text = "ВКЛ"
            CheckBox16.ForeColor = Color.ForestGreen
            CheckBox16.RightToLeft = RightToLeft.Yes
        Else
            If CheckBox15.Checked = False Then
                CheckBox16.Text = "ВЫКЛ"
                CheckBox16.ForeColor = Color.Red
                CheckBox16.RightToLeft = RightToLeft.No
            End If
        End If
    End Sub

    Private Sub CheckBox14_Click(sender As Object, e As EventArgs) Handles CheckBox14.Click
        If CheckBox14.Checked = True Then
            CheckBox15.CheckState = CheckState.Checked
            CheckBox16.CheckState = CheckState.Checked
        End If
    End Sub

    Private Sub CheckBox15_Click(sender As Object, e As EventArgs) Handles CheckBox15.Click
        If CheckBox14.Checked = True Then
            CheckBox15.CheckState = CheckState.Checked
            CheckBox16.CheckState = CheckState.Checked
        End If
        If CheckBox15.Checked = True Then
            CheckBox16.CheckState = CheckState.Checked
        End If
    End Sub

    Private Sub CheckBox16_Click(sender As Object, e As EventArgs) Handles CheckBox16.Click
        If CheckBox15.Checked = True Then
            CheckBox16.CheckState = CheckState.Checked
        End If
    End Sub

    Private Sub Button27_Click(sender As Object, e As EventArgs) Handles Button27.Click
        'Insert a exluded path in the list
        Dim notScanThis As String
        If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            notScanThis = FolderBrowserDialog1.SelectedPath
            If Not notScanThis = "" And Not notScanThis = " " Then
                If ListBox2.Items.Contains("Не указан каталог!") = True Then
                    ListBox2.Items.Remove("Не указан каталог!")
                End If
                ListBox2.Items.Add(notScanThis)
            End If
        End If
    End Sub

    Private Sub Button28_Click(sender As Object, e As EventArgs) Handles Button28.Click
        'Remove a exclued path from the list
        Try
            ListBox2.Items.RemoveAt(ListBox2.SelectedIndex)
            If ListBox2.Items.Count = 0 Then
                ListBox2.Items.Add("Не указан каталог!")
            End If
        Catch ex As Exception
            MsgBox("Нечего удалять!", MsgBoxStyle.Exclamation, "Предупреждение")
        End Try
    End Sub

    Private Sub Panel13_VisibleChanged(sender As Object, e As EventArgs) Handles Panel13.VisibleChanged
        If Panel13.Visible = True Then
            Dim exludedPaths() As String
            If ListBox2.Items.Count = 0 Then
                ListBox2.Items.Add("Не указан каталог!")
            End If
            exludedPaths = fileExGetter()
            If exludedPaths(0) IsNot "" Then
                If ListBox2.Items.Contains("Не указан каталог!") = True Then
                    ListBox2.Items.Remove("Не указан каталог!")
                End If
                For Each Val As String In exludedPaths
                    If Val IsNot Nothing Then
                        ListBox2.Items.Add(Val)
                    End If
                Next
            End If
        Else
            ListBox2.Items.Clear()
        End If
    End Sub


    Private Sub ListBox2_DragEnter(sender As Object, e As DragEventArgs) Handles ListBox2.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub ListBox2_DragDrop(sender As Object, e As DragEventArgs) Handles ListBox2.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim temp As String() = e.Data.GetData(DataFormats.FileDrop)
            If ListBox2.Items.Contains("Не указан каталог!") = True Then
                ListBox2.Items.Remove("Не указан каталог!")
            End If
            For Each Val As String In temp
                If Val IsNot Nothing Then
                    ListBox2.Items.Add(Val)
                End If
            Next
        End If
    End Sub

    Private Sub Panel12_VisibleChanged(sender As Object, e As EventArgs) Handles Panel12.VisibleChanged
        If Panel12.Visible = True Then
            If realTimeGet() = True Then
                CheckBox12.CheckState = CheckState.Checked
            Else
                CheckBox12.CheckState = CheckState.Unchecked
            End If
        End If
    End Sub

    Private Sub Panel11_VisibleChanged(sender As Object, e As EventArgs) Handles Panel11.VisibleChanged
        Dim infDate(5) As Int32
        infDate = getDateSchedScan()
        If infDate IsNot Nothing Then
            DateTimePicker1.Value = New Date(infDate(2), infDate(1), infDate(0)) 'year, month, day
            DateTimePicker2.Value = New Date(infDate(2), infDate(1), infDate(0), infDate(3), infDate(4), 0) '3 and 4 are hours and minutes.
        End If
    End Sub

    Private Sub Button25_Click(sender As Object, e As EventArgs) Handles Button25.Click
        'Upgrade virus db button
        Button25.Enabled = False
        startFreshClam()
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub Button26_Click(sender As Object, e As EventArgs) Handles Button26.Click
        MsgBox("Функция пока недоступна!", MsgBoxStyle.Information, "Информация")
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim stat As Boolean = True
        While stat
            If upgradeInAct = False Then
                Button25.Enabled = True
                stat = False
            End If
        End While
    End Sub
    'End settings panel
    'Secure delete and encryption panel
    Private Sub Button19_Click(sender As Object, e As EventArgs) Handles Button19.Click
        'Encryption
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            encryptFile(OpenFileDialog1)
        End If
    End Sub

    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click
        'Delete
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            secureDelete(OpenFileDialog1)
        End If
    End Sub

    Private Sub Button29_Click(sender As Object, e As EventArgs) Handles Button29.Click
        'Decryption
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            decryptFile(OpenFileDialog1)
        End If
    End Sub
    'End secure delete and encryption panel

    'Report panel
    Private Sub Panel7_VisibleChanged(sender As Object, e As EventArgs) Handles Panel7.VisibleChanged
        If Panel7.Visible = True Then
            'RichTextBox3.Enabled = False
            Try
                Dim fileReader = My.Computer.FileSystem.OpenTextFileReader("./ClamAV/report.rtf")
                Dim striConv As String
                Dim striConv2 As String
                RichTextBox3.Clear()
                striConv = fileReader.ReadToEnd()
                striConv2 = striConv.Replace("\", "\\")
                striConv2 = striConv2.Replace(vbCrLf, " \par ")
                '\highlight1 OTHER\highlight0" is for false positive unwanted application or empty files TO BE CONTINUED..
                striConv2 = striConv2.Replace(" Empty file", " \highlight1 Empty file\highlight0")
                striConv2 = striConv2.Replace(" FOUND", " \highlight2 FOUND\highlight0")
                striConv2 = striConv2.Replace(" OK", " \highlight3 OK\highlight0\")
                RichTextBox3.Rtf = "{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1040{\fonttbl{\f0\fnil\fcharset0 System;}}{\colortbl ;\red255\green255\blue0;\red255\green0\blue0;\red0\green255\blue0;}{\*\generator Riched20 10.0.14393}\viewkind4\uc1\pard\sl240\slmult1\b\f0\fs24\lang16 " & striConv2 & "}"
                fileReader.Close()
            Catch ex As Exception
                RichTextBox3.Clear()
                RichTextBox3.Rtf = "{\rtf1\ansi\ansicpg1252\deff0\nouicompat{\fonttbl{\f0\fnil\fcharset0 System;}}{\*\generator Riched20 10.0.14393}\viewkind4\uc1\pard\sa200\sl240\slmult1\b\f0\fs24\lang16 No report file found! }"
            End Try
        End If
    End Sub

    Private Sub Button22_Click(sender As Object, e As EventArgs) Handles Button22.Click
        'Precedent report file open button
        If OpenFileDialog2.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim striConv As String
            Dim striConv2 As String
            Try
                Dim fird = My.Computer.FileSystem.OpenTextFileReader(OpenFileDialog2.FileName)
                RichTextBox3.Clear()
                striConv = fird.ReadToEnd()
                striConv2 = striConv.Replace("\", "\\")
                striConv2 = striConv2.Replace(vbCrLf, " \par ")
                '\highlight1 OTHER\highlight0" is for false positive unwanted application or empty files TO BE CONTINUED..
                striConv2 = striConv2.Replace(" Empty file", " \highlight1 Empty file\highlight0")
                striConv2 = striConv2.Replace(" FOUND", " \highlight2 FOUND\highlight0")
                striConv2 = striConv2.Replace(" OK", " \highlight3 OK\highlight0\")
                RichTextBox3.Rtf = "{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1040{\fonttbl{\f0\fnil\fcharset0 System;}}{\colortbl ;\red255\green255\blue0;\red255\green0\blue0;\red0\green255\blue0;}{\*\generator Riched20 10.0.14393}\viewkind4\uc1\pard\sl240\slmult1\b\f0\fs24\lang16 " & striConv2 & "}"
                fird.Close()
            Catch ex As Exception
                RichTextBox3.Clear()
                RichTextBox3.Rtf = "{\rtf1\ansi\ansicpg1252\deff0\nouicompat{\fonttbl{\f0\fnil\fcharset0 System;}}{\*\generator Riched20 10.0.14393}\viewkind4\uc1\pard\sa200\sl240\slmult1\b\f0\fs24\lang16 An error has occured, any report file available! }"
            End Try
        End If
    End Sub
    'End Report panel

    'USB panel
    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        'NOT VERIFIED
        'Protect a usb wiht an autorun file in read-only mode that do not execute nothing so not is possible use autorun to make something
        Try
            If FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                Dim tmp As String = FolderBrowserDialog1.SelectedPath()
                If Drive_Status(tmp) Then
                    'Check if the selected device is the main disk drive
                    If Environment.SystemDirectory.ToString().Chars(0) & ":\" <> tmp Then
                        Dim file = My.Computer.FileSystem.OpenTextFileWriter(tmp & "autorun.inf", False)
                        file.WriteLine("[autorun]")
                        file.Close()
                        System.IO.File.SetAttributes(tmp & "autorun.inf", IO.FileAttributes.ReadOnly)
                        System.IO.File.SetAttributes(tmp & "autorun.inf", IO.FileAttributes.Hidden)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.ReadData, AccessControlType.Allow)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.WriteData, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.Write, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.WriteAttributes, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.WriteExtendedAttributes, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.Delete, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.AppendData, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.ReadPermissions, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.ReadAttributes, AccessControlType.Deny)
                        AddFileSecurity(tmp & "autorun.inf", Environment.UserName, FileSystemRights.FullControl, AccessControlType.Deny)
                        MsgBox("Операция завершена!", MsgBoxStyle.Information, "Информация")
                    Else
                        MsgBox("Это устройство является основным разделом ОС!", MsgBoxStyle.Exclamation, "Ошибка")
                    End If
                Else
                    MsgBox("Выбранный путь не является поддерживаемым устройством!", MsgBoxStyle.Exclamation, "Ошибка")
                End If
            End If
        Catch ex As Exception
            MsgBox("Произошла ошибка!", MsgBoxStyle.Exclamation, "Ошибка")
        End Try
    End Sub

    Private Sub AddFileSecurity(ByVal fileName As String, ByVal account As String, ByVal rights As FileSystemRights, ByVal controlType As AccessControlType)
        ' Get a FileSecurity object that represents the 
        ' current security settings.
        Dim fSecurity As FileSecurity = System.IO.File.GetAccessControl(fileName)
        ' Add the FileSystemAccessRule to the security settings. 
        Dim accessRule As FileSystemAccessRule = New FileSystemAccessRule(account, rights, controlType)
        fSecurity.AddAccessRule(accessRule)
        ' Set the new access settings.
        System.IO.File.SetAccessControl(fileName, fSecurity)
    End Sub

    'End USB panel
    'Parental control panel
    Private Sub Panel14_VisibleChanged(sender As Object, e As EventArgs) Handles Panel14.VisibleChanged
        Dim prot As String
        prot = getParentalControlFiles()
        If prot = "H=T" Then
            CheckBox14.CheckState = CheckState.Checked
            CheckBox15.CheckState = CheckState.Checked
            CheckBox16.CheckState = CheckState.Checked
        Else
            CheckBox14.CheckState = CheckState.Unchecked
        End If
        If prot = "N=T" Then
            CheckBox15.CheckState = CheckState.Checked
            CheckBox16.CheckState = CheckState.Checked
        ElseIf prot <> "H=T" Then
            CheckBox15.CheckState = CheckState.Unchecked
        End If
        If prot = "L=T" Then
            CheckBox16.CheckState = CheckState.Checked
        ElseIf prot = "L=F" Or prot = "" Or prot = Nothing Then
            CheckBox16.CheckState = CheckState.Unchecked
        End If
    End Sub
    'End parental control panel
    'Notify icon
    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Show()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Dispose()
        Me.Close()
    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As CancelEventArgs) Handles OpenFileDialog1.FileOk

    End Sub

    Private Sub ContextMenuStrip1_Opening(sender As Object, e As CancelEventArgs) Handles ContextMenuStrip1.Opening

    End Sub

    Private Sub ContributorsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ContributorsToolStripMenuItem.Click

    End Sub
    'End notify icon
End Class

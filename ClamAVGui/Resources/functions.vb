Imports System.Net.NetworkInformation
Imports System.ServiceProcess
Imports System.Threading

Module functions
    Private pathToScan As String
    Private stopScanning As Boolean
    Private pauseScanning As Boolean
    Private shutDevice As Boolean
    Private Const unitLetter As Char = ChrW(65) ' &H41 'A:
    Private toUpgrade As Thread
    Private processGeneric As Process 'Used for freshclam
    Private prToResume As Process 'Used to restart a scan when it is in pause
    Private settingsVet(9) As String
    Private guiSettings(3) As String
    Private exclusePaths() As String = {""}
    Public upgradeInAct As Boolean = False
    Private stopGraphic As Boolean
    'In installer it's to insert a code to create a registry key that allow to do a custum scan with drop-down menu of right mouse click, so in clamav service could be inserted a construct to start clamscan process with passed path.
    'Installer should make clamav-suite default antivirus on windows, so windows must report "ClamAV protect this PC"
    'Thing to do: parental control, firewall, false positive detection (Change report file, so the false positive will be selected in yellow).
    'Build function to start and stop the service
    Public Sub startClamService()
        Try
            Dim service As ServiceController = New ServiceController("ClamAVService")
            If ((service.Status.Equals(ServiceControllerStatus.Stopped)) Or (service.Status.Equals(ServiceControllerStatus.StopPending))) Then
                service.Start()
            End If
        Catch ex As Exception
            Shell("NET START ClamAVService")
        End Try
    End Sub

    Public Sub stopClamService()
        Try
            Dim service As ServiceController = New ServiceController("ClamAVService")
            service.Stop()
        Catch ex As Exception
            Shell("NET STOP ClamAVService")
        End Try
    End Sub

    Public Sub setParentalControlFiles(high As CheckState, normal As CheckState, low As CheckState)
        'H=T is high level true, N=T is normal level true, L=F is low level false so parental control is off
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/parControl.bin", System.IO.FileMode.Create))
                If high = CheckState.Checked Then
                    writer.Write("H=T")
                Else
                    If normal = CheckState.Checked Then
                        writer.Write("N=T")
                    Else
                        If low = CheckState.Checked Then
                            writer.Write("L=T")
                        ElseIf low = CheckState.Unchecked Then
                            writer.Write("L=F")
                        End If
                    End If
                End If
                writer.Close()
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function getParentalControlFiles() As String
        'Return "" or Nothing if the file is empty or not exist else return a string.
        Dim status As String = ""
        If (System.IO.File.Exists("./ClamAV/parControl.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/parControl.bin", System.IO.FileMode.Open))
                Dim stopFR As Boolean = True
                While stopFR
                    status = reader.ReadString
                    If reader.PeekChar = -1 Then
                        stopFR = False
                    End If
                End While
                reader.Close()
            End Using
        Else
            status = Nothing
        End If
        Return status
    End Function

    Public Function getScanFinished() As Boolean
        'Used to stop the scan effect when the process end
        Return stopGraphic
    End Function

    Public Sub shutFileSet(ByRef shutme As Boolean)
        'Write on file shut if the device must be turn off after a scan.
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/shut.bin", System.IO.FileMode.Create))
                writer.Write(shutme)
                writer.Close()
                shutDevice = shutme
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function shutFileGet() As Boolean
        'Read from file shut if after a scansion the device must be turn off.
        Dim toRet As Boolean
        If (System.IO.File.Exists("./ClamAV/shut.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/shut.bin", System.IO.FileMode.Open))
                Dim stopFR As Boolean = True
                While stopFR
                    toRet = reader.ReadBoolean
                    If reader.PeekChar = -1 Then
                        stopFR = False
                    End If
                End While
                reader.Close()
            End Using
        Else
            toRet = False
        End If
        shutDevice = toRet
        Return toRet
    End Function

    Public Sub startFreshClam()
        Try
            Dim sfpr As New Process()
            sfpr.StartInfo.FileName = Environment.CurrentDirectory + "\ClamAV\freshclam.exe"
            sfpr.StartInfo.UseShellExecute = False
            sfpr.StartInfo.RedirectStandardInput = True
            sfpr.StartInfo.RedirectStandardOutput = True
            sfpr.Start()
            processGeneric = sfpr
            upgradeInAct = True
            MsgBox("Фоновый процесс был запущен, когда обновление будет завершено, пользователь будет уведомлен.", MsgBoxStyle.Information, "Информация")
            toUpgrade = New Thread(AddressOf upgradeThread)
            toUpgrade.IsBackground = True
            toUpgrade.Start()
        Catch ex As Exception
            MsgBox("Извините, произошла ошибка, может быть, файл freshclam.exe поврежден или не существует!", MsgBoxStyle.Exclamation, "Предупреждение")
        End Try
    End Sub

    Public Sub setDateSchedScan(ByVal day As Object, ByVal month As Object, ByVal year As Object, ByVal hour As Object, ByVal minute As Object)
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/schedScan.bin", System.IO.FileMode.Create))
                writer.Write(day)
                writer.Write(month)
                writer.Write(year)
                writer.Write(hour)
                writer.Write(minute)
                writer.Close()
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function getDateSchedScan() As Int32()
        Dim customDate(5) As Int32
        If (System.IO.File.Exists("./ClamAV/schedScan.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/schedScan.bin", System.IO.FileMode.Open))
                Dim stopFR As Boolean = True
                While stopFR
                    For i As Byte = 0 To 4
                        customDate(i) = reader.ReadInt32
                    Next
                    If reader.PeekChar = -1 Then
                        stopFR = False
                    End If
                End While
                reader.Close()
            End Using
        Else
            customDate = Nothing
        End If
        Return customDate
    End Function

    Public Sub realTimeSet(ByRef active)
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/realTime.bin", System.IO.FileMode.Create))
                If active IsNot Nothing Then
                    writer.Write(active)
                End If
                writer.Close()
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function realTimeGet() As Boolean
        Dim active = False
        If (System.IO.File.Exists("./ClamAV/realTime.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/realTime.bin", System.IO.FileMode.Open))
                Dim stopFR As Boolean = True
                While stopFR
                    active = reader.ReadBoolean()
                    If reader.PeekChar = -1 Then
                        stopFR = False
                    End If
                End While
                reader.Close()
            End Using
        End If
        Return active
    End Function

    Public Sub fileExSetter(ByVal excluse)
        ReDim exclusePaths(excluse.Length)
        For i As Int16 = 0 To excluse.Length - 1
            exclusePaths(i) = excluse(i).ToString()
        Next
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/exclused.bin", System.IO.FileMode.Create))
                For Each value As String In exclusePaths
                    If value IsNot Nothing Then
                        writer.Write(value)
                    End If
                Next
                writer.Close()
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Sub removeExFile()
        If (System.IO.File.Exists("./ClamAV/exclused.bin")) Then
            Try
                System.IO.File.Delete("./ClamAV/exclused.bin")
                exclusePaths = {""}
            Catch ex As Exception
                MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
            End Try
        Else
            MsgBox("The list is empty!", MsgBoxStyle.Exclamation, "Warning")
        End If
    End Sub

    Public Sub fileExReader()
        Dim listPath = New ArrayList()
        If (System.IO.File.Exists("./ClamAV/exclused.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/exclused.bin", System.IO.FileMode.Open))
                Dim stopFR As Boolean = True
                While stopFR
                    listPath.Add(reader.ReadString())
                    If reader.PeekChar = -1 Then
                        stopFR = False
                    End If
                End While
                Dim objArray = listPath.ToArray()
                ReDim exclusePaths(objArray.Length)
                For i As Int16 = 0 To objArray.Length - 1
                    exclusePaths(i) = objArray(i).ToString()
                Next
                reader.Close()
            End Using
        End If
    End Sub

    Public Function fileExGetter() As String()
        fileExReader()
        Return exclusePaths
    End Function

    Public Sub optionSetter(ByRef optionsArray)
        'This function is called when the user click on apply button in Settings>>other
        'The function save in a binary file the ClamAV settings. (Folder: ./ClamAV/settings.bin)
        'To improve this function must add a CRC to verify the integrity of the settings, so a malware can't edit it without ClamAVGUI knowing. 
        settingsVet = optionsArray
        Try
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./ClamAV/settings.bin", System.IO.FileMode.Create))
                For Each value As String In settingsVet
                    If value IsNot Nothing Then
                        writer.Write(value)
                    End If
                Next
                writer.Close()
            End Using
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function optionGetter() As String()
        optionsReader()
        Return settingsVet
    End Function

    Private Sub optionsReader()
        'This function read ClamAV options in ./ClamAV/settings.bin
        'If settings.bin doesn't exist use the default options.
        'To improve this function it's possible use CRC, so if the file is corrupted ClamAVGUI can use default options.
        If System.IO.File.Exists("./ClamAV/settings.bin") Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./ClamAV/settings.bin", System.IO.FileMode.Open))
                For i As Byte = 0 To 8
                    settingsVet(i) = reader.ReadString()
                Next
            End Using
        Else
            'Default settings
            'ALL SETTINGS: {"-i", "-r", "-leaveTemps=no", "-z", "--cross-fs=yes", "--remove=yes", "-move=QUARANTENADIR", "--detect-pua=no", "--detect-broken=yes"}
            settingsVet = {"-i", "-r", "-leaveTemps=no", "", "--cross-fs=yes", "--remove=yes", "", "--detect-pua=no", "--detect-broken=yes"}
        End If
    End Sub

    Public Sub guiOptionSetter(ByVal val1, ByVal val2, ByVal val3) 'val1=wifi, val2=usb, val3=lastscan
        Try
            Dim tmp1, tmp2 As String
            Using writer As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open("./guiSettings.bin", System.IO.FileMode.Create))
                If TypeOf val1 Is String Then
                    writer.Write(val1)
                    writer.Write(val2)
                    tmp1 = val1.ToString
                    tmp2 = val2.ToString
                Else
                    If val1 = True Then
                        writer.Write("wi-fi=on")
                        tmp1 = "wi-fi=on"
                    Else
                        writer.Write("wi-fi=off")
                        tmp1 = "wi-fi=off"
                    End If
                    If val2 = True Then
                        writer.Write("usb=on")
                        tmp2 = "usb=on"
                    Else
                        writer.Write("usb=off")
                        tmp2 = "usb=off"
                    End If
                End If
                If val3 Is Nothing Then
                    val3 = "Последняя проверка: Никогда"
                End If
                writer.Write(val3)
                writer.Close()
            End Using
            guiSettings = {tmp1, tmp2, val3}
        Catch ex As Exception
            MsgBox("Ошибка при сохранении настроек, убедитесь, что ClamAV запущен под администратором!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Function guiOptionGetter(ByRef ta1, ByRef ta2) As String()
        If (System.IO.File.Exists("./guiSettings.bin")) Then
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./guiSettings.bin", System.IO.FileMode.Open))
                Try
                    For i As Byte = 0 To 2
                        guiSettings(i) = reader.ReadString()
                    Next
                Catch ex As Exception
                    reader.Close()
                    My.Computer.FileSystem.DeleteFile("./guiSettings.bin", Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing)
                    guiSettings = {"wi-fi=off", "usb=off", "Последняя проверка: Никогда"}
                End Try
            End Using
        Else
            'Default settings
            guiSettings = {"wi-fi=off", "usb=off", "Последняя проверка: Никогда"}
        End If
        'Apply settings of wifi and usb
        makeGuiSet(ta1, ta2)
        Return guiSettings
    End Function

    Private Sub makeGuiSet(ByRef textA1, ByRef textA2)
        If guiSettings(0).Contains("on") Then
            'TO BE CONTINUED
            If My.Computer.Network.IsAvailable = False Then
                textA1.Text = "Компьютер не подключен!"
                textA2.Text = "Сетевое устройство не найдено."
            Else
                Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
                For Each adapter As NetworkInterface In adapters
                    Dim properties As IPInterfaceProperties = adapter.GetIPProperties()
                    Dim fisic As PhysicalAddress = adapter.GetPhysicalAddress()
                    Dim fisicByte As Byte() = fisic.GetAddressBytes()
                    Dim macaddress As String = ""
                    For i As Integer = 0 To fisicByte.Length - 1
                        macaddress &= fisicByte(i).ToString("X2")
                        If i <> fisicByte.Length - 1 Then
                            macaddress &= "-"
                        End If
                    Next
                    textA1.Text = adapter.Name & vbTab & (adapter.Description) & vbCrLf
                    textA1.AppendText("MAC-адрес: " & macaddress & vbCrLf)
                    textA1.AppendText(System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString())
                    textA1.AppendText("Всегда обращаться к этому интерфейсу: " & vbCrLf)
                    For Each item As IPAddressInformation In properties.AnycastAddresses
                        textA1.AppendText(item.ToString & vbCrLf)
                    Next
                    textA1.AppendText("DHCP адрес: " & vbCrLf)
                    For Each item As System.Net.IPAddress In properties.DhcpServerAddresses
                        textA1.AppendText(item.ToString & vbCrLf)
                    Next
                    textA1.AppendText("  DNS суффикс................................. :{" & properties.DnsSuffix & "}" & vbCrLf)
                    textA1.AppendText("  DNS включен ............................. : {" & properties.IsDnsEnabled & "}" & vbCrLf)
                    textA1.AppendText("  Динамическая настройка DNS .............. : {" & properties.IsDynamicDnsEnabled & "}" & vbCrLf)
                Next
                textA2.Text = "Обнаруженные устройства: "
            End If
        Else
            If My.Computer.Network.IsAvailable = False Then
                textA1.Text = "Компьютер не подключен!"
                textA2.Text = "Сетевое устройство не найдено."
            Else
                textA1.Text = "Компьютер подключен, информация недоступна."
                textA2.Text = "Сетевое устройство не проверяется!"
            End If
        End If
        If guiSettings(1).Contains("on") Then
            Microsoft.Win32.Registry.CurrentUser.SetValue("\Software\Microsoft\Windows\CurrentVersion\policies\Explorer\NoDriveTypeAutoRun", &H91) 'AutoPlay turned off
        Else
            Microsoft.Win32.Registry.CurrentUser.SetValue("\Software\Microsoft\Windows\CurrentVersion\policies\Explorer\NoDriveTypeAutoRun", &H0) '?AutoPlay turned on value unknow
        End If
    End Sub

    Public Sub fastScan()
        'This function is started as a different thread (code in Form1.vb)
        optionsReader()
        Dim file As System.IO.StreamWriter
        Dim cavpr As New Process()
        Dim myOutput As System.IO.StreamReader
        Dim output As String
        Dim noRececk As Boolean = False
        Dim sameRep As Boolean = False 'With False make a new report file
        stopGraphic = False 'Reset stop graphic
        stopScanning = False
        cavpr.StartInfo.UseShellExecute = False
        cavpr.StartInfo.RedirectStandardOutput = True
        archiveReport()
        For i As Byte = 0 To 4
            If i <> 4 Then
                If i = 0 Then
                    cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
                    cavpr.StartInfo.Arguments = "" & settingsVet(0) & " " & settingsVet(1) & " " & settingsVet(2) & " " + settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & Environment.GetEnvironmentVariable("USERPROFILE") & "\AppData"
                End If
                If i = 1 Then
                    cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
                    cavpr.StartInfo.Arguments = "" & settingsVet(0) & " " & settingsVet(1) & " " & settingsVet(2) & " " & settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & Environment.GetEnvironmentVariable("CSIDL_COMMON_STARTUP")
                End If
                If i = 2 Then
                    cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
                    cavpr.StartInfo.Arguments = "" & settingsVet(0) + " " & settingsVet(1) & " " & settingsVet(2) & " " & settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & Environment.SystemDirectory
                ElseIf i = 3 Then
                    cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
                    cavpr.StartInfo.Arguments = "" & settingsVet(0) + " " & settingsVet(1) & " " & settingsVet(2) & " " & settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & Environment.GetEnvironmentVariable("CSIDL_DEFAULT_STARTMENU")
                End If
                cavpr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                cavpr.StartInfo.CreateNoWindow = True
                stopScanning = False
                Try
                    cavpr.Start()
                    myOutput = cavpr.StandardOutput
                    Do While cavpr.HasExited = False And stopScanning = False 'Wait the end of the scansion
                        Thread.Sleep(3000) 'Delay to reduce throughput of ClamAVGui process
                        If stopScanning = True Then
                            cavpr.Kill()
                        End If
                        If pauseScanning = True Then
                            For Each item As Thread In cavpr.Threads
                                Try
                                    item.Suspend()
                                Catch ex As ThreadStateException
                                    If noRececk = False Then
                                        MsgBox("Невозможно приостановить процесс!", MsgBoxStyle.Exclamation, "Предупреждение")
                                        noRececk = True
                                    End If
                                End Try
                            Next
                            prToResume = cavpr
                        End If
                    Loop
                    If cavpr.HasExited Or stopScanning = True Then
                        output = myOutput.ReadToEnd
                        file = My.Computer.FileSystem.OpenTextFileWriter("./ClamAV/report.rtf", sameRep) 'Default false create a new file
                        sameRep = True 'To append all scan report of a single scan
                        file.WriteLine(output)
                        file.Close()
                    End If
                    If stopScanning = True Then
                        i = 4
                    End If
                Catch w As System.ComponentModel.Win32Exception
                    Console.WriteLine(w.Message)
                    Console.WriteLine(w.ErrorCode.ToString())
                    Console.WriteLine(w.NativeErrorCode.ToString())
                    Console.WriteLine(w.StackTrace)
                    Console.WriteLine(w.Source)
                    Dim e As New Exception()
                    e = w.GetBaseException()
                    MsgBox(e.Message, MsgBoxStyle.Critical, "Ошибка")
                End Try
            End If
        Next
        If shutDevice = True Then
            'shutdown pc after 10 seconds the end of scan
            Process.Start("shutdown", "-s -t 10")
        End If
        stopGraphic = True
    End Sub

    Public Sub fullScan()
        optionsReader()
        Dim file As System.IO.StreamWriter
        Dim unitToPass As String
        Dim myOutput As System.IO.StreamReader
        Dim cavpr As New Process()
        Dim output As String
        Dim unitNum As Int16
        Dim drives(26) As Char
        Dim noRececk As Boolean = False
        Dim sameRep As Boolean = False 'With False make a new report file
        unitToPass = unitLetter
        stopGraphic = False 'Reset stop graphic
        unitNum = 0
        stopScanning = False
        For i As Int16 = 0 To 26
            'Check all units connected
            If (Drive_Status(unitToPass) = True) Then
                drives(i) = unitToPass
                unitNum += 1
            End If
            unitToPass = ChrW(AscW(unitLetter) + (i + 1))
        Next
        archiveReport()
        For j As Int16 = 0 To unitNum
            If Not drives(j) = "" Then
                cavpr.StartInfo.UseShellExecute = False
                cavpr.StartInfo.RedirectStandardOutput = True
                cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
                cavpr.StartInfo.Arguments = "" & settingsVet(0) & " " & settingsVet(1) & " " & settingsVet(2) & " " & settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & drives(j) & ":\"
                cavpr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                cavpr.StartInfo.CreateNoWindow = True
                Try
                    cavpr.Start()
                    myOutput = cavpr.StandardOutput
                    Do While cavpr.HasExited = False And stopScanning = False 'Wait the end of the scan
                        Thread.Sleep(3000)
                        If stopScanning = True Then
                            cavpr.Kill()
                        End If
                        If pauseScanning = True Then
                            For Each item As Thread In cavpr.Threads
                                Try
                                    item.Suspend()
                                Catch ex As ThreadStateException
                                    If noRececk = False Then
                                        MsgBox("Невозможно приостановить процесс!", MsgBoxStyle.Exclamation, "Предупреждение")
                                        noRececk = True
                                    End If
                                End Try
                            Next
                            prToResume = cavpr
                        End If
                    Loop
                    If cavpr.HasExited Or stopScanning = True Then
                        output = myOutput.ReadToEnd
                        file = My.Computer.FileSystem.OpenTextFileWriter("./ClamAV/report.rtf", sameRep) 'Default false create a new file
                        sameRep = True 'To append all scan report of a single scan
                        file.WriteLine(output)
                        file.Close()
                    End If
                    If stopScanning = True Then
                        j = unitNum
                    End If
                Catch w As System.ComponentModel.Win32Exception
                    Console.WriteLine(w.Message)
                    Console.WriteLine(w.ErrorCode.ToString())
                    Console.WriteLine(w.NativeErrorCode.ToString())
                    Console.WriteLine(w.StackTrace)
                    Console.WriteLine(w.Source)
                    Dim e As New Exception()
                    e = w.GetBaseException()
                    MsgBox(e.Message, MsgBoxStyle.Critical, "Ошибка")
                End Try
            End If
        Next
        If shutDevice = True Then
            Process.Start("shutdown", "-s -t 10")
        End If
        stopGraphic = True
    End Sub

    Public Sub customScan(ByVal pathScan As String)
        Dim trd As Thread
        pathToScan = pathScan
        stopScanning = False
        trd = New Thread(AddressOf customScanTH)
        trd.IsBackground = True
        trd.Start()
    End Sub

    Public Sub customScanTH()
        optionsReader()
        Dim file As System.IO.StreamWriter
        Dim cavpr As New Process()
        Dim output As String
        Dim noRececk As Boolean = False
        stopGraphic = False 'Reset stop graphic
        cavpr.StartInfo.UseShellExecute = False
        cavpr.StartInfo.RedirectStandardOutput = True
        cavpr.StartInfo.FileName = Environment.CurrentDirectory & "\ClamAV\clamscan.exe"
        cavpr.StartInfo.Arguments = "" & settingsVet(0) & " " & settingsVet(1) & " " & settingsVet(2) & " " & settingsVet(3) & " " & settingsVet(4) & " " & settingsVet(5) & " " & settingsVet(6) & " " & settingsVet(7) & " " & settingsVet(8) & " " & pathToScan
        cavpr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        cavpr.StartInfo.CreateNoWindow = True
        Try
            cavpr.Start()
            Dim myOutput As System.IO.StreamReader = cavpr.StandardOutput
            Do While cavpr.HasExited = False And stopScanning = False 'Wait the end of the scansion
                Thread.Sleep(3000)
                If stopScanning = True Then
                    cavpr.Kill()
                End If
                If pauseScanning = True Then
                    For Each item As Thread In cavpr.Threads
                        Try
                            item.Suspend()
                        Catch ex As ThreadStateException
                            If noRececk = False Then
                                MsgBox("Невозможно приостановить процесс!", MsgBoxStyle.Exclamation, "Предупреждение")
                                noRececk = True
                            End If
                        End Try
                    Next
                    prToResume = cavpr
                End If
            Loop
            If cavpr.HasExited Or stopScanning = True Then
                stopGraphic = True
                output = myOutput.ReadToEnd
                archiveReport()
                file = My.Computer.FileSystem.OpenTextFileWriter("./ClamAV/report.rtf", False)
                file.WriteLine(output)
                file.Close()
            End If
        Catch w As System.ComponentModel.Win32Exception
            Console.WriteLine(w.Message)
            Console.WriteLine(w.ErrorCode.ToString())
            Console.WriteLine(w.NativeErrorCode.ToString())
            Console.WriteLine(w.StackTrace)
            Console.WriteLine(w.Source)
            Dim e As New Exception()
            e = w.GetBaseException()
            MsgBox(e.Message, MsgBoxStyle.Critical, "Ошибка")
        End Try
        If shutDevice = True Then
            Process.Start("shutdown", "-s -t 10")
        End If
    End Sub

    Private Sub archiveReport()
        'Archive the precedent report file with the time date of the next scan
        Dim nvoPath = Replace(System.DateTime.Now.ToString, ":", "-")
        nvoPath = Replace(nvoPath, "/", "-")
        nvoPath = Replace(nvoPath, " ", "_")
        Try
            My.Computer.FileSystem.CopyFile("./ClamAV/report.rtf", "./ClamAV/Report/precTo_" & nvoPath)
        Catch ex As Exception

        End Try
    End Sub

    Public Function Drive_Status(ByVal strLetter As String) As Boolean
        Dim fsoMain As New System.Object
        Dim unitExist As Boolean
        strLetter = Left$(strLetter, 1) 'Edit check.
        unitExist = False
        If IO.Directory.Exists(strLetter + ":\") Then
            unitExist = True
        End If
        Return unitExist
    End Function

    Public Function GetDiskSpace(ByVal unit As String) As System.UInt64
        Dim cdrive As System.IO.DriveInfo
        cdrive = My.Computer.FileSystem.GetDriveInfo(unit + ":\")
        Return (cdrive.TotalSize - cdrive.AvailableFreeSpace)
    End Function

    Public Sub breakScan()
        stopScanning = True
    End Sub

    Public Sub pauseScan()
        pauseScanning = True
    End Sub

    Public Sub resumeScan()
        pauseScanning = False
        For Each item As Thread In prToResume.Threads
            Try
                item.Resume()
            Catch ex As ThreadStateException
                MsgBox("Невозможно приостановить процесс!", MsgBoxStyle.Exclamation, "Ошибка")
            End Try
        Next
    End Sub

    Public Sub upgradeThread()
        If processGeneric.HasExited = True Then
            MsgBox("Процесс обновления для ClamAV завершен!", MsgBoxStyle.Information, "Информация")
            upgradeInAct = False
        End If
    End Sub

    Public Sub secureDelete(ByRef fileDialog)
        Try
            For Each item As String In fileDialog.FileNames
                If MsgBox("Файл: " & item & " будет навсегда удален с этого жесткого диска. Вы уверены?", MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNo, "Предупреждение") = MsgBoxResult.Yes Then
                    If (System.IO.File.Exists(item)) Then
                        Try
                            Using binw As System.IO.BinaryWriter = New System.IO.BinaryWriter(System.IO.File.Open(item, System.IO.FileMode.Open))
                                Dim pos As Long
                                binw.Seek(0, System.IO.SeekOrigin.Begin)
                                For pos = 0 To (binw.BaseStream.Length - 8) Step 8
                                    binw.Seek(7, System.IO.SeekOrigin.Current)
                                    binw.Write(CType(255, Byte))
                                Next pos
                                binw.Seek(0, System.IO.SeekOrigin.End)
                                For pos = binw.BaseStream.Length To 7 Step -6
                                    binw.Seek(-6, System.IO.SeekOrigin.Current)
                                    binw.Write(CType(254, Byte))
                                    binw.Seek(-1, System.IO.SeekOrigin.Current)
                                Next pos
                                binw.Seek(0, System.IO.SeekOrigin.Begin)
                            End Using
                        Catch ex As Exception
                            MsgBox("Извините, произошла ошибка!", MsgBoxStyle.Critical, "Ошибка")
                        End Try
                    Else
                        MsgBox("Файл не найден!", MsgBoxStyle.Critical, "Ошибка")
                    End If
                    My.Computer.FileSystem.DeleteFile(item, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing)
                End If
            Next
        Catch ex As Exception
            MsgBox("Ошибки времени выполнения!", MsgBoxStyle.Critical, "Ошибка")
        End Try
    End Sub

    Public Sub encryptFile(ByRef fileDialog)
        'NOT VERIFIED (WORK?!)
        For Each item As String In fileDialog.FileNames
            If IO.File.Exists(item) Then
                IO.File.Encrypt(item)
            End If
            MsgBox("Файл: " & item & "зашифрован.", MsgBoxStyle.Information, "Информация")
        Next
    End Sub

    Public Sub decryptFile(ByRef fileDialog)
        'NOT VERIFIED (WORK?!)
        For Each item As String In fileDialog.FileNames
            If IO.File.Exists(item) Then
                IO.File.Decrypt(item)
            End If
            MsgBox("Файл: " & item & "дешифрован.", MsgBoxStyle.Information, "Информация")
        Next
    End Sub
End Module

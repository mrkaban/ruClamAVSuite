Imports System.ServiceProcess

Public Class ClamService

    Private planDate(5) As Int32
    Private dtComp As Date

    Protected Overrides Sub OnStart(ByVal args() As String)
        Timer1.Interval = 10000
        BackgroundWorker1.RunWorkerAsync()
        BackgroundWorker2.RunWorkerAsync()
        Timer1.Start()
        If (System.IO.File.Exists("./guiSettings.bin")) Then
            Dim autoplay As String = ""
            Using reader As System.IO.BinaryReader = New System.IO.BinaryReader(System.IO.File.Open("./guiSettings.bin", System.IO.FileMode.Open))
                Try
                    For i As Byte = 0 To 1
                        autoplay = reader.ReadString()
                    Next
                Catch ex As Exception
                    reader.Close()
                    My.Computer.FileSystem.DeleteFile("./guiSettings.bin", Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing)
                End Try
            End Using
            If autoplay.Contains("on") Then
                'deactive autoplay
                Microsoft.Win32.Registry.CurrentUser.SetValue("\Software\Microsoft\Windows\CurrentVersion\policies\Explorer\NoDriveTypeAutoRun", &H91)
            End If
        End If
    End Sub

    Protected Overrides Sub OnContinue()
        If realTimeGet() = False Then
            OnStop()
        End If
        System.Threading.Thread.Sleep(5000)
        MyBase.OnContinue()
    End Sub

    Protected Overrides Sub OnStop()
        realTimeSet(False)
        Timer1.Stop()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        'Parental control
        'VUOL DIRE CHE SI CONTROLLANO I SITI CHE L'UTENTE VISITA INTERCETTANDO TUTTI GLI INDIRIZZI DNS E REINDIRIZZANDO A UNA PAGINA STATICA CON L'ICONA DI CLAMAV CHE BLOCCA IL SITO WEB
        'Forse va migliorato questo thread facendo in modo che faccia meno carico di lavoro magari usando degli interrupt che avvisano se l'utente ha cambiato le impostazioni di parental control.
        Dim prot As String
        While True
            prot = getParentalControlFiles()
        If prot = "H=T" Then
                'Protezione di alto livello
            End If
        If prot = "N=T" Then
                'Protezione di medio livello
            End If
            If prot = "L=T" Then
                'Protezione di basso livello
            ElseIf prot = "L=F" Or prot = "" Or prot = Nothing Then
                System.Threading.Thread.Sleep(5000) 'Sleep for 5 seconds and after receck prot variable
            End If
        End While
    End Sub


    Private Sub BackgroundWorker2_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        'real time scan
        'AVVIA CLAMSCAN E SCANSIONA SEMPRE ALCUNE DIRECTORY CHE POTREBBERO ESSERE ESPOSTE A VIRUS
        'SI puo anche avviare il servizo clamd
        'NEI BACKGROUND WORKER è necessario un ciclo per ciclare il codice dato che sono come una funzione che viene eseguita da un altro thread
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        planDate = getDateSchedScan()
        If planDate IsNot Nothing Then
            dtComp = New Date(planDate(2), planDate(1), planDate(0), planDate(3), planDate(4), 0) 'year, month, day, hour, minute,second
            If Date.Compare(dtComp, System.DateTime.Now) <= 0 Then
                'start a full and planned scan
                fullScan()
            End If
        End If
    End Sub
End Class

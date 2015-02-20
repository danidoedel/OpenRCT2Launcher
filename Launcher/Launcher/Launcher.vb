﻿Imports System.IO
Imports Microsoft.Win32
Imports Launcher.My.Resources
Imports Launcher.My
Imports HelperLibrary.Classes

Public Class frmLauncher
    Private Async Sub frmLauncher_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Main.Initialize() 'Initialize Main class

        'Check for updates
        CheckForIllegalCrossThreadCalls = False

        Await GameUpdate(False)

        PictureBox1.Image = rollercoaster_tycoon_2_001
        Icon = cat_paw

        'If the OpenRCT2 folder doesn't exist, create it
        If Directory.Exists(Constants.OpenRCT2Folder) = False Then
            Directory.CreateDirectory(Constants.OpenRCT2Folder)
        End If

        'If the programm couldn't find the path look for it in the registry
        If Main.OpenRCT2Config.GamePath = Nothing Then
            Try
                Main.OpenRCT2Config.GamePath = Registry.LocalMachine.OpenSubKey("Software\Infogrames\rollercoaster tycoon 2 setup").GetValue("Path")
                Main.OpenRCT2Config.HasChanged = True
            Catch ex As Exception
                MsgBox(frmLauncher_Load_neverRun)
            End Try
        End If

        If Settings.UserID <> Nothing Then
            'Add code here for Stats etc.
        End If
    End Sub

    Private Async Sub cmdLaunchGame_Click(sender As Object, e As EventArgs) Handles cmdLaunchGame.Click
        If File.Exists(Constants.OpenRCT2Exe) And File.Exists(Constants.OpenRCT2Dll) Then
            Dim launchProcess As New ProcessStartInfo

            'Redirect output if needed
            If Settings.SaveOutput Then
                If Directory.Exists(Path.GetDirectoryName(Settings.OutputPath)) Then
                    launchProcess.RedirectStandardOutput = True
                    launchProcess.RedirectStandardError = True
                    launchProcess.UseShellExecute = False
                End If
            End If

            launchProcess.WorkingDirectory = Constants.OpenRCT2Bin     'OpenRCT2's Executibles will be stored here, so we make this the working dir.
            launchProcess.FileName = Constants.OpenRCT2Exe                  'The EXE of course.

            If Settings.Verbose Then
                launchProcess.Arguments += "--verbose"
            End If

            If Settings.Arguments <> "" Then
                If launchProcess.Arguments <> "" Then 'Add space to arguments (is this necessary?)
                    launchProcess.Arguments += " "
                End If

                launchProcess.Arguments += Settings.Arguments
            End If

            'Save before starting the *.exe to prevent it from failing to load
            If Settings.HasChanged Then
                Settings.HasChanged = False
                Settings.Save()
            End If

            If Main.OpenRCT2Config.HasChanged Then
                Await Main.OpenRCT2Config.SaveINI(Constants.OpenRCT2ConfigFile)
                Main.OpenRCT2Config.HasChanged = False
            End If

            Dim process As Process = process.Start(launchProcess)

            'Start new thread for saving the output of the *.exe
            If Settings.SaveOutput Then
                If Directory.Exists(Path.GetDirectoryName(Settings.OutputPath)) Then
                    Await WriteOutput(process)
                End If
            End If


            'THIS NEEDS TO REMAIN LAST BECAUSE IT HANDLES WHETHER WE NEED TO CLOSE!
            If Settings.UploadTime = True Then
                Visible = False
                tmrUsedForUploadingTime.Enabled = True
            Else
                Close()
            End If


        Else
            MsgBox(frmLauncher_launchGame_RCT2NotFound)

            'Redownload
            Await GameUpdate(True)
        End If
    End Sub

    Private Async Sub cmdUpdate_Click(sender As Object, e As EventArgs) Handles cmdUpdate.Click
        Await GameUpdate(True)
    End Sub

    Private Sub cmdOptions_Click(sender As Object, e As EventArgs) Handles cmdOptions.Click
        frmOptions.ShowDialog()
    End Sub

    Private Sub cmdExtras_Click(sender As Object, e As EventArgs) Handles cmdExtras.Click
        Extras.ShowDialog()
    End Sub

    Private Async Sub frmLauncher_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        'Save changes
        If Settings.HasChanged Then
            Settings.Save()
        End If

        If Main.OpenRCT2Config.HasChanged Then
            Await Main.OpenRCT2Config.SaveINI(Constants.OpenRCT2ConfigFile)
        End If
    End Sub

    Private Async Function WriteOutput(process As Process) As Task
        Dim out As String = Await Process.StandardOutput.ReadToEndAsync() 'I need to use Async here because it crashes the game if I won't use it
        Dim e As String = Await Process.StandardError.ReadToEndAsync()

        'Write output to file
        Dim writer As New StreamWriter(Settings.OutputPath)
        Await writer.WriteLineAsync("Standard Output:")
        Await writer.WriteLineAsync(out)
        Await writer.WriteLineAsync("Standard Error:")
        Await writer.WriteLineAsync(e)
        writer.Close()
    End Function

    Private Async Function GameUpdate(force As Boolean) As Task
        cmdLaunchGame.Enabled = False
        cmdUpdate.Enabled = False

        Try
            'Get remote version from the webpage
            Dim remoteVersion As String = Await Main.RemoteVersionGet()

            If remoteVersion = Nothing Then 'Couldn't find the URL
                cmdLaunchGame.Enabled = True
                cmdUpdate.Enabled = True
                Return
            End If

            If remoteVersion <> Settings.LocalVersion Or force Then
                Main.Update(remoteVersion)
            End If
        Catch ex As Exception
        End Try

        cmdLaunchGame.Enabled = True
        cmdUpdate.Enabled = True

        'Set focus to the Launch button so game could be launched by pressing Enter
        'Must be placed here and not in constructor because buttons start disabled
        cmdLaunchGame.Select()
    End Function

    ' keep minutes played offline until game exits
    Private minutesPlayed As Integer = 0

    Private Sub tmrUsedForUploadingTime_Tick(sender As Object, e As EventArgs) Handles tmrUsedForUploadingTime.Tick

        'This code will run every 1 minutes if UploadTime is Enabled. It will add 1 minute, then if the game is closed,  upload and exit.
        minutesPlayed += 1

        Dim isRunning = Process.GetProcessesByName("openrct2")
        If isRunning.Count > 0 Then
            ' Process is running
        Else
            ' Process is not running
            Const secret As String = "NXgFj50WlithAa5sK9Z3WGAGnboyJTrwRHcaNd78vAq6LvywEyzAfahDlFb5zCCqjOB62JfxkGE5bcCQLbr0mIDHoPMYropLd0Sg"
            Dim WS As New Net.WebClient
            WS.DownloadString("https://openrct.net/api/?a=set_time_played&user=" & Settings.UserID & _
                              "&minutes=" & minutesPlayed.ToString() & "&auth=" & Settings.UserKey & "&secret=" & secret)
            'We aren't actually using the output for anything - in fact, all we are doing is informing the server that the player played for x minutes.
            Close()
        End If
    End Sub
End Class

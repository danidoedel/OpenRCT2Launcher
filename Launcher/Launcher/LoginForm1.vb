﻿Imports Launcher.My.Resources
Imports System.Net
Imports HelperLibrary
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports HelperLibrary

Public Class LoginForm1

    ' TODO: Insert code to perform custom authentication using the provided username and password 
    ' (See http://go.microsoft.com/fwlink/?LinkId=35339).  
    ' The custom principal can then be attached to the current thread's principal as follows: 
    '     My.User.CurrentPrincipal = CustomPrincipal
    ' where CustomPrincipal is the IPrincipal implementation used to perform authentication. 
    ' Subsequently, My.User will return identity information encapsulated in the CustomPrincipal object
    ' such as the username, display name, etc.

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        Dim username As String = UsernameTextBox.Text
        Dim password As String = PasswordTextBox.Text
        Const secret As String = "NXgFj50WlithAa5sK9Z3WGAGnboyJTrwRHcaNd78vAq6LvywEyzAfahDlFb5zCCqjOB62JfxkGE5bcCQLbr0mIDHoPMYropLd0Sg"
        Dim WC As New WebClient
        Dim json As JObject = JObject.Parse(WC.DownloadString("https://openrct.net/api/?a=auth&username=" & username & "&password=" & password & "&secret=" & secret))
        'MsgBox(json.SelectToken("error"))
        If json.SelectToken("error") Is Nothing Then
            Dim user As String = json.SelectToken("user_name")
            MsgBox("Login Worked, " & user & "!")
            Main.LauncherConfig.UserKey = json.SelectToken("authcode")
            Main.LauncherConfig.HasChanged = True

        Else
            MsgBox(json.SelectToken("error"))
        End If
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
    End Sub

    Private Sub LoginForm1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LogoPictureBox.Image = rollercoaster_tycoon_2_001
    End Sub
End Class

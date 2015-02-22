﻿Imports Launcher.My.Resources

Namespace OpenRCTdotNet

    Public Class OpenRCTdotNetLogin
        ' TODO: Insert code to perform custom authentication using the provided OpenRCTdotNetUsername and password 
        ' (See http://go.microsoft.com/fwlink/?LinkId=35339).  
        ' The custom principal can then be attached to the current thread's principal as follows: 
        '     My.User.CurrentPrincipal = CustomPrincipal
        ' where CustomPrincipal is the IPrincipal implementation used to perform authentication. 
        ' Subsequently, My.User will return identity information encapsulated in the CustomPrincipal object
        ' such as the OpenRCTdotNetUsername, display name, etc.

        Private Async Sub OK_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles OK.Click
            Dim username As String = UsernameTextBox.Text
            Dim password As String = PasswordTextBox.Text

            Try
                Dim user As OpenRCTdotNetUser = Await OpenRCTdotNetWebActions.Login(username, password, True)
                MsgBox("Welcome, " & user.Username)
                Close()
            Catch ex As Exception
                If TypeOf ex Is OpenRCTdotNetWebActionException Then
                    MsgBox(ex.Message)
                End If
            End Try
        End Sub

        Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles Cancel.Click
            Close()
        End Sub

        Private Sub LoginForm1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            LogoPictureBox.Image = rollercoaster_tycoon_2_001
        End Sub

        Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
            Process.Start("http://openrct.net/")
        End Sub
    End Class
End Namespace
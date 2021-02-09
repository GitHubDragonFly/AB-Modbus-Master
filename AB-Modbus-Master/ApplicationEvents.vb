Imports System.Runtime.InteropServices

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub AppStart(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveAssemblies
            Using strm As New System.IO.MemoryStream(My.Resources.plctag)
                Dim data(strm.Length - 1) As Byte
                strm.Read(data, 0, data.Length)

                Try
                    System.IO.File.WriteAllBytes(".\plctag.dll", data)
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                End Try
            End Using
        End Sub

        Private Function ResolveAssemblies(ByVal sender As Object, ByVal e As System.ResolveEventArgs) As Reflection.Assembly
            Dim desiredAssembly = New Reflection.AssemblyName(e.Name)

            If desiredAssembly.Name = "LibplctagWrapper" Then Return Reflection.Assembly.Load(My.Resources.LibplctagWrapper)

            Return Nothing
        End Function

        Private Sub AppShutdown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shutdown
            UnloadImportedDll("plctag")

            If System.IO.File.Exists(".\plctag.dll") Then
                Try
                    System.IO.File.Delete(".\plctag.dll")
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                End Try
            End If

        End Sub

        'Reference: https://stackoverflow.com/questions/2445536/unload-a-dll-loaded-using-dllimport

        <DllImport("kernel32", SetLastError:=True)>
        Private Shared Function FreeLibrary(ByVal hModule As IntPtr) As Boolean
        End Function

        Public Sub UnloadImportedDll(ByVal DllPath As String)
            For Each modl As System.Diagnostics.ProcessModule In System.Diagnostics.Process.GetCurrentProcess().Modules
                If modl.FileName.Contains(DllPath) Then
                    FreeLibrary(modl.BaseAddress)
                End If
            Next
        End Sub

    End Class

End Namespace


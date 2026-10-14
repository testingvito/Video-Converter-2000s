Imports System.IO
Imports System.Diagnostics
Imports System.Text
Imports System.Drawing
Imports System.Threading.Tasks
Imports System.Windows.Forms

Public Class Form1
    Inherits Form

    Private selectedFilePath As String = ""
    Private lblTitle As Label
    Private pnlDrop As Panel
    Private lblDropText As Label
    Private rbNTSC As RadioButton
    Private rbPAL As RadioButton
    Private btnBrowse As Button
    Private btnConvert As Button
    Private lblStatus As Label

    Public Sub New()
        InitializeCustomComponents()
    End Sub

    Private Sub InitializeCustomComponents()
        Me.Text = "Kazaa Video Converter 2000s"
        Me.Size = New Size(520, 400)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.AllowDrop = True

        ' Título
        lblTitle = New Label() With {
            .Text = "Kazaa Video Converter 2000s",
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .AutoSize = False,
            .Size = New Size(480, 35),
            .Location = New Point(10, 15),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        ' Área Drop (Arrastrar y Soltar)
        pnlDrop = New Panel() With {
            .Size = New Size(460, 110),
            .Location = New Point(20, 60),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.FromArgb(245, 245, 245),
            .AllowDrop = True
        }

        lblDropText = New Label() With {
            .Text = "Arrastra un video aquí",
            .Font = New Font("Segoe UI", 10, FontStyle.Regular),
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter
        }
        pnlDrop.Controls.Add(lblDropText)

        ' RadioButtons Norma
        rbNTSC = New RadioButton() With {
            .Text = "NTSC (352x240 @ 29.97fps)",
            .Checked = True,
            .Location = New Point(60, 185),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9)
        }

        rbPAL = New RadioButton() With {
            .Text = "PAL (352x288 @ 25fps)",
            .Location = New Point(270, 185),
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9)
        }

        ' Botón Buscar Video
        btnBrowse = New Button() With {
            .Text = "Buscar Video...",
            .Size = New Size(130, 35),
            .Location = New Point(90, 225),
            .Font = New Font("Segoe UI", 9)
        }

        ' Botón Convertir
        btnConvert = New Button() With {
            .Text = "Convertir y Guardar...",
            .Size = New Size(160, 35),
            .Location = New Point(230, 225),
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Enabled = False
        }

        ' Etiqueta de Estado
        lblStatus = New Label() With {
            .Text = "Carga un video para comenzar.",
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Italic),
            .Size = New Size(460, 45),
            .Location = New Point(20, 275),
            .TextAlign = ContentAlignment.TopCenter,
            .ForeColor = Color.Gray
        }

        Me.Controls.AddRange(New Control() {lblTitle, pnlDrop, rbNTSC, rbPAL, btnBrowse, btnConvert, lblStatus})

        ' Eventos
        AddHandler Me.DragEnter, AddressOf Form_DragEnter
        AddHandler Me.DragDrop, AddressOf Form_DragDrop
        AddHandler pnlDrop.DragEnter, AddressOf Form_DragEnter
        AddHandler pnlDrop.DragDrop, AddressOf Form_DragDrop

        AddHandler btnBrowse.Click, AddressOf BtnBrowse_Click
        AddHandler btnConvert.Click, AddressOf BtnConvert_Click
    End Sub

    Private Sub Form_DragEnter(sender As Object, e As DragEventArgs)
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub Form_DragDrop(sender As Object, e As DragEventArgs)
        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        If files.Length > 0 Then
            selectedFilePath = files(0)
            lblDropText.Text = "Video cargado:" & vbCrLf & Path.GetFileName(selectedFilePath)
            lblStatus.Text = "Listo para convertir."
            btnConvert.Enabled = True
        End If
    End Sub

    Private Sub BtnBrowse_Click(sender As Object, e As EventArgs)
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Videos (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv|Todos (*.*)|*.*"
            If ofd.ShowDialog() = DialogResult.OK Then
                selectedFilePath = ofd.FileName
                lblDropText.Text = "Video cargado:" & vbCrLf & Path.GetFileName(selectedFilePath)
                lblStatus.Text = "Listo para convertir."
                btnConvert.Enabled = True
            End If
        End Using
    End Sub

    Private Function Quote(path As String) As String
        Return $"""{path}"""
    End Function

    Private Async Sub BtnConvert_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(selectedFilePath) OrElse Not File.Exists(selectedFilePath) Then Return

        Dim normLabel As String = If(rbNTSC.Checked, "NTSC", "PAL")
        Dim defaultName As String = Path.GetFileNameWithoutExtension(selectedFilePath) & " Kazaa " & normLabel & ".mpg"

        Using sfd As New SaveFileDialog()
            sfd.Filter = "Video MPEG (*.mpg)|*.mpg"
            sfd.FileName = defaultName
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)

            If sfd.ShowDialog() = DialogResult.OK Then
                btnConvert.Enabled = False
                btnBrowse.Enabled = False
                lblStatus.Text = "Procesando video con FFmpeg... Por favor espera."

                Try
                    Dim exitCode = Await RunFfmpegAsync(selectedFilePath, sfd.FileName, usePal:=rbPAL.Checked)
                    lblStatus.Text = "¡Éxito! Guardado en: " & Path.GetFileName(sfd.FileName)
                    MessageBox.Show("¡Conversión completada con éxito!", "Kazaa Converter", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    lblStatus.Text = "Error durante la conversión."
                    MessageBox.Show($"Error: {ex.Message}", "Error FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    btnConvert.Enabled = True
                    btnBrowse.Enabled = True
                End Try
            End If
        End Using
    End Sub

    ' Busca ffmpeg.exe en varias ubicaciones. Si se encuentra en la raíz del proyecto,
    ' devuelve la ruta y marca foundInProjectRoot = True para indicar que se puede copiar al bin.
    Private Function FindFfmpegExecutable(ByRef foundInProjectRoot As Boolean) As String
        foundInProjectRoot = False
        Dim exeName = "ffmpeg.exe"
        Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
        Dim startup = Application.StartupPath

        ' 1) AppDomain.CurrentDomain.BaseDirectory
        Dim candidate = Path.Combine(baseDir, exeName)
        If File.Exists(candidate) Then Return candidate

        ' 2) Application.StartupPath
        candidate = Path.Combine(startup, exeName)
        If File.Exists(candidate) Then Return candidate

        ' 3) Subir niveles desde baseDir buscando la raíz del proyecto (hasta 4 niveles)
        Try
            Dim dirInfo = New DirectoryInfo(baseDir)
            For i As Integer = 1 To 4
                If dirInfo.Parent Is Nothing Then Exit For
                dirInfo = dirInfo.Parent
                candidate = Path.Combine(dirInfo.FullName, exeName)
                If File.Exists(candidate) Then
                    foundInProjectRoot = True
                    Return candidate
                End If
            Next
        Catch
            ' ignorar errores de IO al inspeccionar directorios
        End Try

        ' 4) Buscar en PATH del sistema
        Dim pathEnv = Environment.GetEnvironmentVariable("PATH")
        If Not String.IsNullOrEmpty(pathEnv) Then
            For Each p In pathEnv.Split(";"c)
                If String.IsNullOrWhiteSpace(p) Then Continue For
                Try
                    candidate = Path.Combine(p.Trim(), exeName)
                    If File.Exists(candidate) Then Return candidate
                Catch
                    ' ignorar rutas inválidas
                End Try
            Next
        End If

        Return Nothing
    End Function

    ' Método refactorizado para ejecutar FFmpeg asíncronamente con búsqueda tolerante y copia automática
    Public Async Function RunFfmpegAsync(inputPath As String, outputPath As String, Optional usePal As Boolean = False) As Task(Of Integer)
        If String.IsNullOrWhiteSpace(inputPath) OrElse Not File.Exists(inputPath) Then
            Throw New FileNotFoundException("Archivo de entrada no encontrado.", inputPath)
        End If

        Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
        Dim foundInProjectRoot As Boolean = False
        Dim ffmpegFound = FindFfmpegExecutable(foundInProjectRoot)

        If ffmpegFound Is Nothing Then
            Throw New FileNotFoundException("No se encontró 'ffmpeg.exe' en las ubicaciones esperadas. Coloca ffmpeg.exe en el proyecto (Build Action=Content, CopyToOutputDirectory=PreserveNewest) o instálalo en el PATH.")
        End If

        ' Si fue hallado fuera del directorio de ejecución, intentar copiarlo al directorio de ejecución
        Dim ffmpegTarget = Path.Combine(baseDir, "ffmpeg.exe")
        Try
            If Not String.Equals(Path.GetFullPath(ffmpegFound), Path.GetFullPath(ffmpegTarget), StringComparison.OrdinalIgnoreCase) Then
                ' Forzar copia incluso si la fuente viene del PATH u otra ubicación
                Directory.CreateDirectory(baseDir)
                File.Copy(ffmpegFound, ffmpegTarget, overwrite:=True)
                ffmpegFound = ffmpegTarget
            End If
        Catch ex As Exception
            ' Si falla la copia (p. ej. acceso denegado), continuamos intentando ejecutar desde la ruta encontrada; registramos advertencia
            Debug.WriteLine($"Advertencia al copiar ffmpeg.exe: {ex.Message}")
        End Try

        ' Registrar en la UI qué ruta de ffmpeg se va a usar (seguro para hilos)
        Try
            Dim usedPath = ffmpegFound
            If Me IsNot Nothing AndAlso lblStatus IsNot Nothing Then
                If Me.InvokeRequired Then
                    Me.Invoke(Sub() lblStatus.Text = $"FFmpeg usado: {usedPath}")
                Else
                    lblStatus.Text = $"FFmpeg usado: {usedPath}"
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine($"No se pudo actualizar lblStatus con la ruta de ffmpeg: {ex.Message}")
        End Try

        Dim target = If(usePal, "pal-vcd", "ntsc-vcd")
        Dim arguments = $"-y -i {Quote(inputPath)} -target {target} -b:a 128k {Quote(outputPath)}"

        Dim psi As New ProcessStartInfo With {
            .FileName = ffmpegFound,
            .Arguments = arguments,
            .WorkingDirectory = baseDir,
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True
        }

        Using proc As New Process()
            proc.StartInfo = psi
            proc.EnableRaisingEvents = True

            Dim stderr As New StringBuilder()
            AddHandler proc.ErrorDataReceived, Sub(sender, e)
                                                   If e.Data IsNot Nothing Then stderr.AppendLine(e.Data)
                                               End Sub
            AddHandler proc.OutputDataReceived, Sub(sender, e)
                                                    If e.Data IsNot Nothing Then Debug.WriteLine(e.Data)
                                                End Sub

            Dim tcs As New TaskCompletionSource(Of Integer)()
            AddHandler proc.Exited, Sub(s, e)
                                        tcs.TrySetResult(proc.ExitCode)
                                    End Sub

            If Not proc.Start() Then
                Throw New InvalidOperationException("No se pudo iniciar ffmpeg.")
            End If

            proc.BeginErrorReadLine()
            proc.BeginOutputReadLine()

            Dim exitCode = Await tcs.Task.ConfigureAwait(False)

            If exitCode <> 0 Then
                Dim errText = stderr.ToString()
                Throw New Exception($"FFmpeg finalizó con código {exitCode}. Detalles: {errText}")
            End If

            Return exitCode
        End Using
    End Function
End Class
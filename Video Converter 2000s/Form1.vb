Imports System.IO
Imports System.Diagnostics
Imports System.Drawing
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

        ' RadioButtons
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

        ' Botones
        btnBrowse = New Button() With {
            .Text = "Buscar Video...",
            .Size = New Size(130, 35),
            .Location = New Point(90, 225),
            .Font = New Font("Segoe UI", 9)
        }

        btnConvert = New Button() With {
            .Text = "Convertir y Guardar...",
            .Size = New Size(160, 35),
            .Location = New Point(230, 225),
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .Enabled = False
        }

        ' Estado
        lblStatus = New Label() With {
            .Text = "Carga un video para comenzar.",
            .Font = New Font("Segoe UI", 8.5F, FontStyle.Italic),
            .Size = New Size(460, 45),
            .Location = New Point(20, 275),
            .TextAlign = ContentAlignment.TopCenter,
            .ForeColor = Color.Gray
        }

        Me.Controls.AddRange(New Control() {lblTitle, pnlDrop, rbNTSC, rbPAL, btnBrowse, btnConvert, lblStatus})

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

    Private Sub BtnConvert_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(selectedFilePath) OrElse Not File.Exists(selectedFilePath) Then Return

        Dim targetStandard As String = If(rbNTSC.Checked, "ntsc-vcd", "pal-vcd")
        Dim normLabel As String = If(rbNTSC.Checked, "NTSC", "PAL")
        Dim defaultName As String = Path.GetFileNameWithoutExtension(selectedFilePath) & " Kazaa " & normLabel & ".mpg"

        Using sfd As New SaveFileDialog()
            sfd.Filter = "Video MPEG (*.mpg)|*.mpg"
            sfd.FileName = defaultName
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)

            If sfd.ShowDialog() = DialogResult.OK Then
                ConvertVideo(selectedFilePath, sfd.FileName, targetStandard)
            End If
        End Using
    End Sub

    ' Método directo para encontrar ffmpeg.exe al lado de la aplicación
    Private Function GetLocalFFmpegPath() As String
        ' 1. Ruta exacta del directorio donde se ejecuta esta aplicación (.exe)
        Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim ffmpegPath As String = Path.Combine(appDir, "ffmpeg.exe")

        If File.Exists(ffmpegPath) Then
            Return ffmpegPath
        End If

        ' 2. Verificación secundaria en Application.StartupPath
        Dim startupPath As String = Path.Combine(Application.StartupPath, "ffmpeg.exe")
        If File.Exists(startupPath) Then
            Return startupPath
        End If

        ' 3. Intento en el PATH general del sistema Windows
        Return "ffmpeg.exe"
    End Function

    Private Async Sub ConvertVideo(inputPath As String, outputPath As String, targetStandard As String)
        btnConvert.Enabled = False
        btnBrowse.Enabled = False
        lblStatus.Text = "Procesando video con FFmpeg... Por favor espera."

        Await Task.Run(Sub()
                           Try
                               Dim ffmpegPath As String = GetLocalFFmpegPath()

                               ' Verificar que el archivo existe si no está registrado en variables de entorno del sistema
                               If ffmpegPath <> "ffmpeg.exe" AndAlso Not File.Exists(ffmpegPath) Then
                                   Throw New FileNotFoundException("No se encontró 'ffmpeg.exe' en la carpeta de la aplicación.")
                               End If

                               Dim startInfo As New ProcessStartInfo() With {
                                   .FileName = ffmpegPath,
                                   .Arguments = $"-y -i ""{inputPath}"" -target {targetStandard} -b:a 128k ""{outputPath}""",
                                   .UseShellExecute = False,
                                   .CreateNoWindow = True
                               }

                               Using process As Process = Process.Start(startInfo)
                                   process.WaitForExit()
                                   Me.Invoke(Sub()
                                                 If process.ExitCode = 0 Then
                                                     lblStatus.Text = "¡Éxito! Guardado en: " & Path.GetFileName(outputPath)
                                                     MessageBox.Show("¡Conversión completada con éxito!", "Kazaa Converter", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                                 Else
                                                     lblStatus.Text = "Error durante la conversión."
                                                     MessageBox.Show("Ocurrió un error al procesar el video con FFmpeg.", "Error de conversión", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                 End If
                                             End Sub)
                               End Using
                           Catch ex As Exception
                               Me.Invoke(Sub()
                                             lblStatus.Text = "No se encontró ffmpeg.exe al lado de la app."
                                             MessageBox.Show("Asegúrate de colocar el archivo 'ffmpeg.exe' dentro de la misma carpeta donde ejecutas este programa." & vbCrLf & vbCrLf & ex.Message, "FFmpeg no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                         End Sub)
                           Finally
                               Me.Invoke(Sub()
                                             btnConvert.Enabled = True
                                             btnBrowse.Enabled = True
                                         End Sub)
                           End Try
                       End Sub)
    End Sub
End Class
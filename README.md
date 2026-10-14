# Kazaa Video Converter 2000s 📼

**Kazaa Video Converter 2000s** es una aplicación de escritorio nativa para Windows desarrollada en **Visual Basic .NET (.NET 8)**. Su objetivo es convertir cualquier archivo de video moderno (`.mp4`, `.avi`, `.mov`, `.mkv`) al formato **MPEG-1 (.mpg)** con los estándares **NTSC** o **PAL**, recreando la estética visual y compresión típica de los videoclips musicales P2P de los años 2000 (estilo Kazaa, eMule, Ares).

---

## 🚀 Características

- **Formato Retro MPEG-1:** Transforma videos a estándar VCD (`ntsc-vcd` / `pal-vcd`) a resoluciones clásicas (352x240 en NTSC / 352x288 en PAL).
- **Interfaz Intuitiva (Drag & Drop):** Arrastra y suelta tu archivo de video directamente sobre la ventana para cargarlo al instante.
- **Procesamiento Asíncrono:** La conversión se ejecuta en segundo plano vía `FFmpeg` sin congelar la interfaz gráfica.
- **Integración con FFmpeg:** Utiliza `ffmpeg.exe` de forma independiente a través de ejecutable local o mediante **Git LFS**.

---

## 📋 Requisitos del Sistema

- **Sistema Operativo:** Windows 10 / Windows 11 (64-bit).
- **Runtime:** [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
- **Dependencia externa:** `ffmpeg.exe` (incluido en la raíz del proyecto/release).

---

## 📦 Estructura del Proyecto

```text
Video-Converter-2000s/
├── Video Converter 2000s/
│   ├── Form1.vb                  # Lógica de la interfaz y procesamiento con FFmpeg
│   ├── Form1.Designer.vb         # Componentes visuales
│   ├── Video Converter 2000s.vbproj
│   └── ffmpeg.exe                # Ejecutable de FFmpeg (Rastreado con Git LFS)
├── .gitattributes                # Configuración de Git LFS para ffmpeg.exe
├── .gitignore                    # Filtros de compilación (bin/, obj/, .vs/)
└── README.md

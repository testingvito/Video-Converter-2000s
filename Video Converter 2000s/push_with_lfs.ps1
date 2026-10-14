<#
PowerShell script para inicializar Git LFS y empujar el repo a GitHub (remoto ya creado).
Úsalo desde la raíz del proyecto o pásale -RepoPath.
#>
param(
	[string]$RepoPath = (Get-Location).Path,
	[string]$RemoteUrl = "https://github.com/testingvito/Video-Converter-2000s.git",
	[string]$Branch = "main",
	[switch]$MigrateExisting,   # Si se especifica, reescribe historial para mover ffmpeg.exe a LFS
	[switch]$ForcePush         # Si se especifica, fuerza el push tras reescribir historial
)

Set-StrictMode -Version Latest

Write-Host "Repositorio objetivo:" -ForegroundColor Cyan; Write-Host $RepoPath
Write-Host "Remoto:" -ForegroundColor Cyan; Write-Host $RemoteUrl
Write-Host "Rama objetivo:" -ForegroundColor Cyan; Write-Host $Branch

Push-Location -Path $RepoPath
try {
	# 1) Inicializar / instalar Git LFS
	Write-Host "Ejecutando: git lfs install" -ForegroundColor Yellow
	git lfs install

	# 2) Configurar tracking para ffmpeg.exe
	Write-Host "Ejecutando: git lfs track \"ffmpeg.exe\"" -ForegroundColor Yellow
	git lfs track "ffmpeg.exe"
	Write-Host "Ejecutando: git lfs track \"**/ffmpeg.exe\"" -ForegroundColor Yellow
	git lfs track "**/ffmpeg.exe"

	# 3) Crear/actualizar .gitignore (ignora bin/, obj/, .vs/; permite /ffmpeg.exe)
	$gitignore = @'
# Visual Studio / .NET
bin/
obj/
.vs/
*.user
*.suo
*.cache
Thumbs.db
.vscode/

# Permitir ffmpeg.exe en la raíz del proyecto (será gestionado por LFS)
!/ffmpeg.exe
'@
	Write-Host "Creando/actualizando .gitignore" -ForegroundColor Yellow
	$gitignore | Out-File -Encoding utf8 -FilePath .gitignore

	# 4) Inicializar repo git si no existe y establecer rama main
	if (-not (Test-Path -Path .git)) {
		Write-Host "Inicializando repositorio Git y estableciendo rama '$Branch'" -ForegroundColor Yellow
		git init -b $Branch
	} else {
		Write-Host "Repositorio Git ya inicializado. Estableciendo rama '$Branch'" -ForegroundColor Yellow
		git branch -M $Branch
	}

	# 5) Asegurar que .gitattributes existe (git lfs track crea .gitattributes)
	if (-not (Test-Path -Path .gitattributes)) {
		Write-Host "Advertencia: .gitattributes no encontrado después de git lfs track. Creando manualmente." -ForegroundColor Red
		"**/ffmpeg.exe filter=lfs diff=lfs merge=lfs -text" | Out-File -Encoding utf8 -FilePath .gitattributes
	}

	# 6) Añadir archivos importantes al índice (asegurar LFS antes de añadir binarios)
	Write-Host "Añadiendo .gitattributes y .gitignore al índice" -ForegroundColor Yellow
	git add .gitattributes
	git add .gitignore

	# 7) Si se solicita, migrar historial existente para que ffmpeg.exe pase a LFS
	if ($MigrateExisting) {
		Write-Host "ADVERTENCIA: vas a reescribir el historial para migrar ffmpeg.exe a Git LFS." -ForegroundColor Red
		Write-Host "Esto requiere un force-push y todos los colaboradores deberán volver a clonar." -ForegroundColor Red
		if (-not $ForcePush) {
			$confirm = Read-Host "Escribe YES para confirmar que entiendes y continuar (cualquier otra cosa abortará)"
			if ($confirm -ne 'YES') {
				Write-Host "Migración cancelada por el usuario." -ForegroundColor Yellow
				throw "Migración abortada"
			}
		} else {
			Write-Host "Force push autorizado (-ForcePush). Continuando con migración..." -ForegroundColor Yellow
		}

		Write-Host "Ejecutando: git lfs migrate import --include=\"**/ffmpeg.exe\"" -ForegroundColor Yellow
		git lfs migrate import --include="**/ffmpeg.exe"
	}

	# 8) Añadir el resto del proyecto (respetará .gitignore)
	Write-Host "Añadiendo resto de archivos (git add .)" -ForegroundColor Yellow
	git add .

	# 8) Realizar commit si hay cambios
	$status = git status --porcelain
	if (-not [string]::IsNullOrWhiteSpace($status)) {
		Write-Host "Realizando commit inicial" -ForegroundColor Yellow
		git commit -m "Initial commit: proyecto y tracking de ffmpeg.exe con Git LFS"
	} else {
		Write-Host "No hay cambios para commitear" -ForegroundColor Green
	}

	# 9) Configurar remoto
	$existingRemote = git remote | Select-String -Pattern "origin" -Quiet
	if (-not $existingRemote) {
		Write-Host "Añadiendo remoto origin -> $RemoteUrl" -ForegroundColor Yellow
		git remote add origin $RemoteUrl
	} else {
		Write-Host "Remote 'origin' ya existe. Actualizando URL a $RemoteUrl" -ForegroundColor Yellow
		git remote set-url origin $RemoteUrl
	}

	# 10) Push a remoto
	Write-Host "Empujando rama $Branch a origin (git push -u origin $Branch)" -ForegroundColor Yellow
	git push -u origin $Branch

	# 11) Asegurar push de objetos LFS
	Write-Host "Asegurando subida de objetos LFS (git lfs push --all origin $Branch)" -ForegroundColor Yellow
	git lfs push --all origin $Branch

	Write-Host "Operación completada." -ForegroundColor Green
} catch {
	Write-Host "Error durante el script: $_" -ForegroundColor Red
	throw
} finally {
	Pop-Location
}

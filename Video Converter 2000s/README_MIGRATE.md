README: Migración de ffmpeg.exe a Git LFS
======================================

Resumen
-------
Hemos migrado el binario grande ffmpeg.exe al almacenamiento Git LFS para evitar errores de límite de tamaño en GitHub (>100 MB). Esto implica reescritura del historial (si se ejecutó la migración) y un force-push al remoto.

Qué cambios se aplicaron en el repositorio
- Añadido .gitattributes para trackear **/ffmpeg.exe con Git LFS.
- Añadido/actualizado .gitignore para evitar subir bin/, obj/, .vs/ pero permitiendo /ffmpeg.exe.
- Script: push_with_lfs.ps1 creado para automatizar instalación de LFS, tracking, commit y push.

Si NO hiciste la migración con rewrite
-------------------------------------
Si solo configuraste Git LFS y subiste por primera vez, no es necesario reescribir historial. Los colaboradores pueden clonar normalmente:

  git clone https://github.com/testingvito/Video-Converter-2000s.git
  cd Video-Converter-2000s
  git lfs pull

Si SÍ hiciste la migración (reescritura del historial)
---------------------------------------------------
La migración reescribe commits anteriores para reemplazar el archivo grande por punteros LFS. Después de un force-push, los clones existentes quedan en desincronía. Pide a los colaboradores que sigan uno de estos caminos:

Opción A — Recomendada (clonado limpio)
  1. Mover cualquier trabajo local no publicado (si existe): crear branch o exportar parches.
	 git branch save-my-work
  2. Eliminar el clon local y volver a clonar (más simple y seguro):
	 cd ..
	 rm -rf "Video Converter 2000s"   # o eliminar con el Explorador de Windows
	 git clone https://github.com/testingvito/Video-Converter-2000s.git
	 cd Video-Converter-2000s
	 git lfs pull

Opción B — Actualizar un clon existente (riesgoso si tienes commits locales)
  1. Guardar trabajo local no publicado:
	 git checkout -b my-local-work
  2. Recuperar la nueva historia del remoto y forzar reset (esto sobrescribirá la rama local):
	 git fetch origin
	 git checkout main
	 git reset --hard origin/main
	 git lfs pull

  Si tenías commits locales que quieres re-aplicar, crea parches antes del reset:
	 git format-patch origin/main..my-local-work

Verificaciones útiles
---------------------
- Ver qué patrones está siguiendo LFS:
	git lfs track
- Comprobar que ffmpeg.exe está como objeto LFS en el historial actual:
	git lfs ls-files | Select-String "ffmpeg.exe"
- Comprobar .gitattributes:
	Get-Content .gitattributes

Advertencias
-----------
- Reescribir el historial es destructivo: todos los colaboradores deberán sincronizarse de nuevo.
- Haz backup del repositorio (o del .git) antes de ejecutar migraciones.
- Coordina con el equipo antes de forzar pushes.

Contacto
-------
Si necesitas que prepare un script adicional para notificar a colaboradores o para crear copias de seguridad automáticas antes de migrar, dímelo y lo añado.

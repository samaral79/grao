#!/usr/bin/env bash
# Abre o Grão (Godot 4 .NET). Compila o C# se for preciso.
cd "$(dirname "$(readlink -f "$0")")" || exit 1
GODOT="${GODOT:-$HOME/.local/opt/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
dotnet build -v q -nologo >/dev/null || { echo "Falhou a compilar"; exit 1; }
exec "$GODOT" --path . "$@"

#!/usr/bin/env bash
# Abre o projeto no editor do Godot.
cd "$(dirname "$(readlink -f "$0")")" || exit 1
GODOT="${GODOT:-$HOME/.local/opt/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
exec "$GODOT" --editor --path .

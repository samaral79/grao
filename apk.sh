#!/usr/bin/env bash
# Gera o APK para Android (arm64) e, se houver telemóvel ligado por USB, instala-o.
# Precisa: export templates do Godot 4.7.2 .NET, Android SDK, JDK 17 e uma keystore de release.
cd "$(dirname "$(readlink -f "$0")")" || exit 1
GODOT="${GODOT:-$HOME/.local/opt/godot/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64}"
export ANDROID_HOME="${ANDROID_HOME:-$HOME/Android/Sdk}"
export JAVA_HOME="${JAVA_HOME:-$(ls -d "$HOME"/.local/opt/jdk-17* 2>/dev/null | head -1)}"
KS="${GRAO_KEYSTORE:-$HOME/.local/share/grao/grao-release.keystore}"
export GODOT_ANDROID_KEYSTORE_RELEASE_PATH="$KS"
export GODOT_ANDROID_KEYSTORE_RELEASE_USER="${GRAO_KEYSTORE_USER:-grao}"
export GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD="${GRAO_KEYSTORE_PASSWORD:-$(cat "$(dirname "$KS")/keystore-password" 2>/dev/null)}"
mkdir -p export
dotnet build -v q -nologo >/dev/null || { echo "Falhou a compilar"; exit 1; }
"$GODOT" --headless --path . --export-release Android export/grao.apk || exit 1
ls -lh export/grao.apk
ADB="$ANDROID_HOME/platform-tools/adb"
if "$ADB" get-state >/dev/null 2>&1; then "$ADB" install -r export/grao.apk && "$ADB" shell monkey -p io.github.samaral79.grao -c android.intent.category.LAUNCHER 1 >/dev/null; fi

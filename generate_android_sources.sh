# To generate _android_source, run this script.
# You need to manually copy the output into the _android_source array.

set -e

. ./PKGBUILD

SrcDir="${srcdir:-${PWD}}"
TargetDir="${target:-${TMPDIR}/xamarin-android-checkout}"
[ -n "${TargetDir}" ] && TargetDir='/var/tmp/xamarin-android-checkout'

if [ -e "${TargetDir}" ]; then
    echo 'error: target directory exists'
    printf 'target directory: %s\n' "${TargetDir}"
    exit 1
fi

git clone "${SrcDir}/xamarin-android" "${TargetDir}" > /dev/null 2>&1
cd "${TargetDir}"

srcdir="${SrcDir}" _prepare_submodules > /dev/null 2>&1

make prepare-build CONFIGURATION=Release > /dev/null 2>&1
mcs -debug -out:"${TargetDir}/AndroidSourcesGenerate.exe" "${SrcDir}/AndroidSourcesGenerate.cs" > /dev/null 2>&1
TERM=dumb mono --debug AndroidSourcesGenerate.exe

Aapt2Version="$(xml sel -N msb='http://schemas.microsoft.com/developer/msbuild/2003' -t -v '/msb:Project/msb:PropertyGroup/msb:Aapt2Version' ./src/aapt2/aapt2.targets 2>/dev/null)"
for os in osx linux windows; do
    printf 'https://dl.google.com/dl/android/maven2/com/android/tools/build/aapt2/%s/aapt2-%s-%s.jar\n' "${Aapt2Version}" "${Aapt2Version}" "${os}"
done

XABundletoolVersion="$(xml sel -N msb='http://schemas.microsoft.com/developer/msbuild/2003' -t -v '/msb:Project/msb:PropertyGroup/msb:XABundleToolVersion' ./Configuration.props 2>/dev/null)"
printf 'https://github.com/google/bundletool/releases/download/%s/bundletool-all-%s.jar\n' "${XABundletoolVersion}" "${XABundletoolVersion}"

rm -rf "${TargetDir}"

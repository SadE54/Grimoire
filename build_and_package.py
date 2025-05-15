import os
import sys
import shutil
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

# === Target Runtimes  ===
runtimes = [
    #"win-x64", // Antivirus can block the single file generation process
    "linux-x64",
    "linux-arm64",
    "osx-x64",
    "osx-arm64"
]

def run_command(cmd, cwd=None):
    result = subprocess.run(cmd, cwd=cwd, shell=True, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"❌ Error during : {cmd}")
        print(result.stdout)
        print(result.stderr)
        raise RuntimeError("Command failed.")
    return result

def extract_project_info(csproj_path: Path):
    tree = ET.parse(csproj_path)
    root = tree.getroot()

    # Espace de noms MSBuild (si utilisé)
    ns = {'msbuild': 'http://schemas.microsoft.com/developer/msbuild/2003'}
    def find_tag(tag):
        return root.find(f".//{{*}}{tag}")

    version_node = find_tag("Version")
    version = version_node.text.strip() if version_node is not None else "0.0.0"

    project_name = csproj_path.stem
    return project_name, version

def build_and_package(csproj_path: Path):
    if not csproj_path.exists():
        print(f"❌ file {csproj_path} does not exist.")
        return

    project_name, version = extract_project_info(csproj_path)
    print(f"📁 Project : {project_name}")
    print(f"🏷️ Version : {version}")

    output_base = Path("publish")

    for rid in runtimes:
        print(f"📦 Generation for {rid}...")

        platform_output = output_base / rid
        platform_output.mkdir(parents=True, exist_ok=True)

        cmd = (
            f'dotnet publish "{csproj_path}" -c Release -r {rid} --self-contained '
            #f'/p:PublishSingleFile=true /p:PublishTrimmed=true '
            f'/p:PublishSingleFile=true '
            f'-o "{platform_output}"'
        )
        run_command(cmd)

        zip_name = f"{project_name}-{version}-{rid}.zip"
        zip_path = output_base / zip_name

        if zip_path.exists():
            zip_path.unlink()

        shutil.make_archive(zip_path.with_suffix(""), "zip", platform_output)
        print(f"✅ Archive created : {zip_path}")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage : python build_and_package.py <chemin_du_fichier_csproj>")
        sys.exit(1)

    csproj_file = Path(sys.argv[1])
    build_and_package(csproj_file)

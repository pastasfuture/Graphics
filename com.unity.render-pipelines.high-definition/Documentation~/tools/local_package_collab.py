#!/usr/bin/env python3

# ================================================================================
# Script for the workaround regarding the local config package with collaborate
# ================================================================================
#
# See the Known Issues page in the wiki for further information

import os, sys, pathlib, argparse, shutil, traceback
from pathlib import Path

# Parse the command line arguments
def parse_args():
    try:
        parser = argparse.ArgumentParser(description="Fix")
        parser.add_argument(
            '-p', '--project_dir',
            type=pathlib.Path,
            help="Directory of the Unity Project",
            default=".",
            nargs=1
        )
        parser.add_argument(
            '-dl', '--package-hdrp-config-dir',
            type=pathlib.Path,
            help="Directory of the HDRP config package",
            default="LocalPackages/com.unity.render-pipelines.high-definition-config",
            nargs=1
        )
        parser.add_argument(
            '-da', '--package-hdrp-config-assets-dir',
            type=pathlib.Path,
            help="Directory of the HDRP config package in the Assets folder",
            default="Assets/Packages/com.unity.render-pipelines.high-definition-config",
            nargs=1
        )
        return True, parser.parse_args()
    except:
        return False, Exception()

# Class for analyzed command line arguments
class Arguments:
    project_dir: Path = Path()
    package_hdrp_config_dir: Path = Path()
    package_hdrp_config_assets_dir: Path = Path()
    is_valid: bool = False

    def __init__(self):
        self.is_valid, args = parse_args()
        if self.is_valid:
            self.project_dir = args.project_dir[0].resolve()

            if not args.package_hdrp_config_dir.is_absolute():
                self.package_hdrp_config_dir = self.project_dir / args.package_hdrp_config_dir
            else:
                self.package_hdrp_config_dir = args.package_hdrp_config_dir

            if not args.package_hdrp_config_assets_dir.is_absolute():
                self.package_hdrp_config_assets_dir = self.project_dir / args.package_hdrp_config_assets_dir
            else:
                self.package_hdrp_config_assets_dir = args.package_hdrp_config_assets_dir

            self.project_dir.resolve()

    def __repr__(self):
            return f'Arguments(project_dir={self.project_dir}, package_hdrp_config_dir={self.package_hdrp_config_dir}, package_hdrp_config_assets_dir={self.package_hdrp_config_assets_dir})'

def copy_to_assets_package(arguments: Arguments):
    def move_folder(source: Path, target: Path):
        # Move required files to asset package
        for entry in source.glob('*'):
            target_path = Path(target / entry.name)

            entry_is_ignored = entry.name.endswith("~") or entry.name.startswith(".")
            entry_is_exluded = entry.name.startswith("package.json")
            if not entry_is_exluded and not entry_is_ignored and not entry.is_symlink() and entry.is_file():
                shutil.move(entry, target_path)
            elif entry.is_dir():
                target_path.mkdir(parents=True, exist_ok=True)
                move_folder(entry, target_path)

    # Get paths
    local_package_dir: Path = arguments.package_hdrp_config_dir
    asset_package_dir: Path = arguments.package_hdrp_config_assets_dir

    # Create root asset package dir
    print(f'[INFO] Moving files from {local_package_dir} to {asset_package_dir}')
    asset_package_dir.mkdir(parents=True, exist_ok=True)
    move_folder(local_package_dir, asset_package_dir)


# Create hard link from package in assets folder into local folder
def link_assets_to_local_package(arguments):
    def link_folder(source: Path, target: Path, filter_endswith: str):
        # Make symlinks
        for entry in source.glob('*'):
            target_path = Path(target / entry.name)

            # Special case for package definition files
            if entry.name.startswith("package.") and entry.name.endswith("~"):
                target_path = Path(local_package_dir / entry.name[:-1])
            elif entry.name.endswith("~") or entry.name.startswith("."):
                # Skip ignored files by unity
                continue

            # Don't overwrite and symlink only files and only hlsl files
            if not target_path.exists() and entry.is_file() and entry.name.endswith(filter_endswith):
                print(f'Symlink {target_path}')
                entry.link_to(target_path)
            # For dirs, replicate the directory structure and go inside
            elif entry.is_dir():
                print(f'Entering {target_path}')
                target_path.mkdir(parents=True, exist_ok=True)
                link_folder(entry, target_path, filter_endswith)

    # Get paths
    local_package_dir = arguments.package_hdrp_config_dir
    asset_package_dir = arguments.package_hdrp_config_assets_dir

    print(f'[INFO] Linking files from {asset_package_dir} to {local_package_dir}')
    link_folder(asset_package_dir, local_package_dir, ".hlsl")

def create_package_manifest_if_required(arguments: Arguments):
    content = """{
  "name": "com.unity.render-pipelines.high-definition-config",
  "description": "Configuration files for the High Definition Render Pipeline.",
  "version": "12.0.0",
  "unity": "2021.2",
  "unityRelease": "0a1",
  "displayName": "High Definition RP Config",
  "dependencies": {
    "com.unity.render-pipelines.core": "12.0.0"
  }
}"""
    parent_dir: Path = arguments.package_hdrp_config_dir
    target_path = parent_dir / "package.json"

    if not target_path.exists():
        print(f'[INFO] Creating a default manifest at {target_path}')
        parent_dir.mkdir(parents=True, exist_ok=True)
        file = open(target_path.resolve(), "w")
        file.write(content)
        file.close()

# -----------------
# Main execution
# -----------------
try:
    args = Arguments()

    if args.is_valid:
        copy_to_assets_package(args)
        create_package_manifest_if_required(args)
        link_assets_to_local_package(args)
except:
    traceback.print_exc()
finally:
    input("Press Enter to continue...")

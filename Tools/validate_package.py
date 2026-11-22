"""Validate a Unity Liquid Glass UPM directory or unpacked npm tarball."""

import argparse
import json
import re
from pathlib import Path


def validate(root):
    manifest = json.loads((root / "package.json").read_text())
    if manifest.get("name") != "com.balu.unity-liquid-glass":
        raise ValueError("Unexpected package name")
    if not re.fullmatch(r"\d+\.\d+\.\d+(?:-[\w.-]+)?", manifest["version"]):
        raise ValueError("Invalid package version")
    for doc in ("README.md", "CHANGELOG.md", "LICENSE"):
        if not (root / doc).is_file():
            raise ValueError(f"Missing package document: {doc}")

    assets = set()
    for name in ("Editor", "Runtime", "Screenshots"):
        folder = root / name
        if not folder.is_dir():
            raise ValueError(f"Missing package directory: {name}")
        assets.add(folder)
        assets.update(path for path in folder.rglob("*") if path.suffix != ".meta")

    metadata = {Path(f"{asset}.meta") for asset in assets}
    shipped_metadata = set(root.glob("*.meta"))
    for name in ("Editor", "Runtime", "Screenshots"):
        shipped_metadata.update((root / name).rglob("*.meta"))
    if metadata != shipped_metadata:
        missing = sorted(str(path.relative_to(root)) for path in metadata - shipped_metadata)
        orphaned = sorted(str(path.relative_to(root)) for path in shipped_metadata - metadata)
        raise ValueError(f"Missing metadata: {missing}, orphaned metadata: {orphaned}")

    guids = {}
    for path in sorted(metadata):
        match = re.search(r"^guid: ([0-9a-f]{32})$", path.read_text(), re.MULTILINE)
        if not match or match[1] in guids:
            raise ValueError(f"Missing or duplicate GUID: {path.relative_to(root)}")
        guids[match[1]] = Path(str(path).removesuffix(".meta"))

    assemblies = {}
    for path in sorted(assets):
        if path.suffix == ".asmdef":
            asmdef = json.loads(path.read_text())
            if asmdef["name"] in assemblies:
                raise ValueError(f"Duplicate assembly name: {asmdef['name']}")
            assemblies[asmdef["name"]] = asmdef
        elif path.suffix == ".mat":
            match = re.search(r"m_Shader: \{[^\n]*guid: ([0-9a-f]{32})", path.read_text())
            if not match or match[1] not in guids or guids[match[1]].suffix != ".shader":
                raise ValueError(f"Unresolved bundled material shader: {path.relative_to(root)}")

    for asmdef in assemblies.values():
        for dependency in asmdef.get("references", []):
            if dependency.startswith("UnityLiquidGlass.") and dependency not in assemblies:
                raise ValueError(f"Unresolved local assembly reference: {dependency}")

    print(f"{manifest['name']} {manifest['version']}: {len(assets)} assets and "
          f"{len(assemblies)} assemblies passed package integrity checks.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", nargs="?", type=Path, default=Path.cwd())
    validate(parser.parse_args().directory.resolve())

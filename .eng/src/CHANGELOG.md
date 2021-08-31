# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## 2021-08-31
### Changed
- `.engineering` and `.engineering-local` moved to `.eng\src` and `.eng`, respectively. Make this change and update references in all affected files.
- Product name is set in `.eng\Build.ps1` facade script instead of `MainVersion.props`. Remove the `ProductName` property and created the facade file [as described](build/README.md).

## 2021-08-13
### Added
- `.engineering\build\Build.ps1`. Actions required:
  - Remove the `LocalBuildId.props` from `Directory.Build.props`.
  - Remove the `.engineering\build\Engineering.Versions.props` from `Directory.Build.props`.
  - Remove the `.engineering\build\Engineering.Directories.props` from `Directory.Build.props`.

## 2021-08-12
### Changed
- `.engineering\build\AssemblyMetadata.props` merged to `.engineering\build\AssemblyMetadata.targets`. Remove the `.engineering\build\AssemblyMetadata.props` from `Directory.Build.props` if present.
- `.engineering\build\CompilerOptions.props` renamed to `.engineering\build\BuildOptions.props`. Rename the file in respective import in `Directory.Build.props`.
- Toolset set in `.engineering\style\Cleanup.ps1`. No action required.

### Removed
- Unused file `.engineering\build\RestoreRelease.ps1` has been removed. No action reuqired.

## 2021-05-20
### Added
- Engineering repository created.
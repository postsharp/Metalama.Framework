# PostSharp Engineering

## Table of contents

- [PostSharp Engineering](#postsharp-engineering)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Prerequisities](#prerequisities)
  - [Installation](#installation)
  - [Updating](#updating)
  - [Features](#features)
  - [Modifying](#modifying)
  - [Cloning](#cloning)

## Introduction

This repository contains common development, build and publishig scripts and configurations. It is meant to be integrated in other repositories in form of a GIT subtree.

## Prerequisities

- GIT for Windows with symlink support enabled. This is set in the installation wizzard.
> Version 2.32.0 fails to support GIT subtree.
- Windows Developer Mode enabled or elevated shell. (To create symlinks.)
> New-Item CMD-let requires elevation to create symlinks even with Windows Developer Mode enabled.

## Installation

- Enable symlinks in .git/config.
- Add the `.engineering` subtree:

`git subtree add --prefix .engineering https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash`

- Check `README.md` in each directory in the `.engineering` subtree for futher installation steps.

## Updating

- From the repository root containing the `.engineering` subtree, execute `& .engineering\PullEngineering.ps1`.
- Follow the steps described in [the changelog](CHANGELOG.md).

## Features

The features provided by this repository are grouped by categories in the top-level directories. Each directory contains a `README.md` file describing the features in that category.

- [build](build/README.md)
- [deploy](deploy/README.md)
- [style](style/README.md)

## Modifying

To share modifications in the `.engineering` GIT subtree:

- Make sure that all documentation reflects your changes.
- Add an entry to [the changelog](CHANGELOG.md) to let others know which changes have been introduced and which actions are required when updating the `.engineering` GIT subtree in other repositories.
- Commit your changes.
- From the repository root containing the `.engineering` subtree, execute `& .engineering\PushEngineering.ps1`.
- Follow the [Updating](#updating) section in the other repositories containing the `.engineering` GIT subtree.

## Cloning

To clone a repository containing `.engineering` GIT subtree, use the following command:

`git clone -c core.symlinks=true <URL>`


# GitHubMilestoneCleaner

[![standard-readme compliant][]][standard-readme]
[![Contributor Covenant][contrib-covenantimg]][contrib-covenant]
[![Build][githubimage]][githubbuild]
[![NuGet package][nugetimage]][nuget]

.NET tool to clean GitHub milestones before doing a release.

## Table of Contents

- [Install](#install)
- [Usage](#usage)
  - [Available Commands](#commands)
    - [version-bumps](#version-bumps)
- [Maintainer](#maintainer)
- [Contributing](#contributing)
- [License](#license)

## Install

```pwsh
dotnet tool install -g GitHubMilestoneCleaner
```

## Usage

Get Help
```ps
dotnet gh-milestone-cleaner --help
```

### Commands

#### `version-bumps`

> Clean version bumps in a milestone

When dependabot or renovate are activated in a project it is possible that the same dependency is
bumped multiple times between releases. For clarity each release/milestone should contain only
the latest/newest release. The `version-bumps` command is used to cleanup multiple bumps of the
same dependency.

**Example:**
Silently remove all issues from the milestone, that are detected
as version bumps and have newer versions.

```ps
dotnet gh-milestone-cleaner version-bumps `
   -o [owner] `
   -r [repo-name] `
   -t [token] `
   -m [milestone] ` 
   -q
```

**Example:**
Interactively remove some issues from a milestone.

```ps
dotnet gh-milestone-cleaner version-bumps `
  -o cake-build `
  -r cake-rider `
  -t "my-secure-token" `
  -m 2.0.0 `
  -s "manually removed" `
  -i "manually removed"
```

## Maintainer

[Nils Andresen @nils-a][maintainer]

## Contributing

GitHubMilestoneCleaner follows the [Contributor Covenant][contrib-covenant] Code of Conduct.

We accept Pull Requests.

Small note: If editing the Readme, please conform to the [standard-readme][] specification.

## License

[MIT License © Nils Andresen][license]

[githubbuild]: https://github.com/nils-org/GitHubMilestoneCleaner/actions/workflows/build.yaml?query=branch%3Adevelop
[githubimage]: https://github.com/nils-org/GitHubMilestoneCleaner/actions/workflows/build.yaml/badge.svg?branch=develop
[maintainer]: https://github.com/nils-a
[contrib-covenant]: https://www.contributor-covenant.org/version/2/0/code_of_conduct/
[contrib-covenantimg]: https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg
[nuget]: https://nuget.org/packages/GitHubMilestoneCleaner
[nugetimage]: https://img.shields.io/nuget/v/GitHubMilestoneCleaner.svg?logo=nuget&style=flat-square
[license]: LICENSE.txt
[standard-readme]: https://github.com/RichardLitt/standard-readme
[standard-readme compliant]: https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square
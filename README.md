# Enterprise Startup

[![Coverage Status](https://coveralls.io/repos/github/ne1410s/DemoLibrary/badge.svg?branch=main)](https://coveralls.io/github/ne1410s/DemoLibrary?branch=main)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/161506d0d7d947a8999892f5bfad673d)](https://app.codacy.com/gh/ne1410s/DemoLibrary/dashboard)

[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fne1410s%2FDemoLibrary%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/ne1410s/DemoLibrary/main)

## Overview
This is a library designed to help get started with enterprise ASP.NET apps!

## Notes
### Commands
```powershell
# Restore tools
dotnet tool restore

# Run unit tests
gci **/TestResults/ | ri -r; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests
gci **/StrykerOutput/ | ri -r; dotnet stryker -o;

# Pack and publish a pre-release to a local feed
$suffix="alpha001"; dotnet pack -c Release -o nu --version-suffix $suffix; dotnet nuget push "nu\*.*$suffix.nupkg" --source localdev; gci nu/ | ri -r; rmdir nu;
```

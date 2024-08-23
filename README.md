# Enterprise Startup

[![Coverage Status](https://coveralls.io/repos/github/ne1410s/EnterpriseStartup/badge.svg?branch=main)](https://coveralls.io/github/ne1410s/EnterpriseStartup?branch=main)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/a29359ac9f90425892c9fa92e6079585)](https://app.codacy.com/gh/ne1410s/EnterpriseStartup/dashboard)

[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Fne1410s%2FEnterpriseStartup%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/ne1410s/EnterpriseStartup/main)


## Overview
This is a library designed to help get started with enterprise ASP.NET apps!

## Notes
### Commands
```powershell
# Restore tools
dotnet tool restore

# Run unit tests
rd -r ../**/TestResults/; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests
rd -r ../**/StrykerOutput/; dotnet stryker -o;

# Pack and publish a pre-release to a local feed
$suffix="alpha001"; dotnet pack -c Release -o nu --version-suffix $suffix; dotnet nuget push "nu\*.*$suffix.nupkg" --source localdev; gci nu/ | ri -r; rmdir nu;
```

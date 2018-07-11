@echo off

rem reset current directory to the location of this script
pushd "%~dp0"

dotnet build --force --no-incremental --configuration Release "./icfpc2018.sln" || exit /b 1

dotnet pack --no-build --configuration Release /p:NuspecFile="HoustonRunner.nuspec" /p:NuspecBasePath="./bin/Release" /p:NuspecProperties=\"PackageVersion=1.0.1\" "./houston-runner/houston-runner.csproj" || exit /b 1

pause
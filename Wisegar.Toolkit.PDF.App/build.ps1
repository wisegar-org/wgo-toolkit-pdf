Remove-Item -Path "./publish" -Recurse
# Trimming only works with self-contained deployments
# dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=true -o ./publish
msbuild /restore /t:build /p:TargetFramework=net9.0-windows10.0.19041 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x64 /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x64
# msbuild /restore /t:build /p:TargetFramework=net9.0-windows10.0.19041 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x86 /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x86

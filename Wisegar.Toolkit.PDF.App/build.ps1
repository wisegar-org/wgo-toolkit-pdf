Remove-Item -Path "./publish" -Recurse
# Trimming only works with self-contained deployments
dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=true -o ./publish
REM Compile installer
set Name="LocalDeviceAdapterInstaller"
set Suffix="net472"
set Sha="1234efab"
set Root=%~dp0..
echo %Root%
"c:\Program Files (x86)\Inno Setup 6\iscc.exe"^
 master.iss^
 /DRootDir="%Root%"^
 /DInstallerFileName=LocalDeviceAdapterInstaller_net7.0-windows^
 /DOutputDir="%Root%\output"^
 /DMainFile="%Root%\src\LocalDeviceAdapter\published\LocalDeviceAdapter.exe"^
 /DOtherFiles="%Root%\src\LocalDeviceAdapter\published"


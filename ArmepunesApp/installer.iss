[Setup]
AppName=DEPONIM I ARMEVE
AppVersion=3.1.0.0
AppPublisher=ArmepunesApp
DefaultDirName={autopf}\ArmepunesApp
DefaultGroupName=DEPONIM I ARMEVE
UninstallDisplayIcon={app}\ArmepunesApp.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=DeponimArm_eve_Installer
PrivilegesRequired=admin

[Languages]
Name: "albanian"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\DEPONIM I ARMEVE"; Filename: "{app}\ArmepunesApp.exe"
Name: "{commondesktop}\DEPONIM I ARMEVE"; Filename: "{app}\ArmepunesApp.exe"

[Run]
Filename: "{app}\ArmepunesApp.exe"; Description: "Nis aplikacionin"; Flags: postinstall nowait skipifsilent

# dir2iso
A command line tool to create an ISO file from a directory using .NET on Windows. It might be useful for command line automations and build pipelines such as Jenkins.

Refactored the GIST from: https://gist.github.com/sayurin/ea78ece46350214f8773#file-isoimage2-cs

Usage: ```dir2iso <myOutIsoFile.iso> <myVolumeName> <sourceDirectory> (optional)<FileSystemsToCreate>```

The FileSystemsToCreate to create is a bitfield:
```
typedef enum FsiFileSystems {
  FsiFileSystemNone,
  FsiFileSystemISO9660,
  FsiFileSystemJoliet,
  FsiFileSystemUDF,
  FsiFileSystemUnknown
} ;
```
For further documentation for IMAPI2: https://docs.microsoft.com/en-us/windows/win32/api/imapi2fs/ne-imapi2fs-fsifilesystems

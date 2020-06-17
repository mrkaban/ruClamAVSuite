Module disk_sector_functions
    Private Declare Function CreateFile Lib "kernel32" Alias "CreateFileA" (ByVal lpFileName As String, ByVal dwDesiredAccess As Long, ByVal dwShareMode As Long, lpSecurityAttributes As Long, ByVal dwCreationDisposition As Long, ByVal dwFlagsAndAttributes As Long, ByVal hTemplateFile As Long) As Long
    Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Long) As Long
    Private Declare Function ReadFile Lib "kernel32" (ByVal hFile As Long, lpBuffer As Object, ByVal nNumberOfBytesToRead As Long, lpNumberOfBytesRead As Long, ByVal lpOverlapped As Long) As Long '/ / declare has changed 
    Private Declare Function WriteFile Lib "kernel32" (ByVal hFile As Long, lpBuffer As Object, ByVal nNumberOfBytesToWrite As Long, lpNumberOfBytesWritten As Long, ByVal lpOverlapped As Long) As Long '/ / declare has changed 
    Private Declare Function SetFilePointer Lib "kernel32" (ByVal hFile As Long, ByVal lDistanceToMove As Long, lpDistanceToMoveHigh As Long, ByVal dwMoveMethod As Long) As Long

    Private Const GENERIC_READ = &H80000000
    Private Const GENERIC_WRITE = &H40000000

    Private Const FILE_SHARE_READ = &H1
    Private Const FILE_SHARE_WRITE = &H2
    Private Const OPEN_EXISTING = 3

    Private Const INVALID_HANDLE_VALUE = -1

    '/ / file seek 
    Private Const FILE_BEGIN = 0
    Private Const FILE_CURRENT = 1
    Private Const FILE_END = 2

    Private Const ERROR_SUCCESS = 0&

    '/ / device io control 
    Private Declare Function DeviceIoControl Lib "kernel32" (ByVal hDevice As Long, ByVal dwIoControlCode As Long, lpInBuffer As Object, ByVal nInBufferSize As Long, lpOutBuffer As Object, ByVal nOutBufferSize As Long, lpBytesReturned As Long, ByVal lpOverlapped As Long) As Long

    Private Const IOCTL_DISK_GET_DRIVE_GEOMETRY As Long = &H70000 '458752 
    Private Const IOCTL_STORAGE_GET_MEDIA_TYPES_EX As Long = &H2D0C04
    Private Const IOCTL_DISK_FORMAT_TRACKS As Long = &H7C018
    Private Const FSCTL_LOCK_VOLUME As Long = &H90018
    Private Const FSCTL_UNLOCK_VOLUME As Long = &H9001C
    Private Const FSCTL_DISMOUNT_VOLUME As Long = &H90020
    Private Const FSCTL_GET_VOLUME_BITMAP = &H9006F

    '/ / type 
    Private Structure LARGE_INTEGER
        Dim lowpart As Long
        Dim highpart As Long
    End Structure

    Private Enum MEDIA_TYPE
        Unknown
        F5_1Pt2_512
        F3_1Pt44_512
        F3_2Pt88_512
        F3_20Pt8_512
        F3_720_512
        F5_360_512
        F5_320_512
        F5_320_1024
        F5_180_512
        F5_160_512
        RemovableMedia
        FixedMedia
    End Enum

    Private Structure DISK_GEOMETRY
        Dim Cylinders As LARGE_INTEGER
        Dim MediaType As MEDIA_TYPE
        Dim TracksPerCylinder As Long
        Dim SectorsPerTrack As Long
        Dim BytesPerSector As Long
    End Structure

    '/ / private vars 
    Private hDisk As Long 'disk handle 
    Private lpGeometry As DISK_GEOMETRY 'disk info 
    Private lBufferSize As Long 'the buffer size of read / write 

    Private Function ReadHDBytes(ByVal ByteCount As Long, ByRef DataBytes() As Byte, ByRef ActuallyReadByte As Long) As Boolean
        Dim RetVal As Long
        RetVal = ReadFile(hDisk, DataBytes(0), ByteCount, ActuallyReadByte, 0)
        ReadHDBytes = Not (RetVal = 0)

    End Function

    Private Function WriteHDBytes(ByVal ByteCount As Long, ByRef DataBytes() As Byte) As Boolean
        Dim RetVal As Long
        Dim BytesToWrite As Long
        Dim BytesWritten As Long
        RetVal = WriteFile(hDisk, DataBytes(0), ByteCount, BytesWritten, 0)
        WriteHDBytes = Not (RetVal = 0)
    End Function
End Module

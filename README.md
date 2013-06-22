ReverseCryptoDrive
==============
ReverseCryptoDrive allows you to mount a folder as a drive in Windows that is encrypted on-the-fly (i.e. while data is read from the drive). It uses AES-CBC, encrypting each file block of 4KiB separateley to maintain random access. On the first run, you have to enter the path to mount as an encrypted view. All required crypto keys are generated and saved to a config file. The config file's path is displayed at the start. File and folder names are replaced by random strings with random lengths. The associations are stored in a B-Tree which is stored in the same directory as the config file. You may want to regularly create backups of this folder in order to be able to restore file names and decrypt the files. Encrypted file content is guaranteed to be the same across subsequent reads as long as the config file contains the same keys.

ReverseCryptoDrive is based on the Dokan user mode file system for Windows: http://dokan-dev.net/en/
This project uses some code from AzureBlobDrive which is available at:
https://github.com/downloads/richorama/AzureBlobDrive/

Installation
------------

ReverseCryptoDrive requires the Dokan library to be installed, this can be downloaded from here: http://dokan-dev.net/en/download/#dokan

If you run ByteStorm.ReverseCryptoDrive.exe, a new drive will appear in My Computer with the encrypted view of the chosen folder.

Configuration
-------------

By default, ReverseCryptoDrive mounts your folder on the R:\ drive, however you can change this in the config file.

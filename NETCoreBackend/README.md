# Project notes

The project requires a C++ compiler which for Windows can be found at `https://www.codeblocks.org/downloads/binaries/`

In its current phase, the project uses the g++.exe compiler from [GNU](https://sourceforge.net/projects/codeblocks/files/Binaries/20.03/Windows/codeblocks-20.03mingw-setup.exe/download)

For reasons which are related to security, the SHA256 algorithm is used and the hashed admin password is stored on the server
A tool which can make changing the password easier is using a HTTP GET request to send the word you want to encrypt and the server will show the hashed string

EXAMPLE: 

Request:		http://localhost:5024/cpptester/hash/toBeHashed
Server console: Hash of toBeHashed is [hashedWord]

In the application.json file the current configuration can be found, in order for this project to work on a different computer the LocalPaths section might need to be 
changed. CMDPath is the path to the cmd executable on your Windows PC, WorkingDirectoryPath is the contating directory if cmd, compiler path is previously explained and
self explanatory and UploadPath is the path to the directory on your computer where the problems will be uploaded 

HINT: SPACE characters ( and other special characters) might need to be escaped 

EXAMPLE:

Original path: C:\Program Files\CodeBlocks\MinGW\bin\g++.exe 
Escaped path:  C:\\\"Program Files\"\\CodeBlocks\\MinGW\\bin\\g++.exe 

NOTICE: the server can only have ONE token at a time which is deleted when the admin component is destroyed (on page closing/ on component routing)

TIPS: 
	 - the "ng build" command will compile the angular project to html, css and javascript files which can be accessed via browser
	 - to reproduce the server running environment might be trickier than the client environment, the project required a command interpreter and a C++ compiler
	   (cmd and g++ are recommended)
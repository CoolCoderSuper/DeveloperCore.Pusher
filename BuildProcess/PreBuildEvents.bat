rem This file should be put in a specific directory, i.e., C:\tools\vs, and that dir should be added to the dev machine's path
rem so that this file can be called during prebuild events to remove old files and prevent visual studio from not being able to rebuild after a crash.

echo PreBuildEvents 
echo  All arguments are %*
echo  $(TargetPath) is %1
echo  $(TargetFileName) is %2 
echo  $(TargetDir) is %3   
echo  $(TargetName) is %4

(if exist "%3*.old" del "%3*.old") 

(if exist "%3*.exe" ren "%3*.exe" *.exe.old) 
(if exist "%3*.dll" ren "%3*.dll" *.dll.old) 
(if exist "%3*.xml" ren "%3*.xml" *.xml.old) 
(if exist "%3*.pdb" ren "%3*.pdb" *.pdb.old)

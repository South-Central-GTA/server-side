#moves-files.bat
```bat
xcopy "south-central-resources\resources\*" "altv-server" /Y /E
xcopy "server-side\Server\bin\Debug\*" "altv-server\resources\southcentral\server" /Y /E
xcopy "client-side-ts\output\*" "altv-server\resources\southcentral\client" /Y /E
move "altv-server\resources\southcentral\server\resource.cfg" "altv-server\resources\southcentral\resource.cfg"
```

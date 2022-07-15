# mediaInfo-service
> MediaInfo Service
[![Windowns](https://github.com/OleksandrNazaruk/mediaInfo-service/actions/workflows/build_win_ci.yml/badge.svg)](https://github.com/OleksandrNazaruk/mediaInfo-service/actions/workflows/build_win_ci.yml) [![Linux](https://github.com/OleksandrNazaruk/mediaInfo-service/actions/workflows/build_linux_ci.yml/badge.svg)](https://github.com/OleksandrNazaruk/mediaInfo-service/actions/workflows/build_linux_ci.yml)

## Requirements

* Operating Systems: Linux or Windows
 - [.NET Runtime 6.0+](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 

## Compile and install (Linux)

### Compile from source
Once you have installed all the dependencies, get the code:

```bat
git clone https://github.com/OleksandrNazaruk/mediaInfo-service.git
cd mediaInfo-service
```

Then just use:

```bat
sudo mkdir /opt/mediaInfo-service/bin
dotnet restore
dotnet build
dotnet publish --runtime linux-x64 --output /opt/mediaInfo-service/bin -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true .\mediaInfo-service
```

### Install as daemon
   
```bat
sudo nano /etc/systemd/system/mediaInfo-service.service
```

The content of the file will be the following one

```ini
[Unit]
Description=MediaInfo Service (FreeHand)

[Service]
Type=notify
WorkingDirectory=/opt/mediaInfo-service/etc/mediaInfo-service
Restart=always
RestartSec=10
KillSignal=SIGINT
ExecStart=/opt/mediaInfo-service/bin/mediaInfo-service
Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install]
WantedBy=multi-user.target
```

Add daemon to startup
```bat
sudo systemctl daemon-reload
sudo systemctl start mediaInfo-service
sudo systemctl status mediaInfo-service
sudo systemctl enable mediaInfo-service
```


## Compile and install (Windows Service)

### Compile from source
Once you have installed all the dependencies, get the code:

```bat
git clone https://github.com/OleksandrNazaruk/mediaInfo-service.git
cd mediaInfo-service
```

Then just use:

```bat
mkdir "%ProgramData%\FreeHand\mediaInfo-service\bin\"
dotnet restore
dotnet build
dotnet publish --runtime win-x64 --output %ProgramData%\FreeHand\mediaInfo-service\bin\ -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true .\mediaInfo-service
```

### Install from binary
Unzip the mediaInfo-service.zip archive to `"%ProgramData%\FreeHand\mediaInfo-service\bin\`
  
```powershell
PS> Expand-Archive -Path "mediaInfo-service" -DestinationPath "%ProgramData%\FreeHand\mediaInfo-service\bin\"
```

### Install as Windows Service
   
```bat
sc create mediaInfo-service
binPath= "%ProgramData%\FreeHand\mediaInfo-service\bin\mediaInfo-service.exe" 
DisplayName= "MediaInfo Service (FreeHand)" 
start= auto
description= ""
```


## Configure and start
To start the service, you can use the `mediaInfo-service` executable as the application or `sc start mediaInfo-service` as a Windows service or `sudo systemctl start mediaInfo-service` as a daemon. For configuration you can edit a configuration file:

Windows
```bat
notepad.exe %ProgramData%\FreeHand\mediaInfo-service\mediaInfo-service.conf
```

Linux
```bat
sudo nano /opt/mediaInfo-service/etc/mediaInfo-service/mediaInfo-service.conf
```

The content of the file will be the following one
```ini
#
[Logging:LogLevel]
Default=Information
Microsoft=None
Microsoft.Hosting.Lifetime=None
mediaInfo-service=Debug
_MediaInfoService.Controllers.MediaInfoController=Debug
_MediaInfoService.Controllers.VolumeDetectController=Debug
_MediaInfoService.Controllers.ThumbnailController=Debug
_MediaInfoService.Models.BackgroundTaskQueue=Debug
_MediaInfoService.FFmpegLogger=Debug

#
[Kestrel:Endpoints]
Http:Url=http://*:55564
```

## Docker

```bat

```

## Health checks
Health checks are part of the middleware and libraries to help report the health of application infrastructure components. We can monitor the functioning of an application in real time, in a simple and direct way. 

To know if an API is running in a healthy way or not, we use a health check to validate the status of a service and its dependencies through an endpoint in the API of the REST service.

This allows us to quickly and in a standardized manner decide if the service or our dependences is off.

This endpoint uses a separate service that assesses the availability of the functions of the rest of the system and also the state of its dependencies.
The information collected during the check can include performance calculations, runtime or connection to other downstream services. After completing the evaluation, an HTTP code and a JSON object are returned depending on the evaluation of the services in the health check.


### Request
```bat
curl -X 'GET' \
  'http://localhost:55564/health' \
  -H 'accept: application/json'
```

### Response

> Code: `200` (OK)
```json
{
   "Name":"mediaInfo-service",
   "Status":"Healthy",
   "Duration":"00:00:00.1490277",
   "Services":[
      {
         "Key":"MediaInfo.Service.Health",
         "Description":"A healthy result.",
         "Duration":"00:00:00.0138317",
         "Status":"Healthy",
         "Data":[
            
         ]
      }
   ]
}
```

## High availability
[HAProxy](http://www.haproxy.org/) is a free, very fast and reliable reverse-proxy offering high availability, load balancing, and proxying for TCP and HTTP-based applications.

### HAProxy backend sample config
```yaml
backend mediaInfo-service
  option httpchk
  http-check send meth GET uri /health
  http-check expect status 200
  server server1 192.168.50.2:80 check
  server server2 192.168.50.3:80 check
  server server3 192.168.50.4:80 check
```
 
## Rest API
> [OpenAPI](http://localhost:55564/swagger/index.html)
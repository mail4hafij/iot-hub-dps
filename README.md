# iot-hub-dps
The idea here is to deploy iot-devices at scale using Azure Device Provisioning Service (DPS). 
We cover both Individual Enrollments and Group Enrollments procedures with DPS. We then send telemetry data from our provisioned iot devices to azure iot hub. 

## Conceptual model - IoT Hub Message Routing
<img src="iot-hub.jpg" />

## DPS Group Enrollment

### create test certificate
read - https://docs.microsoft.com/en-us/azure/iot-hub/tutorial-x509-openssl
follow - https://github.com/dotnet/samples/blob/main/iot/dotnet-iot-and-nanoframework/create-certificate.md
demo client - https://github.com/MicrosoftLearning/MSLearnLabs-AZ-220-Microsoft-Azure-IoT-Developer/blob/master/Allfiles/Labs/06-Automatic%20Enrollment%20of%20Devices%20in%20DPS/Final/ContainerDevice/Program.cs

### way 1 (the way I did it):
https://github.com/dotnet/samples/blob/main/iot/dotnet-iot-and-nanoframework/create-certificate.md

### way 2:
Download the certGen.sh, openssl_root_ca.cnf, and openssl_device_intermediate_ca.cnf from https://github.com/Azure/azure-iot-sdk-c/tree/main/tools/CACertificates
Follow - https://github.com/Azure/azure-iot-sdk-c/blob/main/tools/CACertificates/CACertificateOverview.md


## TempSimulator
Add property in key value pair to set routing. Add Message Routing rule in azure iot-hub.


## Function App
Built-in-endpoint
custorm Blob-endpoint

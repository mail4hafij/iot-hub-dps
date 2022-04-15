# iot-hub-dps
The idea here is to deploy iot-devices at scale using Azure Device Provisioning Service (DPS). 
We cover both Individual Enrollments and Group Enrollments procedures with DPS. We then send telemetry data from our provisioned iot devices to azure iot hub. 

## Conceptual model - IoT Hub Message Routing
<img src="iot-hub.jpg" />

## DPS Group Enrollment

### create test certificates
Microsoft docs for details - https://docs.microsoft.com/en-us/azure/iot-hub/tutorial-x509-openssl

A demo client from Microsoft - https://github.com/MicrosoftLearning/MSLearnLabs-AZ-220-Microsoft-Azure-IoT-Developer/blob/master/Allfiles/Labs/06-Automatic%20Enrollment%20of%20Devices%20in%20DPS/Final/ContainerDevice/Program.cs

### way 1 (the way I did it):
Minimumal docs - https://github.com/dotnet/samples/blob/main/iot/dotnet-iot-and-nanoframework/create-certificate.md

### way 2:
Download the following from https://github.com/Azure/azure-iot-sdk-c/tree/main/tools/CACertificates
1. certGen.sh, 
2. openssl_root_ca.cnf, 
3. and openssl_device_intermediate_ca.cnf 

Then follow - https://github.com/Azure/azure-iot-sdk-c/blob/main/tools/CACertificates/CACertificateOverview.md


## TempSimulator
I added a dummy temperature simulator built in .net core 3.1. 
Add properties in key value pair while sending the telemetry data. Add Message Routing rule in azure iot-hub according to those key value pair that were added in the client.


## Function App
There are two functions apps 
1. Built-in-endpoint - Listening to default iot-hub.
2. Blob-endpoint - Listening to blob storage for those messages that were routed to blob container.



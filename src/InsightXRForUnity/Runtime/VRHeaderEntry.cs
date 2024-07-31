using System.IO;
using UnityEngine;

namespace InsightDesk
{
    public class VRHeaderEntry
    {
        public string deviceName;
        public float displayFrequency;
        public string pcName;
        public string cpuDetails;
        public string gpuDetails;
        public float batteryLevel;
        public string operatingsystem;
        public string processorfrequency;
        public string memorysize;
        public string city;
        public string country;
        public string region;
        public float latitude;
        public float longitude;
        public string engine;
        public string engineVersion;
        public string projectName;

        public VRHeaderEntry(string deviceName, float displayFrequency, string pcName, string cpuDetails, string gpuDetails,
            float batteryLevel, string operatingsystem, string memorysize, string processorfrequency,
            string city, string country, string region, float latitude, float longitude, string engine, string engineVersion, string projectName)
        {
            this.deviceName = deviceName;
            this.displayFrequency = displayFrequency;
            this.pcName = pcName;
            this.cpuDetails = cpuDetails;
            this.gpuDetails = gpuDetails;
            this.batteryLevel = batteryLevel;
            this.operatingsystem = operatingsystem;
            this.memorysize = memorysize;
            this.processorfrequency = processorfrequency;
            this.city = city;
            this.country = country;
            this.region = region;
            this.latitude = latitude;
            this.longitude = longitude;
            this.engine = engine;
            this.engineVersion = engineVersion;
            this.projectName = projectName;
        }

        public static void Write(InsightBuffer buffer, string deviceName, float displayFrequency, string pcName, string cpuDetails,
            string gpuDetails, float batteryLevel, string operatingsystem, string memorysize, string processorfrequency,
            string city, string country, string region, float latitude, float longitude, string engine, string engineVersion, string projectName)
        {
            buffer.Write(deviceName);
            buffer.Write(displayFrequency);
            buffer.Write(pcName);
            buffer.Write(cpuDetails);
            buffer.Write(gpuDetails);
            buffer.Write(batteryLevel);
            buffer.Write(operatingsystem);
            buffer.Write(memorysize);
            buffer.Write(processorfrequency);
            buffer.Write(city);
            buffer.Write(country);
            buffer.Write(region);
            buffer.Write(latitude);
            buffer.Write(longitude);
            buffer.Write(engine);
            buffer.Write(engineVersion);
            buffer.Write(projectName);
        }

        public VRHeaderEntry(BinaryReader binaryReader)
        {
            deviceName = binaryReader.ReadString();
            displayFrequency = binaryReader.ReadSingle();
            pcName = binaryReader.ReadString();
            cpuDetails = binaryReader.ReadString();
            gpuDetails = binaryReader.ReadString();
            batteryLevel = binaryReader.ReadSingle();
            operatingsystem = binaryReader.ReadString();
            memorysize = binaryReader.ReadString();
            processorfrequency = binaryReader.ReadString();
            city = binaryReader.ReadString();
            country = binaryReader.ReadString();
            region = binaryReader.ReadString();
            latitude = binaryReader.ReadSingle();
            longitude = binaryReader.ReadSingle();
            engine = binaryReader.ReadString();
            engineVersion = binaryReader.ReadString();
            projectName = binaryReader.ReadString();
            // Debug.Log($"Constructor - Device Name: {deviceName}, Display Frequency: {displayFrequency}, PC Name: {pcName}, CPU Details: {cpuDetails}, GPU Details: {gpuDetails}, Battery Level: {batteryLevel}, OS: {operatingsystem}, Memory Size: {memorysize}, Processor Frequency: {processorfrequency}, City: {city}, Country: {country},Region: {region} Latitude: {latitude}, Longitude: {longitude}, Engine: {engine}, EngineVersion: {engineVersion}, projectName: {projectName}");
        }
    }
}

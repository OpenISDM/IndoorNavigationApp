using IndoorNavigation.Models;
using IndoorNavigation.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndoorNavigationTest
{
    class GenerateMapData
    {
        public static void Generate()
        {
            Dictionary<Guid,Beacon> beacons = GenerateBeacons().ToDictionary(c => c.UUID);

            List<WaypointModel> beaconGroups = GenerateBeaconGroup(beacons);

            List<LocationConnectModel> locationConnects = GenerateLocationConnect(beaconGroups);

            Utility.BeaconsDict = beacons;
            Utility.Waypoints = beaconGroups;
            Utility.LocationConnects = locationConnects;
        }

        private static List<Beacon> GenerateBeacons()
        {
            List<Beacon> beacons = new List<Beacon>()
            {
                // A
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-4c3d-c941-0000c35ef342"),
                    Threshold = -70,
                    Type = BeaconType.Waypoint
                },
                // B
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-803d-c941-0000d45ef342"),
                    Threshold = -75,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-563d-c941-0000d55ef342"),
                    Threshold = -65,
                    Type = BeaconType.Waypoint
                },
                // C
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-563d-c941-0000e85ef342"),
                    Threshold = -70,
                    Type = BeaconType.Waypoint
                },
                // D
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-223d-c941-0000175ff342"),
                    Threshold = -40,
                    Type = BeaconType.Waypoint
                },
                // E
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-de3c-c941-0000ed5ef342"),
                    Threshold = -30,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342"),
                    Threshold = -80,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-f33c-c941-0000195ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                // F
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-b43c-c941-0000ed5ef342"),
                    Threshold = -40,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-be3c-c941-0000025ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-be3c-c941-0000185ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                // G
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-aa3d-c941-0000375ff342"),
                    Threshold = -45,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-713d-c941-0000395ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                // H
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-b03d-c941-0000575ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-763d-c941-0000575ff342"),
                    Threshold = -70,
                    Type = BeaconType.Waypoint
                },
                // I
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-323d-c941-0000585ff342"),
                    Threshold = -80,
                    Type = BeaconType.Waypoint
                },
                // J
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-f83c-c941-00005a5ff342"),
                    Threshold = -90,
                    Type = BeaconType.Waypoint
                },
                // K
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342"),
                    Threshold = -40,
                    Type = BeaconType.Waypoint
                },
                // L
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-ee3c-c941-0000395ff342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("0000803f-0000-be3c-c941-0000395ff342"),
                    Threshold = -80,
                    Type = BeaconType.Waypoint
                },
                // M
                new LBeaconModel{
                    UUID = Guid.Parse("00000000-0000-7b3d-c941-0000ae5ef342"),
                    Threshold = -75,
                    Type = BeaconType.Waypoint
                },
                // N
                new LBeaconModel{
                    UUID = Guid.Parse("00000000-0000-a03d-c941-0000695ff342"),
                    Threshold = -40,
                    Type = BeaconType.Waypoint
                },
                // O
                new LBeaconModel{
                    UUID = Guid.Parse("00000000-0000-8a3c-c941-0000045ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-7b3d-c941-0000c15ef342"),
                    Threshold = -80,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-4c3d-c941-0000c35ef342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-803d-c941-0000d45ef342"),
                    Threshold = -65,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-563d-c941-0000d55ef342"),
                    Threshold = -75,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-863d-c941-0000ea5ef342"),
                    Threshold = -95,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-563d-c941-0000e85ef342"),
                    Threshold = -45,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-183d-c941-0000eb5ef342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-223d-c941-0000ff5ef342"),
                    Threshold = -80,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-223d-c941-0000175ff342"),
                    Threshold = -70,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-de3c-c941-0000ed5ef342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-ee3c-c941-0000005ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-f33c-c941-0000195ff342"),
                    Threshold = -45,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-b43c-c941-0000ed5ef342"),
                    Threshold = -35,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-be3c-c941-0000025ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-be3c-c941-0000185ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-aa3d-c941-0000375ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-713d-c941-0000395ff342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-b03d-c941-0000575ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-763d-c941-0000575ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-323d-c941-0000585ff342"),
                    Threshold = -45,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-f83c-c941-00005a5ff342"),
                    Threshold = -60,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-b93c-c941-00005a5ff342"),
                    Threshold = -65,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-ee3c-c941-0000395ff342"),
                    Threshold = -55,
                    Type = BeaconType.Waypoint
                },
                new LBeaconModel{
                    UUID = Guid.Parse("00000040-0000-be3c-c941-0000395ff342"),
                    Threshold = -50,
                    Type = BeaconType.Waypoint
                }
            };

            return beacons;
        }

        private static List<WaypointModel> GenerateBeaconGroup(Dictionary<Guid, Beacon> beacons)
        {
            List<WaypointModel> beaconGroups = new List<WaypointModel>()
            {
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "A1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-7b3d-c941-0000c15ef342")],
                        beacons[Guid.Parse("0000803f-0000-4c3d-c941-0000c35ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "B1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-803d-c941-0000d45ef342")],
                        beacons[Guid.Parse("0000803f-0000-563d-c941-0000d55ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "C1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-863d-c941-0000ea5ef342")],
                        beacons[Guid.Parse("0000803f-0000-563d-c941-0000e85ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "D1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-183d-c941-0000eb5ef342")],
                        beacons[Guid.Parse("0000803f-0000-223d-c941-0000ff5ef342")],
                        beacons[Guid.Parse("0000803f-0000-223d-c941-0000175ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "E1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-de3c-c941-0000ed5ef342")],
                        beacons[Guid.Parse("0000803f-0000-ee3c-c941-0000005ff342")],
                        beacons[Guid.Parse("0000803f-0000-f33c-c941-0000195ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "F1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-b43c-c941-0000ed5ef342")],
                        beacons[Guid.Parse("0000803f-0000-be3c-c941-0000025ff342")],
                        beacons[Guid.Parse("0000803f-0000-be3c-c941-0000185ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "G1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-aa3d-c941-0000375ff342")],
                        beacons[Guid.Parse("0000803f-0000-713d-c941-0000395ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "H1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-b03d-c941-0000575ff342")],
                        beacons[Guid.Parse("0000803f-0000-763d-c941-0000575ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "I1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-323d-c941-0000585ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "J1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-f83c-c941-00005a5ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "K1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-b93c-c941-00005a5ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "L1F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("0000803f-0000-ee3c-c941-0000395ff342")],
                        beacons[Guid.Parse("0000803f-0000-be3c-c941-0000395ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "M",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000000-0000-7b3d-c941-0000ae5ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "N",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000000-0000-a03d-c941-0000695ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "O",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000000-0000-8a3c-c941-0000045ff342")]
                        
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "A2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-7b3d-c941-0000c15ef342")],
                        beacons[Guid.Parse("00000040-0000-4c3d-c941-0000c35ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "B2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-803d-c941-0000d45ef342")],
                        beacons[Guid.Parse("00000040-0000-563d-c941-0000d55ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "C2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-863d-c941-0000ea5ef342")],
                        beacons[Guid.Parse("00000040-0000-563d-c941-0000e85ef342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "D2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-183d-c941-0000eb5ef342")],
                        beacons[Guid.Parse("00000040-0000-223d-c941-0000ff5ef342")],
                        beacons[Guid.Parse("00000040-0000-223d-c941-0000175ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "E2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-de3c-c941-0000ed5ef342")],
                        beacons[Guid.Parse("00000040-0000-ee3c-c941-0000005ff342")],
                        beacons[Guid.Parse("00000040-0000-f33c-c941-0000195ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "F2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-b43c-c941-0000ed5ef342")],
                        beacons[Guid.Parse("00000040-0000-be3c-c941-0000025ff342")],
                        beacons[Guid.Parse("00000040-0000-be3c-c941-0000185ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "G2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-aa3d-c941-0000375ff342")],
                        beacons[Guid.Parse("00000040-0000-713d-c941-0000395ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "H2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-b03d-c941-0000575ff342")],
                        beacons[Guid.Parse("00000040-0000-763d-c941-0000575ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "I2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-323d-c941-0000585ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "J2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-f83c-c941-00005a5ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "K2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-b93c-c941-00005a5ff342")]
                    }
                },
                new WaypointModel
                {
                    Id = Guid.NewGuid(),
                    Name = "L2F",
                    Beacons = new List<Beacon>{
                        beacons[Guid.Parse("00000040-0000-ee3c-c941-0000395ff342")],
                        beacons[Guid.Parse("00000040-0000-be3c-c941-0000395ff342")]
                    }
                }
            };

            return beaconGroups;
        }

        private static List<LocationConnectModel> GenerateLocationConnect(List<WaypointModel> beaconGroups)
        {
            List<LocationConnectModel> locationConnects = new List<LocationConnectModel>()
            {
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "A1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "B1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "B1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "C1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "C1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "D1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "D1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "E1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "E1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "F1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "F1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "L1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "L1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "K1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "K1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "J1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "J1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "I1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "I1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "H1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "H1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "G1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "G1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "D1F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "A1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "M").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "A2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "M").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "H1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "N").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "H2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "N").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "F1F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "O").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "F2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "O").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "A2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "B2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "B2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "C2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "C2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "D2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "D2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "E2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "E2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "F2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "F2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "L2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "L2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "K2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "K2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "J2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "J2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "I2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "I2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "H2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "H2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "G2F").First(),
                    IsTwoWay = true
                },
                new LocationConnectModel
                {
                    BeaconA = beaconGroups.Where(c => c.Name == "G2F").First(),
                    BeaconB = beaconGroups.Where(c => c.Name == "D2F").First(),
                    IsTwoWay = true
                }
            };

            return locationConnects;
        }
    }
}

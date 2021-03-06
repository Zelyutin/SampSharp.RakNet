﻿// SampSharp.RakNet
// Copyright 2018 Danil Zelyutin
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;

using SampSharp.GameMode;

using SampSharp.RakNet.Events;
using SampSharp.RakNet.Definitions;

namespace SampSharp.RakNet.Syncs
{
    public class UnoccupiedSync : ISync
    {
        public BitStream BS { get; set; }

        public int PacketId { get; set; }
        public int FromPlayerId { get; set; }

        public int VehicleId { get; set; }
        public int SeatId { get; set; }
        public Vector3 Roll { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public float VehicleHealth { get; set; }

        public UnoccupiedSync(BitStream bs)
        {
            BS = bs;
        }
        public void ReadIncoming()
        {
            Read(false);
        }
        public void ReadOutcoming()
        {
            Read(true);
        }
        public void WriteIncoming()
        {
            Write(false);
        }
        public void WriteOutcoming()
        {
            Write(true);
        }
        private void Read(bool outcoming)
        {
            var arguments = new List<object>()
            {
                ParamType.UInt8, "packetId",
                ParamType.UInt16, "vehicleId",
                ParamType.UInt8, "seatId",
                ParamType.Float, "roll_0",
                ParamType.Float, "roll_1",
                ParamType.Float, "roll_2",
                ParamType.Float, "direction_0",
                ParamType.Float, "direction_1",
                ParamType.Float, "direction_2",
                ParamType.Float, "position_0",
                ParamType.Float, "position_1",
                ParamType.Float, "position_2",
            };

            if (outcoming)
            {
                arguments.Insert(2, ParamType.UInt16);
                arguments.Insert(3, "fromPlayerId");
            }

            var result = BS.ReadValue(arguments.ToArray());
            //Need to divide up the reading cause of native arguments limit(32) in SampSharp.

            PacketId = (int)result["packetId"];
            if (outcoming)
            {
                FromPlayerId = (int)result["fromPlayerId"];
            }

            VehicleId = (int)result["vehicleId"];
            SeatId = (int)result["seatId"];
            Roll = new Vector3((float)result["roll_0"], (float)result["roll_1"], (float)result["roll_2"]);
            Direction = new Vector3((float)result["direction_0"], (float)result["direction_1"], (float)result["direction_2"]);
            Position = new Vector3((float)result["position_0"], (float)result["position_1"], (float)result["position_2"]);

            result = BS.ReadValue(
                ParamType.Float, "velocity_0",
                ParamType.Float, "velocity_1",
                ParamType.Float, "velocity_2",
                ParamType.Float, "angularVelocity_0",
                ParamType.Float, "angularVelocity_1",
                ParamType.Float, "angularVelocity_2",
                ParamType.Float, "vehicleHealth"
            );

            Velocity = new Vector3((float)result["velocity_0"], (float)result["velocity_1"], (float)result["velocity_2"]);
            AngularVelocity = new Vector3((float)result["angularVelocity_0"], (float)result["angularVelocity_1"], (float)result["angularVelocity_2"]);
            VehicleHealth = (float)result["vehicleHealth"];
        }
        private void Write(bool outcoming)
        {
            var arguments = new List<object>()
            {
                ParamType.UInt8, PacketId,
                ParamType.UInt16, VehicleId,
                ParamType.UInt8, SeatId,
                ParamType.Float, Roll.X,
                ParamType.Float, Roll.Y,
                ParamType.Float, Roll.Z,
                ParamType.Float, Direction.X,
                ParamType.Float, Direction.Y,
                ParamType.Float, Direction.Z,
                ParamType.Float, Position.X,
                ParamType.Float, Position.Y,
                ParamType.Float, Position.Z,
            };

            if (outcoming)
            {
                arguments.Insert(2, ParamType.UInt16);
                arguments.Insert(3, FromPlayerId);
            }

            BS.WriteValue(arguments.ToArray());

            arguments = new List<object>()
            {
                ParamType.Float, Velocity.X,
                ParamType.Float, Velocity.Y,
                ParamType.Float, Velocity.Z,
                ParamType.Float, AngularVelocity.X,
                ParamType.Float, AngularVelocity.Y,
                ParamType.Float, AngularVelocity.Z,
                ParamType.Float, VehicleHealth,
            };

            BS.WriteValue(arguments.ToArray());
        }
    }
}
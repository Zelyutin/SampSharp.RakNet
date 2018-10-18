﻿using System;
using System.Collections.Generic;

using SampSharp.GameMode;

using SampSharp.RakNet.Events;
using SampSharp.RakNet.Definitions;

namespace SampSharp.RakNet.Syncs
{
    public class DriverSync : ISync
    {
        public event EventHandler<SyncReadEventArgs> ReadCompleted;

        public BitStream BS;

        public int packetID;
        public int fromPlayerID;
        public int vehicleID;
        public int lrKey;
        public int udKey;
        public int keys;
        public Vector4 quaternion;
        public Vector3 position;
        public Vector3 velocity;
        public float vehicleHealth;
        public int playerHealth;
        public int playerArmour;
        public int additionalKey;
        public int weaponID;
        public int sirenState;
        public int landingGearState;
        public int trailerID;
        public float trainSpeed;

        public DriverSync(BitStream bs)
        {
            this.BS = bs;
        }
        public void ReadIncoming()
        {
            BS.ReadCompleted += (sender, args) =>
            {
                var result = args.Result;
                this.packetID = (int)result["packetID"];

                this.vehicleID = (int)result["vehicleID"];
                this.lrKey = (int)result["lrKey"];
                this.udKey = (int)result["udKey"];
                this.keys = (int)result["keys"];
                this.quaternion = new Vector4((float)result["quaternion_X"], (float)result["quaternion_Y"], (float)result["quaternion_Z"], (float)result["quaternion_W"]); // order is different from one in a bitstream
                this.position = new Vector3((float)result["position_0"], (float)result["position_1"], (float)result["position_2"]);


                var BS2 = new BitStream(BS.ID);
                BS2.ReadCompleted += (sender2, args2) =>
                {
                    result = args2.Result;

                    this.velocity = new Vector3((float)result["velocity_0"], (float)result["velocity_1"], (float)result["velocity_2"]);
                    this.vehicleHealth = (float)result["vehicleHealth"];
                    this.playerHealth = (int)result["playerHealth"];
                    this.playerArmour = (int)result["playerArmour"];
                    this.additionalKey = (int)result["additionalKey"];
                    this.weaponID = (int)result["weaponID"];
                    this.sirenState = (int)result["sirenState"];
                    this.landingGearState = (int)result["landingGearState"];
                    this.trailerID = (int)result["trailerID"];
                    this.trainSpeed = (float)result["trainSpeed"];

                    this.ReadCompleted.Invoke(this, new SyncReadEventArgs(this));
                };

                BS2.ReadValue(
                    ParamType.Float, "velocity_0",
                    ParamType.Float, "velocity_1",
                    ParamType.Float, "velocity_2",
                    ParamType.Float, "vehicleHealth",
                    ParamType.Uint8, "playerHealth",
                    ParamType.Uint8, "playerArmour",
                    ParamType.Bits, "additionalKey", 2,
                    ParamType.Bits, "weaponID", 6,
                    ParamType.Uint8, "sirenState",
                    ParamType.Uint8, "landingGearState",
                    ParamType.Uint16, "trailerID",
                    ParamType.Float, "trainSpeed"
                );
            };

            var arguments = new List<object>()
            {
                ParamType.Uint8, "packetID",
                ParamType.Uint16, "vehicleID",
                ParamType.Uint16, "lrKey",
                ParamType.Uint16, "udKey",
                ParamType.Uint16, "keys",
                ParamType.Float, "quaternion_W",
                ParamType.Float, "quaternion_X",
                ParamType.Float, "quaternion_Y",
                ParamType.Float, "quaternion_Z",
                ParamType.Float, "position_0",
                ParamType.Float, "position_1",
                ParamType.Float, "position_2",

            };

            BS.ReadValue(arguments.ToArray());
            //Need to divide up the reading cause of native arguments limit(32) in SampSharp.
        }
        public void ReadOutcoming()
        {
            this.packetID = this.BS.ReadUint8();
            this.fromPlayerID = this.BS.ReadUint16();
            this.vehicleID = this.BS.ReadUint16();

            // LEFT/RIGHT KEYS
            this.lrKey = this.BS.ReadUint16();

            // UP/DOWN KEYS
            this.udKey = this.BS.ReadUint16();

            // GENERAL KEYS
            this.keys = this.BS.ReadUint16();
            
            // ROTATION
            this.quaternion = BS.ReadNormQuat();

            float x = BS.ReadFloat();
            float y = BS.ReadFloat();
            float z = BS.ReadFloat();
            this.position = new Vector3(x, y, z);

            this.velocity = BS.ReadVector();
            this.vehicleHealth = (float)BS.ReadUint16();

            byte healthArmour = Convert.ToByte(BS.ReadUint8());
            HealthArmour.GetFromByte(healthArmour, ref this.playerHealth, ref this.playerArmour);

            this.weaponID = BS.ReadUint8();

            bool sirenState = BS.ReadCompressedBool();
            if(sirenState)
                this.sirenState = 1;

            bool landingGear = BS.ReadCompressedBool();
            if (landingGear)
                this.landingGearState = 1;

            // HYDRA THRUST ANGLE AND TRAILER ID
            bool hydra = BS.ReadCompressedBool();
            bool trailer = BS.ReadCompressedBool();

            
            int trailerID_or_thrustAngle = BS.ReadUint32();
            bool train = BS.ReadCompressedBool();

            if (train)
            {
                this.trainSpeed = (float)BS.ReadUint8();
            }

            this.ReadCompleted.Invoke(this, new SyncReadEventArgs(this));
        }
        public void WriteIncoming()
        {
            var arguments = new List<object>()
            {
                ParamType.Uint8, this.packetID,
                ParamType.Uint16, this.vehicleID,
                ParamType.Uint16, this.lrKey,
                ParamType.Uint16, this.udKey,
                ParamType.Uint16, this.keys,
                ParamType.Float, this.quaternion.W,
                ParamType.Float, this.quaternion.X,
                ParamType.Float, this.quaternion.Y,
                ParamType.Float, this.quaternion.Z,
                ParamType.Float, this.position.X,
                ParamType.Float, this.position.Y,
                ParamType.Float, this.position.Z,
                ParamType.Uint16, this.fromPlayerID
            };

            BS.WriteValue(arguments.ToArray());

            arguments = new List<object>()
            {
                ParamType.Float, this.velocity.X,
                ParamType.Float, this.velocity.Y,
                ParamType.Float, this.velocity.Z,
                ParamType.Float, this.vehicleHealth,
                ParamType.Uint8, this.playerHealth,
                ParamType.Uint8, this.playerArmour,
                ParamType.Bits, this.additionalKey, 2,
                ParamType.Bits, this.weaponID, 6,
                ParamType.Uint8, this.sirenState,
                ParamType.Uint8, this.landingGearState,
                ParamType.Uint16, this.trailerID,
                ParamType.Float, this.trainSpeed
            };

            BS.WriteValue(arguments.ToArray());
        }
        public void WriteOutcoming()
        {
            BS.WriteValue(
                ParamType.Uint8, this.packetID,
                ParamType.Uint16, this.fromPlayerID,
                ParamType.Uint16, this.vehicleID,
                ParamType.Uint16, this.lrKey,
                ParamType.Uint16, this.udKey,
                ParamType.Uint16, this.keys
           );

            BS.WriteNormQuat(this.quaternion);
            
            BS.WriteValue(
               ParamType.Float, this.position.X,
               ParamType.Float, this.position.Y,
               ParamType.Float, this.position.Z
            );

            BS.WriteVector(this.velocity);
            BS.WriteUint16((int)this.vehicleHealth);

            byte healthArmour = HealthArmour.SetInByte(this.playerHealth, this.playerArmour);
            BS.WriteUint8((int)healthArmour);
            BS.WriteUint8(this.weaponID);
            
            if (this.sirenState == 1)
                BS.WriteBool(true);
            else
                BS.WriteBool(false);

            if (this.landingGearState == 1)
                BS.WriteBool(true);
            else
                BS.WriteBool(false);

            // HYDRA THRUST ANGLE AND TRAILER ID
            BS.WriteBool(false);
            BS.WriteBool(false);

            int trailerID_or_thrustAngle = 0;
            BS.WriteUint32(trailerID_or_thrustAngle);

            // TRAIN SPECIAL
            BS.WriteBool(false);
        }
    }
}

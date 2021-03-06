﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Files.Formats.IFF.Chunks
{
    /// <summary>
    /// This class defines a single family in the neighbourhood, and various properties such as their 
    /// budget and house assignment. These can be modified using GenericTS1Calls, but are mainly
    /// defined within CAS.
    /// </summary>
    public class FAMI : IffChunk
    {
        public uint Version;

        public int HouseNumber;
        //this is not a typical family number - it is unique between user created families, but -1 for townies.
        //i believe it is an alternate family UID that basically runs on an auto increment to obtain its value.
        //(in comparison with the ChunkID as family that is used ingame, which appears to fill spaces as they are left)
        public int Offset;
        public int FamilyNumber;
        public int Budget;
        public int NetWorth;
        public int FamilyFriends;
        public int Unknown; //19, 17 or 1? could be flags, (1, 16, 2) ... 0 for townies
        public uint[] FamilyGUIDs;

        public uint[] RuntimeSubset; //the members of this family currently active. don't save!

        public void SelectWholeFamily()
        {
            RuntimeSubset = FamilyGUIDs;
        }

        public void SelectOneMember(uint guid)
        {
            RuntimeSubset = new uint[] { guid };
        }

        /// <summary>
        /// Reads a FAMI chunk from a stream.
        /// </summary>
        /// <param name="iff">An Iff instance.</param>
        /// <param name="stream">A Stream object holding a OBJf chunk.</param>
        public override void Read(IffFile iff, Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                io.ReadUInt32(); //pad
                Version = io.ReadUInt32(); //0x9 for latest game
                string magic = io.ReadCString(4); //IMAF

                HouseNumber = io.ReadInt32();
                FamilyNumber = io.ReadInt32();
                Budget = io.ReadInt32();
                NetWorth = io.ReadInt32();
                FamilyFriends = io.ReadInt32();
                Unknown = io.ReadInt32();
                FamilyGUIDs = new uint[io.ReadInt32()];
                for (int i=0; i<FamilyGUIDs.Length; i++)
                {
                    FamilyGUIDs[i] = io.ReadUInt32();
                }
            }
        }

        public override bool Write(IffFile iff, Stream stream)
        {
            using (var io = IoWriter.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                io.WriteInt32(0);
                io.WriteUInt32(9);
                io.WriteCString("IMAF", 4);
                io.WriteInt32(HouseNumber);
                io.WriteInt32(FamilyNumber);
                io.WriteInt32(Budget);
                io.WriteInt32(NetWorth);
                io.WriteInt32(FamilyFriends);
                io.WriteInt32(Unknown);
                io.WriteInt32(FamilyGUIDs.Length);
                foreach (var guid in FamilyGUIDs)
                    io.WriteUInt32(guid);

                for (int i = 0; i < 4; i++)
                    io.WriteInt32(0);
            }
            return true;
        }
    }
}
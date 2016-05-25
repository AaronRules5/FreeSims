﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.SimsAntics.Engine;
using TSO.Files.utils;
using TSO.SimsAntics.Engine.Utils;
using TSO.SimsAntics.Engine.Scopes;
using System.IO;

namespace TSO.SimsAntics.Primitives
{
    public class VMGetDistanceTo : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand args)
        {
            var operand = (VMGetDistanceToOperand)args;

            var obj1 = context.StackObject;
            VMEntity obj2;
            if ((operand.Flags & 1) == 0) obj2 = context.Caller;
            else obj2 = context.VM.GetObjectById(VMMemory.GetVariable(context, (VMVariableScope)operand.ObjectScope, operand.OScopeData));

            var pos1 = obj1.Position;
            var pos2 = obj2.Position;

            var result = (short)Math.Floor(Math.Sqrt(Math.Pow(pos1.x - pos2.x, 2) + Math.Pow(pos1.y - pos2.y, 2))/16.0);

            context.Thread.TempRegisters[operand.TempNum] = result;        
            return VMPrimitiveExitCode.GOTO_TRUE;
        }
    }

    public class VMGetDistanceToOperand : VMPrimitiveOperand
    { 
        public ushort TempNum { get; set; }
        public byte Flags { get; set; }
        public VMVariableScope ObjectScope { get; set; }
        public short OScopeData { get; set; }

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
                TempNum = io.ReadUInt16();
                Flags = io.ReadByte();
                ObjectScope = (VMVariableScope)io.ReadByte();
                OScopeData = io.ReadInt16();

                if ((Flags & 1) == 0)
                {
                    ObjectScope = VMVariableScope.MyObject;
                    OScopeData = 11;
                }
                Flags |= 1;
            }
        }

        public void Write(byte[] bytes) {
            using (var io = new BinaryWriter(new MemoryStream(bytes)))
            {
                io.Write(TempNum);
                io.Write(Flags);
                io.Write((byte)ObjectScope);
                io.Write(OScopeData);
            }
        }
        #endregion
    }
}

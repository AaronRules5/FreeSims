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
using TSO.SimsAntics.Engine.Scopes;
using TSO.SimsAntics.Model;
using TSO.SimsAntics.Engine.Utils;
using System.IO;

namespace TSO.SimsAntics.Primitives
{
    public class VMSetMotiveChange : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand args)
        {
            var operand = (VMSetMotiveChangeOperand)args;
            var avatar = ((VMAvatar)context.Caller);

            if (operand.Once) { }

            if (operand.ClearAll)
            {
                avatar.ClearMotiveChanges();
            }
            else
            {
                var rate = VMMemory.GetVariable(context, (VMVariableScope)operand.DeltaOwner, operand.DeltaData);
                var MaxValue = VMMemory.GetVariable(context, (VMVariableScope)operand.MaxOwner, operand.MaxData);
                if (operand.Once) {
                    var motive = avatar.GetMotiveData(operand.Motive);
                   motive += rate;
                   if (((rate > 0) && (motive > MaxValue)) || ((rate < 0) && (motive < MaxValue))) { motive = MaxValue; }
                   avatar.SetMotiveData(operand.Motive, motive);
                   }
                else avatar.SetMotiveChange(operand.Motive, rate, MaxValue);

            }

            return VMPrimitiveExitCode.GOTO_TRUE;
        }
    }

    public class VMSetMotiveChangeOperand : VMPrimitiveOperand {

        public VMVariableScope DeltaOwner { get; set; }
        public short DeltaData { get; set; }

        public VMVariableScope MaxOwner { get; set; }
        public short MaxData { get; set; }

        public VMSetMotiveChangeFlags Flags;
        public VMMotive Motive { get; set; }

        public bool ClearAll
        {
            get
            {
                return (Flags & VMSetMotiveChangeFlags.ClearAll) > 0;
            }
            set
            {
                if (value) Flags |= VMSetMotiveChangeFlags.ClearAll;
                else Flags &= ~VMSetMotiveChangeFlags.ClearAll;
            }
        }

        public bool Once
        {
            get
            {
                return (Flags & VMSetMotiveChangeFlags.Once) > 0;
            }
            set
            {
                if (value) Flags |= VMSetMotiveChangeFlags.Once;
                else Flags &= ~VMSetMotiveChangeFlags.Once;
            }
        }

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes){
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN)){

                DeltaOwner = (VMVariableScope)io.ReadByte();
                MaxOwner = (VMVariableScope)io.ReadByte();
                Motive = (VMMotive)io.ReadByte();
                Flags = (VMSetMotiveChangeFlags)io.ReadByte();

                DeltaData = io.ReadInt16();
                MaxData = io.ReadInt16();
            }
        }

        public void Write(byte[] bytes) {
            using (var io = new BinaryWriter(new MemoryStream(bytes)))
            {
                io.Write((byte)DeltaOwner);
                io.Write((byte)MaxOwner);
                io.Write((byte)Motive);
                io.Write((byte)Flags);
                io.Write(DeltaData);
                io.Write(MaxData);
            }
        }
        #endregion
    }

    [Flags]
    public enum VMSetMotiveChangeFlags {
        ClearAll = 1,
        Once = 2,
    }
}

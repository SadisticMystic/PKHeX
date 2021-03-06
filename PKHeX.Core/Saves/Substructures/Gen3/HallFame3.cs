﻿using System;
using System.Diagnostics;

namespace PKHeX.Core
{
    public class HallFame3Entry
    {
        private readonly byte[] Parent;
        private readonly bool Japanese;
        private readonly int Offset;

        private const int Count = 6;
        public const int SIZE = Count * HallFame3PKM.SIZE;

        public HallFame3Entry(byte[] data, int offset, bool japanese)
        {
            Parent = data;
            Japanese = japanese;
            Offset = offset;
        }

        private int GetMemberOffset(int i) => Offset + (i * HallFame3PKM.SIZE);
        private HallFame3PKM GetMember(int i) => new HallFame3PKM(Parent, GetMemberOffset(i), Japanese);

        public HallFame3PKM[] Team
        {
            get
            {
                var team = new HallFame3PKM[6];
                for (int i = 0; i < Count; i++)
                    team[i] = GetMember(i);
                return team;
            }
        }

        private const int MaxEntries = 50;
        private const int MaxLength = MaxEntries * SIZE;

        public static HallFame3Entry[] GetEntries(SAV3 sav)
        {
            byte[] data = new byte[SAV3.SIZE_BLOCK_USED * 2];
            Debug.Assert(data.Length > MaxLength);

            // HoF Data is split across two sav blocks
            Array.Copy(sav.Data, 0x1C000, data, 0, SAV3.SIZE_BLOCK_USED);
            Array.Copy(sav.Data, 0x1D000, data, SAV3.SIZE_BLOCK_USED, SAV3.SIZE_BLOCK_USED);
            bool Japanese = sav.Japanese;

            var entries = new HallFame3Entry[MaxEntries];
            for (int i = 0; i < entries.Length; i++)
                entries[i] = new HallFame3Entry(data, SIZE, Japanese);
            return entries;
        }

        public static void SetEntries(SAV3 sav, HallFame3Entry[] entries)
        {
            byte[] data = entries[0].Team[0].Data;
            if (data.Length != MaxLength)
                throw new ArgumentException(nameof(data));

            // HoF Data is split across two sav blocks
            Array.Copy(data, 0,                    sav.Data, 0x1C000, SAV3.SIZE_BLOCK_USED);
            Array.Copy(data, SAV3.SIZE_BLOCK_USED, sav.Data, 0x1D000, SAV3.SIZE_BLOCK_USED);
        }
    }

    public class HallFame3PKM
    {
        public const int SIZE = 20;

        public HallFame3PKM(byte[] data, int offset, bool jp)
        {
            Data = data;
            Offset = offset;
            Japanese = jp;
        }

        public readonly byte[] Data;
        private readonly int Offset;
        private readonly bool Japanese;

        public int TID { get => BitConverter.ToUInt16(Data, 0 + Offset); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0 + Offset); }
        public int SID { get => BitConverter.ToUInt16(Data, 2 + Offset); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 2 + Offset); }
        public uint PID { get => BitConverter.ToUInt32(Data, 4 + Offset); set => BitConverter.GetBytes(value).CopyTo(Data, 4 + Offset); }
        private int SpecLevel { get => BitConverter.ToUInt16(Data, 8 + Offset); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 8 + Offset); }
        public string Nickname { get => GetString(Data, 10 + Offset, 10); set => SetString(value, 10).CopyTo(Data, 10 + Offset); }

        public int Species { get => SpecLevel & 0x1FF; set => SpecLevel = (SpecLevel & 0xFE00) | value; }
        public int Level { get => SpecLevel >> 9; set => SpecLevel = (SpecLevel & 0x1FF) | (value << 9); }

        private string GetString(byte[] data, int offset, int length) => StringConverter.GetString3(data, offset, length, Japanese);
        private byte[] SetString(string value, int maxLength) => StringConverter.SetString3(value, maxLength, Japanese, 10);
    }
}

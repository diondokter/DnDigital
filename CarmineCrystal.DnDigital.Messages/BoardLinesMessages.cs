﻿using CarmineCrystal.Networking;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Messages
{
	[ProtoContract]
    public class BoardLinesMessage : Message
    {
		[ProtoMember(1, OverwriteList = true)]
		public byte[] Data { get; set; }
    }
}

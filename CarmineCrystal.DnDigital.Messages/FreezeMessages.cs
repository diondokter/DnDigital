using CarmineCrystal.Networking;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Messages
{
	[ProtoContract]
    public class FreezeMessage : Message
    {
		[ProtoMember(1)]
		public bool Freeze;
    }
}

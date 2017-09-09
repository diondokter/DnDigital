using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	public class PingRequest : Request
	{
		[ProtoMember(1)]
		public DateTime Time;
	}

	[ProtoContract]
	public class PingResponse : Response
	{
		[ProtoMember(1)]
		public DateTime Time;
	}
}

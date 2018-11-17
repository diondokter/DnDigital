using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	internal class AuthenticationRequest : Request
	{
		[ProtoMember(1)]
		internal string Username { get; set; }
		[ProtoMember(2)]
		internal string Password { get; set; }
	}

	[ProtoContract]
	internal class AuthenticationResponse : Response
	{
		[ProtoMember(1)]
		internal bool Accepted { get; set; }
		[ProtoMember(2)]
		internal string Reason { get; set; }
	}
}

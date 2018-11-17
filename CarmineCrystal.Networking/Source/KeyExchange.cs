using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	internal class KeyExchangeRequest : Request
	{
		[ProtoMember(1)]
		public byte[] RSAExponent;
		[ProtoMember(2)]
		public byte[] RSAModulus;
		[ProtoMember(3)]
		public int RSAKeySize;
		[ProtoMember(4)]
		public int AESKeySize;
	}

	[ProtoContract]
	internal class KeyExchangeResponse : Response
	{
		[ProtoMember(1)]
		public bool Accepted;
		[ProtoMember(2)]
		public byte[] EncryptedAESKey;
		[ProtoMember(3)]
		public byte[] EncryptedAESIV;

	}
}

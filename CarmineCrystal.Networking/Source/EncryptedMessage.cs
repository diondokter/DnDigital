using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	internal class EncryptedMessage : Message
    {
		[ProtoMember(1)]
		internal byte[] EncryptedPayload;

		internal Message GetPayload(AesManaged Cryptor)
		{
			using (MemoryStream MS = new MemoryStream(EncryptedPayload, false))
			{
				using (CryptoStream CS = new CryptoStream(MS, Cryptor.CreateDecryptor(), CryptoStreamMode.Read))
				{
					return DeserializeFrom(CS);
				}
			}
		}

		internal void SetPayload(Message Payload, AesManaged Cryptor)
		{
			using (MemoryStream MS = new MemoryStream())
			{
				using (CryptoStream CS = new CryptoStream(MS, Cryptor.CreateEncryptor(), CryptoStreamMode.Write))
				{
					Payload.SerializeInto(CS);
					CS.FlushFinalBlock();
					EncryptedPayload = MS.ToArray();
				}
			}
		}
	}
}

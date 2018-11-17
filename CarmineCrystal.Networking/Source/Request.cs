using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	public abstract class Request : Message
	{
		private static uint HighestID = 0;
		[ProtoMember(1)]
		public uint ID { get; private set; }

		[ProtoBeforeSerialization]
		private void SetID()
		{
			ID = HighestID;
			HighestID++;
		}
	}
}

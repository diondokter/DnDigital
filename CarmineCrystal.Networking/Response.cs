using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	[ProtoContract]
	public abstract class Response : Message
	{
		[ProtoMember(1)]
		public uint ID { get; set; }

		/// <summary>
		/// Empty constructor for protobuf
		/// </summary>
		protected Response()
		{

		}
	}
}

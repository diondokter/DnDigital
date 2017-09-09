using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Datamodels
{
	[ProtoContract]
	public class Character
	{
		[ProtoMember(1)]
		public Color Color { get; set; }

		[ProtoMember(2)]
		public Point Position { get; set; }

		[ProtoMember(3)]
		public string Name { get; set; }
	}
}

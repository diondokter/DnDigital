using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Datamodels
{
	[ProtoContract]
	public class Character
	{
		public static uint MaxID = 0;

		[ProtoMember(1)]
		public Color Color { get; set; }

		[ProtoMember(2)]
		public Point Position { get; set; }

		[ProtoMember(3)]
		public string Name { get; set; }

		[ProtoMember(4)]
		public string Visuals { get; set; }

		[ProtoMember(5)]
		public uint ID { get; set; }

		[ProtoMember(6)]
		public Rect Size { get; set; } = new Rect(new Point(0, 0), 0.8, 0.8);

		[ProtoMember(7)]
		public double Rotation { get; set; }
	}
}

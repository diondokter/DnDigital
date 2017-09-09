using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarmineCrystal.DnDigital.Datamodels
{
	/// <summary>
	/// .NET standard does not provide a color type. This is a small custom one.
	/// </summary>
	[ProtoContract]
    public struct Color
    {
		[ProtoMember(1)]
		public byte R { get; private set; }
		[ProtoMember(2)]
		public byte G { get; private set; }
		[ProtoMember(3)]
		public byte B { get; private set; }
		[ProtoMember(4)]
		public byte A { get; private set; }

		public float Rf
		{
			get => R / 255f;
			set => R = (byte)(value * 255);
		}
		public float Gf
		{
			get => G / 255f;
			set => G = (byte)(value * 255);
		}
		public float Bf
		{
			get => B / 255f;
			set => B = (byte)(value * 255);
		}
		public float Af
		{
			get => A / 255f;
			set => A = (byte)(value * 255);
		}

		public Color Negative => FromRGBA(1 - Rf, 1 - Gf, 1 - Bf, Af);

		private Color(byte R, byte G, byte B, byte A)
		{
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}

		private Color(float R, float G, float B, float A)
		{
			this.R = this.G = this.B = this.A = 0;

			this.Rf = R;
			this.Gf = G;
			this.Bf = B;
			this.Af = A;
		}

		public static Color FromRGB(float R, float G, float B)
		{
			return FromRGBA(R, G, B, 1);
		}
		public static Color FromRGBA(float R, float G, float B, float A)
		{
			return new Color(R, G, B, A);
		}
		public static Color FromHSV(float H, float S, float V)
		{
			return FromHSVA(H, S, V, 1);
		}
		public static Color FromHSVA(float H, float S, float V, float A)
		{
			while (H < 0)
			{
				H += 360;
			}
			while (H >= 360)
			{
				H -= 360;
			}

			float R, G, B;

			if (V <= 0)
			{
				R = G = B = 0;
			}
			else if (S <= 0)
			{
				R = G = B = V;
			}
			else
			{
				float hf = H / 60.0f;
				int i = (int)Math.Floor(hf);
				float f = hf - i;
				float pv = V * (1 - S);
				float qv = V * (1 - S * f);
				float tv = V * (1 - S * (1 - f));

				switch (i)
				{
					// Red is the dominant color
					case 0:
						R = V;
						G = tv;
						B = pv;
						break;
					// Green is the dominant color
					case 1:
						R = qv;
						G = V;
						B = pv;
						break;
					case 2:
						R = pv;
						G = V;
						B = tv;
						break;
					// Blue is the dominant color
					case 3:
						R = pv;
						G = qv;
						B = V;
						break;
					case 4:
						R = tv;
						G = pv;
						B = V;
						break;
					// Red is the dominant color
					case 5:
						R = V;
						G = pv;
						B = qv;
						break;
					// Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
					case 6:
						R = V;
						G = tv;
						B = pv;
						break;
					case -1:
						R = V;
						G = pv;
						B = qv;
						break;
					// The color is not defined, we should throw an error.
					default:
						//LFATAL("i Value error in Pixel conversion, Value is %d", i);
						R = G = B = V; // Just pretend its black/white
						break;
				}
			}

			return FromRGBA(R, G, B, A);
		}

		public Color LerpTo(Color End, float t)
		{
			return new Color(Rf * (1 - t) + End.Rf * t, Gf * (1 - t) + End.Gf * t, Bf * (1 - t) + End.Bf * t, Af * (1 - t) + End.Af * t);
		}

		public float Hue
		{
			get
			{
				float MaxValue = Math.Max(Math.Max(Rf, Gf), Bf);
				float MinValue = Math.Min(Math.Min(Rf, Gf), Bf);
				float MinMaxDelta = MaxValue - MinValue;

				// Grey, so no hue. Just return 0
				if (MinMaxDelta == 0)
				{
					return 0;
				}

				if (MaxValue == Rf)
				{
					return ((Gf - Bf) / (MaxValue - MinValue)) * 60;
				}
				else if (MaxValue == Gf)
				{
					return (2 + (Bf - Rf) / (MaxValue - MinValue)) * 60;
				}
				else
				{
					return (4 + (Rf - Gf) / (MaxValue - MinValue)) * 60;
				}
			}
		}

		public float Saturation => (Math.Max(Math.Max(Rf, Gf), Bf) - Math.Min(Math.Min(Rf, Gf), Bf)) / Math.Max(Math.Max(Rf, Gf), Bf);
		public float Value => Math.Max(Math.Max(Rf, Gf), Bf);
	}
}

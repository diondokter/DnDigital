using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.DnDigital.Client
{
	public static class Extentions
	{
		public static Windows.UI.Color ToWindowsColor(this Datamodels.Color value)
		{
			return Windows.UI.Color.FromArgb(value.A, value.R, value.G, value.B);
		}
	}
}

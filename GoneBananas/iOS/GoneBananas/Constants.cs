using System;

namespace GoneBananas
{
	public class Constants
	{
		public static String AppKey = "c020f89cb3b517b568b5519a95eaeede67dc152ddec1c5fcfeafc25a50a49d9f";
		public static String SecretKey = "0377b4546b51203b86d828dbbf17990157506339764fd48d28caf56eb42cc95d";

	}

	enum MessageType{
		start,
		over,
		banana,
		monkey,
		hitcount
	};
}


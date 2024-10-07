using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseFramework.Services {
	public static class AppEventsService {
		public static EventAggregator EventAggregator { get; } = new();
	}


}

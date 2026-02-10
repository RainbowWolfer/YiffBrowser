using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace YiffBrowser.E621.Models;

/// <summary> map to self </summary>
[JsonObject]
public class E621ProfileModel {

	public string? E621_Username { get; set; }
	public string? E621_ApiKey { get; set; }

	public string? E6AI_Username { get; set; }
	public string? E6AI_ApiKey { get; set; }

	public string? E926_Username { get; set; }
	public string? E926_ApiKey { get; set; }


}

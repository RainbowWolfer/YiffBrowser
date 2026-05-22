using System;
using System.Collections.Generic;
using System.Text;

namespace YiffBrowser.BaseFramework.Interfaces;

public interface INameTemplateItem {
	string Id { get; }
	string Md5 { get; }
	IEnumerable<string> Authors { get; }
	string Extension { get; }
}

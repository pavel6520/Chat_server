using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketSharp.Net {
	[Serializable]
	[ComVisible(true)]
	public class NonUniqueCollection : Dictionary<string, NonUniqueCollectionElement> {
		public new string[] this[string key] {
			get {
				return base.ContainsKey(key) ? base[key].ToArray() : null;
			}
		}
		public string[] this[int index] {
			get {
				return base[GetKey(index)].ToArray();
			}
		}

		public virtual void Add(string key, string value) {
			if (!base.ContainsKey(key))
				base.Add(key, new NonUniqueCollectionElement());

			base[key].Add(value);
		}

		public virtual void Add(string key, string[] values) {
			if (!base.ContainsKey(key))
				base.Add(key, new NonUniqueCollectionElement());

			foreach (var value in values) {
				base[key].Add(value);
			}
		}

		public void Set(string key, string value) {
			if (base.ContainsKey(key)) {
				base[key].Clear();
			}
			else {
				base.Add(key, new NonUniqueCollectionElement());
			}
			base[key].Add(value);
		}

		public virtual string[] Get(int index) {
			return this[GetKey(index)];
		}

		public virtual string[] Get(string key) {
			return this[key];
		}

		public virtual string[] GetValues(int index) {
			return this[GetKey(index)];
		}

		public virtual string[] GetValues(string key) {
			return this[key];
		}

		public virtual string GetKey(int index) {
			return base.Keys.ToArray()[index];
		}

		private new bool ContainsValue(NonUniqueCollectionElement value) {
			return false;
		}

		public string[] AllKeys { get { return Keys.ToArray(); } }
	}

	[Serializable]
	[ComVisible(true)]
	public class NonUniqueCollectionElement : List<string> {
		public new void Add(string item) {
			if (!this.Contains(item)) {
				base.Add(item);
			}
		}
	}
}

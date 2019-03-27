namespace ConnectionWorker {
	[System.Serializable]
	public class EchoClass {
		public EchoType type;
		public object param;

		public EchoClass(EchoType t = EchoType.String, object p = null) {
			type = t;
			param = p;
		}

		public enum EchoType {
			String,
			Layout,
			MySqlCommand,
			End
		}
	}
}
using System;

[Serializable]
	public class CPlayerData {
		public string id;
		public string name;
		public int turnIndex;

		public CPlayerData()
		{
			this.id = string.Empty;
			this.name = string.Empty;
			this.turnIndex = -1;
		}

		public CPlayerData(CPlayerData value)
		{
			this.id = value.id;
			this.name = value.name;
			this.turnIndex = value.turnIndex;
		}
	}

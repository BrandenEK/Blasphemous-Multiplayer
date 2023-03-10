namespace BlasClient.Structures
{
    [System.Serializable]
    public class Config
    {
        public int serverPort;
        public float notificationDisplaySeconds;
        public bool displayNametags;
        public bool displayOwnNametag;
        public bool showPlayersOnMap;
        public bool showOtherTeamOnMap;
        public bool enablePvP;
        public bool enableFriendlyFire;
        public int team;
        public SyncSettings syncSettings;

        // Default config
        public Config()
        {
            serverPort = 8989;
            notificationDisplaySeconds = 4f;
            displayNametags = true;
            displayOwnNametag = true;
            showPlayersOnMap = true;
            showOtherTeamOnMap = false;
            enablePvP = true;
            enableFriendlyFire = false;
            team = 1;
            syncSettings = new SyncSettings();
        }
    }
}

public class PlayerNetworkProfile
{
    private static PlayerNetworkProfile instance;
    public static PlayerNetworkProfile Instance => instance ?? (instance = new PlayerNetworkProfile());



    private string name = "";
    private string room = "";



    public string Name { get => name; set => name = value; }
    public string Room { get => room; set => room = value; }
    public bool CanJoinGame => (Name != "" && Room != "");
}

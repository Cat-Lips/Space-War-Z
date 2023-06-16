using Godot;

namespace GodotSharp.BuildingBlocks
{
    [SceneTree]
    public partial class MainMenu : Root
    {
        private readonly Settings<MainMenu> settings = new();

        private static readonly Color WarnColor = Colors.Yellow;
        private static readonly Color ErrorColor = Colors.Red;
        private static readonly Color SuccessColor = Colors.Green;

        public void SetGameOptions(Action<string> optionSelected, params string[] options)
        {
            var popup = GameOptions.GetPopup();
            options.ForEach(x => popup.AddItem(x));
            popup.IndexPressed += idx => optionSelected(popup.GetItemText((int)idx));
        }

        public void SetNetworkActions(Network network)
        {
            InitActions();
            InitDisplay();

            void InitActions()
            {
                StartServer.Pressed += () => network.StartServer(ServerPort.Value, SetServerStatus);
                StopServer.Pressed += network.StopServer;
                CreateClient.Pressed += () => network.CreateClient(ConnectAddress.Text, ConnectPort.Value, SetClientStatus);
                CloseClient.Pressed += network.CloseClient;

                void SetServerStatus(StatusType status, string message)
                    => SetStatus(ServerStatus, status, message);

                void SetClientStatus(StatusType status, string message)
                    => SetStatus(ClientStatus, status, message);

                static void SetStatus(Label statusLabel, StatusType status, string message)
                {
                    SetStatus(statusLabel, message, StatusColor());

                    Color? StatusColor() => status switch
                    {
                        StatusType.Warn => WarnColor,
                        StatusType.Error => ErrorColor,
                        StatusType.Success => SuccessColor,
                        _ => null,
                    };

                    static void SetStatus(Label statusLabel, string message, Color? color = null)
                    {
                        statusLabel.Text = message;

                        if (color is null)
                            statusLabel.ResetFontColor();
                        else
                            statusLabel.SetFontColor(color.Value);
                    }
                }
            }

            void InitDisplay()
            {
                InitNetworkMenus();
                network.NetworkStateChanged += InitNetworkMenus;

                var windowTitle = GetWindow().Title;
                network.NetworkStateChanged += SetWindowTitle;

                void InitNetworkMenus()
                {
                    switch (network.NetworkState)
                    {
                        case NetworkState.ClientConnecting:
                            ServerMenu.Visible = false;

                            CreateClient.Visible = false;
                            CloseClient.Visible = true;
                            ConnectAddress.Editable = false;
                            ConnectPort.Editable = false;

                            PlayerName.Editable = false;
                            GameOptions.Enabled(false);

                            break;

                        case NetworkState.ServerStarting:
                            ClientMenu.Visible = false;

                            StartServer.Visible = false;
                            StopServer.Visible = true;
                            ServerPort.Editable = false;

                            PlayerName.Editable = false;
                            GameOptions.Enabled(false);

                            break;

                        case NetworkState.None:
                            ClientMenu.Visible = true;
                            ServerMenu.Visible = true;

                            CreateClient.Visible = true;
                            CloseClient.Visible = false;
                            ConnectAddress.Editable = true;
                            ConnectPort.Editable = true;

                            StartServer.Visible = true;
                            StopServer.Visible = false;
                            ServerPort.Editable = true;

                            PlayerName.Editable = true;
                            GameOptions.Enabled(true);

                            break;
                    }
                }

                void SetWindowTitle()
                {
                    GetWindow().Title = $"{windowTitle}{NetworkStateSuffix()}";

                    string NetworkStateSuffix() => network.NetworkState switch
                    {
                        NetworkState.ServerStarted => " - SERVER",
                        NetworkState.ClientConnected => " - CLIENT",
                        _ => "",
                    };
                }
            }
        }

        public string GetPlayerName() => PlayerName.Text;

        [GodotOverride]
        private void OnReady()
        {
            InitPlayerMenu();

            Quit.Pressed += QuitGame;
            PreSortChildren += ResetSize;
            GetWindow().CloseRequested += OnWindowClosing;

            ServerAddress.Text = Network.GetLocalAddress();

            void InitPlayerMenu()
            {
                PlayerName.Text = GetPlayerName();
                PlayerName.FocusExited += SetPlayerName;

                string GetPlayerName()
                    => settings.TryGet(PlayerMenu, PlayerName, DefaultPlayerName());

                void SetPlayerName()
                {
                    ValidatePlayerName();
                    SetPlayerName();

                    void ValidatePlayerName()
                    {
                        if (string.IsNullOrWhiteSpace(PlayerName.Text))
                            PlayerName.Text = DefaultPlayerName();
                    }

                    void SetPlayerName()
                        => settings.Set(PlayerMenu, PlayerName, PlayerName.Text);
                }

                string DefaultPlayerName()
                    => System.Environment.UserName;
            }

            void QuitGame()
            {
                GetWindow().PropagateNotification((int)NotificationWMCloseRequest);
                GetTree().Quit();
            }

            void OnWindowClosing()
                => Input.MouseMode = default;
        }

        public override partial void _Ready();
    }
}

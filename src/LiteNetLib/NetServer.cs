//using LiteNetLib;
//using Microsoft.Xna.Framework;

//namespace Dcrew.LiteNetLib;

//public static class NetServer {
//    public const int Port = 6121;
//    const float SYNC_PLAYERS_TIME = .01666666666f;

//    public static bool IsRunning => _manager.IsRunning;
//    public static int PlayerCap = 4;
//    internal static int _maxPacketId;

//    static double _syncPlayersTimer;
//    static int _packetClearStart;

//    static readonly EventBasedNetListener _listener = new();
//    internal static readonly NetManager _manager = new(_listener) { AutoRecycle = true, UpdateTime = 15, ChannelsCount = 4 };
//    static readonly Dictionary<NetPeer, int> _players = new();
//    static readonly Dictionary<int, NetPeer> _peers = new();
//    static readonly Dictionary<int, NetWriter> _packets = new();
//    static readonly NetWriter _initialData = new NetWriter();
//    static readonly NetReader _r = new NetReader();
//    static readonly Action<NetPeer>[] _action;
//    static FreeList _player;

//    static NetServer() {
//        _action = new Action<NetPeer>[0];
//        _listener.NetworkReceiveEvent += (peer, readerOutdated, delieryMethod) => {
//            int senderId = _players[peer];
//            if (readerOutdated.EndOfData) {
//                var w = CreatePacket(Packets.PlayerJoin);
//                w.Put(true); // true if add, false if remove
//                w.PutPlayerId(senderId);
//                //w.Put(Player.Get(senderId).Name);
//                SendToAllExcept(w, 0, DeliveryMethod.ReliableOrdered, peer);
//                _peers.Add(senderId, peer);
//                return;
//            }
//            _r.ReadFrom(readerOutdated);
//            var packet = (NetClient.Packets)_r.ReadInt(0, NetClient._maxPacketId);
//            if (packet == NetClient.Packets.SyncPlayer) {
//                while (!_r.EndOfData) {
//                    var i = _r.ReadPlayerId();
//                }
//            }
//        };
//        _listener.PeerConnectedEvent += peer => {
//            var p = _players[peer];
//            _initialData.Clear(0);
//            {
//                static void PutPlayer(int j) {
//                    ref var player = ref Player.Get(j);
//                    _initialData.PutPlayerId(j);
//                    //_initialData.Put(player.Name);
//                }
//                _initialData.Put(0, Player.CAP, _peers.Count);
//                PutPlayer(Player.Local);
//                foreach (var other in _peers.Values) {
//                    int i = _players[other];
//                    if (i != p)
//                        PutPlayer(i);
//                }
//                _initialData.PutPlayerId(p);
//                Send(_initialData, peer, 0, DeliveryMethod.ReliableOrdered);
//            }
//        };
//        _listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
//            int i = _players[peer];
//            _peers.Remove(i);
//            _players.Remove(peer);
//            var w = CreatePacket(Packets.PlayerJoin);
//            w.Put(false); // true if add, false if remove
//            w.PutPlayerId(i);
//            SendToAll(w, 0, DeliveryMethod.ReliableOrdered);
//            Player.Remove(i);
//        };
//        _listener.ConnectionRequestEvent += request => {
//            if (_manager.ConnectedPeersCount + 1 >= PlayerCap) {
//                request.Reject();
//                return;
//            }
//            int free = _player.Add();
//            var peer = request.Accept();
//            _players.Add(peer, free);
//        };
//    }

//    public static void RegisterPacket<T>(T packet, Action<NetPeer> action) where T : Enum {
//        var i = Convert.ToInt32(packet);
//        _action[i] = action;
//        _maxPacketId = i;
//        var w = new NetWriter();
//        _packetClearStart = w.Put(0, _maxPacketId, i);
//        _packets.Add(i, w);
//    }

//    public static void Host() {
//        _manager.Start(Port);
//        _initialData.Clear();
//    }
//    public static void Stop() {
//        _manager.Stop(true);
//        _players.Clear();
//        _peers.Clear();
//    }

//    public static NetWriter CreatePacket<T>(T packet) where T : Enum {
//        var i = Convert.ToInt32(packet);
//        var p = _packets[i];
//        p.Clear(_packetClearStart);
//        return p;
//    }
//    public static void Update() {
//        if (!IsRunning)
//            return;
//        _manager.PollEvents();
//        if ((_syncPlayersTimer += Time.DeltaD) >= SYNC_PLAYERS_TIME) {
//            _syncPlayersTimer -= SYNC_PLAYERS_TIME;
//            foreach (int i in _peers.Keys) {
//                var w = CreatePacket(Packets.SyncPlayers);
//                static void PutInfo(NetWriter w, int i, ref Player.Inst p) {
//                    w.PutPlayerId(i);
//                    w.PutPos(p.Pos);
//                    if (p.MoveRot.HasValue) {
//                        w.Put(true);
//                        w.PutRotation(p.MoveRot.Value, PlayerRotationBits);
//                    } else
//                        w.Put(false);
//                    if (p.AimRot.HasValue) {
//                        w.Put(true);
//                        w.PutRotation(p.AimRot.Value, PlayerRotationBits);
//                    } else
//                        w.Put(false);
//                    w.Put(p.ShotsFired);
//                }
//                PutInfo(w, Player.Local, ref Player.GetLocal());
//                Send(w, _peers[i], 1, DeliveryMethod.Sequenced);
//            }
//        }
//    }

//    public static void PutPlayerId(this NetWriter w, int i) => w.Put(-1, Player.CAP - 1, i);
//    public static int ReadPlayerId(this NetReader r) => r.ReadInt(-1, Player.CAP - 1);
//    public static void PutPos(this NetWriter w, Vector2 pos) {
//        w.Put(-Main.MapWidth, (int)MathF.Ceiling(Main.MapWidth), (int)pos.X);
//        w.Put(-Main.MapHeight, (int)MathF.Ceiling(Main.MapHeight), (int)pos.Y);
//    }
//    public static Vector2 ReadPos(this NetReader r) {
//        return new Vector2(r.ReadInt(-Main.MapWidth, (int)MathF.Ceiling(Main.MapWidth)),
//            r.ReadInt(-Main.MapHeight, (int)MathF.Ceiling(Main.MapHeight)));
//    }

//    public static void SendToAll(NetWriter writer, byte channel, DeliveryMethod deliveryMethod) => _manager.SendToAll(writer.Data, 0, writer.LengthBytes, channel, deliveryMethod);
//    public static void SendToAllExcept(NetWriter writer, byte channel, DeliveryMethod deliveryMethod, NetPeer excludePeer) => _manager.SendToAll(writer.Data, 0, writer.LengthBytes, channel, deliveryMethod, excludePeer);
//    public static void Send(NetWriter writer, NetPeer peer, byte channel, DeliveryMethod deliveryMethod) => peer.Send(writer.Data, 0, writer.LengthBytes, channel, deliveryMethod);
//}

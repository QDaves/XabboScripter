using System;

using Xabbo.Core;
using Xabbo.Core.Game;

namespace Xabbo.Scripter.Services;

public interface IGameManager
{
    ProfileManager ProfileManager { get; }
    FriendManager FriendManager { get; }
    RoomManager RoomManager { get; }
    InventoryManager InventoryManager { get; }
    PetInventoryManager PetInventoryManager { get; }
    TradeManager TradeManager { get; }
}

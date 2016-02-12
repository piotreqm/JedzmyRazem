using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InzWebApi.Models
{
    public interface IFriendListRepository
    {
        List<FriendListItem> Get(long ownerId);
        FriendListItem Add(FriendListItem item);
        bool Remove(long friendListId);
    }
}

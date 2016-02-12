using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace InzWebApi.Models
{
    internal class FriendListRepository : IFriendListRepository
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private List<FriendListItem> friendLists = new List<FriendListItem>();

        public FriendListRepository()
        {
            RefreshFriendListsList();
        }

        private void RefreshFriendListsList()
        {
            friendLists = (from p in context.FriendLists select p).ToList();
        }

        public List<FriendListItem> Get(long ownerId)
        {
            return friendLists.FindAll(p => p.OwnerId == ownerId).OrderBy(p => p.Friend.Name).ToList();
        }

        public FriendListItem Add(FriendListItem item)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }
            else
            {
                var itemOwner = (from p in context.Travelers where p.Id == item.OwnerId select p).First();
                var itemFriend = (from p in context.Travelers where p.Id == item.FriendId select p).First();
                item.Owner = itemOwner;
                item.Friend = itemFriend;
                FriendListItem itemToCompare;
                try
                {
                    itemToCompare = (from p in context.FriendLists where ((p.OwnerId == item.OwnerId) && (p.FriendId == item.FriendId)) select p).First();
                    throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
                }
                catch(InvalidOperationException)
                {
                        context.FriendLists.Add(item);
                        context.SaveChanges();
                        RefreshFriendListsList();
                        return item;
                }
            }
        }

        public bool Remove(long friendListId)
        {
            try
            {
                var friendListToRemove = (from p in context.FriendLists where p.Id == friendListId select p).First();
                context.FriendLists.Remove(friendListToRemove);
                context.SaveChanges();
                RefreshFriendListsList();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

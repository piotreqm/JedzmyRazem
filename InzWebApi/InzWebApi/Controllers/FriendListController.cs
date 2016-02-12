using InzWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InzWebApi.Controllers
{
    public class FriendListController : ApiController
    {
        static readonly IFriendListRepository repo = new FriendListRepository();
        private List<FriendListItem> friendList { get; set; }

        //GET api/friendlist/getfriendlist?ownerId=1
        [Authorize]
        public List<FriendListItem> GetFriendList (long ownerId)
        {
            friendList = repo.Get(ownerId);
            if (friendList.Count == 0) 
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            else return friendList;
        }

        //POST api/friendlist/postfriendlistitem
        [Authorize]
        public HttpResponseMessage PostFriendListItem(FriendListItem newFriendListItem)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            else
            {
                newFriendListItem = repo.Add(newFriendListItem);
                var response = Request.CreateResponse(HttpStatusCode.Created, newFriendListItem);
                return response;
            }
        }

        //DELETE api/friendlist/deletefriendlistitem?friendlistitemId=1
        [Authorize]
        public void DeleteFriendListItem(long friendListItemId)
        {
            var removeMessage = repo.Remove(friendListItemId);
            if (!removeMessage)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}

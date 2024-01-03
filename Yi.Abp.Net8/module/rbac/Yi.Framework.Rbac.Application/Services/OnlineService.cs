﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.Rbac.Application.Contracts.IServices;
using Yi.Framework.Rbac.Domain.SignalRHubs;
using Yi.Framework.Rbac.Domain.SignalRHubs.Model;

namespace Yi.Framework.Rbac.Application.Services
{
    public class OnlineService : ApplicationService, IOnlineService
    {
        private ILogger<OnlineService> _logger;
        private IHubContext<OnlineUserHub> _hub;
        public OnlineService(ILogger<OnlineService> logger, IHubContext<OnlineUserHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        /// <summary>
        /// 动态条件获取当前在线用户
        /// </summary>
        /// <param name="online"></param>
        /// <returns></returns>
        public PagedResultDto<OnlineUserModel> GetListAsync([FromQuery] OnlineUserModel online)
        {
            var data = OnlineUserHub.clientUsers;
            IEnumerable<OnlineUserModel> dataWhere = data.AsEnumerable();

            if (!string.IsNullOrEmpty(online.Ipaddr))
            {
                dataWhere = dataWhere.Where((u) => u.Ipaddr!.Contains(online.Ipaddr));
            }
            if (!string.IsNullOrEmpty(online.UserName))
            {
                dataWhere = dataWhere.Where((u) => u.UserName!.Contains(online.UserName));
            }
            return new PagedResultDto<OnlineUserModel>() { TotalCount = data.Count, Items = dataWhere.ToList() };
        }


        /// <summary>
        /// 强制退出用户
        /// </summary>
        /// <param name="connnectionId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("online/{connnectionId}")]
        public async Task<bool> ForceOut(string connnectionId)
        {
            if (OnlineUserHub.clientUsers.Exists(u => u.ConnnectionId == connnectionId))
            {
                //前端接受到这个事件后，触发前端自动退出
                await _hub.Clients.Client(connnectionId).SendAsync("forceOut", "你已被强制退出！");
                return true;
            }
            return false;
        }
    }
}

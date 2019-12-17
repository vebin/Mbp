﻿using AutoMapper;
using Mbp.AspNetCore.Mvc.Convention;
using Mbp.Core.Core;
using Mbp.Core.Modularity;
using Mbp.Ddd.Application.Mbp.UI;
using Mbp.Ddd.Application.System.Linq;
using Medical.Ai.Mbdp.Application.Contracts.LogService;
using Medical.Ai.Mbdp.Application.Contracts.LogService.Dto;
using Medical.Ai.Mbdp.Application.Contracts.LogService.DtoSearch;
using Medical.Ai.Mbdp.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Mbp.EntityFrameworkCore.PermissionModel;
using Microsoft.Extensions.Configuration;

namespace Medical.Ai.Mbdp.Application.LogService
{
    [Authorize(Roles = "admin")]
    [AutoAop]
    [AutoWebApi]
    [Route("api/[controller]")]
    public class LogManageAppService : ILogManageAppService
    {
        private readonly IMapper _mapper = AutofacService.Resolve<IMapper>();

        private readonly DefaultDbContext _defaultDbContext = null;

        private readonly IConfiguration _config;

        public LogManageAppService(DefaultDbContext defaultDbContext, IConfiguration config)
        {
            _defaultDbContext = defaultDbContext;
            _config = config;
        }

        /// <summary>
        /// 获取日志列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLogs")]
        public async Task<PagedList<LogOutInputDto>> GetLogs(SearchOptions<LogSearchOptions> searchOptions)
        {
            int total = 0;

            Expression<Func<MbpOperationLog, bool>> expression = options =>
           (!string.IsNullOrEmpty(searchOptions.Search.UserName) ? options.UserName == searchOptions.Search.UserName : true) &&
           (!string.IsNullOrEmpty(searchOptions.Search.ClientIP) ? options.ClientIP == searchOptions.Search.ClientIP : true) &&
           (!string.IsNullOrEmpty(searchOptions.Search.AppName) ? options.AppName == searchOptions.Search.AppName : true) &&
           (!string.IsNullOrEmpty(searchOptions.Search.Desc) ? options.Desc.Contains(searchOptions.Search.Desc) : true) &&
           (!string.IsNullOrEmpty(searchOptions.Search.ModuleName) ? options.ModuleName == searchOptions.Search.ModuleName : true) &&
           (!string.IsNullOrEmpty(searchOptions.Search.OpName) ? options.OpName.Contains(searchOptions.Search.OpName) : true) &&
           (searchOptions.Search.OpDateTimeBegin != null ? options.OpDateTime >= searchOptions.Search.OpDateTimeBegin : true) &&
           (searchOptions.Search.OpDateTimeEnd != null ? options.OpDateTime <= searchOptions.Search.OpDateTimeEnd : true);

            var logs = _defaultDbContext.MbpOperationLogs.PageByAscending(searchOptions.PageSize, searchOptions.PageIndex, out total, expression, (c => c.Id)).ToList();

            return new PagedList<LogOutInputDto>()
            {
                Content = _mapper.Map<List<LogOutInputDto>>(logs),
                PageIndex = searchOptions.PageIndex,
                PageSize = searchOptions.PageSize,
                Total = total
            };
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        [HttpPost("AddLog")]
        public int AddLog(LogInputDto logInputDto)
        {
            var log = _mapper.Map<MbpOperationLog>(logInputDto);

            _defaultDbContext.MbpOperationLogs.Add(log);

            return _defaultDbContext.SaveChanges();
        }
    }
}

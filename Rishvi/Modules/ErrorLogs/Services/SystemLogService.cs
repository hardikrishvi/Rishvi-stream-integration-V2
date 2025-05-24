using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.Core.Extensions;
using Rishvi.Modules.ErrorLogs.Filters;
using Rishvi.Modules.ErrorLogs.ListOrders;
using Rishvi.Modules.ErrorLogs.Models;
using Rishvi.Modules.ErrorLogs.Models.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace Rishvi.Modules.ErrorLogs.Services
{
    public interface ISystemLogService
    {
        bool Create(SystemLog dto);
        Task<bool> CreateAsync(SystemLog dto);
        Task<Result> ListAsync(SystemLogFilterDto dto);
        Task<Result> ByIdAsync(Guid id);
        Task<Result> DeleteAsync(IdsDto dto);
        Task<Result> GetStatus();
    }
    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SystemLog> _systemLogRepository;
        private readonly IMapper _mapper;
        public SystemLogService(IUnitOfWork unitOfWork,
            IRepository<SystemLog> systemLogRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _systemLogRepository = systemLogRepository;
            _mapper = mapper;
        }
        public bool Create(SystemLog dto)
        {
            dto.SystemLogID = Guid.NewGuid();
            dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt = DateTime.Now;
            _systemLogRepository.Add(dto);
            _unitOfWork.Commit();
            return true;
        }
        public async Task<bool> CreateAsync(SystemLog dto)
        {
            dto.SystemLogID = Guid.NewGuid();
            dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt = DateTime.Now;
            await _systemLogRepository.AddAsync(dto);
            await _unitOfWork.CommitAsync();
            return await Task.FromResult(true);
        }
        public async Task<Result> ListAsync(SystemLogFilterDto dto)
        {
            var filter = dto ?? new SystemLogFilterDto();
            var query = _systemLogRepository.AsNoTracking()
               .Select(t => new SystemLogListDto
               {
                   SystemLogID = t.SystemLogID,
                   ModuleName = t.ModuleName,
                   RequestHeader = t.RequestHeader,
                   RequestJson = t.RequestJson,
                   ResponseJson = t.ResponseJson,
                   Status = t.Status,
                   Message = t.Message,
                   IsError = t.IsError,
                   CreatedAt = t.CreatedAt,
                   UpdatedAt = t.UpdatedAt
               });
            query = new SystemLogFilter(query, filter).FilteredQuery();
            query = new SystemLogListOrder(query, filter).OrderByQuery();
            var result = await new Result().SetPagingAsync(filter, query.Count());
            result.Data = query.ToPaged(result.Paging.Page, result.Paging.Size).ToList();
            return await Task.FromResult(result);
        }
        public async Task<Result> GetStatus()
        {
            var query = _systemLogRepository.AsNoTracking()
                 .Select(t => new KeyValueDto
                 {
                     Key = t.Status,
                     Value = t.Status,
                 }).Distinct();
            return await new Result().SetDataAsync(query.ToList());
        }
        public async Task<Result> ByIdAsync(Guid id)
        {
            var entity = new SystemLog();
            var systemLogListDto = new SystemLogListDto();
            if (id == Guid.Empty)
                return await new Result().SetDataAsync(systemLogListDto);
            entity = await _systemLogRepository.AsNoTracking().FirstOrDefaultAsync(s => s.SystemLogID == id);
            systemLogListDto = _mapper.Map<SystemLogListDto>(entity);
            return await new Result().SetDataAsync(systemLogListDto);
        }
        public async Task<Result> DeleteAsync(IdsDto dto)
        {
            if (dto == null || dto.Ids.Count == 0)
                return await new Result().SetErrorAsync(Messages.SelectAtLeastOneItemFromList);
            int i = 0;
            foreach (var id in dto.Ids)
            {
                var entity = await _systemLogRepository.AsNoTracking().FirstOrDefaultAsync(s => s.SystemLogID == id);
                if (entity != null)
                {
                    await _systemLogRepository.DeleteAsync(id);
                    i++;
                }
            }
            await _unitOfWork.CommitAsync();
            return await new Result().SetSuccessAsync(Messages.RecordDelete, i);
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.Core.Extensions;
using Rishvi.Modules.Core.Validators;
using Rishvi.Modules.Users.CacheManagers;
using Rishvi.Modules.Users.Filters;
using Rishvi.Modules.Users.ListOrders;
using Rishvi.Modules.Users.Models;
using Rishvi.Modules.Users.Models.DTOs;
using Rishvi.Modules.Users.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rishvi.Modules.Users.Services
{
    public interface IUserAdminService
    {
        Task<Result> ListAsync(UserAdminFilterDto dto);
        Task<Result> ByIdAsync(Guid id);
        Task<Result> CreateAsync(UserAdminCreateDto dto);
        Task<Result> EditAsync(UserAdminEditDto dto);
        Task<Result> ActiveAsync(IdsDto dto);
        Task<Result> InActiveAsync(IdsDto dto);
        Task<Result> DeleteAsync(IdsDto dto);
        Task<Result> GetKlaviyoEmailAndTokenByEmailIdAsync(string emailAddress);
        Task<Result> UpdateKlaviyoEmailAndTokenByEmailIdAsync(UserAdminEmailDto dto);
        
    }

    public class UserAdminService : IUserAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;
        private readonly UserAdminCreateValidator _userAdminCreateValidator;
        private readonly UserAdminEditValidator _userAdminEditValidator;
        private readonly UserAdminActiveValidator _userAdminActiveValidator;
        private readonly UserAdminDeleteValidator _userAdminDeleteValidator;
        private readonly UserAdminEmailValidator _userAdminEmailValidator;
        
        public UserAdminService(IRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IMapper mapper,
            UserAdminCreateValidator userAdminCreateValidator,
            UserAdminEditValidator userAdminEditValidator,
            UserAdminActiveValidator userAdminActiveValidator,
            UserAdminDeleteValidator userAdminDeleteValidator,
            UserAdminEmailValidator userAdminEmailValidator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _config = config;
            _mapper = mapper;
            _userAdminCreateValidator = userAdminCreateValidator;
            _userAdminEditValidator = userAdminEditValidator;
            _userAdminActiveValidator = userAdminActiveValidator;
            _userAdminDeleteValidator = userAdminDeleteValidator;
            _userAdminEmailValidator = userAdminEmailValidator;
        }



        public async Task<Result> ListAsync(UserAdminFilterDto dto)
        {
            var filter = dto ?? new UserAdminFilterDto();

            var query = _userRepository.AsNoTracking()
                .Where(w => !w.IsDeleted)
               .Select(t => new UserAdminListDto
               {
                   UserId = t.UserId,
                   Firstname = t.Firstname,
                   Lastname = t.Lastname,
                   Username = t.Username,
                   EmailAddress = t.EmailAddress,
                   CreatedAt = t.CreatedAt,
                   UpdatedAt = t.UpdatedAt,
                   IsDeleted = t.IsDeleted,
                   IsActive = t.IsActive,
                   Company = t.Company,
                   LinnworksId = t.LinnworksId,
                   LinnworksApplicationToken = t.LinnworksApplicationToken,
                   LinnworksUserToken = t.LinnworksUserToken,
                   LinnworksServerUrl = t.LinnworksServerUrl
               });

            query = new UserAdminFilter(query, filter).FilteredQuery();
            query = new UserAdminListOrder(query, filter).OrderByQuery();

            var result = await new Result().SetPagingAsync(filter, query.Count());
            result.Data = query.ToPaged(result.Paging.Page, result.Paging.Size).ToList();
            return await Task.FromResult(result);
        }

        public async Task<Result> ByIdAsync(Guid id)
        {

            var entity = new User();
            var userAdminEditDto = new UserAdminEditDto();

            if (id == Guid.Empty)
                return await new Result().SetDataAsync(userAdminEditDto);

            entity = await _userRepository.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == id);

            userAdminEditDto = _mapper.Map<UserAdminEditDto>(entity);
            return await new Result().SetDataAsync(userAdminEditDto);
        }


        public async Task<Result> CreateAsync(UserAdminCreateDto dto)
        {
            var result = await _userAdminCreateValidator.ValidateResultAsync(dto);
            if (!result.Success)
                return result;

            var entity = _mapper.Map<User>(dto);
            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;
            await _userRepository.AddAsync(entity);

            await _unitOfWork.CommitAsync();
            ClearCache();

            await result.SetIdAsync(entity.UserId);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        public async Task<Result> EditAsync(UserAdminEditDto dto)
        {
            dto ??= new UserAdminEditDto();

            var result = await _userAdminEditValidator.ValidateResultAsync(dto);
            if (!result.Success)
                return result;

            var entity = await _userRepository.AsNoTracking().FirstAsync(s => s.UserId == dto.UserId);
            var IsDeleted = entity.IsDeleted;
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDeleted = IsDeleted;
            await _userRepository.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();
            ClearCache();
            await result.SetIdAsync(entity.UserId);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }

        public async Task<Result> ActiveAsync(IdsDto dto)
        {
            var result = await _userAdminActiveValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
                return result;

            var query = _userRepository.AsNoTracking().Where(q => dto.Ids.Contains(q.UserId));

            foreach (var entity in query)
            {
                entity.IsActive = true;
                await _userRepository.UpdateAsync(entity);
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordActivate, await query.CountAsync());
        }

        public async Task<Result> InActiveAsync(IdsDto dto)
        {
            var result = await _userAdminActiveValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
                return result;

            var query = _userRepository.AsNoTracking()
              .Where(q => dto.Ids.Contains(q.UserId));

            foreach (var entity in query)
            {
                entity.IsActive = false;
                await _userRepository.UpdateAsync(entity);
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordActivate, await query.CountAsync());
        }

        public async Task<Result> DeleteAsync(IdsDto dto)
        {
            if (dto == null || dto.Ids.Count == 0)
                return await new Result().SetErrorAsync(Messages.SelectAtLeastOneItemFromList);

            var result = await _userAdminDeleteValidator.ValidateResultAsync(dto?.Ids);
            if (!result.Success)
                return result;
            int i = 0;
            foreach (var id in dto.Ids)
            {
                var entity = await _userRepository.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == id && !s.IsDeleted);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.Username = entity.Username + '~' + entity.UserId.ToString();
                    entity.EmailAddress = entity.EmailAddress + '~' + entity.UserId.ToString();
                    //_mapper.Map(dto, entity);
                    await _userRepository.UpdateAsync(entity);
                    i++;
                }
            }

            await _unitOfWork.CommitAsync();
            ClearCache();

            return await new Result().SetSuccessAsync(Messages.RecordDelete, i);
        }

        public async Task<Result> GetKlaviyoEmailAndTokenByEmailIdAsync(string emailAddress)
        {

            var entity = new User();
            var userAdminEmailDto = new UserAdminEmailDto();

            if (String.IsNullOrEmpty(emailAddress))
                return await new Result().SetDataAsync(userAdminEmailDto);

            entity = await _userRepository.AsNoTracking().FirstOrDefaultAsync(s => s.EmailAddress == emailAddress);
            if(entity == null)
                return await new Result().SetErrorAsync("Invalid Email Address");
            userAdminEmailDto = _mapper.Map<UserAdminEmailDto>(entity);
            return await new Result().SetDataAsync(userAdminEmailDto);
        }


        public async Task<Result> UpdateKlaviyoEmailAndTokenByEmailIdAsync(UserAdminEmailDto dto)
        {
            dto ??= new UserAdminEmailDto();

            var result = await _userAdminEmailValidator.ValidateResultAsync(dto);
            if (!result.Success)
                return result;

            var entity = await _userRepository.AsNoTracking().FirstAsync(s => s.EmailAddress == dto.EmailAddress);
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();
            ClearCache();
            await result.SetDataAsync(entity.EmailAddress);
            await result.SetSuccessAsync(Messages.RecordSaved);
            return result;
        }
        private static void ClearCache()
        {
            UserAdminCacheManager.ClearCache();
        }

    }
}
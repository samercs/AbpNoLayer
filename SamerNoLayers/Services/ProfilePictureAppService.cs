using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SamerNoLayers.Blob;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace SamerNoLayers.Services;

[Authorize]
public class ProfilePictureAppService: ApplicationService, ITransientDependency
{
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;
    private readonly IRepository<IdentityUser, Guid> _repository;
    private readonly ICurrentUser _currentUser;

    public ProfilePictureAppService(IBlobContainer<ProfilePictureContainer> blobContainer, IRepository<IdentityUser, Guid> repository, ICurrentUser currentUser)
    {
        _blobContainer = blobContainer;
        _repository = repository;
        _currentUser = currentUser;
    }

    public virtual async Task<Guid> UploadAsync(IFormFile file)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream).ConfigureAwait(false);
        if (_currentUser.Id == null)
        {
            return Guid.Empty;
        }

        var user = await _repository.GetAsync(_currentUser.Id.Value).ConfigureAwait(false);
        var pictureId = user.GetProperty<Guid>(ApplicationConsts.ProfilePictureId);

        if (pictureId == Guid.Empty)
        {
            pictureId = Guid.NewGuid();
        }
        var id = pictureId.ToString();
        if (await _blobContainer.ExistsAsync(id).ConfigureAwait(false))
        {
            await _blobContainer.DeleteAsync(id).ConfigureAwait(false);
        }
        await _blobContainer.SaveAsync(id, memoryStream.ToArray()).ConfigureAwait(false);
        user.SetProperty(ApplicationConsts.ProfilePictureId, pictureId);
        await _repository.UpdateAsync(user).ConfigureAwait(false);
        return pictureId;
    }

    public async Task<FileResult> GetAsync()
    {
        if (_currentUser.Id == null)
        {
            throw new FileNotFoundException();
        }

        var user = await _repository.GetAsync(_currentUser.Id.Value).ConfigureAwait(false);
        var pictureId = user.GetProperty<Guid>(ApplicationConsts.ProfilePictureId);
        if (pictureId == default)
        {
            throw new FileNotFoundException();
        }

        var profilePicture = await _blobContainer.GetAllBytesOrNullAsync(pictureId.ToString()).ConfigureAwait(false);
        return new FileContentResult(profilePicture, "image/jpeg");

    }
}